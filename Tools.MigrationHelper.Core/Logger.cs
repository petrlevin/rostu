using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using NAnt.Core;
using NAnt.Core.Tasks;
using Platform.BusinessLogic.Denormalizer;
using Platform.Common;
using Platform.Unity;

namespace Tools.MigrationHelper.Core
{
    public class Logger : DefaultLogger
    {

        public Logger()
        {
            Console.OutputEncoding = Console.IsOutputRedirected ? Encoding.UTF8 : Encoding.GetEncoding(866);
            Console.SetOut(new ReplaceBuild(Console.Out));
            
        }

        private bool _writeError = false;

        private readonly ErrorTextWriter _errorTextWriter = new ErrorTextWriter();
        private bool _registered;

        public override void MessageLogged(object sender, BuildEventArgs e)
        {
            _writeError = e.MessageLevel == Level.Error && Console.IsOutputRedirected;
            base.MessageLogged(sender, e);
            //if (e.MessageLevel==Level.Error)
            //    Console.Error.Write(e.Message);
        }

        public override void BuildFinished(object sender, BuildEventArgs e)
        {
            _writeError = e.Exception != null && Console.IsOutputRedirected;
            

            base.BuildFinished(sender, e);


        }


        public override void BuildStarted(object sender, BuildEventArgs e)
        {
            if (e.Project.Properties.Contains("connectionstring") &&
                (!String.IsNullOrWhiteSpace(e.Project.Properties["connectionstring"])))
                RegisterInDI(e.Project.Properties["connectionstring"]);
            base.BuildStarted(sender, e);
        }

        public override void TaskFinished(object sender, BuildEventArgs e)
        {
            base.TaskFinished(sender, e);
             if ((sender is PropertyTask ) && (((PropertyTask)sender).PropertyName =="connectionstring"))
                 RegisterInDI(((PropertyTask)sender).Value);
            
        }

        private void RegisterInDI(string connectionString)
        {
            if (_registered)
                return;

            IUnityContainer unityContainer = new UnityContainer();

            Platform.PrimaryEntities.Factoring.DependencyInjection.RegisterIn(unityContainer, true, false, connectionString);
            IoCServices.InitWith(new DependencyResolverBase(unityContainer),false);

            _registered = true;

        }



        public override TextWriter OutputWriter
        {
            get
            {
                if (_writeError)
                    return _errorTextWriter;
                return base.OutputWriter;

            }
            set
            {
                base.OutputWriter = value;
            }
        }




    }


    class ErrorTextWriter : TextWriter
    {
        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }

        public override void WriteLine(string value)
        {
            Console.OutputEncoding = Encoding.GetEncoding(866);
            Console.Error.WriteLine(value.Replace("BUILD", "MIGRATION HELPER"));
            Console.OutputEncoding = Encoding.UTF8;
            
        }

        public override void Flush()
        {

        }
    }

    class ReplaceBuild : TextWriter
    {
        private readonly TextWriter _textWriter;

        public ReplaceBuild(TextWriter textWriter)
        {
            _textWriter = textWriter;
        }

        public override Encoding Encoding
        {
            get { throw new NotImplementedException(); }
        }

        public override void WriteLine(string value)
        {
            _textWriter.WriteLine(value.Replace("BUILD", "MIGRATION HELPER").Replace("Buildfile","Configuration"));

        }

        public override void Flush()
        {

        }
    }



}
