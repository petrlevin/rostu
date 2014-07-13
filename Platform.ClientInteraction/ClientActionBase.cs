using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.ClientInteraction
{
    /// <summary>
    /// Базовый класс для описания некоторого действия на клиентской стороне.
    /// Класс наследник должен предоставить <see cref="ClientHandler"/>, соответствующий клиентскому классу из пространства имен App.clientactions.*.
    /// Клиентский класс в качестве конфигурационного объекта примет объект действия.
    /// См. метод interactWithUser клиентского класса App.direct.ControlingProvider.
    /// 
    /// Возможные варианты реализации:
    ///  OpenForm
	///  CreateForm
	///  CloseWindows
	///  SetFieldValues
	///  RefreshFields
	///  Eval
    ///  CloseCurrentWindow
    /// </summary>
    public class ClientActionBase
    {
        private const string _generic = "Generic";
        public string ClientHandler { get; set; }

        public ClientActionBase()
        {
            ClientHandler = "Generic";
        }
    }
}
