using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Platform.BusinessLogic.Activity.Controls;
using Platform.PrimaryEntities;

namespace Platform.BusinessLogic.Tests
{
	[ExcludeFromCodeCoverage]
	public class ControlInvokerTests
    {



        private String MyNameProp { get; set; }
        
        [Test]
        [TestCase("Jane")]
        public void Test(string stringArgument)
        {
            var ci = new ControlInvoker<Some>(new Some(),new ControlLauncher(new ControlDispatcherBase(), false));
            
            var b = new Some();            
            ci.InvokeControl(s=>s.Do(9,"Peter" , b ,MyNameProp,MyNameProp ,stringArgument));
        }

        public class Some:BaseEntity
        {
            public void Do(int parameter,string name ,Some some ,string s1, string s2 , string s3)
            { }
        }


        [Test]
        [Ignore]
        public void TimeMeasure()
        {
            Some b = new Some();
            var watch = Stopwatch.StartNew();
            const int LOOP = 50000;
            var ci = new ControlInvoker<Some>(new Some(), new ControlLauncher(new ControlDispatcherBase(), false),false);
            for (int i = 0; i < LOOP; i++)
            {
                ci.InvokeControl(s=>s.Do(9,"Peter" , b ,MyNameProp,MyNameProp ,"hghhghg"));
            }
            watch.Stop();
            Debug.Print("When transformed: {0}ms", watch.ElapsedMilliseconds);
            watch = Stopwatch.StartNew();
            ci = new ControlInvoker<Some>(new Some(), new ControlLauncher(new ControlDispatcherBase(), false), true);
            for (int i = 0; i < LOOP; i++)
            {
                ci.InvokeControl(s => s.Do(9, "Peter", b, MyNameProp, MyNameProp, "hghhghg"));
            }
            watch.Stop();
            Debug.Print("Alwaws compile: {0}ms", watch.ElapsedMilliseconds);
        }
        

    }

}
