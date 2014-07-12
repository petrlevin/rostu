using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAnt.Core;
using NUnit.Framework;

namespace Tools.MigrationHelper.Core.Tests.Tasks
{
    public class CheckTests : BuildTestBase
    {
        // nant -ext:Tools.MigrationHelper.Core.dll -logger:Tools.MigrationHelper.Core.Logger
        [Test]
        public void Test()
        {
            string _xml = @"
                    <project>
                        
                        <loadtasks assembly=""${nant::get-base-directory()}\Tools.MigrationHelper.Core.dll"" />
                        <property name=""connectionstring"" value=""" + connectionString + @""" />
                        <registerindi connectionstring =""${connectionstring}""></registerindi> 
                        <tofs connectionstring =""${connectionstring}"" devid=""7"" sourcepath=""C:\VSProjects\3.0""/>
                    </project>";
            var output = RunBuild(_xml, Level.Verbose);
        }

        //<loadtasks assembly=""Tools.MigrationHelper.Core.dll"" />
        //<property name=""connectionString"" value=""" + connectionString+ @""" />
        //<check connectionString =""${connectionString}"" />



    }
}
