using System;
using System.Data.Entity;
using System.Linq;
using System.Collections.Generic;
using BaseApp.Common.Interfaces;
using BaseApp.DbEnums;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using BaseApp.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common;
using Platform.Common.Exceptions;

namespace BaseApp.Reference
{
    public partial class Role : ReferenceEntity
    {
        /// <summary>
        /// Системная роль администратора операций
        /// </summary>
        public static Role OperationAdmin
        {
            get
            {
                var db = IoC.Resolve<DbContext>().Cast<DataContext>();
                var _operationAdmin = db.Role.SingleOrDefault(r => r.Caption == "OperationAdmin");
                if (_operationAdmin == null)
                {
                    throw new PlatformException("Не найдена системная роль OperationAdmin");
                }
                return _operationAdmin;                
            }
        }

        /// <summary>   
        /// Контроль "Проверка заполнения поля «Группа доступа»"
        /// </summary>  
        [Control(ControlType.Update | ControlType.Insert, Sequence.After, ExecutionOrder = 20)]
        public void Control_501702(DataContext context)
        {
            if (this.IdRoleType == (int)RoleType.User)
            {
                if (!this.IdAccessGroup.HasValue)
                {
                    Controls.Throw("Необходимо заполнить поле «Группа доступа»");
                }
            }// если Тип = Системная, то группа доступа должна быть очищена
            else if (this.IdRoleType == (int)RoleType.System || this.IdRoleType == (int)RoleType.Preset)
            {
                this.IdAccessGroup = null;
                //context.SaveChanges();
            }
        }

        /// <summary>   
        /// Контроль "Редактирование системной роли"
        /// </summary>  
        [Control(ControlType.Update | ControlType.Insert, Sequence.After, ExecutionOrder = 20)]
        public void Control_501704(DataContext context)
        {
            var name = (IoC.Resolve<IUser>("CurrentUser")).Name;
            var b = name == "admin" || name == "bis";
            if (!b && this.IdRoleType == (int)RoleType.System)
            {
                Controls.Throw("Редактировать системные роли могут только разработчики");
            }
        }
    }
}

