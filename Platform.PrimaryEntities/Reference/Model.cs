using System;
using System.Collections.Generic;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;

namespace Platform.PrimaryEntities.Reference
{
    public class Model : Metadata, IIdentitied
	{
		#region Поля БД
		/// <summary>
		/// Идентификатор элемента
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Системное наименование
		/// </summary>
		public string Name { get; set; }

		public EntityType EntityType { get; private set; }

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption { get; set; }

		/// <summary>
		/// Описание
		/// </summary>
		public string Description;

		/// <summary>
		/// Ссылка на сущность
		/// </summary>
		public int? IdEntity;

		#endregion

		public List<IEntityField> GetFields()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Поля формы
		/// </summary>
		public IEnumerable<ModelField> Fields { get; set; }

		public string Schema
		{
			get { return "gen"; }
		}

		public static Model ById(int id)
		{
			throw new NotImplementedException();
		}

		public Entity Entity
		{
			get { return IdEntity.HasValue ? Objects.ById<Entity>(IdEntity.Value) : null; }
		}


        public Model()
            : base()
        {

        }


	}
}
