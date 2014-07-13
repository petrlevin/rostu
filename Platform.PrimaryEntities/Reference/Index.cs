using System;
using System.Collections.Generic;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Multilink;

namespace Platform.PrimaryEntities.Reference
{
	/// <summary>
	/// Индекс
	/// </summary>
	public class Index : Metadata
	{
		#region Данные из БД
		/// <summary>
		/// Идентификатор
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Идентификатор сущности
		/// </summary>
		public int IdEntity { get; set; }

		/// <summary>
		/// Системное наименование
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Идентифкатор типа индекса
		/// </summary>
		public byte IdIndexType { get; set; }

		/// <summary>
		/// Признак кластерного индекса
		/// </summary>
		public bool IsClustered { get; set; }

		/// <summary>
		/// WHERE условие для индекса
		/// </summary>
		public string Filter { get; set; }

		/// <summary>
		/// Идентификатор статуса элемента
		/// </summary>
		public byte IdRefStatus { get; set; }
		#endregion

		/// <summary>
		/// Сущность
		/// </summary>
		private Entity _entity;

		/// <summary>
		/// Сущность
		/// </summary>
		public Entity Entity
		{
			get { return _entity ?? (_entity = Objects.ById<Entity>(IdEntity)); }
		}

		/// <summary>
		/// Тип индекса
		/// </summary>
		public IndexType IndexType
		{
			get { return (IndexType) IdIndexType; }
		}

		/// <summary>
		/// Статус элемента
		/// </summary>
		public RefStatus Status
		{
			get { return (RefStatus) IdRefStatus; }
		}
		
		/// <summary>
		/// Индексируемые поля сущности
		/// </summary>
		public virtual ICollection<Index_EntityField_Indexable> MlIndexableEntityFields { get; set; }

		public override int EntityId
	    {
			get { return -2013265751; }
	    }


	}
}
