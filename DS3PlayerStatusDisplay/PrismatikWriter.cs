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

		public PrismatikWriter(string address, int port, int nLeds, int offset, bool reverse)
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

		public void DisplayPlayerStatus(double staminaRatio, double hpRatio)
		{
			if (staminaRatio < 0.0)
				staminaRatio = 0.0;
			else if (staminaRatio > 100.0)
				staminaRatio = 100.0;

			if (hpRatio < 0.0)
				hpRatio = 0.0;
			else if (hpRatio > 100.0)
				hpRatio = 100.0;

			StringBuilder messageBuilder = new StringBuilder("setcolor:");

			// 1 - 0,255,0; 5 - 255,200,30;
			int staminaThreshold = (int)(staminaRatio * nLeds / 200.0);
			int hpThreshold = nLeds - (int)(hpRatio * nLeds / 200.0);

			for (int i = 0; i < nLeds; i++)
			{
				string value = "";
				if (i < nLeds / 2)
					value = (staminaRatio == 0.0) ? "255,255,0" : ((i < staminaThreshold) ? (staminaRatio == 100.0 ? "0,230,0" : "70,255,30") : "0,0,0");
				else
					value = (i >= hpThreshold) ? (hpRatio == 100.0 ? "230,0,0" : "255,70,30") : "0,0,0";
				messageBuilder.AppendFormat("{0}-{1};", ShiftLedPosition(i), value);
			}
			messageBuilder.AppendLine();
			socket.Send(messageBuilder.ToString());
		}

		private int ShiftLedPosition(int i)
		{
			int pos = i + 1;
			if (reverse)
				pos = nLeds - pos + 1;
			pos += offset;
			if (pos > nLeds || pos < 1)
				pos += nLeds;
			return pos;
		}
	}
}
