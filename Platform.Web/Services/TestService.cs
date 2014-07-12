using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Web;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.EditionsComparision;
using Platform.Client;
using Platform.ClientInteraction;
using Platform.ClientInteraction.Actions;

namespace Platform.Web.Services
{
	public class TestService
	{
		public string InteractiveTest(CommunicationContext context, int arg1, string arg2)
		{
			var form = new
				{
					items = new[]
						{
							new {xtype = "label", name = "", fieldLabel = "", text = "some text"},
							new {xtype = "textfield", name = "fld1", fieldLabel = "Имя", text = ""},
							new {xtype = "textfield", name = "fld2", fieldLabel = "Фамилия", text = ""},
						},
					buttons = new[]
						{
							new
								{
									text = "Button text",
									handler = new ExtFunction {FunctionText = "this.up('form').continueCb('btn1'); this.up('window').close();"}
								},
							new
								{
									text = "Sun is my uncle",
									handler = new ExtFunction {FunctionText = "this.up('form').continueCb('btn1'); this.up('window').close();"}
								},
							new
								{
									text = "Закрыть",
									handler = new ExtFunction {FunctionText = "this.up('window').close();"}
								}
						},
					xtype = "form"
				};


			var action1 = new ClientActionList().Add(new CreateForm(form));

			var answer1 = context.GetAnswer<ClientAnswer>(action1);
			
			/*if (answer1.Price > 10)
			{
				var answer2 = context.GetAnswer<Answer1>("action2", "Вопрос 2");
			}

			var answer3 = context.GetAnswer<Answer2>("Вопрос 3");
			*/

			return "ok";
		}

        public int Sleep(int ms)
        {
            Thread.Sleep(ms);
            return ms;
        }

        public string CompareEditions()
        {
            var comparator = new EditionsComparator()
            {
                EntityId = -1207959525, // PublicInstitutionEstimate	Смета казенного учреждения
                EditionA = 153,
                EditionB = 361
            };

            comparator.Compare();
            return "ok";
        }
	}
}