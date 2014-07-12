using System;
using System.Globalization;
using System.Collections.Generic;
using System.Data;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.DataAccess;
using Platform.BusinessLogic.EntityTypes;
using System.Linq;
using System.ComponentModel.DataAnnotations.Schema;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common.Extensions;
using Sbor.DbEnums;
using Sbor.Reference;
using Sbor.Tablepart;
using BaseApp.Reference;
using Sbor.Document;
using Sbor.Logic;

namespace Sbor.Document
{
    public partial class RegisterActivity : DocumentEntity<RegisterActivity>
    {
        #region Контроли

        private void InitMaps(DataContext context)
        {
            if (PublicLegalFormation == null)
                PublicLegalFormation = context.PublicLegalFormation.SingleOrDefault(w => w.Id == IdPublicLegalFormation);
        }

        /// <summary>   
        /// Генерация значения поля «Номер» с 1 до n. 
        /// </summary>
        [Control(ControlType.Insert, Sequence.Before, ExecutionOrder = 0)]
        public void AutoSet(DataContext context)
        {
            InitMaps(context);

            var sc =
                context.RegisterActivity.Where(w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                        .Select(s => s.Number).Distinct().ToList();

            Number = CommonMethods.GetNextCode(sc);

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.After, ExecutionOrder = -1000)]
        public void AutoSetHeader(DataContext context)
        {
            Header = String.Format("{0} № {1} от {2}", DocType.Caption, Number, Date.ToString("dd.MM.yyyy")); ;
        }

        /// <summary>   
        /// Контроль "Проверка уникальности документа"
        /// </summary> 
        [Control(ControlType.Update | ControlType.Insert, Sequence.Before, ExecutionOrder = 10)]
        public void Control_0401(DataContext context)
        {
            if (IdParent.HasValue)
                return;
            
            const string sMsg = "Уже существует документ {0}{1}.";

            var err = context.RegisterActivity.Where(r => 
                r.IdDocType == IdDocType 
                && r.IdPublicLegalFormation == IdPublicLegalFormation 
                && (r.IdSBP ?? 0) == (IdSBP ?? 0)
                && r.Id != Id
            );

            if (err.Any())
                Controls.Throw(string.Format(sMsg, this.DocType.Caption,
                                                                 (this.IdSBP.HasValue
                                                                      ? " для ведомства " + SBP.Caption
                                                                      : "")));
        }

        /// <summary>   
        /// Контроль "Проверка указания контингента"
        /// </summary> 
        public void Control_0402(DataContext context)
        {
            const string msg = "Для мероприятий с типом «Услуга», «Публичное обязательство» и «Публичное нормативное обязательство» требуется указывать контингент.";

            var err = context.RegisterActivity_Activity.Where(r =>
                                                              r.IdOwner == this.Id &&
                                                              (r.Activity.IdActivityType == (byte)ActivityType.Service ||
                                                               r.Activity.IdActivityType == (byte)ActivityType.PublicLiability ||
                                                               r.Activity.IdActivityType == (byte)ActivityType.PublicNormativeLiability) &&
                                                              !r.IdContingent.HasValue);

            if (err.Any())
                Controls.Throw(msg);
        }

        /// <summary>   
        /// Контроль "Проверка указания показателей качества"
        /// </summary> 
        public void Control_0403(DataContext context)
        {
            var sMsg = "Для следующих мероприятий требуется указать показатели качества:<br>{0}";

            var indicators = context.RegisterActivity_IndicatorActivity.Select(s => s.IdMaster).Distinct().ToArray();
 
            IQueryable<RegisterActivity_Activity> err;

            if (this.DocType.Id == DocType.DepartmentRegistryService) // Ведомственный реестр услуг (работ)
            {
                err = context.RegisterActivity_Activity.Where(r =>
                                                                  r.IdOwner == this.Id &&
                                                                  !indicators.Contains(r.Id));
            }
            else if (this.DocType.Id == DocType.StateRegistryService) // Реестр государственных (муниципальных) услуг по 210-ФЗ
            {
                err = context.RegisterActivity_Activity.Where(r =>
                                                                  r.IdOwner == this.Id &&
                                                                  r.IdRegistryKeyActivity != (byte)DbEnums.RegistryKeyActivity.Key2 &&
                                                                  !indicators.Contains(r.Id));
            }
            else
            {
                return;
            }

            if (err.Any())
            
            {
                Controls.Throw(string.Format(sMsg, LineRegistryActivityToString(err)));
            }
        }

        /// <summary>   
        /// Контроль "Проверка указания основной услуги"
        /// </summary> 
        public void Control_0404(DataContext context)
        {
            var sMsg = "Для следующих услуг требуется указать основную услугу:<br>{0}";

            if (this.DocType.Id != DocType.StateRegistryService) // Не Реестр государственных (муниципальных) услуг по 210-ФЗ
            {
                return;
            }

            var err = context.RegisterActivity_Activity
                             .Where(r =>
                                    r.IdOwner == this.Id &&
                                    r.IdRegistryKeyActivity == (byte)RegistryKeyActivity.Key2 &&
                                    !r.IdRegystryActivity_ActivityMain.HasValue);

            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg, LineRegistryActivityToString(err)));
            }
        }


        /// <summary>   
        /// метод преобразования набора строк ТЧ Мероприятия в строки вида «Тип» «Наименование» - контингент «Контингент» 
        /// </summary>  
        private static string LineRegistryActivityToString(IQueryable<RegisterActivity_Activity> err)
        {
              return err.ToList().Select(s =>
                              new
                                  {
                                      t = s.Activity.ActivityType.Caption(),
                                      a = "«" + s.Activity.Caption + "»",
                                      c = (s.IdContingent.HasValue ? " - контингент «" + s.Contingent.Caption + "»" : "")
                                  })
                      .ToList()
                      .Select(s => string.Format("{0} {1}{2}", s.t, s.a, s.c))
                      .Aggregate((a, b) => a + "<br>" + b);
        }


        /// <summary>   
        /// Контроль "Проверка указания контингента"
        /// </summary> 
        public void Control_0406(DataContext context)
        {
            var sMsg = "Для следующих мероприятий не указаны исполнители:<br>{0}";

            var Performers = context.RegisterActivity_Performers.Select(s => s.IdMaster).Distinct().ToArray();
 
            var err = context.RegisterActivity_Activity.Where(r =>
                                                              r.IdOwner == this.Id &&
															  !Performers.Contains(r.Id));
            if (err.Any())
            {
                Controls.Throw(string.Format(sMsg, LineRegistryActivityToString(err)));
            }
        }

        #endregion

        #region Методы операций

        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            DateLastEdit = DateTime.Now;
            if (IdDocType == DocType.StateRegistryService) // Реестр государственных (муниципальных) услуг по 210-ФЗ
            {
                IdSBP = null;
            }
            ExecuteControl(e => e.Control_0401(context));
        }

        /// <summary>   
        /// Операция «Редактировать»   
        /// </summary>  
        public void Edit(DataContext context)
        {
            DateLastEdit = DateTime.Now;
        }

        /// <summary>   
        /// Операция «Обработать»   
        /// </summary>  
        public void Process(DataContext context)
        {
            ReasonCancel = null;
            ExecuteControl(e => e.Control_0402(context)); 
            ExecuteControl(e => e.Control_0403(context));
            ExecuteControl(e => e.Control_0404(context));
            ExecuteControl(e => e.Control_0406(context));

            var prevDoc = GetPrevVersionDoc(context, this);
            if (prevDoc != null)
            {
                prevDoc.ExecuteOperation(e => e.Archive(context));
            }

        }
    
        /// <summary>
        /// Операция «Отменить обработку»
        /// </summary>  
        public void UndoProcess(DataContext context)
        {
            if (this.IdParent.HasValue)
            {
                var prevDoc = GetPrevVersionDoc(context, this);
                if (prevDoc != null)
                {
                    prevDoc.ExecuteOperation(e => e.UndoArchive(context));
                }
            }
        }
    
        /// <summary>   
        /// Операция «Утвердить»   
        /// </summary>  
        public void Confirm(DataContext context)
        {
            DateCommit = DateTime.Now;
            IsApproved = true;
        }

        /// <summary>   
        /// Операция «Отменить утверждение»   
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            DateCommit = null;
            IsApproved = false;
        }
    
        /// <summary>   
        /// Операция «Изменить»   
        /// </summary>  
        public void Change(DataContext context){
            Clone cloner = new Clone(this);
            RegisterActivity newDoc = (RegisterActivity)cloner.GetResult();
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.Date = DateTime.Now.Date;
            newDoc.IdParent = Id;
            newDoc.DateCommit = null;
            newDoc.DocType = this.DocType;
            newDoc.IsApproved = false;
            var ids = GetIdAllVersionDoc(context);
            var rootNum = context.RegisterActivity.Single(w => !w.IdParent.HasValue && ids.Contains(w.Id)).Number;
            newDoc.Number = rootNum + "." + ids.Count().ToString(CultureInfo.InvariantCulture);
            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();            
        }


        /// <summary>   
        /// Операция «Отменить изменение»   
        /// </summary>
        public void UndoChange(DataContext context)
        {
            var q = context.RegisterActivity.Where(w => w.IdParent == Id);
            foreach (var doc in q)
            {
                context.RegisterActivity.Remove(doc);
            }

            IdDocStatus = DateCommit.HasValue
                              ? DocStatus.Approved
                              : (!String.IsNullOrEmpty(ReasonCancel) ? DocStatus.Denied : DocStatus.Project);

            context.SaveChanges();
        }
    
        /// <summary>
        /// Операция «Отказать»
        /// </summary>  
        public void Deny(DataContext context)
        {
            if (string.IsNullOrEmpty(ReasonCancel))
            {
                Controls.Throw("Не заполнено поле 'Причина отказа'");
            }
        }   
 
        /// <summary>   
        /// Операция «Вернуть в проект»   
        /// </summary>  
        public void ReturnToProject(DataContext context)
        {
            ReasonCancel = null;
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

        /// <summary>
        /// Вернуть на черновик
        /// </summary>
        /// <param name="context"></param>
        public void BackToDraft(DataContext context)
        {
            UndoProcess(context);
        }

        #endregion

        #region Функции для работы с версиями

        /// <summary>   
        /// Кэш массива идентификаторов документов всех версий этого документа
        /// </summary>         
        private int[] _ids;

        /// <summary>   
        /// Получение массива идентификаторов документов всех версий этого документа, включая его самого
        /// </summary>         
        public int[] GetIdAllVersionDoc(DataContext context, bool isClearCache = false)
        {
            if (isClearCache || _ids == null)
            {
                var curdoc = this;
                List<int> tmp = new List<int>();
                while (curdoc != null)
                {
                    tmp.Add(curdoc.Id);
                    curdoc = GetPrevVersionDoc(context, curdoc);
                }
                _ids = tmp.ToArray();
            }

            return _ids;
        }

        /// <summary>   
        /// Получение предыдущей версии документа
        /// </summary>         
        public RegisterActivity GetPrevVersionDoc(DataContext context, RegisterActivity curdoc)
        {
            if (curdoc.IdParent.HasValue)
            {
                return
                    context.RegisterActivity.Where(w => w.Id == curdoc.IdParent).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        #endregion
    }
}

