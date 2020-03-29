using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace DS3Stamina
{
	class Stamina
	{
		public Stamina(int SP, int MaxSP)
		{
			this.SP = SP;
			this.MaxSP = MaxSP;
		}
		public int SP;
		public int MaxSP;
	}
	class StaminaReader
	{
		private Process process;
		private IntPtr hProcess;

		public StaminaReader(Process process)
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

		~StaminaReader()
		{
			WinAPIs.CloseHandle(hProcess);
		}

		private static bool ReadPTR(IntPtr hProcess, IntPtr lpBaseAddress, out IntPtr value)
		{
			byte[] buffer = new byte[8];
			if (WinAPIs.ReadProcessMemory(hProcess, lpBaseAddress, buffer, buffer.Length, out IntPtr _))
			{
				value = (IntPtr)BitConverter.ToInt64(buffer);
				return true;
			}
			value = IntPtr.Zero;
			return false;
		}

		internal Stamina ReadStamina()
		{
			const int XA = 0x1F90;
			try
			{
				var BaseB = process.MainModule.BaseAddress + 0x4768E78;
				if (ReadPTR(hProcess, BaseB, out IntPtr P1))
				{
					if (ReadPTR(hProcess, P1 + 0x80, out IntPtr P2))
						if (ReadPTR(hProcess, P2 + XA, out IntPtr P3))
							if (ReadPTR(hProcess, P3 + 0x18, out IntPtr P4))
								if (ReadPTR(hProcess, P4 + 0xF0, out IntPtr P5))
									return new Stamina((int)((Int64)P5 & 0xFFFFFFFF), (int)((Int64)P5 >> 32));
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
	}
}
