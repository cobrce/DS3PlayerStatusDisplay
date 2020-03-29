using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;

namespace DS3Stamina
{
	class PrismatikWriter
	{
		private string address;
		private int port;
		private int nLeds;
		private int offset;
		private bool reverse;
		private readonly Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

		public PrismatikWriter(string address, int port, int nLeds,int offset,bool reverse)
		{
			this.address = address;
			this.port = port;
			this.nLeds = nLeds;
			this.offset = offset;
			this.reverse = reverse;

		}

		~PrismatikWriter()
		{
			if (Connected)
				Unlock();
		}

		public bool Connect()
		{
			socket.Connect(address, port);
			return socket.Connected;
		}

		public bool Connected { get => socket.Connected; }

		public void Disconnect()
		{
			socket.Disconnect(true);
		}

		public void Lock()
		{
			if (Connected)
				socket.Send("lock\n");
		}

		public void Unlock()
		{
			if (Connected)
				socket.Send("unlock\n");
		}

		public void DisplayStamina(double ratioPerCent)
		{
			if (ratioPerCent < 0.0)
				ratioPerCent = 0.0;
			else if (ratioPerCent > 100.0)
				ratioPerCent = 100.0;

			StringBuilder messageBuilder = new StringBuilder("setcolor:");

			// 1 - 0,255,0; 5 - 255,200,30;
			int threshold = (int)(ratioPerCent * nLeds / 100.0);
			for (int i = 0; i < nLeds; i++)
			{
				string value = (i < threshold) ? "70,255,30" : "0,0,0";
				messageBuilder.AppendFormat("{0}-{1};", ShiftLedPosition(i), value);
			}
			messageBuilder.AppendLine();
			socket.Send(messageBuilder.ToString());
		}

		private int ShiftLedPosition(int i)
		{
			int pos = i+1;
			if (reverse)
				pos = nLeds - pos+1;
			pos += offset;
			if (pos > nLeds || pos < 1)
				pos += nLeds;
			return pos;
		}
	}
}
