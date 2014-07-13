using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.OpenXMLProcessing.MSWordHelpers;

namespace Platform.BusinessLogic.Tests.MSWordProcessingTests
{
    [TestFixture]
    class SimpleFieldTests
    {
        private static readonly char[] SplitChar = new char[] { ' ' };

        private string getName(string param)
        {
            var reg = new Regex(@"[^\\\\]""(.*)""");

            if (param.IndexOf('"') < 0)
                return param.Split(SplitChar, StringSplitOptions.RemoveEmptyEntries)[1];
            else
            {
                return reg.Match(param).Groups[1].Value;
                
                //return param.Substring(first + 1, last - first - 1);
            }
        }

        [Test]
        public void TestGetFieldNames()
        {
            var _params = new[]{@" MERGEFIELD  ""Param2[par1=3][par2=новый текст]"" \m ",
                             @" MERGEFIELD Param3[par1=-1879048157] ",
                             @"MERGEFIELD Param1 \s"};

            var param = @" MERGEFIELD  ""Param2[par1=3][par2=новый текст]"" \m ";

            var t = getName(param);
           
            Assert.AreEqual("Param2[par1=3][par2=новый текст]", t);

            param = @" MERGEFIELD  Param1 \m ";

            t = getName(param);
             
            Assert.AreEqual("Param1", t);

            param = @" MERGEFIELD  ""Param2[par1=3][par2=\\""новый текст\\""]"" \m  ";

            t = getName(param);
            Assert.AreEqual(@"Param2[par1=3][par2=\\""новый текст\\""]", t);
        }
    }
}
