using System;
using System.Diagnostics;
using System.ServiceModel;
using System.ServiceProcess;
using BgTasks.Wcf;

namespace BgTasks.WinService
{
    public partial class SborService : ServiceBase
    {
        private const string SborServiceSource = "SborServiceSource";
        private const string SborServiceLog = "Sbor Service Log";

        private ServiceHost serviceHost;

        public SborService()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
            }

            serviceHost = new ServiceHost(typeof(BackgroundTask));
            try
            {
                serviceHost.Open();
            }
            catch (Exception ex)
            {
                AddLog("start failed: " + ex.Message);
                throw;
            }
            
            AddLog("start successfull");
        }

        protected override void OnStop()
        {
            if (serviceHost != null)
            {
                serviceHost.Close();
                serviceHost = null;
            }
            AddLog("stop");
        }

        private void AddLog(string log)
        {
            try
            {
                if (!EventLog.SourceExists(SborServiceSource))
                {
                    EventLog.CreateEventSource(SborServiceSource, SborServiceLog);
                }
                eventLog1.Source = SborServiceSource;
                eventLog1.WriteEntry(log);
            }
            catch { }
        }
    }
}
