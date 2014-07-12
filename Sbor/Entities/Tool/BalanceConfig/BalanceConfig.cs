using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Platform.BusinessLogic;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;


namespace Sbor.Tool
{
	/// <summary>
	/// Настройка балансировки
	/// </summary>
	public partial class BalanceConfig
	{
        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            DateLastEdit = DateTime.Now;
        }

        /// <summary>   
        /// Операция «Редактировать»   
        /// </summary>  
        public void Edit(DataContext context)
        {
            DateLastEdit = DateTime.Now;
        }

        /// <summary>   
        /// Операция «Утвердить»   
        /// </summary>  
        public void Confirm(DataContext context)
        {
            ExecuteControl(e => e.Control_600101(context));

            if (Parent != null)
            {
                Parent.ExecuteOperation(e => e.Archive(context));
            }
        }

        /// <summary>   
        /// Операция «Отменить утверждение»   
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            if (Parent != null)
            {
                Parent.ExecuteOperation(e => e.UndoArchive(context));
            }
        }

        /// <summary>   
        /// Операция «Изменить»   
        /// </summary>  
        public void Change(DataContext context)
        {
            Clone cloner = new Clone(this);
            BalanceConfig newTool = (BalanceConfig)cloner.GetResult();
            newTool.IdDocStatus = DocStatus.Draft;
            newTool.IdParent = Id;

            newTool.Date = DateTime.Now.Date;

            context.Entry(newTool).State = EntityState.Added;
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Отменить изменение»   
        /// </summary>
        public void UndoChange(DataContext context)
        {
            var q = context.BalanceConfig.Where(w => w.IdParent == Id);
            foreach (var tool in q)
            {
                context.BalanceConfig.Remove(tool);
            }
        }

        /// <summary>   
        /// Операция «В архив»   
        /// </summary>  
        public void Archive(DataContext context)
        {

        }

        /// <summary>   
        /// Операция «Вернуть на изменен»   
        /// </summary>  
        public void UndoArchive(DataContext context)
        {

        }
    }
}