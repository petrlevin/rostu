using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using NAnt.Core;
using NAnt.Core.Attributes;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.Utils;
using Platform.Utils.Extensions;

namespace Tools.MigrationHelper.Core.Tasks
{
    /// <summary>
    /// 
    /// </summary>
    [TaskName("setcudcontrols")]
    public class SetCUDControls : SetControlsTask
    {
        public SetCUDControls()
        {
            FailOnError = false;
        }

        protected override void ExecuteTask()
        {
            if (!IsDeveloper()) return;
            try
            {
                DoExecuteTask();
            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при установке контролей", ex);
            }
        }

        private void DoExecuteTask()
        {
            Log(Level.Verbose, "Загрузка сборок ...");
            var asmbls = GetAssemblies();
            Log(Level.Verbose, "Получение данных о контролях...");
            var controls = GetControlMethods(asmbls);
            Log(Level.Verbose, "Записываем контролы в базу...");
            WriteControls(controls);

            Log(Level.Verbose, "Получение данных об общих  контролях...");

            var commonControlTargetsWithInital = GetCommonControlTargets(asmbls);
            var commonControls = GetCommonControlTypes(asmbls);

            WriteCommonControls(commonControls, commonControlTargetsWithInital);

            Log(Level.Verbose, "Записываем общие контролы в базу...");
        }

        private void WriteCommonControls(IEnumerable<Type> commonControls, IEnumerable<Type> commonControlTargetsWithInital)
        {
            WriteCommonControls(commonControls, commonControlTargetsWithInital, typeof (ICommonControl<,>));
        }

        private void WriteControls(IEnumerable<MethodInfo> controls)
        {
            WriteControls(
                controls.Select(
                    mi =>
                    CreateControlInfo(mi, mi.GetAttributeExactlyMatch<ControlInitialAttribute>(), null, mi.DeclaringType)));
        }

        private IEnumerable<Type> GetCommonControlTypes(IEnumerable<Assembly> asmbls)
        {
            return GetCommonControlTypes(asmbls, typeof (ICommonControl<,>));
        }

        private IEnumerable<MethodInfo> GetControlMethods(IEnumerable<Assembly> asmbls)
        {
            var controls = new List<MethodInfo>();

            foreach (Assembly assembly in asmbls)
            {
                //                var name = assembly.GetName().Name;
                Log(Level.Verbose, "Обработка сборки {0}...", assembly.FullName);
                try
                {
                    var methods =
                        assembly.GetTypes()
                                .SelectMany(t => t.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic |
                                                              BindingFlags.Public | BindingFlags.DeclaredOnly));

                    methods = methods.Where(m => m.GetCustomAttributes(true).Any(oa => (oa.GetType().FullName == typeof(ControlAttribute).FullName))
                              && (
                                !m.GetCustomAttributes(true).Any(oa => (oa.GetType().FullName == typeof(ControlInitialAttribute).FullName))
                                || m.GetCustomAttributes(true).Any(oa => (oa.GetType().FullName == typeof(ControlInitialAttribute).FullName)
                                    && (!(bool)oa.GetType().GetProperty(Reflection<ControlInitialAttribute>.Property(ca => ca.ExcludeFromSetup).Name).GetValue(oa)))));



                    controls.AddRange(methods.ToList());

                }
                catch (ReflectionTypeLoadException)
                {
                }
            }
            return controls;
        }
    }
}
