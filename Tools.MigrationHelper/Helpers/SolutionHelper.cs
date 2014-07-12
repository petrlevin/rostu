using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Platform.PrimaryEntities.DbEnums;
using SharpSvn;

namespace Tools.MigrationHelper.Helpers
{
	/// <summary>
	/// Помощник по работе с файлами проектов (*.csproj)
	/// </summary>
	public class SolutionHelper
	{
		/// <summary>
		/// Добавляет в xml-файл проекта (*.csproj) информацию о входящих в него файлах.
		/// </summary>
		/// <param name="names">Перечень имен файлов. Пример одного элемента: DbStructure\Abc.xml</param>
		/// <param name="projectDirectory">Путь до директории с проектом</param>
		/// <param name="projectName">Имя проекта. Например: Platform.PrimaryEntities</param>
		/// <param name="sectionName">Имя секции в xml-файле проекта. Например: Content</param>
		public static void InsertToProject(List<string> names, string projectDirectory, string projectName, string sectionName)
		{
			if(!names.Any())
				return;
			var doc = new XmlDocument();
			var project = Path.Combine(projectDirectory , projectName + ".csproj");
			var reader = new StreamReader(project);
			doc.Load(reader);

			bool createNewSection = true;
			bool hasInsertedNodes = false;

			foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
			{
				if ((childNode.FirstChild != null ? childNode.FirstChild.Name : " ") != sectionName)
					continue;
				foreach (var name in names)
				{
					//проверяем на существование этого файла в проекте
					if (!childNode.ChildNodes.Cast<XmlNode>().Any(a => a.Attributes.Cast<XmlAttribute>().Any(w => w.Value == name)))
					{
						// вставляем в конец списка новый элемент
						childNode.InsertAfter(CreateXmlElement(doc, sectionName, name), childNode.LastChild);
						hasInsertedNodes = true;
					}
				}
				createNewSection = false;
			}

			//Если нет области с такой секцией создаем новую
			if (createNewSection)
			{
				var node = doc.GetElementsByTagName("ItemGroup");
				var newNode = node[0].CloneNode(false);
				foreach (var name in names)
				{
					// вставляем в конец списка новый элемент
					newNode.InsertAfter(CreateXmlElement(doc,sectionName,name), newNode.LastChild);
					hasInsertedNodes = true;
				}
				doc.DocumentElement.AppendChild(newNode);
			}

			reader.Close();
			if (hasInsertedNodes)
			{
				var writer = new StreamWriter(project);
				doc.Save(writer);
				writer.Close();
			}

			List<string> forSvn = new List<string>();
			foreach (string name in names)
			{
				string path = Path.Combine(projectDirectory, name);
				int index = path.LastIndexOf("\\");
				path = path.Substring(0, index);
				if (forSvn.All(a => a != path))
				{
					forSvn.Add(path);
				}
			}
			using (SvnClient svnClient = new SvnClient())
			{
				foreach (string name in forSvn)
				{
					try
					{
						Collection<SvnStatusEventArgs> statuses;
						svnClient.GetStatus(name, out statuses);

						svnClient.Add(name, new SvnAddArgs { Force = true, Depth = SvnDepth.Infinity });
					}
					catch (Exception exc)
					{
						throw new Exception(@"Ошибка метода Add SVN: " + exc.Message + " Добавляемый файл: " + name);
					}
					
				}
			}
		}

		private static XmlElement CreateXmlElement(XmlDocument doc, string sectionName, string name)
		{
			XmlElement element = doc.CreateElement(sectionName, "http://schemas.microsoft.com/developer/msbuild/2003");
			element.RemoveAllAttributes();
			element.SetAttribute("Include", name);
			return element;
		}

		/// <summary>
		/// Удаляет из xml-файла проекта (*.csproj) информацию о входящих в него файлах.
		/// </summary>
		/// <param name="file">Файл для удаления</param>
		public static void DeleteFromProject(FileSystemInfo file)
		{
			var doc = new XmlDocument();
			var projectDirectory = GetFileProjectDir(file.FullName);
			var project = Path.Combine(projectDirectory.FullName, projectDirectory.Name + ".csproj");
			var reader = new StreamReader(project);
			doc.Load(reader);

			foreach (XmlNode childNode in doc.DocumentElement.ChildNodes)
			{
				var item = GetFileProjectInfo(file.FullName,projectDirectory.Name);
				var nodeToDelete = childNode.ChildNodes.Cast<XmlNode>()
											.FirstOrDefault(a => a.Attributes.Cast<XmlAttribute>().Any(w => w.Value == item));
				if (nodeToDelete != null)//проверяем на существование этого файла в проекте
				{
					childNode.RemoveChild(nodeToDelete);
				}
			}

			reader.Close();
			var writer = new StreamWriter(project);
			doc.Save(writer);
			writer.Close();

			using (SvnClient svnClient = new SvnClient())
			{
				svnClient.Delete(file.FullName, new SvnDeleteArgs {Force = true});
			}
		}

		/// <summary>
		/// Метод для получения по пути до файла папки проекта в котором он находится
		/// </summary>
		/// <param name="filePath">путь файла</param>
		/// <returns>Папка проекта в котором находится файл</returns>
		public static DirectoryInfo GetFileProjectDir(string filePath)
		{
			StringBuilder builder = new StringBuilder();
			var strings = filePath.Split('\\');
			var projectNames = Enum.GetNames(typeof(SolutionProject));
			foreach (var s in strings)
			{
				if (projectNames.Any(a => a.Replace('_','.') == s))
				{
					builder.Append(s);
					break;
				}
				else
				{
					builder.Append(s + '\\');
					continue;
				}
			}

			var dir = builder.ToString();

			if(string.IsNullOrEmpty(dir))
				throw new Exception("Для файла не найден проект");

			return new DirectoryInfo(builder.ToString());
		}

		/// <summary>
		/// Метод для получения пути до файла как хранится в файле проекта(Пример: DbEnums\Entity.xml)
		/// </summary>
		private static string GetFileProjectInfo(string path, string projectName)
		{
			var item = path.Substring(path.IndexOf(projectName, StringComparison.Ordinal) + projectName.Length);
			if (item.StartsWith("\\"))
				item = item.Substring(1);
			return item;
		}

		/// <summary>
		/// Удаление xml-файлов из папок DbStructure и DbEnums.
		/// Во всех поддиректориях любой вложенности каталога <paramref name="directoryInfo"/> ищутся папки DbStructure и DbEnums, в которых удаляются xml-файлы.
		/// </summary>
		/// <param name="directoryInfo">Папка проекта</param>
		/// <param name="ignoredFiles">Файлы которые были изменены и их не надо удалять</param>
		public static void CleanDbStructure(DirectoryInfo directoryInfo, List<string> ignoredFiles = null)
		{
			if (directoryInfo == null)
				return;
			if (!directoryInfo.FullName.Contains("DbStructure") && !directoryInfo.FullName.Contains("DbEnums"))
			{
				DirectoryInfo directoryStr = directoryInfo.GetDirectories("DbStructure").FirstOrDefault();
				CleanDbStructure(directoryStr, ignoredFiles);
				DirectoryInfo directoryEnm = directoryInfo.GetDirectories("DbEnums").FirstOrDefault();
				CleanDbStructure(directoryEnm, ignoredFiles);
			}
			else
			{
				FileInfo[] files = directoryInfo.GetFiles("*.xml");
				foreach (FileInfo fileInfo in files)
				{
					if (ignoredFiles != null && ignoredFiles.Any(a => fileInfo.FullName.EndsWith(a))) continue;
					DeleteFromProject(fileInfo);
					fileInfo.Delete();
				}
				foreach (var directory in directoryInfo.GetDirectories())
				{
					CleanDbStructure(directory, ignoredFiles);
				}
			}
		}

		/// <summary>
		/// Проверяет наличие пути, в случае отсутствия создает
		/// </summary>
		/// <param name="path">Путь который должен быть</param>
		public static void CheckPath(string path)
		{
			DirectoryInfo dir = new DirectoryInfo(path);
			if (!dir.Exists)
			{
				CheckPath(dir.Parent.FullName);
				Directory.CreateDirectory(dir.FullName);
			}
		}

		/// <summary>
		/// Имя проекта
		/// </summary>
		/// <param name="idSolutionProject">Идентификатор проекта</param>
		public static string GetProjectName(int idSolutionProject)
		{
			return ((SolutionProject)idSolutionProject).ToString().Replace("_", ".");
		}
	}
}
