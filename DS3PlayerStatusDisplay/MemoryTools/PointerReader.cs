using System;
using System.Collections.Generic;
using System.Text;

namespace DS3Stamina.MemoryTools
{
	public class PointerReader
	{
		public enum TypeOfValue
		{
			Byte,
			Word,
			Dword,
			Qword
		}
		public PointerReader(long baseAddress, int[] offsets, TypeOfValue type)
		{
			BaseAddress = baseAddress;
			Offsets = offsets;
			Type = type;
		}

		public readonly long BaseAddress;
		public readonly int[] Offsets;
		public readonly TypeOfValue Type;

		public struct ReadPtrStatus
		{
			public override string ToString()
			{
				return (Value == null) ? "null" : Value.ToString();
			}
			public bool Error => ErrorChain | ErrorFirstRead;

			public readonly bool ErrorFirstRead; // usually means that the process is not existing or not open

			public readonly bool ErrorChain; // usually mean that the process is running but the 
											 // pointers' offsets are invalid or not filled yet 
											 // (example : reading playerbase of a gain while in main screen)
			public object Value;
			public ReadPtrStatus(object value)
			{
				Value = value;
				ErrorChain = ErrorFirstRead = false;
			}
			public ReadPtrStatus(bool errorFirstRead, bool errorChain = false)
			{
				Value = null;
				ErrorChain = errorChain;
				ErrorFirstRead = errorFirstRead;
			}
		}


		public ReadPtrStatus Read(IntPtr hProcess, long ModuleBase)
		{
			byte[] buffer = new byte[8];
			if (!WinAPIs.ReadProcessMemory(hProcess,(IntPtr)(ModuleBase + BaseAddress), buffer, buffer.Length, out IntPtr _))
				return new ReadPtrStatus(true);
			else
			{
				IntPtr NextBase = (IntPtr)BitConverter.ToInt64(buffer);

				for (int i = 0; i < Offsets?.Length; i++)
				{
					if (!ReadPTR(hProcess, (IntPtr)NextBase + Offsets[i], out NextBase))
						return new ReadPtrStatus(false, true);
				}
				return new ReadPtrStatus(Cast((long)NextBase, Type));
			}
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

		private static object Cast(Int64 value, TypeOfValue type)
		{
			switch (type)
			{
				case TypeOfValue.Byte:
					return (byte)value;
				case TypeOfValue.Word:
					return (short)value;
				case TypeOfValue.Dword:
					return (int)value;
				case TypeOfValue.Qword:
					return value;
				default:
					return null;
			}
		}

	}
}
