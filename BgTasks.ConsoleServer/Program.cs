using System;
using System.ServiceModel;
using BgTasks.Wcf;

namespace BgTasks.ConsoleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var myServiceHost = new ServiceHost(typeof(BackgroundTask));
            myServiceHost.Open();
            
            Console.WriteLine("Сервис запущен. Для завершения нажмите Enter.");
            Console.Read();
            
            myServiceHost.Close();
            myServiceHost = null;
        }
    }
}
