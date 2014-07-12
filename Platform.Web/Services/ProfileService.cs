using System;
using System.Collections.Generic;
using System.Globalization;
using System.Security.Cryptography;
using System.Web;
using System.Web.Security;
using BaseApp.DataAccess;
using BaseApp.DbEnums;
using BaseApp.Reference;
using Platform.BusinessLogic.Auditing;
using Platform.BusinessLogic.Auditing.Auditors;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.Web.UserManagement;

namespace Platform.Web.Services
{
	/// <summary>
    /// Сервис авторизации
	/// </summary>
 	public class ProfileService
    {
        #region private
        /// <summary>
        /// Имя куки в которой будем хранить информацию
        /// </summary>
        private const string TicketName = "rostu.auth";

        FormsAuthenticationTicket GetTicket()
        {
            var temp = HttpContext.Current.Request.Cookies.Get(TicketName);
            //FormsAuthentication.GetAuthCookie()
            return temp != null && temp.Value != null
                       ? FormsAuthentication.Decrypt(temp.Value)
                       : null;
        }

        private string GetRequestSessionId()
        {
            var httpCookie = HttpContext.Current.Request.Cookies.Get("SessionID");
            if (httpCookie != null)
                return httpCookie.Value;
            return null;
        }


        /// <summary>
        /// Дополнительные параметры необходимые для входа в систему
        /// </summary>
        /// <returns></returns>
        object Options()
        {
            var data = GetTicket();
            return new
            {
                authenticated = data != null,
                username = data != null ? data.Name : null,
                needChangePassword = Users.Current.ChangePasswordNextTime,
                ticketName = TicketName
            };

        }

        private User GetUser(string userName)
        {
            return Users.ByName(userName);

        }

        private Guid CurrentSessionId()
        {
            return BaseApp.Environment.BaseAppEnvironment.Instance.SessionStorage.Id;
        }

        private void _setSysDimensions(Dictionary<string, int> inData)
        {
            BaseApp.Environment.BaseAppEnvironment.Instance.SessionStorage.CurentDimensions.PublicLegalFormation =
                SysDimensions.PublicLegalFormationById(inData[SysDimension.PublicLegalFormation.ToString()]);

            BaseApp.Environment.BaseAppEnvironment.Instance.SessionStorage.CurentDimensions.Version =
                SysDimensions.VersionById(inData[SysDimension.Version.ToString()]);

            BaseApp.Environment.BaseAppEnvironment.Instance.SessionStorage.CurentDimensions.Budget =
                SysDimensions.BudgetById(inData[SysDimension.Budget.ToString()]);
        }

        private object DoLogin()
        {
            var user = Users.Current;
            if (user == null)
                throw new NotLogedException();

            Audit<LoginAuditor>.Do(new LoginAuditor(HttpContext.Current.Session["SessionId"].ToString()));

            var encTicket = FormsAuthentication.Encrypt(
                new FormsAuthenticationTicket(1, user.Name, DateTime.Now, DateTime.Now.AddDays(1), true, user.Name,
                                              FormsAuthentication.FormsCookiePath)
                );

            HttpContext.Current.Request.Cookies.Remove(TicketName);
            HttpContext.Current.Request.Cookies.Remove("SessionID");

            var authCookie = new HttpCookie(TicketName, encTicket);
            authCookie.Path = FormsAuthentication.FormsCookiePath;
            HttpContext.Current.Response.Cookies.Add(authCookie);

            var sessionCookie = new HttpCookie("SessionID", CurrentSessionId().ToString());
            sessionCookie.Path = FormsAuthentication.FormsCookiePath;

            HttpContext.Current.Response.Cookies.Add(sessionCookie);
            return Options();
        }

        #endregion
        

        #region  Public
        public static event Action OnLogout;

        /// <summary>
        /// Метод возвращает информацию, является ли текущий пользователь супер-пользователем.
        /// </summary>
        /// <returns></returns>
        public bool IsSuperUser()
        {
            return Users.Current.IsSuperUser();
        }

        /// <summary>
        /// Метод возвращает информацию, авторизован пользователь в систему или нет
        /// </summary>
        /// <returns>logged - признак авторизованности, authCookieName - имя авторизационной cookie</returns>
        public object IsLogged()
        {
            var result = new
                {
                    logged = false,
                    authCookieName = TicketName
                };
            if (GetRequestSessionId() != CurrentSessionId().ToString())
                return result;

            var ticket = GetTicket();
            if (ticket == null)
                return result;
            var user = GetUser(ticket.Name);
            if (user == null)
                return result;
            Users.Current = user;
            return new
                {
                    logged = true,
                    authCookieName = TicketName
                };
        }


        /// <summary>
        /// Функция входа в систему
        /// </summary>
        /// <param name="userName">Имя пользователя</param>
        /// <param name="pass">Пароль пользователя</param>
        /// <returns>Дополнительные параметры необходимые для входа в систему (bool authenticated, string username, bool needChangePassword, string ticketName)</returns>
        public virtual object Login(string userName, string pass)
        {
            var user = GetUser(userName);
            if (user == null)
                throw new SystemUFException("Не верно указан пользователь");
            if (user.IsBlocked)
                throw new SystemUFException("Пользователь заблокирован");
            if (user.DecryptedPassword() != pass)
                throw new SystemUFException("Неверно указан пароль");

            Users.Current = user;
            return DoLogin();
        }

	    /// <summary>
	    /// Выполнить вход, используя сохраненное состояние сессии
	    /// </summary>
	    /// <param name="authTicketValue">Сохраненное значение авторизационной cookie</param>
	    /// <exception cref="NotLogedException">Выбрасывается при проблемах в авторизации, связанных с получением пользователя из сохраненного значения авторизационной cookie </exception>
	    /// <returns>Дополнительные параметры необходимые для входа в систему (bool authenticated, string username, bool needChangePassword, string ticketName)</returns>
	    public virtual object AutoLogin(string authTicketValue)
	    {
	        FormsAuthenticationTicket ticket = null;
	      
            try
	        {
                ticket = FormsAuthentication.Decrypt(authTicketValue);
	        }
	        catch (CryptographicException)
	        {
                throw new NotLogedException();
	        }

	        if (ticket == null)
                throw new NotLogedException();

            var user = GetUser(ticket.Name);
            if (user == null || user.IsBlocked)
                throw new NotLogedException();

            Users.Current = user;
	        return DoLogin();
	    }

	    
	    /// <summary>
        /// Функция выхода из системы
        /// </summary>
        public void Logout()
        {
            if (GetTicket() != null)
            {
                HttpContext.Current.Request.Cookies.Remove(TicketName);
            }
            if (OnLogout != null)
                OnLogout();
        }

	    /// <summary>
	    /// Сменить пароль авторизованного (текущего) пользователя
	    /// </summary>
	    /// <param name="oldPassword">Текущий пароль </param>
	    /// <param name="newPassword">Новый пароль</param>
	    /// <exception cref="SystemUFException"></exception>
	    public void ChangePassword(string oldPassword, string newPassword)
        {
            if (String.IsNullOrWhiteSpace(newPassword))
                throw new SystemUFException("Пароль не может быть пустым.");
            var curentUser = Users.Current;
            if (curentUser == null)
                throw new SystemUFException("Текущий пользователь не определен. Вероятно вы не залогинились.");
            if (curentUser.DecryptedPassword() != oldPassword)
                throw new SystemUFException("Старый пароль введен неправильно.");
            curentUser.Password = User.EncryptPassword(newPassword);
            curentUser.ChangePasswordNextTime = false;
            Users.UpdateCurrent();
        }

        /// <summary>
        /// Задать системные измерения для текущего пользователя
        /// </summary>
        /// <param name="inData">Значения системных измерений (PublicLegalFormation, Version, Budget)</param>
        public virtual void SetSysDimensions(Dictionary<string, int> inData)
        {
            foreach (var data in inData)
            {
                var cookie = new HttpCookie(data.Key, data.Value.ToString()){Path = FormsAuthentication.FormsCookiePath};
                HttpContext.Current.Response.Cookies.Add(cookie);
            }

            _setSysDimensions(inData);
        }

        /// <summary>
        /// Задать характеристики сетевого подключения пользователя. 
        /// Диагностический метод, в продакшене желательно отключить.
        /// </summary>
        /// <param name="inData">Характеристики (ping, downloadSpeed)</param>
	    public void SetUserBandWidth(Dictionary<string, object> inData)
	    {
	        int ping;
            if (!inData.ContainsKey("ping")) 
                ping = 0;
            else
                Int32.TryParse(inData["ping"].ToString(), out ping);

	        decimal downloadSpeed;
            if (!inData.ContainsKey("downloadSpeed")) 
                downloadSpeed = 0;
            else
                Decimal.TryParse(inData["downloadSpeed"].ToString(), NumberStyles.Number, CultureInfo.InvariantCulture, out downloadSpeed);

            Users.SetCurrentBandWidth( ping, downloadSpeed );
	    }

	    #endregion
	}
}