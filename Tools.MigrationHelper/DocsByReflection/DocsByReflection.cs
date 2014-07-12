﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Xml;

namespace Tools.MigrationHelper
{
	/// <summary>
	/// Utility class to provide documentation for various types where available with the assembly
	/// </summary>
	public static class DocsByReflection
	{
		/// <summary>
		/// Provides the documentation comments for a specific method
		/// </summary>
		/// <param name="methodInfo">The MethodInfo (reflection data ) of the member to find documentation for</param>
		/// <returns>The XML fragment describing the method</returns>
		public static XmlElement XmlFromMember(MethodInfo methodInfo)
		{
			// Calculate the parameter string as this is in the member name in the XML
			string parametersString = "";
			foreach (ParameterInfo parameterInfo in methodInfo.GetParameters())
			{
				if (parametersString.Length > 0)
				{
					parametersString += ",";
				}

				parametersString += parameterInfo.ParameterType.FullName;
			}

			//AL: 15.04.2008 ==> BUG-FIX remove “()” if parametersString is empty
			if (parametersString.Length > 0)
				return XMLFromName(methodInfo.DeclaringType, 'M', methodInfo.Name + "(" + parametersString + ")");
			else
				return XMLFromName(methodInfo.DeclaringType, 'M', methodInfo.Name);
		}

		/// <summary>
		/// Provides the documentation comments for a specific member
		/// </summary>
		/// <param name="memberInfo">The MemberInfo (reflection data) or the member to find documentation for</param>
		/// <returns>The XML fragment describing the member</returns>
		public static XmlElement XmlFromMember(MemberInfo memberInfo)
		{
			// First character [0] of member type is prefix character in the name in the XML
			return XMLFromName(memberInfo.DeclaringType, memberInfo.MemberType.ToString()[0], memberInfo.Name);
		}

		/// <summary>
		/// Provides the documentation comments for a specific type
		/// </summary>
		/// <param name="type">Type to find the documentation for</param>
		/// <returns>The XML fragment that describes the type</returns>
		public static XmlElement XMLFromType(Type type)
		{
			// Prefix in type names is T
			return XMLFromName(type, 'T', "");
		}

		/// <summary>
		/// Obtains the XML Element that describes a reflection element by searching the 
		/// members for a member that has a name that describes the element.
		/// </summary>
		/// <param name="type">The type or parent type, used to fetch the assembly</param>
		/// <param name="prefix">The prefix as seen in the name attribute in the documentation XML</param>
		/// <param name="name">Where relevant, the full name qualifier for the element</param>
		/// <returns>The member that has a name that describes the specified reflection element</returns>
		private static XmlElement XMLFromName(Type type, char prefix, string name)
		{
			string fullName;

			if (String.IsNullOrEmpty(name))
			{
				fullName = prefix + ":" + type.FullName;
			}
			else
			{
				fullName = prefix + ":" + type.FullName + "." + name;
			}

			XmlDocument xmlDocument = XMLFromAssembly(type.Assembly);

			XmlElement matchedElement = null;


			var badlyComments = xmlDocument["doc"]["members"].SelectNodes(@"comment()");
			if (badlyComments.Count > 0)
			{
				StringBuilder mess = new StringBuilder();
				foreach (XmlComment badlyComment in badlyComments)
				{
					mess.Append(badlyComment.InnerText + "\n");
				}
				throw new Exception("Ошибка в описании xml комментария у следующих элементов :" + mess);
			}
			 
			foreach (XmlElement xmlElement in xmlDocument["doc"]["members"])
			{
				if (xmlElement.Attributes["name"].Value.Equals(fullName))
				{
					if (matchedElement != null)
					{
						throw new DocsByReflectionException("Multiple matches to query", null);
					}

					matchedElement = xmlElement;
				}
			}

//			if (matchedElement == null)
//			{
//				throw new DocsByReflectionException("Отсутствует xml комментарий", null);
//			}

			return matchedElement;
		}

		/// <summary>
		/// A cache used to remember Xml documentation for assemblies
		/// </summary>
		static Dictionary<Assembly, XmlDocument> cache = new Dictionary<Assembly, XmlDocument>();

		/// <summary>
		/// A cache used to store failure exceptions for assembly lookups
		/// </summary>
		static Dictionary<Assembly, Exception> failCache = new Dictionary<Assembly, Exception>();

		/// <summary>
		/// Obtains the documentation file for the specified assembly
		/// </summary>
		/// <param name="assembly">The assembly to find the XML document for</param>
		/// <returns>The XML document</returns>
		/// <remarks>This version uses a cache to preserve the assemblies, so that 
		/// the XML file is not loaded and parsed on every single lookup</remarks>
		public static XmlDocument XMLFromAssembly(Assembly assembly)
		{
			if (failCache.ContainsKey(assembly))
			{
				throw failCache[assembly];
			}

			try
			{

				if (!cache.ContainsKey(assembly))
				{
					// load the docuemnt into the cache
					cache[assembly] = XMLFromAssemblyNonCached(assembly);
				}

				return cache[assembly];
			}
			catch (Exception exception)
			{
				failCache[assembly] = exception;
				throw exception;
			}
		}

		/// <summary>
		/// Loads and parses the documentation file for the specified assembly
		/// </summary>
		/// <param name="assembly">The assembly to find the XML document for</param>
		/// <returns>The XML document</returns>
		private static XmlDocument XMLFromAssemblyNonCached(Assembly assembly)
		{
			string assemblyFilename = assembly.CodeBase;

			const string prefix = "file:///";

			if (assemblyFilename.StartsWith(prefix))
			{
				StreamReader streamReader;

				try
				{
					streamReader = new StreamReader(Path.ChangeExtension(assemblyFilename.Substring(prefix.Length), ".xml"));
				}
				catch (FileNotFoundException exception)
				{
					throw new DocsByReflectionException("XML документации нет (убедитесь, что он включен в свойствах проекта при билде)", exception);
				}

				var xmlDocument = new XmlDocument();
				xmlDocument.Load(streamReader);
				return xmlDocument;
			}
			else
			{
				throw new DocsByReflectionException("Не удалось установить имя файла сборки", null);
			}
		}
	}
}
