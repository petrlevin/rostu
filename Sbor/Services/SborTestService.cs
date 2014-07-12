using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.AppServices;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Actions;

namespace Sbor.Services
{
	public class SborTestServiceData
	{
		public string Name;
		public int Count;
	}


	/// <summary>
	/// Тестовый прикладной веб-сервис
	/// </summary>
	[AppService]
	public class SborTestService
	{
		/// <summary>
		/// 
		/// </summary>
		/// <remarks>
		/// Пример вызова: SborTestService.getSomeData({Name:'some name', Count:123}, [5,6,7])
		/// </remarks>
		/// <param name="data"></param>
		/// <param name="collection"></param>
		/// <returns></returns>
		public List<Dictionary<string,object>> getSomeData(SborTestServiceData data, List<int> collection)
		{
			return new List<Dictionary<string, object>>()
				{
					new Dictionary<string, object>()
						{
							{data.Name, data.Count}
						},
					new Dictionary<string, object>()
						{
							{"others data", collection.Select(a => a.ToString()).Aggregate((a, b) => string.Format("{0}, {1}", a, b))}
						}
				};
		}

        [ListFormAction(EntityName = "testAllScalarField", Caption = "Мотор !")]
        public ClientActionList doSomething(Dictionary<string, object> fieldValues)
        {
            return new ClientActionList().Add(new Eval("alert('Поехали !')"));
        }

        //пример добавления кнопки в форму списка Пользователи и получения значений из строки
        //[ListFormAction(EntityName = "User", Caption = "Сбросить пароль")]
        //public ClientActionList changeUserPasswordAction(Dictionary<string, object> fieldValues)
        //{
        //    string userName;
        //    int userId;
        //    userName = fieldValues["name"].ToString();
        //    userId = Convert.ToInt32(fieldValues["id"].ToString());
        //    return new ClientActionList().Add(new Eval(string.Format("alert('test change password - {0},{1}')",userName,userId.ToString())));
        //}

	}
}
