using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using BaseApp;
using BaseApp.Numerators;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.EntityTypes;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Reference;
using System.Xml.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.Application.Common;
using Platform.Utils.Common;
using BaseApp.Interfaces;
using Sbor.Tablepart;
using EntityFieldType = Platform.PrimaryEntities.Common.DbEnums.EntityFieldType;


namespace Sbor.Tool
{
    /// <summary>
    /// Балансировка доходов, расходов и ИФДБ
    /// </summary>
    public partial class BalancingIFDB
    {
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1500)]
        public void AutoSet(DataContext context, ControlType ctType)
        {
            BalanceConfig cfgdoc = context.BalanceConfig_FilterRule.Where(s => s.Id == IdBalanceConfig_FilterRule).Select(a => a.Owner).Single();
            if (!IdBalancingIFDBType.HasValue) IdBalancingIFDBType = cfgdoc.IdBalancingIFDBType;
            IdSourcesDataTools = cfgdoc.IdSourcesDataTools; // if (!IdSourcesDataTools.HasValue)
        }

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert, Sequence.After, ExecutionOrder = -1000)]
        public void AutoSetAfter(DataContext context, ControlType ctType)
        {
            List<BalancingIFDB_SetShowKBK> list = context.EntityField.Where(w =>
                w.IdEntity == BalancingIFDB_Expense.EntityIdStatic &&
                w.IdEntityFieldType == (byte)EntityFieldType.Link 
                && !(new string[] { "idOwner", "id", "idMaster" }).Contains(w.Name) 
            ).Select(s => s.Id).ToList().Select(s => new BalancingIFDB_SetShowKBK {
                IdOwner = Id,
                IdEntityField = s
            }).ToList();

            context.BalancingIFDB_SetShowKBK.InsertAsTableValue(list, context);
        }

        [ControlInitial(ExcludeFromSetup = true)]
        public void UniqueTest(DataContext context)
        {
            var otherDoc = context.BalancingIFDB.FirstOrDefault(s =>
                s.IdBalanceConfig_FilterRule == IdBalanceConfig_FilterRule && s.Id != Id
                && s.IdPublicLegalFormation == IdPublicLegalFormation
                && s.IdBudget == IdBudget
                && s.IdVersion == IdVersion
                && s.IdDocStatus != DocStatus.Draft
            );

            if (otherDoc != null)
                Controls.Throw(string.Format("Создан другой инструмент с таким же правилом фильтрации:<br>{0}", otherDoc.Caption));
        }

        [ControlInitial(ExcludeFromSetup = true)]
        public void TestRole(DataContext context)
        {
            var idCurUser = new BaseAppNumerators().IdUser();
            bool fail = !context.BalanceConfig_User.Any(a => a.IdMaster == IdBalanceConfig_FilterRule && a.IdUser == idCurUser);

            if (fail)
                Controls.Throw("Вы не имеете право редактировать этот инструмент.");
        }

        [ControlInitial(ExcludeFromSetup = true)]
        public void TestStatusOtherTools(DataContext context)
        {
            var tool = context.ApprovalBalancingIFDB.SingleOrDefault(a =>
                a.IdSourcesDataTools == IdSourcesDataTools
                && (a.IdDocStatus == DocStatus.CreateDocs || a.IdDocStatus == DocStatus.Completed)
                && a.Id != Id
            );

            if (tool != null)
                Controls.Throw(string.Format(
                    "Предыдущая балансировка документов по источнику данных '{0}' не завершена. Существует сводный инструмент '{1}' на статусе '{2}'. Требуется завершить указанный инструмент.",
                    tool.SourcesDataTools.Caption(),
                    string.Format("№ {0} от {1}", tool.Number, tool.Date.ToString("dd.MM.yyyy")),
                    tool.DocStatus.Caption
                ));
        }
    }
}
