using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.DbEnums;

namespace Platform.PrimaryEntities.Reference
{
	/// <summary>
	/// Класс описывающий хранение программируемых объектов MS SQL
	/// </summary>
    public class Programmability : Metadata, IIdentitied
	{
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Наименование схемы
		/// </summary>
		public string Schema { get; set; }

		/// <summary>
		/// Наименование объекта
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Команда создания
		/// </summary>
		public string CreateCommand { get; set; }

		/// <summary>
		/// Идентифкатор типа объекта
		/// </summary>
		public byte IdProgrammabilityType { get; set; }

        /// <summary>
        /// Порядок выполнения
        /// </summary>
        public string Order { get; set; }

        /// <summary>
        /// Отключен
        /// </summary>
        public bool IsDisabled { get; set; }

        /// <summary>
        /// Проект
        /// </summary>
        public int IdProject { get; set; }

        /// <summary>
        /// Идентифкатор проекта
        /// </summary>
        public virtual RefStatus Project
        {
            get { return (RefStatus)this.IdProject; }
            set { this.IdProject = (byte)value; }
        }

	    /// <summary>
		/// Тип объекта
		/// </summary>
        public virtual ProgrammabilityType ProgrammabilityType
		{
			get { return (ProgrammabilityType)IdProgrammabilityType; }
			set { IdProgrammabilityType = (byte)value; }
		}


        public Programmability()
            : base()
        {

        }


	    public override int EntityId
	    {
			get { return -2147483610; }
	    }

        /// <summary>
        /// 
        /// </summary>
        public static int EntityIdStatic
        {
            get { return -2147483610; }
        }
	}
}
