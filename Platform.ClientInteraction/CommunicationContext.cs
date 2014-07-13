using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Platform.ClientInteraction
{
    /// <summary>
    /// Контекст взаимодействия с пользователем.
    /// 
    /// Для того, чтобы некоторый метод веб-сервиса мог 
    /// <a href="http://conf.rostu-comp.ru/pages/viewpage.action?pageId=13599286">взаимодействовать с пользователем</a> 
    /// (причем не обязательно непосредственно сам метод, имеется в виду, что также и любой код, вызываемый внутри данного метода) 
    /// нужно, чтобы 
    /// 1. данный метод первым аргументом принимал объект класса <see cref="CommunicationContext"/>.
    /// 2. та часть кода метода веб-сервиса, внутри которой может происходить взаимодействие, была обернута в конструкцию:
    /// <code>
    /// using (new CommunicationContextScope(communicationContext))
    /// {
    ///    ...
    /// }
    /// </code>
    /// 
    /// Объект данного класса 
    /// содержит словарь ответов пользователя <see cref="Answers"/>, а также методы, назначение которых - получать ответ.
    /// Словарь <see cref="Answers"/> сделан публичным только для того, чтобы он мог быть заполнен компонентом ExtDirectHandler 
    /// при вызове метода с клиента.
    /// Для получения ответа не следует использовать словарь <see cref="Answers"/> напрямую, а следует использовать 
    /// один из предназначенных для этого методов.
    /// Каждый из них устроен т.о., что если ответа не содержится в <see cref="Answers">словаре</see>, 
    /// то вызывается <see cref="InteractiveException">интерактивное исключение</see>.
    /// 
    /// См. также:
    /// <seealso cref="Platform.ClientInteraction.Scopes.CommunicationContextScope"/>.
    /// <seealso cref="InteractiveException"/>.
    /// </summary>
	public class CommunicationContext
	{
		public CommunicationContext()
		{
			Answers = new Dictionary<string, object>();
		}

		/// <summary>
		/// Ответы пользователя.
		/// Ключ - идентификатор действия. Значение - полученный от пользователя ответ.
		/// </summary>
		public Dictionary<string, object> Answers { get; set; }

        /// <summary>
        /// Получить ответ от клиента.
        /// </summary>
        /// <param name="clientActions"></param>
        /// <returns></returns>
        public object GetAnswer(ClientActionList clientActions)
        {
            return GetAnswer<Object>(clientActions);
        }

		/// <summary>
		/// Получить ответ от клиента
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="clientActions"></param>
		/// <returns></returns>
		public T GetAnswer<T>(ClientActionList clientActions)
		{
			return GetAnswer<T>(typeof(T).Name, clientActions);
		}

	    /// <summary>
	    /// Обращение к клиенту с явным указанием идентификатора действия. 
	    /// Удобно использовать когда в одном методе есть два (или несколько) обращения к клиенту с одинаковым типом ответа. 
	    /// </summary>
	    /// <typeparam name="T"></typeparam>
	    /// <param name="interactionId"></param>
	    /// <param name="clientActions"></param>
	    /// <returns></returns>
	    public T GetAnswer<T>(string interactionId, ClientActionList clientActions)
		{
			if (Answers.ContainsKey(interactionId))
			{
			    var o = Answers[interactionId] as JObject;
			    if (o != null)
				{
                    JObject jo = o;
					return jo.ToObject<T>();
				}
				return default(T);
			}
			throw new InteractiveException(interactionId, clientActions);
		}

		public string GetAnswer(string interactionId, ClientActionList clientActions)
		{
			if (Answers.ContainsKey(interactionId))
			{
				return Answers[interactionId].ToString();
			}
			throw new InteractiveException(interactionId, clientActions);
		}

	}
}
