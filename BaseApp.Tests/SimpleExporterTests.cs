using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using BaseApp.XmlExchange.Export;
using NUnit.Framework;
using Platforms.Tests.Common;

namespace BaseApp.Tests
{
	[ExcludeFromCodeCoverage]
	[TestFixture]
	class SimpleExporterTests : SqlTests
	{
		[Test]
		public void Test()
		{
			XDocument document = new XDocument();
			document.Add(new XElement("root"));
			XDocument document1 = new XDocument();
			document1.Add(new XElement("root"));
			document1.Root.Add(new XElement("test1", new XElement("id", 1)));
			document1.Root.Add(new XElement("test1", new XElement("id", 2)));
			document1.Root.Add(new XElement("test1", new XElement("id", 3)));
			document.Root.AddFirst(document1.Root.Elements());

			XDocument document2 = new XDocument();
			document2.Add(new XElement("root"));
			document2.Root.Add(new XElement("test2", new XElement("id", 11)));
			document2.Root.Add(new XElement("test2", new XElement("id", 22)));
			document2.Root.Add(new XElement("test2", new XElement("id", 33)));
			document.Root.AddFirst(document2.Root.Elements());

			var aa2 = document.Root.Elements("test2").Descendants("id");
			IEnumerable<int> aa = document.Root.Elements("test2").Descendants("id").Select(a => int.Parse(a.Value));
		}

		[Test]
		public void Test2()
		{
			SimpleExporter simpleExporter = new SimpleExporter(-2147483488);
			simpleExporter.Execute();
		}
	}
}
