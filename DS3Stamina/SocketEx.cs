using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;

namespace DS3Stamina
{
	static class SocketExt
	{
		public static void Send(this Socket socket, string data)
		{
			socket.Send(Encoding.ASCII.GetBytes(data));
		}
	}
}
