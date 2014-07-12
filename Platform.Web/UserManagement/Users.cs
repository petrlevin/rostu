using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Web;
using BaseApp;
using BaseApp.Common.Interfaces;
using Platform.Common;
using Platform.Common.Exceptions;
using Platform.BusinessLogic;
using Platform.Web.Services;
using Sbor.Reference;
using DataContext = BaseApp.DataContext;


namespace Platform.Web.UserManagement
{
    /// <summary>
    /// 
    /// </summary>
    public static class Users
    {

        static public IUser Current
        {
            get
            {
                lock (_lock)
                {
                    return BaseApp.Environment.BaseAppEnvironment.Instance.SessionStorage.CurrentUser;
                }

            }
            set
            {
                lock (_lock)
                {
                    BaseApp.Environment.BaseAppEnvironment.Instance.SessionStorage.CurrentUser = value;
                }

            }
        }

        public static object _lock = new Object();


        public static void UpdateCurrent()
        {
            var dataContext = GetDataContext();
            var user = Current as BaseApp.Reference.User;
            if (user == null)
                throw new PlatformException("Зарегистрированный пользователь не является сохраняемым. Изменения не возможны.");
            dataContext.User.Attach(user);
            dataContext.Entry(user).State = EntityState.Modified;
            dataContext.SaveChanges();
        }

        public static void SetCurrentBandWidth(int ping, decimal downloadSpeed)
        {
            var dataContext = GetSborDataContext();
            var user = Current as BaseApp.Reference.User;
            if (user == null)
                throw new PlatformException("Зарегистрированный пользователь не является сохраняемым. Изменения не возможны.");

            var userBandWidth = dataContext.UserBandWidth.Create();
            userBandWidth.IdUser = user.Id;
            userBandWidth.Ping = ping;
            userBandWidth.DownloadSpeed = downloadSpeed;
            userBandWidth.Date = DateTime.Now;

            dataContext.Entry(userBandWidth).State = EntityState.Added;
            dataContext.SaveChanges();
        }

        private static DataContext GetDataContext()
        {
            return IoC.Resolve<DbContext>().Cast<DataContext>();
        }

        private static Sbor.DataContext GetSborDataContext()
        {
            return IoC.Resolve<DbContext>().Cast<Sbor.DataContext>();
        }


        public static BaseApp.Reference.User ByName(string userName)
        {

            var dataContext = GetDataContext();
            return dataContext.User.SingleOrDefault(u => u.Name == userName);

        }

        public static UserBandWidth GetUserBandWidth(int userId)
        {
            var dataContext = GetSborDataContext();
            return dataContext.UserBandWidth.Where(u => u.IdUser == userId).OrderByDescending(u => u.Date).FirstOrDefault();
        }

        public static bool SetCurrentDimensions()
        {
            var ppo = HttpContext.Current.Request.Cookies.Get("PublicLegalFormation");
            int ppoId;
            if (ppo == null || String.IsNullOrEmpty(ppo.Value) || !Int32.TryParse(ppo.Value, out ppoId))
                return false;

            var budget = HttpContext.Current.Request.Cookies.Get("Budget");
            int budgetId;
            if (budget == null || String.IsNullOrEmpty(budget.Value) || !Int32.TryParse(budget.Value, out budgetId))
                return false;

            var version = HttpContext.Current.Request.Cookies.Get("Version");
            int versionId;
            if (version == null || String.IsNullOrEmpty(version.Value) || !Int32.TryParse(version.Value, out versionId))
                return false;
            try
            {
                (new ProfileService()).SetSysDimensions(new Dictionary<string, int>()
                    {
                        {"PublicLegalFormation", ppoId},
                        {"Budget", budgetId},
                        {"Version", versionId},

                    });
            }
            catch
            {
                return false;
            }

            return true;
        }

    }
}
