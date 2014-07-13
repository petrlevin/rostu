using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Interfaces;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;

namespace Platform.BusinessLogic.Activity.Controls
{
    /// <summary>
    /// 
    /// </summary>
    public class SpecificControlLauncher : ControlLauncher
    {
        private Settings _settings;

        public SpecificControlLauncher([Dependency]DbContext dbContext,[Dependency]IControlDispatcher controlDispatcher, [Dependency]Settings settings):base(dbContext,controlDispatcher)
        {
            if (settings == null) throw new ArgumentNullException("settings");
            _settings = settings;
        }



        public override void ProcessControls(ControlType controlType, Sequence sequence, IBaseEntity entityValue,
                            IBaseEntity oldEntityValue)
        {
            if ((_settings.EntityOptions == EntityOptions.AllExclude) &&
                (_settings.Entities.Any(e => e.Id == entityValue.EntityId)))
                return;
            if ((_settings.EntityOptions == EntityOptions.JustThese ) &&
                (_settings.Entities.All(e => e.Id != entityValue.EntityId)))
                return;
            if ((_settings.ControlType&controlType)!=controlType)
                return;
            base.ProcessControls(controlType,sequence,entityValue,oldEntityValue);
            }

        public enum EntityOptions
        {
            AllExclude,
            JustThese
        }

        public class Settings
        {
            public EntityOptions EntityOptions { get; set; }
            public List<IEntity> Entities { get; set; }
            public ControlType ControlType { get; set; }
            public Settings()
            {
                EntityOptions = EntityOptions.AllExclude;
                Entities = new List<IEntity>();
                ControlType = ControlType.Any;
            }

            public Settings(IEntity entity , EntityOptions entityOptions, ControlType controlType)
            {
                EntityOptions = entityOptions;
                Entities = new List<IEntity>() { entity };
                ControlType = controlType;
            }

            public Settings(int entityId, EntityOptions entityOptions, ControlType controlType):
                this(Objects.ById<Entity>(entityId), entityOptions, controlType)
            {
            }



            public Settings(IEntity entity)
                : this(entity, EntityOptions.JustThese, ControlType.Any)
            {
            }

            public Settings(int entityId)
                : this(Objects.ById<Entity>(entityId))
            {
            }


            public Settings(IEnumerable<Int32> entityIds , ControlType controlType = ControlType.Any , EntityOptions entityOptions = EntityOptions.JustThese)

            {
                Entities = new List<IEntity>();
                foreach (var entityId in entityIds)
                {
                    Entities.Add(Objects.ById<Entity>(entityId));
                }
                ControlType = controlType;
                EntityOptions = entityOptions;

            }
        }

    }

    
}
