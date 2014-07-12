using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Crypto;

namespace Tools.GenerateLicense
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length==2)
			{
				Console.WriteLine("Тест декодирования");
				if (args[0]!="decode")
					return;
				using (FileStream input = File.Open(args[1], FileMode.Open, FileAccess.Read))
				{
					using (StreamReader streamReader = new StreamReader(input))
					{
						var inputString = streamReader.ReadToEnd();
						Console.WriteLine(inputString.Decrypt());
						streamReader.Close();
					}
					input.Close();
				}
				return;
			}
			Console.Write("Введите наименование ППО: ");
			string ppo = Console.ReadLine();
			Console.Write("Введите количество пользователей: ");
			string userCount = Console.ReadLine();
			Console.Write("Введите дату окончания действия лицензии в формате ДД.ММ.ГГГГ: ");
			string endDate = Console.ReadLine();
			int validInt;
			if (!int.TryParse(userCount, out validInt))
				throw new ArgumentException("Не верно введено количество пользователей");
			DateTime validDate;
			if (!DateTime.TryParse(endDate, out validDate))
				throw new ArgumentException("Не верно введена дата окончания действия лицензии");

			using (FileStream output = File.Open(ppo + ".key", FileMode.OpenOrCreate, FileAccess.Write))
			{
				using (StreamWriter streamWriter = new StreamWriter(output))
				{
					string forWrite = ppo + "," + userCount + "," + endDate;
					streamWriter.Write(forWrite.Encrypt());
					streamWriter.Flush();
					streamWriter.Close();
				}
				output.Close();
			}
		}
	}
}
