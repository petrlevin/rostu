using System;
using System.Collections.Generic;
using System.Linq;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Interfaces;
using Platform.PrimaryEntities.Multilink;

namespace Platform.PrimaryEntities.Reference
{
    /// <summary>
    /// Класс описывающий ссылку в иерархии
    /// </summary>
	public class HierarchicalLink : Metadata, IIdentitied
	{
		#region private Поля

		private string _caption;

		private int _idEntity;
		#endregion
		/// <summary>
		/// Идентификатор элемента
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Наименование
		/// </summary>
		public string Caption
		{
			get { return _caption; }
			set
			{
				_caption = value;
				UpdatePathElements();
			}
		}

		/// <summary>
		/// Ссылка на сущность, которой принадлежит иерархическая ссылка
		/// </summary>
		public int IdEntity
		{
			get { return _idEntity; }
			set
			{
				_idEntity = value;
				UpdatePathElements();
			}
		}

		public List<HierarchicalLinkEntityFieldPathElements> PathElements;

		/// <summary>
		/// Обновляет лист PathElements после изменения IdEntity или Caption, при условии что оба поля заполнены
		/// </summary>
		private void UpdatePathElements()
		{
			if (string.IsNullOrEmpty(_caption) || _idEntity != 0 )
				return;
			string[] fieldsName = _caption.Split(new char[] {'.'});
			Entity startEntity = Objects.ById<Entity>(_idEntity);
			EntityField startEntityField = (EntityField) startEntity.Fields.SingleOrDefault(a => a.Name == fieldsName[0]);
			if (startEntityField == null)
				throw new Exception(string.Format("UpdatePathElements: Поле '{0}' не принадлежит сущности '{1}'",
												  fieldsName[0], startEntity.Name));
			if (startEntityField.EntityFieldType != EntityFieldType.Link)
				throw new Exception(string.Format("UpdatePathElements: Поле '{0}' должно быть с типом Link",
												  fieldsName[0]));
			int order = 1;
			PathElements.Add(new HierarchicalLinkEntityFieldPathElements {IdHierarchicalLink = Id, IdEntityField = startEntityField.Id, OrdEntityField = order});
			foreach (string fieldName in fieldsName.Skip(1))
			{
				if (!startEntityField.IdEntityLink.HasValue)
					throw new Exception(string.Format("У поля '{0}' не заполнено IdEntityLink", startEntityField.Name));
				startEntity = Objects.ById<Entity>(startEntityField.IdEntityLink.Value);
				startEntityField = (EntityField) startEntity.Fields.SingleOrDefault(a => a.Name == fieldsName[0]);
				if (startEntityField == null)
					throw new Exception(string.Format("UpdatePathElements: Поле '{0}' не принадлежит сущности '{1}'",
													  fieldsName[0], startEntity.Name));
				order++;
				PathElements.Add(new HierarchicalLinkEntityFieldPathElements { IdHierarchicalLink = Id, IdEntityField = startEntityField.Id, OrdEntityField = order });
			}
		}

        public override int EntityId
        {
			get { return -2080374750; }
        }


	}
}
