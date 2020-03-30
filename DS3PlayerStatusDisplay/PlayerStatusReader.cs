using DS3Stamina.MemoryTools;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DS3Stamina
{
	class Gauge
	{
		public Gauge(int current, int max)
		{
			this.Current = current;
			this.Max = max;
		}
		public int Current;
		public int Max;
		public double Ratio { get => (double)Current * 100.0 / (double)Max; }
	}
	class PlayerStatusReader
	{
		private readonly Process process;
		private readonly IntPtr hProcess;

		const int XA = 0x1F90;

		// stamina
		private readonly PointerReader StaminaPointerReader =
			new PointerReader(0x4768E78, new int[] { 0x80, XA, 0x18, 0xF0 }, PointerReader.TypeOfValue.Dword);
		private readonly PointerReader MaxStaminaPointerReader =
			new PointerReader(0x4768E78, new int[] { 0x80, XA, 0x18, 0xF4 }, PointerReader.TypeOfValue.Dword);
		// hp
		private readonly PointerReader HPPointerReader =
			new PointerReader(0x4768E78, new int[] { 0x80, XA, 0x18, 0xD8 }, PointerReader.TypeOfValue.Dword);
		private readonly PointerReader MaxHPPointerReader =
			new PointerReader(0x4768E78, new int[] { 0x80, XA, 0x18, 0xDC }, PointerReader.TypeOfValue.Dword);



		public PlayerStatusReader(Process process)
		{
			this.process = process;
			this.hProcess = WinAPIs.OpenProcess(process, WinAPIs.ProcessAccessFlags.VirtualMemoryRead);
			if ((int)this.hProcess == -1)
				throw new Exception("Can't open process");
		}

		internal void Close()
		{
			WinAPIs.CloseHandle(hProcess);
		}

		~PlayerStatusReader()
		{
			WinAPIs.CloseHandle(hProcess);
		}

		private Gauge ReadGauge(PointerReader currentValuePointReader, PointerReader maxValuePointerReader)
		{
			try
			{
				var resultCurrent = currentValuePointReader.Read(hProcess, (long)process.MainModule.BaseAddress);
				var resultMAX = maxValuePointerReader.Read(hProcess, (long)process.MainModule.BaseAddress);

				if (!resultCurrent.ErrorFirstRead && !resultMAX.ErrorFirstRead)
				{
					if (!resultCurrent.ErrorChain && !resultMAX.ErrorChain)
						return new Gauge((int)resultCurrent.Value, (int)resultMAX.Value);
				}
				else
				{
					throw new ReadProcessException();
				}
			}
			catch
			{
				throw new ReadProcessException();
			}
			return null;
		}

		internal Gauge ReadStamina()
		{
			return ReadGauge(StaminaPointerReader, MaxStaminaPointerReader);
		}

		internal Gauge ReadHP()
		{
			return ReadGauge(HPPointerReader, MaxHPPointerReader);
		}
	}
}
