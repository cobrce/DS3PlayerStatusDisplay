﻿using System;
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
			Qword,
			IntPtr
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
											 // (example : reading the playerbase of a game while in main screen)
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

		public ReadPtrStatus Next(IntPtr hProcess, IntPtr previousPointer)
		{
			for (int i = 0; i < Offsets?.Length; i++)
			{
				if (!ReadPTR(hProcess, (IntPtr)previousPointer + Offsets[i], out previousPointer))
					return new ReadPtrStatus(false, true);
			}
			return new ReadPtrStatus(Cast((long)previousPointer, Type));
		}

		public ReadPtrStatus Read(IntPtr hProcess, long ModuleBase)
		{
			IntPtr pointer = IntPtr.Zero;
			if (!ReadPTR(hProcess, (IntPtr)(ModuleBase + BaseAddress),out pointer))
				return new ReadPtrStatus(true);
			else
				return Next(hProcess, pointer);
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
				case TypeOfValue.IntPtr:
					return new IntPtr(value);
				default:
					return null;
			}
		}

	}
}
