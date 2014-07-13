using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.Application.Common
{
    /// <summary>
    /// Метод Execute класса, реализующего данный интерфейс будет выполнен после старта приложения (после всех инструкций метода Global.Application_Start)
    /// </summary>
    public interface IAfterAplicationStart
    {
        void Execute();
    }
}
