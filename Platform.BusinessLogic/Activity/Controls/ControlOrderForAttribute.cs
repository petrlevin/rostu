using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// С помощью этого атрибута можно усчтановить порядок выполнения для общих контролей
    /// </summary>
    [AttributeUsage(AttributeTargets.Class , AllowMultiple = true)]
    public class ControlOrderForAttribute : Attribute, IHasTarget
    {
        /// <summary>
        ///если атрибут применен к классу контрола  - тип сущности для которой устанавливается порядок
        ///если атрибут применен к сущностному классу для кторого выполняется контрол - тип контрола  
        /// </summary>
        public Type Target { get; private set; }
        /// <summary>
        /// Порядок
        /// </summary>
        public int ExecutionOrder { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="executionOrder"></param>
        public ControlOrderForAttribute(Type target, int executionOrder)
        {
            if (target == null) throw new ArgumentNullException("target");
            Target = target;
            ExecutionOrder = executionOrder;
        }
    }
}
