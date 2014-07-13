using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class ,AllowMultiple = true)]
    public class ControlInitialForAttribute : ControlInitialAttribute, IHasTarget
    {
        private bool? _excludeFromSetup;
        private bool? _initialSkippable;
        private bool? _initialManaged;

        /// <summary>
        ///если атрибут применен к классу контроля  - тип сущности для которой устанавливается начальные значения
        ///если атрибут применен к сущностному классу для которого выполняется контроль - тип контроля
        /// </summary>
        public Type Target { get; private set; }

        /// <summary>
        /// признак генерации элемента справочника контроли
        /// MigrationHelper-ом
        /// если установлена (true)
        /// при выполнении задачи (task) "setcudcontrols" элемент справочника не будет создан
        /// </summary>
        public override bool ExcludeFromSetup
        {
            get { return _excludeFromSetup.HasValue && _excludeFromSetup.Value; }
            set { _excludeFromSetup = value; }
        }


        /// <summary>
        /// признак установки флажка "мягкий" при генерации элемента справочника контроли
        /// MigrationHelper-ом
        /// </summary>
        public override bool InitialSkippable
        {
            get { return _initialSkippable.HasValue && _initialSkippable.Value; }

            set { _initialSkippable = value; }
        }

        /// <summary>
        /// признак установки флажка "управлеямый" при генерации элемента справочника контроли
        /// MigrationHelper-ом
        /// </summary>
        public override bool InitialManaged
        {
            get { return _initialManaged.HasValue && _initialManaged.Value; }
            set { _initialManaged = value; }
        }


        public bool? ActualExcludeFromSetup
        {
            get { return _excludeFromSetup; }
        }

        public bool? ActualInitialSkippable
        {
            get { return _initialSkippable; }
        }

        public bool? ActualInitialManaged
        {
            get { return _initialManaged; }
        }


        public ControlInitialForAttribute(Type target) :
            base()
        {
            Target = target;
        }
    }
}
