using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Xml.Linq;
using Platform.BusinessLogic.Crypto;

namespace Tools.ImportLicense
{
	class Program
	{
		static void Main(string[] args)
		{
			if (args.Length == 0)
			{
				Console.WriteLine("Укажите файл лицензии с полным путем и строку подключения к БД в кавычках");
				return;
			}
			if (!File.Exists(args[0]))
				Console.WriteLine("Файл '"+args[0]+"' не существует");

			string key = "";
			using (FileStream input = File.Open(args[0], FileMode.Open, FileAccess.Read))
			{
				using (StreamReader streamReader = new StreamReader(input))
				{
					key = streamReader.ReadToEnd();
					streamReader.Close();
				}
				input.Close();
			}
			
			XDocument document = XDocument.Load(AppDomain.CurrentDomain.BaseDirectory+"..\\web.config");
			string connectionString = document.Descendants("connectionStrings").Elements().First().Attribute("connectionString").Value;
			using (SqlConnection connection=new SqlConnection(connectionString))
			{
				try
				{
					connection.Open();
				}
				catch
				{
					Console.WriteLine("Не удалось подключиться к БД");
					return;
				}
				string decodeKey = key.Decrypt();
				string caption = decodeKey.Split(new char[] {','})[0];
				int userCount = int.Parse(decodeKey.Split(new char[] {','})[1]);
				DateTime endDate = Convert.ToDateTime(decodeKey.Split(new char[] {','})[2]);
				string publicLegalFormation;
				using (SqlCommand command = new SqlCommand("SELECT count(1) FROM [ref].[PublicLegalFormation] WHERE [Caption]='"+caption+"'", connection))
				{
					int result = (int)command.ExecuteScalar();
					if (result==0)
					{
						connection.Close();
						Console.WriteLine("В базе нет ППО для которого импортируется лицензия");
						return;
					}
					publicLegalFormation = caption;
				}
				int idLicense;
				using (SqlCommand command = new SqlCommand("declare @tmp table(id int);INSERT INTO [ref].[License] ([Caption], [Key], [PublicLegalFormation], [UserCount], [EndDate], [idRefStatus]) OUTPUT inserted.[id] INTO @tmp VALUES (@Name, @key, @PublicLegalFormation, @UserCount, @EndDate, 2); SELECT TOP 1 id FROM @tmp", connection))
				{
					command.Parameters.AddWithValue("@key", key);
					command.Parameters.AddWithValue("@Name", "Лицензия " + caption);
					command.Parameters.AddWithValue("@PublicLegalFormation", publicLegalFormation);
					command.Parameters.AddWithValue("@UserCount", userCount);
					command.Parameters.AddWithValue("@EndDate", endDate);
					idLicense = (int) command.ExecuteScalar();
				}
				using (SqlCommand command = new SqlCommand("INSERT INTO [ml].[License_User] ([idLicense], [idUser]) select top 200 @idLicense, b.id from [ref].[PublicLegalFormation] a inner join [ref].[User] b on b.[idAccessGroup]=a.[idAccessGroup] inner join [ml].[UserRole] c on c.[idUser]=b.[id] inner join [ref].[Role] d on d.[id]=c.[idRole] and d.[Caption]='Администратор' where a.[Caption]=@PublicLegalFormation", connection))
				{
					command.Parameters.AddWithValue("@idLicense", idLicense);
					command.Parameters.AddWithValue("@PublicLegalFormation", publicLegalFormation);
					command.ExecuteNonQuery();
				}
				connection.Close();
			}
		}
	}
}
