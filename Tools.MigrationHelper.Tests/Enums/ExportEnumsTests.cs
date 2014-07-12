using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Assert = NUnit.Framework.Assert;
using Tools.MigrationHelper.EnumsProcessing;

namespace Tools.MigrationHelper.Tests.Enums
{
	/// <summary>
	/// Tests for ExportEnums
	/// </summary>
	[ExcludeFromCodeCoverage]
	[TestFixture]
	public class ExportEnumsTests
	{
		[Test]
        [System.Obsolete("")]
		public void GetEnumsTest_Exeption()
		{
			var enumProc = new EnumsFetcher();
			Assert.Throws<Exception>(() => enumProc.GetEnums(null));
			Assert.Throws<Exception>(() => enumProc.GetEnums(""));
			Assert.Throws<Exception>(() => enumProc.GetEnums(Directory.GetCurrentDirectory()));
			Assert.Throws<Exception>(() => enumProc.GetEnumsWithoutPrimary(null));
			Assert.Throws<Exception>(() => enumProc.GetEnums(null,null,false,null));
		}
		
		[Test]
        [System.Obsolete("")]
		public void GetEnumsTest()
		{
			var enumProc = new EnumsFetcher();
			var path = Directory.GetCurrentDirectory();
			var solutionPath = GetFileProjectDir(path, "Tools.MigrationHelper.Tests").Parent.FullName;
			var tables = enumProc.GetEnums(solutionPath);

			Assert.IsTrue(tables.Any());

			//Необходимые енумераторы
			var enums = new[] { "EntityType" };
			//Игнорируемые енумераторы
			var ingnoredEnums = new[] { "EntityType" };
			var tables2 = enumProc.GetEnums(solutionPath, enums, false, ingnoredEnums);

			Assert.IsTrue(!tables2.Any());
		}

		private static DirectoryInfo GetFileProjectDir(string filePath, string projectName)
		{
			StringBuilder builder = new StringBuilder();
			var strings = filePath.Split('\\');
			foreach (var s in strings)
			{
				if (projectName == s)
				{
					builder.Append(s);
					break;
				}
				builder.Append(s + '\\');
			}

			var dir = builder.ToString();

			if (string.IsNullOrEmpty(dir))
				throw new Exception("Для файла не найден проект");

			return new DirectoryInfo(builder.ToString());
		}
	}
}
