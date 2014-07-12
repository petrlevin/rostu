using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAnt.Core;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;
using Tools.MigrationHelper.Core.Controls;
using TypesExtension = Platform.Utils.TypesExtension;

namespace Tools.MigrationHelper.Core.Tasks
{
    abstract public class SetControlsTask : DbDeployTask
    {

        //BuildAttributeAttribute
        protected void WriteControls(IEnumerable<ControlInfo> controls)
        {
            using (var dataContext = new DataContext(ConnectionString))
            {
                controls.Where(c => !dataContext.Control.Any(cc => (cc.Name == c.Name) && (c.IdEntity.HasValue ? c.IdEntity.Value == cc.IdEntity : cc.IdEntity == null))).ToList().ForEach(


                    c =>
                    {
                        Log(Level.Verbose, "Создание контроля {0}", c.Name);
                        if (!(dataContext.Control.Any(cc => (cc.Name == c.Name) && (c.IdEntity.HasValue ? c.IdEntity.Value == cc.IdEntity : cc.IdEntity == null))))

                            dataContext.Control.Add(new Control()
                            {
                                IdEntity = c.IdEntity,
                                Caption = c.Caption ?? c.Name,
                                Enabled = true,
                                Name = c.Name,
                                Skippable = c.Skippable,
                                Managed = c.Managed,
                                UNK = c.UNK
                            });
                        else
                            Log(Level.Verbose, "Обнаружен дубль контроля {0}", c);
                    }
                    );

                dataContext.SaveChanges();


            }
        }

        protected void WriteControls(IEnumerable<String> controls)
        {
            WriteControls(controls.Select(c => new ControlInfo() { Name = c, Skippable = false }));
        }


        protected class ControlInfo
        {
            public String Name { get; set; }
            public String UNK
            {
                get { return _UNK; }
                set { _UNK = String.IsNullOrEmpty(value) ? null : value; }
            }

            private String _UNK;

            public String Caption { get; set; }

            public bool Managed { get; set; }
            public bool Skippable { get; set; }
            public int? IdEntity { get; set; }

            public ControlInfo()
            {

            }

            public ControlInfo(MemberInfo control, Type attribute)
            {

            }


        }
        protected List<Assembly> GetAssemblies()
        {

            return Assemblies.Get(SourcePath, (ex, fn) =>
                               {
                                   ErrorFormat(
                                       "Ошибка при загрузке сборки {0} .  {1} - {2} ", fn, ex.GetType(), ex.Message);
                                   Log(Level.Debug, ex.StackTrace);

                               });

        }

        protected ControlInfo CreateControlInfo(MemberInfo control, ControlInitialAttribute mainAttr,
                                                ControlInitialForAttribute attribute, Type targetType)
        {
            return CreateControlInfo(control, mainAttr,
                                     attribute, targetType != null ? GetIdEntity(targetType) : null);
        }

        protected ControlInfo CreateControlInfo(MemberInfo control, ControlInitialAttribute mainAttr,
                                              ControlInitialForAttribute attribute, int? idEntity)
        {
            ControlInfo ci;
            if ((mainAttr == null) && (attribute != null))
            {
                ci = new ControlInfo()
                         {
                             Caption = attribute.InitialCaption ?? control.Name,
                             IdEntity = idEntity,
                             Name = control.Name,
                             Managed = attribute.InitialManaged,
                             Skippable = attribute.InitialSkippable,
                             UNK = attribute.InitialUNK
                         };
            }
            else if (attribute != null)
            {
                ci = new ControlInfo()
                         {
                             Caption = attribute.InitialCaption ?? mainAttr.InitialCaption ?? control.Name,
                             IdEntity = idEntity,
                             Name = control.Name,
                             Managed =
                                 attribute.ActualInitialManaged.HasValue
                                     ? attribute.ActualInitialManaged.Value
                                     : mainAttr.InitialManaged,
                             Skippable =
                                 attribute.ActualInitialSkippable.HasValue
                                     ? attribute.ActualInitialSkippable.Value
                                     : mainAttr.InitialSkippable,
                             UNK = attribute.InitialUNK ?? mainAttr.InitialUNK
                         };
            }
            else if (mainAttr != null)
            {
                ci = new ControlInfo()
                         {
                             Caption = mainAttr.InitialCaption ?? control.Name,
                             IdEntity = idEntity,
                             Name = control.Name,
                             Managed = mainAttr.InitialManaged,
                             Skippable = mainAttr.InitialSkippable,
                             UNK = mainAttr.InitialUNK
                         };
            }
            else
            {
                ci = new ControlInfo()
                {
                    Caption = control.Name,
                    IdEntity = idEntity,
                    Name = control.Name,
                    Managed = false,
                    Skippable = false,
                    UNK = null
                };

            }


            return ci;

        }

        private int? GetIdEntity(Type declaringType)
        {
            return Objects.ByName<Entity>(declaringType.Name).Id;
        }

        protected IEnumerable<Type> GetCommonControlTargets(List<Assembly> asmbls)
        {
            
            var targets = new List<Type>();
            foreach (Assembly assembly in asmbls)
            {
                Log(Level.Verbose, "Общие контроли. Получение сущностных типов (Target). Обработка сборки {0}...", assembly.FullName);
                targets.AddRange(TypesExtension.WhitchHasAttribute(assembly.AllTypes(), typeof(ControlInitialForAttribute)));
            }
            return targets;
        }

        protected IEnumerable<Type> GetCommonControlTypes(IEnumerable<Assembly> asmbls, Type cctdef)
        {
            var controls = new List<Type>();

            foreach (Assembly assembly in asmbls)
            {
                //                var name = assembly.GetName().Name;
                Log(Level.Verbose, "Общие контроли.Обработка сборки {0}...", assembly.FullName);
                controls.AddRange(TypesExtension.WhitchInherit(assembly.AllTypes(), cctdef, TypeOptions.AutoInvokable));
            }

            return controls;
        }

        protected IEnumerable<ControlInfo> ControlsForType(Type commonControl, IEnumerable<Type> controlTargets , Type cctdef)
        {
            var result = new List<ControlInfo>();
            var mainAttr = (ControlInitialAttribute)commonControl.GetAttributeExactlyMatch<ControlInitialAttribute>();
            var attrs = commonControl.GetCustomAttributes(typeof(ControlInitialForAttribute)).Where(aa => !((ControlInitialForAttribute)aa).ExcludeFromSetup);
            foreach (ControlInitialForAttribute attribute in attrs)
            {

                result.Add(CreateControlInfo(commonControl, mainAttr, attribute ,attribute.Target));
            }

            foreach (Type controlTarget in controlTargets)
            {
                foreach (Type controlTargetParent in controlTarget.GetAllParents())
                {
                    if (
                        commonControl.GetAllParents(cctdef, controlTargetParent).SingleOrDefault() !=
                        null)
                    {
                        var attribute = (ControlInitialForAttribute)controlTarget.GetCustomAttributes(typeof(ControlInitialForAttribute), true)
                                                                                 .SingleOrDefault(a => (!((ControlInitialForAttribute)a).ExcludeFromSetup) && (((ControlInitialForAttribute)a).Target == commonControl));
                        if (attribute != null)
                        {
                            result.Add(CreateControlInfo(commonControl, mainAttr, attribute, controlTarget));
                        }
                    }
                }


            }
            if ((mainAttr==null)||(!mainAttr.ExcludeFromSetup))
            {
                result.Add(CreateControlInfo(commonControl, mainAttr, null, (int?) null));
            }

            return result;

        }

        protected void WriteCommonControls(IEnumerable<Type> commonControls, IEnumerable<Type> commonControlTargetsWithInital, Type cctef)
        {
            WriteControls(commonControls.SelectMany(cc => ControlsForType(cc, commonControlTargetsWithInital, cctef)));
        }
    }


}
