using System;
using System.Collections.Generic;
using System.Text;

namespace DS3Stamina.MemoryTools
{
	class ReadProcessException : Exception
	{
		public override string Message => "Can't read process memory";
	}
}
