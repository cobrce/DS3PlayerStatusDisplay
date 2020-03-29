using System;
using System.Collections.Generic;
using System.Text;

namespace DS3Stamina
{
	class ReadProcessException : Exception
	{
		public override string Message => "Can't read process memory";
	}
}
