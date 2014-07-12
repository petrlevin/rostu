//
// NAntContrib
// Copyright (C) 2001-2005 Gerry Shaw
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307 USA
//

using System;
using System.IO;
using System.Text;
using System.Xml;
using NAnt.Core;
using NUnit.Framework;
using Platforms.Tests.Common;

namespace Tools.MigrationHelper.Tests
{
    /// <summary>
    /// Base class for running build files and checking results.
    /// </summary>
    /// <remarks>
    ///   <para>Provides support for quickly running a build and capturing the output.</para>
    /// </remarks>
    public abstract class BuildTestBase :SqlTestBase
    {

        /// <summary>
        /// Выполняет указанную операцию MigrationHelper'а
        /// </summary>
        /// <param name="action"></param>

        protected Project ExucuteDeployMH(string action)
        {
            string xml = @"
                    <project>
                        
                        <loadtasks assembly=""${nant::get-base-directory()}\Tools.MigrationHelper.Core.dll"" />
                        <registerindi connectionstring =""" + connectionString + @"""></registerindi> 
                        <" + action.ToLower() + @" connectionstring =""" + connectionString + @""" devid=""1"" sourcepath=""" + solutionPath + @"""/>
                    </project>";


            return RunBuild(xml).Project;

        }


        #region Private Instance Fields



        private string _tempDirName = null;
        protected string solutionPath;

        #endregion Private Instance Fields

        #region Public Instance Properties

        /// <summary>
        /// Gets the temporary directory name for this test case.
        /// </summary>
        /// <value>
        /// The temporary directory name for this test case.
        /// </value>
        /// <remarks>
        /// Should be in the form %temp%\ClassName (ex. c:\temp\Tests.NAnt.Core.BuildTestBase).
        /// </remarks>
        public string TempDirName
        {
            get { return _tempDirName; }
        }

        /// <summary>
        /// Gets the temporary directory for this test case.
        /// </summary>
        /// <value>
        /// The temporary directory for this test case.
        /// </value>
        /// <remarks>
        /// Should be in the form %temp%\ClassName (ex. c:\temp\Tests.NAnt.Core.BuildTestBase).
        /// </remarks>
        public DirectoryInfo TempDirectory
        {
            get { return new DirectoryInfo(_tempDirName); }
        }

        #endregion Public Instance Properties

        #region Public Instance Methods

        /// <summary>
        /// Runs the XML as a NAnt project and returns the console output as a 
        /// string.
        /// </summary>
        /// <param name="xml">XML representing the build file contents.</param>
        /// <returns>
        /// The console output.
        /// </returns>
        public BuildResult RunBuild(string xml)
        {
            return RunBuild(xml, Level.Info);
        }

        /// <summary>
        /// Runs the XML as a NAnt project and returns the console output as a 
        /// string.
        /// </summary>
        /// <param name="xml">XML representing the build file contents.</param>
        /// <param name="listener">A <see cref="IBuildListener" /> to which all build events will be dispatched.</param>
        /// <returns>
        /// The console output.
        /// </returns>
        public BuildResult RunBuild(string xml, IBuildListener listener)
        {
            return RunBuild(xml, Level.Info, listener);
        }

        /// <summary>
        /// Runs the XML as NAnt project and returns the console output as a 
        /// string.
        /// </summary>
        /// <param name="xml">XML representing the build file contents.</param>
        /// <param name="level">The build output threshold.</param>
        /// <returns>
        /// The console output.
        /// </returns>
        public BuildResult RunBuild(string xml, Level level)
        {
            Project project = CreateFilebasedProject(xml, level);
            return new BuildResult()
                       {
                           Project = project,
                           Output = ExecuteProject(project)
                       };

                
        }

        /// <summary>
        /// Runs the XML as NAnt project and returns the console output as a 
        /// string.
        /// </summary>
        /// <param name="xml">XML representing the build file contents.</param>
        /// <param name="level">The build output threshold.</param>
        /// <param name="listener">A <see cref="IBuildListener" /> to which all build events will be dispatched.</param>
        /// <returns>
        /// The console output.
        /// </returns>
        public BuildResult RunBuild(string xml, Level level, IBuildListener listener)
        {
            Project project = CreateFilebasedProject(xml, level);

            //use Project.AttachBuildListeners to attach.
            IBuildListener[] listners = { listener };
            project.AttachBuildListeners(new BuildListenerCollection(listners));

            // execute the project
            return new BuildResult()
                       {
                           Project = project,
                           Output = ExecuteProject(project)
                       };
        }




        /// <summary>
        /// Executes the project and returns the console output as a string.
        /// </summary>
        /// <param name="p">The project to execute.</param>
        /// <returns>
        /// The console output.
        /// </returns>
        /// <remarks>
        /// Any exception that is thrown as part of the execution of the 
        /// <see cref="Project" /> is wrapped in a <see cref="TestBuildException" />.
        /// </remarks>
        public static string ExecuteProject(Project p)
        {
            using (ConsoleCapture c = new ConsoleCapture())
            {
                string output = null;
                try
                {
                    p.Execute();
                }
                catch (Exception e)
                {
                    output = c.Close();
                    throw new TestBuildException("Error Executing Project", output, e);
                }
                finally
                {
                    if (output == null)
                    {
                        output = c.Close();
                    }
                }
                return output;
            }
        }

        /// <summary>
        /// Creates a new <see cref="Project" /> with output level <see cref="Level.Info" />.
        /// </summary>
        /// <param name="xml">The XML of the build file</param>
        /// <returns>
        /// A new <see cref="Project" /> with output level <see cref="Level.Info" />.
        /// </returns>
        public Project CreateFilebasedProject(string xml)
        {
            return CreateFilebasedProject(xml, Level.Info);
        }

        /// <summary>
        /// Creates a new <see cref="Project" /> with the given output level.
        /// </summary>
        /// <param name="xml">The XML of the build file</param>
        /// <param name="level">The build output level.</param>
        /// <returns>
        /// A new <see cref="Project" /> with the specified output level.
        /// </returns>
        public Project CreateFilebasedProject(string xml, Level level)
        {
            // create the build file in the temp folder
            string buildFileName = Path.Combine(TempDirName, "test.build");
            TempFile.CreateWithContents(xml, buildFileName);

            return new Project(buildFileName, level, 0);
        }

        /// <summary>
        /// Creates an empty project <see cref="XmlDocument" /> and loads it 
        /// with a new project.
        /// </summary>
        /// <returns>
        /// The new <see cref="Project" />.
        /// </returns>
        protected Project CreateEmptyProject()
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            doc.AppendChild(doc.CreateElement("project"));
            return new Project(doc, Level.Info, 0);
        }

        /// <summary>
        /// Creates a temporary file in the temporary directory of the test.
        /// </summary>
        /// <param name="name">The filename, should not be absolute.</param>
        /// <returns>
        /// The full path to the temporary file.
        /// </returns>
        /// <remarks>
        /// The file is created and existance is checked.
        /// </remarks>
        public string CreateTempFile(string name)
        {
            return CreateTempFile(name, null);
        }
        /// <summary>
        /// Creates a tempfile in the test temp directory.
        /// </summary>
        /// <param name="name">The filename, should not be absolute.</param>
        /// <param name="contents">The content of the file.</param>
        /// <returns>
        /// The full path to the new file.
        /// </returns>
        /// <remarks>
        /// The file is created and existance is checked.
        /// </remarks>
        public string CreateTempFile(string name, string contents)
        {
            string filename = Path.Combine(TempDirName, name);

            if (Path.IsPathRooted(name))
            {
                filename = name;
            }

            if (contents == null)
            {
                return TempFile.Create(filename);
            }

            return TempFile.CreateWithContents(contents, filename);
        }

        /// <summary>
        /// Creates a temporary directory.
        /// </summary>
        /// <param name="name">The name of the directory to create (name only, no path info).</param>
        /// <returns>
        /// The full path to the temp directory.
        /// </returns>
        /// <remarks>
        /// The directory is created and existance is checked.
        /// </remarks>
        public string CreateTempDir(string name)
        {
            return TempDir.Create(Path.Combine(TempDirName, name));
        }



        #endregion Public Instance Methods

        /// <summary>
        /// Возвращает директорию проекта <paramref name="projectName"/>.
        /// </summary>
        /// <param name="filePath">
        /// Подкаталог директории, выдаваемой в качестве результата данной функции, любой степени вложенности. 
        /// Например, если filePath = c:\sbor\tools\bin\debug, и projectName = tools, то результат будет c:\sbor\tools
        /// </param>
        /// <param name="projectName"></param>
        /// <returns></returns>
        private DirectoryInfo GetFileProjectDir(string filePath, string projectName)
        {
            StringBuilder builder = new StringBuilder();
            var strings = filePath.Split('\\');
            foreach (var s in strings)
            {
                builder.Append(s);
                if (projectName == s)
                    break;
                builder.Append('\\');
            }

            var dir = builder.ToString();

            if (string.IsNullOrEmpty(dir))
                throw new Exception("Для файла не найден проект");

            return new DirectoryInfo(builder.ToString());
        }



        #region Protected Instance Methods



        /// <remarks>
        ///   <para>No need to add SetUp attribute to overriden method.</para>
        ///   <para>Super classes that override SetUp must call the base class first.</para>
        /// </remarks>
        protected virtual void SetUp()
        {
            _tempDirName = TempDir.Create(this.GetType().FullName);
            // путь до солюшена
            var directoryInfo = Directory.GetCurrentDirectory();
             solutionPath = GetFileProjectDir(directoryInfo, "Tools.MigrationHelper.Tests").Parent.FullName;
        }

        /// <summary>
        /// This method will be called by NUnit for setup.
        /// </summary>
        [SetUp]
        protected void NUnitSetUp()
        {
            SetUp();
        }

        /// <remarks>
        /// Classes that derive from <see cref="BuildTestBase" /> and override 
        /// <see cref="TearDown" /> must call this method on the base class last.
        /// </remarks>
        [TearDown]
        protected virtual void TearDown()
        {
            TempDir.Delete(TempDirName);
        }

        #endregion Protected Instance Methods


        public struct BuildResult
        {
            public Project Project;
            public string Output;


        }


    }
}
