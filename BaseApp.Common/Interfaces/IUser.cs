using System;

namespace BaseApp.Common.Interfaces
{
    /// <summary>
    /// Пользователь
    /// </summary>
    public interface IUser 
    {
        /// <summary>
        /// Идентификатор элемента
        /// </summary>
		Int32 Id { get; set; }

        /// <summary>
        /// Системное наименование
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Наименование
        /// </summary>
        string Caption { get; set; }


        /// <summary>
        /// является ли пользователь суперюзером
        /// </summary>
        /// <returns></returns>
        bool IsSuperUser();

	    /// <summary>
	    /// Лицензированн ли пользователь в укзанном ППО
	    /// </summary>
	    /// <returns></returns>
		bool IsLicensed(int idPublicLegalFormation);

        /// <summary>
        /// Зашифрованный пароль
        /// </summary>
        string Password { get; set; }


        /// <summary>
        /// Сменить пароль при следующем входе в систему
        /// </summary>
        bool ChangePasswordNextTime{get; set;}


        /// <summary>
        /// Заблокирован
        /// </summary>
        bool IsBlocked { get; set; }


        /// <summary>
        /// Рашифрованный пароль
        /// </summary>
        /// <returns></returns>
        string DecryptedPassword();

    }

}
