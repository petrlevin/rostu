using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;
using Platform.ClientInteraction.Actions;

namespace Platform.ClientInteraction
{
    /// <summary>
    /// Список <see cref="ClientActionBase">клиентских действий</see>.
    /// Метод <see cref="Add"/> перекрыт для удобства, в дополнении к стандартному методу добавления элемента в список, он возвращает сам список.
    /// Т.о. можно составлять цепочки вида: list.Add(action1).Add(action2)...
    /// </summary>
    public class ClientActionList : List<ClientActionBase>
    {
        public new ClientActionList Add(ClientActionBase action)
        {
            base.Add(action);
            return this;
        }

        public ClientActionList()
        {

        }

        public ClientActionList(ClientActionBase clientAction)
        {
            Add(clientAction);
        }
    }
}
