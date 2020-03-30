using DS3Stamina.MemoryTools;
using System;
using System.Diagnostics;
using System.Threading;

namespace DS3Stamina
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Darksouls3 player status display by COB\n" +
				"Based on table bt \"The Grand Archives\"\n\n" +
				"What does it do?\n" +
				"It reads HP and SP from DarksoulsIII process and displays it with Prismatik\n" +
				"-----------------------------------------\n");

			while (true)
			{
				Console.WriteLine("Waiting for process");
				Process process = null;
				while (process == null)
				{
					Thread.Sleep(200);
					process = SearchForProces();
				}

				Console.WriteLine($"Working on process with ID : {process.Id}");
				var reader = new PlayerStatusReader(process);
				var writer = new PrismatikWriter("127.0.0.1", 3636, 80, -12, true);
				StaminaDisplayLoop(reader, writer);
			}
		}

		private static void StaminaDisplayLoop(PlayerStatusReader reader, PrismatikWriter writer)
		{
			writer.Connect();

			double? prevStamina = null;
			double? prevHP = null;

			try
			{
				while (true)
				{
					var stamina = reader.ReadStamina();
					var hp = reader.ReadHP();
					if (stamina == null || hp == null)
					{
						writer.Unlock();
						continue;
					}

					writer.Lock();
					if (ShouldUpdate(ref prevStamina, ref prevHP, stamina, hp))
						writer.DisplayPlayerStatus(stamina.Ratio,hp.Ratio);
				}
			}
			catch (ReadProcessException ex)
			{
				Console.WriteLine(ex.Message);
			}
			writer.Unlock();
		}

		private static bool ShouldUpdate(ref double? prevStamina, ref double? prevHP, Gauge stamina, Gauge hp)
		{
			if (prevStamina == null || Math.Abs((stamina.Ratio - prevStamina.Value)) >= 0.2)
			{
				prevStamina = stamina.Ratio;
				return true;
			}

			if (prevHP == null || Math.Abs((hp.Ratio - prevHP.Value)) >= 0.2)
			{
				prevHP = hp.Ratio;
				return true;
			}
			return false;
		}

		private static Process SearchForProces()
		{
			var ds3 = Process.GetProcessesByName("DarkSoulsIII");
			if (ds3.Length > 0)
			{
				int index = 0;
				if (ds3.Length > 1)
				{
					Console.WriteLine("Found multiple processes, please select one:");
					for (int i = 0; i < ds3.Length; i++)
						Console.WriteLine($"{i + 1} - {ds3[i].Id}");
					while (true)
					{
						int selection = 0;
						if (!int.TryParse(Console.ReadLine(), out selection))
						{
							Console.WriteLine("couldn't parse value, please enter process index");
							continue;
						}
						if (selection > ds3.Length || selection < 1)
						{
							Console.WriteLine("Invalid index");
							continue;
						}
						index = selection - 1;
						break;
					}

				}
				return ds3[index];
			}
			return null;
		}
	}
}

