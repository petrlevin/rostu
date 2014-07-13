using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic.Common.Enums;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// Атрибут для создания контроля, вызываемого по событию БД.
    /// Применяется к методу прикладного сущностного класса. 
    /// Либо, для создания общего контроля, данный атрибут следует применять к классу, реализующему интерфейс <see cref="Platform.BusinessLogic.Common.Interfaces.ICommonControl"/>.
    /// Если <see cref="ControlType"/> не задан, то контроль считается свободным (Free), т.е. вызывающимся вручную из прикладного кода операций.
    /// 
    /// Сам метод контроля может иметь параметры:
    /// 1) типа ControlType
    /// 2) типа Sequence  
    /// 3) типа DataContext (контекст Entity Framework)
    /// 4) типа сущностого класса для передачи старого значения объекта сущностного класса 
    /// (имеет смысл только для контрола на обновление 'Update').
    /// Порядок и наименование параметров произвольны.
    /// <example>
    /// Примеры определения контролей в сущностном классе:
    /// <code>
    /// public class  SomeEntity
    /// 
    ///     [Control(ControlType.Update, Sequence.After)]
    ///     public void SimpleValidation()
    ///     {
    ///     }
    /// 
    ///     [Control(ControlType.Any, Sequence.After)]
    ///     public void CheckSomething(DataContext dbContext, ControlType controlType, Sequence sequence)
    ///     {
    ///     }
    /// 
    ///     [Control(ControlType.Insert|ControlType.Delete, Sequence.After|Sequence.Before)]
    ///     public void CheckOther(Sequence seq ,ControlType controlType, DataContext context)
    ///     {
    ///     }
    /// 
    ///     [Control(ControlType.Any,Sequence.Any)]
    ///     public void VeryFunnyControl(ControlType ct, DataContext context,SomeEntity oldEntityValue)
    ///     {
    ///     }
    /// }
    /// </code>
    /// </example>
    /// </summary>
    [AttributeUsage(AttributeTargets.Method|AttributeTargets.Class)]
    public class ControlAttribute : Attribute
    {
        /// <summary>
        /// Событие, на котором должен сработать контроль (вставка, обновление, удаление).
        /// Может быть использвана операция побитового или ('|'), если контроль должен выполнятся для нескольких событий. 
        /// Например, ControlType.Insert|ControlType.Delete - контроль выполнится при вставке и при удалении.
        /// </summary>
        public ControlType ControlType { get; set; }

        /// <summary>
        /// Порядок (до или после срабатывания события).
        /// </summary>
        public Sequence Sequence { get; set; }

        /// <summary>
        /// Порядок исполнения контроля относительно других контролей. 
        /// Если данный параметр не указан (или значение 0), то контроль будет выполнятся последним.
        /// </summary>
        public int ExecutionOrder { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="controlType">тип контрола (вставка , обновление , удаление)</param>
        /// <param name="sequence">порядок (до или после вставки обновления удаления)</param>
        /// <param name="executionOrder">порядок исполнения контрола</param>
        public ControlAttribute(ControlType controlType, Sequence sequence, int executionOrder = 1000000000)
        {
            ControlType = controlType;
            Sequence = sequence;
            ExecutionOrder = executionOrder;
        }

    }
}
