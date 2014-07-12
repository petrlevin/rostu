using System;
using NAnt.Core;

namespace Tools.MigrationHelper.Core.Tasks
{
    public abstract class MhTask : Task
    {
        protected virtual void ErrorFormat(string format, params object[] args)
        {
            if (FailOnError)
                throw new BuildException(String.Format(format, args));
            else
                Log(Level.Error, format, args);
        }

        protected virtual void Error(string message, Exception inner)
        {
            if (FailOnError)
                throw new BuildException(message,inner);
            else 
                Log(Level.Error, "{0}.{1}" ,message,inner.Message);
            
        }

        protected virtual void Error(string message)
        {
            if (FailOnError)
                throw new BuildException(message);
            else
                Log(Level.Error, message);


        }



        protected T FatalFormat<T>(string format, Exception inner, params object[] args)
        {
            FatalFormat(format, inner, args);
            return default(T);
        }

        protected void FatalFormat(string format, Exception inner, params object[] args)
        {
            Fatal(String.Format(format, args), inner);
        }

        protected void FatalFormat(string format, params object[] args)
        {

            FailOnError = true;

            throw new BuildException(String.Format(format, args));
        }

        protected void Fatal(string message)
        {

            FailOnError = true;

            throw new BuildException(message + "\r\n");
        }

        protected void Fatal(string message, Exception inner)
        {

            FailOnError = true;

            if (!Verbose)
                throw new BuildException(message, inner);
            else
            {
                message = message + Environment.NewLine + inner.Message + Environment.NewLine + inner.StackTrace;
                throw new BuildException(message);
            }
        }

        protected T Fatal<T>(string message, Exception inner)
        {
            Fatal(message, inner);
            return default(T);
        }

        protected override void Initialize()
        {
            if (!RuntimePolicyHelper.LegacyV2RuntimeEnabledSuccessfully)
                throw new BuildException(
                    "Для корректной работы необходимо изменить Nant.exe.config \" <startup useLegacyV2RuntimeActivationPolicy=\"true\">\" ");
            base.Initialize();
        }



    }




}
