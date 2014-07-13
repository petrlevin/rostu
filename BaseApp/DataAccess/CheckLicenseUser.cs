using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using BaseApp.Reference;
using Platform.Common;
using Platform.DbCmd;
using Platform.PrimaryEntities.DbEnums;

namespace BaseApp.DataAccess
{
	/// <summary>
	/// Проверятор пользовательских лицензий
	/// </summary>
	public static class CheckLicenseUser
	{
		/// <summary>
		/// Соединение с БД
		/// </summary>
		private static readonly SqlConnection Connection = IoC.Resolve<SqlConnection>("DbConnection");

		/// <summary>
		/// Проверить лицензию пользователя в указанном ППО
		/// </summary>
		/// <param name="idUser"></param>
		/// <param name="idPublicLegalFormation"></param>
		/// <returns></returns>
		public static bool Check(int idUser, int idPublicLegalFormation)
		{
			SqlCmd sqlCmd = new SqlCmd(Connection);
			List<License> listLicense = sqlCmd.Select<License>(
				"select b.* from [ml].[License_User] [a] inner join [ref].[License] [b] on [b].[id]=[a].[idLicense] inner join [ref].[PublicLegalFormation] [c] on [c].[Caption]=[b].[PublicLegalFormation] where [a].[idUser]={0} AND [c].[id]={1} AND [b].[idRefStatus]={2} and [b].[EndDate]>={3}",
				idUser, idPublicLegalFormation, (byte) RefStatus.Work, DateTime.Now.Date);
			if (listLicense.Any(a => a.EndDate.HasValue && a.EndDate.Value >= DateTime.Now.Date))
				return true;
			
			return false;
		}
	}
}
