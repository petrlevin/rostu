using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Tools.MigrationHelper.Core.Tasks.CheckTask;


namespace Tools.MigrationHelper.Tests.Tasks
{
	/// <summary>
	/// Данные методы являются вспомогательным механизмом для получения рузультата сериализации как эталона формата хранения информации о тестах.
	/// Т.е. вызвал Serialize - получит xml - записал в ресурс и начал вводить реальные данные.
	/// </summary>
	[NUnit.Framework.TestFixture]
	[ExcludeFromCodeCoverage]
	public class CheckTests
	{
		[NUnit.Framework.Test]
		public void Serialize()
		{
			var tests = new List<Test>()
				{
					new Test
						{
							Title = "title",
							SqlCommand = "command"
						}
				};

			var t = typeof (List<Test>);
			var serializer = new XmlSerializer(t);
			StringBuilder sb = new StringBuilder();
			XmlWriter wr = XmlWriter.Create(sb);
			serializer.Serialize(wr, tests);
			var result = sb.ToString();
		}

		[NUnit.Framework.Test]
		[NUnit.Framework.Ignore]
		public void Deserialize()
		{

		}
	}
}
