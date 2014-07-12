using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using Platform.PrimaryEntities;
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
	    /// <param name="addToSvn">Надо ли добавлять к свн</param>
	    public static void InsertToProject(List<string> names, string projectDirectory, string projectName, string sectionName, bool addToSvn = true)
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

	        if (addToSvn)
	        {
	            using (SvnClient svnClient = new SvnClient())
	            {
	                foreach (string name in forSvn)
	                {
	                    try
	                    {
	                        Collection<SvnStatusEventArgs> statuses;
	                        svnClient.GetStatus(name, out statuses);

	                        svnClient.Add(name, new SvnAddArgs {Force = true, Depth = SvnDepth.Infinity});
	                    }
	                    catch (Exception exc)
	                    {
	                        throw new Exception(@"Ошибка метода Add SVN: " + exc.Message + " Добавляемый файл: " + name);
	                    }

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
            if (!File.Exists(project))
                throw new FileNotFoundException(String.Format("Файли проекта '{0}' не найден." ,project));
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
		/// Метод для получения пути до файла как хранится в файле проекта(Пример: Sbor\DbEnums\Entity.xml)
		/// </summary>
		public static string GetFileSolutionInfo(string path, string solutionPath)
		{
			var item = path.Substring(solutionPath.Count());
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
		/// Имя проекта
		/// </summary>
		/// <param name="idSolutionProject">Идентификатор проекта</param>
		public static string GetProjectName(int idSolutionProject)
		{
		    return Solution.ProjectName(idSolutionProject);
		}

		/// <summary>
		/// Возвращает список директорий DbStructure и DbEnums
		/// </summary>
		/// <param name="startPath">Путь, от которого начинается поиск</param>
		/// <returns></returns>
		public static List<string> GetXMLDirectories(string startPath)
		{
			List<string> result = new List<string>();
			DirectoryInfo directoryInfo = new DirectoryInfo(startPath);
			foreach (DirectoryInfo nameDirectory in directoryInfo.GetDirectories())
			{
				if (nameDirectory.Name == "DbStructure" || nameDirectory.Name == "DbEnums")
					result.Add(startPath + @"\" + nameDirectory);
				else
					result.AddRange(GetXMLDirectories(startPath + @"\" + nameDirectory));
			}
			return result;
		}

        /// <summary>
        /// Копирование папки со всеми подпапками и файлами
        /// </summary>
        /// <param name="from">копируем это</param>
        /// <param name="to">копируем сюда</param>
        /// <param name="searchPatternForFiles">Шаблон для файлов которые надо скопировать</param>
        public static void CopyWithRoot(string from, string to, string searchPatternForFiles = null)
        {
            var fromDir = new DirectoryInfo(from);
            var newToDir = Path.Combine(to, fromDir.Name);
            Directory.CreateDirectory(newToDir);
            Copy(from, newToDir, searchPatternForFiles);
	    }

	    /// <summary>
	    /// Копирование папки со всеми подпапками и файлами
	    /// </summary>
	    /// <param name="from">копируем это</param>
	    /// <param name="to">копируем сюда</param>
	    /// <param name="searchPatternForFiles">Шаблон для файлов которые надо скопировать</param>
	    public static void Copy(string from, string to, string searchPatternForFiles = null)
	    {
	        //Now Create all of the directories
	        foreach (string dirPath in Directory.GetDirectories(from, "*", SearchOption.AllDirectories))
	            Directory.CreateDirectory(dirPath.Replace(from, to));

	        var pattern = string.IsNullOrEmpty(searchPatternForFiles) ? "*.*" : searchPatternForFiles;

	        //Copy all the files
	        foreach (string newPath in Directory.GetFiles(from, pattern, SearchOption.AllDirectories))
	            File.Copy(newPath, newPath.Replace(from, to), true);
	    }

	    /// <summary>
	    /// Путь до папки проекта
	    /// </summary>
	    /// <param name="solutionPath">Путь до папки с Solution</param>
	    /// <param name="idSolutionProject">Идентификатор проекта</param>
	    /// <returns></returns>
	    public static string GetProjectPath(string solutionPath,int idSolutionProject)
        {
            return Path.Combine(solutionPath, GetProjectName(idSolutionProject));
        }

        /// <summary>
        /// Удаление папки с файлами
        /// </summary>
        /// <param name="targetDir"></param>
        public static void DeleteDirectory(string targetDir)
        {
            string[] files = Directory.GetFiles(targetDir);
            string[] dirs = Directory.GetDirectories(targetDir);

            foreach (string file in files)
            {
                File.SetAttributes(file, FileAttributes.Normal);
                File.Delete(file);
            }

            foreach (string dir in dirs)
            {
                DeleteDirectory(dir);
            }

            Directory.Delete(targetDir, false);
        }
	}
}
