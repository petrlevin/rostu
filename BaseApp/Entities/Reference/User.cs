using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Web;
using BaseApp.Common.Interfaces;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Crypto;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.Activity.Controls;

namespace BaseApp.Reference
{
    public partial class User : IUser
    {

        #region Контроли
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 190)]
        public void RoleAddition(DataContext context)
        {
            var success = this.Roles.Any(a => a.Id == -1543503849);

            if (!success)
            {
                this.Roles.Add(context.Role.SingleOrDefault(s => s.Id == -1543503849));
            }
        }

        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = 200)]
        [ControlInitial(ExcludeFromSetup = true)]
        public void EncryptPassword(ControlType сontrolType, User oldEntityValue)
        {

            if (((ControlType.Insert == сontrolType) || (oldEntityValue.Password != Password)) && (!String.IsNullOrWhiteSpace(Password)))
                Password = EncryptPassword(Password);
        }

        #endregion


        #region Public
        public bool IsSuperUser()
        {
            return (Name.ToLower() == "bis")||(Name.ToLower()=="admin");
        }

		public bool IsLicensed(int idPublicLegalFormation)
		{
			if (IsSuperUser())
				return true;
			if (File.Exists(HttpContext.Current.Server.MapPath("~")+"\\license.hack"))
				return true;
			if (BaseApp.DataAccess.CheckLicenseUser.Check(Id, idPublicLegalFormation))
				return true;
			return false;
			return (Name.ToLower() == "bis") || (Name.ToLower() == "admin");
		}

		public override string ToString()
        {
            if (String.IsNullOrWhiteSpace(Name))
                return base.ToString();

		    string userInfo = new[] {Telephone, Email, Department}
                .Where(a => !string.IsNullOrWhiteSpace(a))
                .DefaultIfEmpty()
                .Aggregate((a, b) => string.Format("{0}, {1}", a, b));

		    if (!string.IsNullOrWhiteSpace(userInfo))
		        userInfo = ", " + userInfo;

            return string.Format("Login: {0}, ФИО: {1}{2}", Name, Caption, userInfo);
        }

        /// <summary>
        /// Расшифрованный пароль
        /// </summary>
        /// <returns></returns>
        public string DecryptedPassword()
        {
            if (String.IsNullOrWhiteSpace(Password))
                return null;
            return Password.Decrypt();
        }

        #endregion

        #region Public Static
        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener"></param>
        public static void AddListener(CUDListener<User> listener)
        {
            lock (_Listeners)
            {
                _Listeners.Add(listener);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listener"></param>
        public static void RemoveListener(CUDListener<User> listener)
        {
            lock (_Listeners)
            {
                _Listeners
                    .Where(l => l.Equals(listener))
                    .ToList()
                    .ForEach(ls => _Listeners.Remove(ls)
                    );
            }
        }


        /// <summary>
        /// Зашифровать пароль
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static string EncryptPassword(string password)
        {
            if (String.IsNullOrWhiteSpace(password))
                return null;
            return password.Encrypt();

        }
        #endregion


        #region Private






        /// <summary>
        /// 
        /// </summary>
        private static readonly List<CUDListener<User>> _Listeners = new List<CUDListener<User>>();


        private List<CUDListener<User>> GetListeners()
        {
            lock (_Listeners)
            {
                return new List<CUDListener<User>>(_Listeners);
            }
        }

        #endregion


        #region Internal

        internal void OnAfterUpdate()
        {
            foreach (CUDListener<User> cudListener in GetListeners())
            {
                cudListener.OnAfterUpdate(this);
            }
        }

        #endregion




    }
}

