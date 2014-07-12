using System;
using System.Globalization;
using System.Linq;
using System.Collections.Generic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes;
using Sbor.Logic;
using Sbor.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.PrimaryEntities.DbEnums;
using Sbor;
using Sbor.DbEnums;
using RefStats = Platform.PrimaryEntities.DbEnums.RefStatus;

namespace Sbor.Reference
{

    public partial class Activity : ReferenceEntity 
	{
        private void InitMaps(DataContext context)
        {
            if (PublicLegalFormation == null)
                PublicLegalFormation = context.PublicLegalFormation.SingleOrDefault(w => w.Id == IdPublicLegalFormation);

        }

        /// <summary>   
        /// Генерация значения поля «Код» с 1 до n. 
        /// </summary>
        [Control(ControlType.Insert, Sequence.Before, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {

            InitMaps(context);

            if (PublicLegalFormation.IdMethodofFormingCode_Activity == (byte)BaseApp.DbEnums.MethodofFormingCode.Auto || string.Equals(Code, "Автоматически"))
            {
                var sc =
                    context.Activity.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation)
                           .Select(s => s.Code).Distinct().ToList();

                Code = CommonMethods.GetNextCode(sc);
            }
        }

        /// <summary>   
        /// Контроль "Проверка заполнения ведомства"
        /// </summary>
        [Control(ControlType.Update, Sequence.After, ExecutionOrder = 2)]
        public void Control_502102(DataContext context)
        {
            if (IdRefStatus != (byte)RefStats.Work)
            {
                return;
            }

            var MsgErr = "Необходимо указать хотя бы одно ведомство.";

			if (!this.Activity_SBP.Any())
            {
                Controls.Throw(MsgErr);
            }
        }

        /// <summary>   
        /// Контроль "Проверка наличия объемного показателя"
        /// </summary>
        [Control(ControlType.Update, Sequence.After, ExecutionOrder = 3)]
        public void Control_502103(DataContext context)
        {
            if (IdRefStatus != (byte)RefStats.Work || !Activity_SBP.Any())
            {
                return;
            }

            var sMsg = "Необходимо указать хотя бы один показатель объема. <br>Не указано для ведомств:<br>{0}";

            var exists = Activity_SBP
                .Where(r =>
                       !context.Activity_Indicator
                               .Any(w =>
                                    w.IdOwner == Id &&
                                    w.IdSBP == r.Id &&
                                    w.IndicatorActivity.IdIndicatorActivityType == (int) IndicatorActivityType.VolumeIndicator
                            ))
                .Select(s => s.Caption);

            if (exists.Any())
            {
                Controls.Throw(string.Format(sMsg, exists.Aggregate((a,b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка заполнения контингента"
        /// </summary>
        [Control(ControlType.Update, Sequence.After, ExecutionOrder = 4)]
        public void Control_502104(DataContext context)
        {
            if ( (IdRefStatus != (byte)RefStats.Work) ||
                    (ActivityType != ActivityType.Service && ActivityType != ActivityType.PublicLiability && ActivityType != ActivityType.PublicNormativeLiability))
                return;

            const string msgErr = "Для мероприятий с типом «Услуга», «Публичное обязательство» и «Публичное нормативное обязательство» требуется указывать контингент.";

			if (!Activity_Contingent.Any())
                Controls.Throw(msgErr);
        }

        /// <summary>   
        /// Контроль "Проверка наличия показателей качества"
        /// </summary>
        [Control(ControlType.Update, Sequence.After, ExecutionOrder = 5)]
        public void Control_502105(DataContext context)
        {
            if ((IdRefStatus != (byte)RefStats.Work) || (ActivityType != ActivityType.Service) || !Activity_SBP.Any())
            {
                return;
            }
            var sMsgErr = "Для мероприятий с типом «Услуга» требуется указывать показатель качества.<br>Не указано для ведомств:<br>{0}";

            var exists = Activity_SBP
                                .Where(r =>
                                       !context.Activity_Indicator
                                               .Any(w =>
                                                    w.IdOwner == Id &&
                                                    w.IdSBP == r.Id &&
                                                    w.IndicatorActivity.IdIndicatorActivityType == (int)IndicatorActivityType.QualityIndicator))
                                                    .Select(s => s.Caption);

            if (exists.Any())
            {
                Controls.Throw(string.Format(sMsgErr, exists.ToList().Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка НПА"
        /// </summary>
        [Control(ControlType.Update, Sequence.After, ExecutionOrder = 6)]
        [ControlInitial(InitialCaption = "Проверка НПА", InitialManaged = true, InitialSkippable = true)]
        public void Control_502106(DataContext context)
        {
            if ((IdRefStatus != (byte)RefStats.Work) ||
                (ActivityType != ActivityType.Service && ActivityType != ActivityType.PublicLiability && ActivityType != ActivityType.PublicNormativeLiability) || 
                !Activity_SBP.Any())
                return;

            const string sMsgErr = @"Для мероприятий с типом «Услуга», «Публичное обязательство» и «Публичное нормативное обязательство» необходимо указывать НПА, являющийся основанием для предоставления.<br/>
                                     Не указано для ведомств:<br/>{0}";

            var exists = Activity_SBP
                                .Where(r =>
                                       !context.Activity_RegulatoryAct
                                               .Any(w => w.IdOwner == this.Id &&
                                                         w.IdSBP == r.Id &&
                                                         (w.IsBasis ?? true)))
                                .Select(s => s.Caption);

            if (exists.Any())
            {
                Controls.Throw(string.Format(sMsgErr, exists.ToList().Aggregate((a, b) => a + "<br/>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка НПА по платным мероприятиям"
        /// </summary>
        [Control(ControlType.Update, Sequence.After, ExecutionOrder = 6)]
        public void Control_502107(DataContext context)
        {
            if ((IdRefStatus != (byte)RefStats.Work) || !this.IdPaidType.HasValue || (this.PaidType == DbEnums.PaidType.NotPaid) || !Activity_SBP.Any())
            {
                return;
            }

            const string sMsgErr = "Для мероприятий, выполняемых на платной основе, необходимо указывать НПА, устанавливающий предельные цены (тарифы).<br/>Не указано для ведомств:<br/>{0}";


            var exists = Activity_SBP
                                .Where(r =>
                                       !context.Activity_RegulatoryAct
                                               .Any(w => w.IdOwner == this.Id &&
                                                         w.IdSBP == r.Id &&
                                                         (w.IsSetMaxPrice ?? true)))
                                .Select(s => s.Caption);

            if (exists.Any())
            {
                Controls.Throw(string.Format(sMsgErr, exists.ToList().Aggregate((a, b) => a + "<br>" + b)));
            }
        }

        /// <summary>   
        /// Контроль "Каскадное удаление в табличных частях, зависимых от МТЧ «Ведомство»"
        /// </summary>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = 0)]
        public void Control_502108(DataContext context)
        {
            var idsbps = Activity_SBP.Select(s => s.Id).ToList();

            var delIndicator = context.Activity_Indicator.Where(r => !idsbps.Contains(r.IdSBP) && r.IdOwner == this.Id).ToList();
            foreach (var del in delIndicator) context.Activity_Indicator.Remove(del);

            var delRegulatoryAct = context.Activity_RegulatoryAct.Where(r => !idsbps.Contains(r.IdSBP) && r.IdOwner == this.Id).ToList();
            foreach (var del in delRegulatoryAct) context.Activity_RegulatoryAct.Remove(del);

            var delActivity_ExtInfo = context.Activity_ExtInfo.Where(r => !idsbps.Contains(r.IdSBP) && r.IdOwner == this.Id).ToList();
            foreach (var del in delActivity_ExtInfo) context.Activity_ExtInfo.Remove(del);

        }

    }
}

