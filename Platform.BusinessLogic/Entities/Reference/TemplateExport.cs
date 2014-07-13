using System;
using System.Linq;
using System.Linq.Expressions;
using System.Collections.Generic;
using Platform.BusinessLogic.DbEnums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Reference;
using System.ComponentModel.DataAnnotations.Schema;
using System.Xml.Linq;
using System.Data.Objects.DataClasses;
using Platform.Application.Common;
using Platform.Utils.Common;




namespace Platform.BusinessLogic.Reference
{
	// Это автоматически сгенерированный класс, изменения будут уничтожены при следующей генерации
	/// <summary>
	/// Шаблон экпорта
	/// </summary>
    public partial class TemplateExport : IExportSettings
	{
	
		/// <summary>
		/// Идентификатор
		/// </summary>



        IEnumerable<ISelectItem> IExportSettings.Entities
        {
            get { return Entities; }
        }

	    public TargetType TargetType {
	        get { return TargetType.Source;}
	    }

	    public IExportSettings AsSettingsForLinked()
        {
            return new _Wrapper(this);
        }


        private class _Wrapper : IExportSettings
        {
            private TemplateExport _instance;

            public TargetType TargetType
            {
                get { return TargetType.Links; }
            }


            public _Wrapper(TemplateExport instance)
            {
                _instance = instance;
            }

            public SelectionType SelectionType
            {
                get { return _instance.LinkedSelectionType; }
            }

            public IEnumerable<ISelectItem> Entities
            {
                get { return _instance.LinkedEntities.Select(e => (ISelectItem)new _SelectItem() { Entity = e }); }
            }

            public string EntitiesSql { get { return _instance.LinkedEntitiesSql; }
            }


            struct _SelectItem :ISelectItem
            {
                public Entity Entity { get; set; }
                public string Sql { get { return null; } }
            }

        }




    }
}