using System;
using NAnt.Core;


namespace Tools.MigrationHelper
{
	static class Program
	{
	    /// <summary>
	    /// The main entry point for the application.
	    /// </summary>
	    [STAThread]
	    private static int Main(string[] args)
	    {
	        var path = AppDomain.CurrentDomain.BaseDirectory;
	        var nantArgs = new string[args.Length + 2];
	        nantArgs[0] = "-ext:"+path+"Tools.MigrationHelper.Core.dll";
            nantArgs[1] = "-logger:Tools.MigrationHelper.Core.Logger";
            args.CopyTo(nantArgs,2);

	        try
	        {
                ConsoleDriver.Main(nantArgs);
	        }
	        catch (Exception)
	        {
                return 1; 
	        }
            return 0;
		}


	}
}
