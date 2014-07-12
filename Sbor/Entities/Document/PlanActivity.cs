using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using BaseApp;
using EntityFramework.Extensions;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Enums;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.Denormalizer;
using Platform.BusinessLogic.Denormalizer.Interfaces;
using Platform.BusinessLogic.Reference;
using Platform.BusinessLogic.Activity.Controls;
using Platform.Common;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.Utils;
using Sbor.CommonControls;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Logic.Hierarchy;
using Sbor.Reference;
using Sbor.Registry;
using Sbor.Tablepart;
using System.Linq;
using ValueType = Sbor.DbEnums.ValueType;

namespace Sbor.Document
{
    public partial class PlanActivity : IColumnFactoryForDenormalizedTablepart, IClarificationDoc, IDocStatusTerminate, IDocOfSbpBudget
    {
        #region Функции для сервисов

        public void FillData_ActivityAUBU(DataContext context)
        {
            var qReg2 = context.TaskVolume.Where(w =>
                                                 w.IdPublicLegalFormation == IdPublicLegalFormation
                                                 && w.IdBudget == IdBudget
                                                 && w.IdVersion == IdVersion
                                                 && !w.IdTerminator.HasValue
                                                 && w.SBP.IdParent == IdSBP
                                                 && w.IdValueType == (byte) ValueType.Plan
                                                 && w.HierarchyPeriod.Year >= Budget.Year
                                                 && w.HierarchyPeriod.Year <= Budget.Year + 2
                ).Join(
                    context.HierarchyPeriod.Where(w => (w.DateEnd.Month - w.DateStart.Month + 1) == 12),
                    a => a.HierarchyPeriod.Year, b => b.DateStart.Year,
                    (a, b) => new {Reg = a, IdHierarchyPeriod = b.Id}
                ).GroupBy(g => new
                    {
                        g.Reg.TaskCollection,
                        g.Reg.IndicatorActivity_Volume,
                        g.IdHierarchyPeriod
                    }).Select(s => new
                        {
                            s.Key,
                            Value = s.Sum(c => c.Reg.Value)
                        }).ToList();

            var qReg1 = qReg2.GroupBy(g => new
                {
                    g.Key.TaskCollection,
                    g.Key.IndicatorActivity_Volume
                }).Select(s => new
                    {
                        s.Key,
                        Value = s.Sum(c => c.Value)
                    }).ToList();

            var qTp1 = context.PlanActivity_ActivityAUBU.Where(w => w.IdOwner == Id).ToList();

            var newItem = qReg1.Where(w =>
                                      !qTp1.Any(a =>
                                                a.IdActivity == w.Key.TaskCollection.IdActivity
                                                && (a.IdContingent ?? 0) == (w.Key.TaskCollection.IdContingent ?? 0)
                                                && a.IdIndicatorActivity == w.Key.IndicatorActivity_Volume.Id
                                           )
                );
            foreach (var item in newItem)
            {
                context.PlanActivity_ActivityAUBU.Add(new PlanActivity_ActivityAUBU
                    {
                        IdOwner = Id,
                        IdActivity = item.Key.TaskCollection.IdActivity,
                        IdContingent = item.Key.TaskCollection.IdContingent,
                        IdIndicatorActivity = item.Key.IndicatorActivity_Volume.Id
                    });
            }

            var delItem = qTp1.Where(a =>
                                     !qReg1.Any(w =>
                                                a.IdActivity == w.Key.TaskCollection.IdActivity
                                                && (a.IdContingent ?? 0) == (w.Key.TaskCollection.IdContingent ?? 0)
                                                && a.IdIndicatorActivity == w.Key.IndicatorActivity_Volume.Id
                                          )
                );
            foreach (var item in delItem)
            {
                context.PlanActivity_ActivityAUBU.Remove(item);
            }

            context.SaveChanges();

            var qTp2 = context.PlanActivity_ActivityVolumeAUBU.Where(w => w.IdOwner == Id).ToList();

            var newItem2 = qReg2.Where(w =>
                                       !qTp2.Any(a =>
                                                 a.Master.IdActivity == w.Key.TaskCollection.IdActivity
                                                 &&
                                                 (a.Master.IdContingent ?? 0) ==
                                                 (w.Key.TaskCollection.IdContingent ?? 0)
                                                 && a.Master.IdIndicatorActivity == w.Key.IndicatorActivity_Volume.Id
                                                 && a.IdHierarchyPeriod == w.Key.IdHierarchyPeriod
                                                 && a.Volume == w.Value
                                            )
                ).Join(
                    context.PlanActivity_ActivityAUBU.Where(w => w.IdOwner == Id),
                    a =>
                    new
                        {
                            a.Key.TaskCollection.IdActivity,
                            IdContingent = a.Key.TaskCollection.IdContingent ?? 0,
                            IdIndicatorActivity = a.Key.IndicatorActivity_Volume.Id
                        },
                    b => new {b.IdActivity, IdContingent = b.IdContingent ?? 0, b.IdIndicatorActivity},
                    (a, b) => new {Reg = a, IdMaster = b.Id}
                );
            ;
            foreach (var item in newItem2)
            {
                context.PlanActivity_ActivityVolumeAUBU.Add(new PlanActivity_ActivityVolumeAUBU
                    {
                        IdOwner = Id,
                        IdMaster = item.IdMaster,
                        IdHierarchyPeriod = item.Reg.Key.IdHierarchyPeriod,
                        Volume = item.Reg.Value
                    });
            }

            var delItem2 = qTp2.Where(a =>
                                      !qReg2.Any(w =>
                                                 a.Master.IdActivity == w.Key.TaskCollection.IdActivity
                                                 &&
                                                 (a.Master.IdContingent ?? 0) ==
                                                 (w.Key.TaskCollection.IdContingent ?? 0)
                                                 && a.Master.IdIndicatorActivity == w.Key.IndicatorActivity_Volume.Id
                                                 && a.IdHierarchyPeriod == w.Key.IdHierarchyPeriod
                                                 && a.Volume == w.Value
                                           )
                );
            foreach (var item in delItem2)
            {
                context.PlanActivity_ActivityVolumeAUBU.Remove(item);
            }

            context.SaveChanges();
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
        public PlanActivity GetPrevVersionDoc(DataContext context, PlanActivity curdoc)
        {
            if (curdoc.IdParent.HasValue)
            {
                return
                    context.PlanActivity.Where(w => w.Id == curdoc.IdParent).FirstOrDefault();
            }
            else
            {
                return null;
            }
        }

        #endregion

 
        #region Контроли

        [ControlInitial(ExcludeFromSetup = true)]
        [Control(ControlType.Insert | ControlType.Update, Sequence.Before, ExecutionOrder = -1000)]
        public void AutoSet(DataContext context, ControlType ctType, PlanActivity old)
        {
            if (ctType == ControlType.Insert)
            {
                var sc =
                    context.PlanActivity.Where(
                        w => w.IdPublicLegalFormation == IdPublicLegalFormation && !w.IdParent.HasValue)
                           .Select(s => s.Number).Distinct().ToList();
                Number = CommonMethods.GetNextCode(sc);
            }

            if (
                ctType == ControlType.Insert
                || IdSBP != old.IdSBP
                ||
                (IdDocStatus == (byte) DocStatus.Draft && !IdDocAUBUPeriodType_OFG.HasValue &&
                 !IdDocAUBUPeriodType_PFG1.HasValue && !IdDocAUBUPeriodType_PFG2.HasValue)
                )
            {
                UpdateDocAUBUPeriodTypes(context, true);
            }

            Caption = this.ToString();
        }

        /// <summary>   
        /// Контроль "Проверка наличия бланка доведения"
        /// </summary>         
        [ControlInitial(InitialUNK = "0616", InitialCaption = "Проверка наличия бланка доведения")]
        public void Control_0616(DataContext context)
        {
            var btyp = (this.SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment
                            ? BlankType.BringingKU
                            : BlankType.BringingAUBU);
            
            var sbp = context.SBP.Single(a => a.Id == IdSBP).Parent;
            if (sbp == null) return;


            bool fail =
                !context.SBP_Blank.Any(w => w.IdBudget == IdBudget && w.IdOwner == sbp.Id && w.IdBlankType == (byte) btyp);

            if (fail)
                Controls.Throw(string.Format(
                    "В справочник «СБП» для СБП «{0}» необходимо добавить бланк «{1}» с бюджетом «{2}».",
                    sbp.Caption, btyp.Caption(), context.Budget.Single(s => s.Id == IdBudget).Caption
                                   ));
        }

        /// <summary>   
        /// Контроль "Проверка даты документа"
        /// </summary>         
        [ControlInitial(InitialUNK = "0602", InitialCaption = "Проверка даты документа")]
        public void Control_0602(DataContext context)
        {
            if (Parent != null && Parent.Date > Date)
                Controls.Throw(string.Format(
                    "Дата документа не может быть меньше даты предыдущей редакции.<br>Дата текущего документа: {0}<br>Дата предыдущей редакции: {1}",
                    Date.ToString("dd.MM.yyyy"),
                    Parent.Date.ToString("dd.MM.yyyy")
                                   ));
        }

        /// <summary>   
        /// Контроль "Наличие мероприятий в документе"
        /// </summary>         
        [ControlInitial(InitialUNK = "0605", InitialCaption = "Наличие мероприятий в документе")]
        public void Control_0605(DataContext context)
        {
            if (!context.PlanActivity_Activity.Any(a => a.IdOwner == Id))
                Controls.Throw("Не указано ни одно мероприятие, осуществляемое в рамках текущего документа.");
        }

        /// <summary>   
        /// Контроль "Наличие объемов у мероприятий"
        /// </summary>         
        [ControlInitial(InitialUNK = "0606", InitialCaption = "Наличие объемов у мероприятий")]
        public void Control_0606(DataContext context)
        {
            List<string> list = context.PlanActivity_Activity.Where(w =>
                                                                    w.IdOwner == Id &&
                                                                    !context.PlanActivity_ActivityVolume.Any(
                                                                        a => a.IdMaster == w.Id)
                ).Select(s =>
                         " - " + s.Activity.Caption + (s.Contingent == null ? "" : " - " + s.Contingent.Caption)
                ).OrderBy(o => o).ToList();

            List<string> list2 = context.PlanActivity_ActivityAUBU.Where(w =>
                                                                         w.IdOwner == Id &&
                                                                         !context.PlanActivity_ActivityVolumeAUBU.Any(
                                                                             a => a.IdMaster == w.Id)
                ).Select(s =>
                         " - " + s.Activity.Caption + (s.Contingent == null ? "" : " - " + s.Contingent.Caption)
                ).OrderBy(o => o).ToList();

            if (list2.Any())
            {
                list.Add((list.Any() ? "<br>" : "") + "Мероприятия АУ/БУ");
                list.AddRange(list2);
            }

            Controls.Check(list, "У следующих мероприятий не указаны объемы:<br>{0}");
        }

        /// <summary>   
        /// Контроль "Наличие мероприятий учреждения в программах ведомства"
        /// </summary>         
        [ControlInitial(InitialUNK = "0607", InitialCaption = "Наличие мероприятий учреждения в программах ведомства")]
        public void Control_0607(DataContext context)
        {
            if (PublicLegalFormation.UsedGMZ ?? false) return;

            var sbp = SBP;
            while (sbp.IdParent.HasValue)
            {
                if (sbp.SBPType == SBPType.GeneralManager || sbp.SBPType == SBPType.Manager) break;
                sbp = context.SBP.Single(s => s.Id == sbp.IdParent);
            }

            List<string> list = new List<string>();

            var q1 = context.PlanActivity_ActivityVolume.Where(w => w.IdOwner == Id).Select(s => new
                {
                    s.Master.IdActivity,
                    s.Master.IdContingent,
                    s.Master.IdIndicatorActivity,
                    s.HierarchyPeriod.Year,
                    CapActivity =
                                                                                                     " - " +
                                                                                                     s.Master.Activity
                                                                                                      .Caption +
                                                                                                     (s.Master
                                                                                                       .Contingent ==
                                                                                                      null
                                                                                                          ? ""
                                                                                                          : " - " +
                                                                                                            s.Master
                                                                                                             .Contingent
                                                                                                             .Caption),
                    CapIndicator = "   - " + s.Master.IndicatorActivity.Caption
                }).Where(w =>
                         !context.TaskVolume.Any(a =>
                                                 a.IdVersion == IdVersion
                                                 //&& a.IdBudget == IdBudget
                                                 //&& a.IdPublicLegalFormation == IdPublicLegalFormation
                                                 && !a.IdTerminator.HasValue
                                                 && a.TaskCollection.IdActivity == w.IdActivity
                                                 && (a.TaskCollection.IdContingent ?? 0) == (w.IdContingent ?? 0)
                                                 && a.IdIndicatorActivity_Volume == w.IdIndicatorActivity
                                                 && a.HierarchyPeriod.Year == w.Year
                                                 && a.IdSBP == sbp.Id
                              )
                ).GroupBy(g => g.CapActivity).OrderBy(o => o.Key).ToList();

            foreach (var ac in q1)
            {
                list.Add(ac.Key);
                foreach (var i in ac.GroupBy(g => g.CapIndicator).OrderBy(o => o))
                {
                    list.Add(i.Key + " - " +
                             i.Select(s => s.Year.ToString(CultureInfo.InvariantCulture))
                              .Distinct()
                              .Aggregate((a, b) => a + ", " + b) + " гг");
                }
            }

            var q2 = context.PlanActivity_ActivityVolumeAUBU.Where(w => w.IdOwner == Id).Select(s => new
                {
                    s.Master.IdActivity,
                    s.Master.IdContingent,
                    s.Master.IdIndicatorActivity,
                    s.HierarchyPeriod.Year,
                    CapActivity =
                                                                                                         " - " +
                                                                                                         s.Master
                                                                                                          .Activity
                                                                                                          .Caption +
                                                                                                         (s.Master
                                                                                                           .Contingent ==
                                                                                                          null
                                                                                                              ? ""
                                                                                                              : " - " +
                                                                                                                s.Master
                                                                                                                 .Contingent
                                                                                                                 .Caption),
                    CapIndicator = "   - " + s.Master.IndicatorActivity.Caption
                }).Where(w =>
                         !context.TaskVolume.Any(a =>
                                                 a.IdVersion == IdVersion
                                                 //&& a.IdBudget == IdBudget
                                                 //&& a.IdPublicLegalFormation == IdPublicLegalFormation
                                                 && !a.IdTerminator.HasValue
                                                 && a.TaskCollection.IdActivity == w.IdActivity
                                                 && (a.TaskCollection.IdContingent ?? 0) == (w.IdContingent ?? 0)
                                                 && a.IdIndicatorActivity_Volume == w.IdIndicatorActivity
                                                 && a.HierarchyPeriod.Year == w.Year
                                                 && a.IdSBP == sbp.Id
                              )
                ).GroupBy(g => g.CapActivity).OrderBy(o => o.Key).ToList();

            if (q2.Any())
                list.Add((list.Any() ? "<br>" : "") + "Мероприятия АУ/БУ");

            foreach (var ac in q2)
            {
                list.Add(ac.Key);
                foreach (var i in ac.GroupBy(g => g.CapIndicator).OrderBy(o => o))
                {
                    list.Add(i.Key + " - " +
                             i.Select(s => s.Year.ToString(CultureInfo.InvariantCulture))
                              .Distinct()
                              .Aggregate((a, b) => a + ", " + b) + " гг");
                }
            }

            Controls.Check(list, string.Format(
                "В документе имеются мероприятия, выполнение которых в указанные года не запланировано ни в одной действующей программе с версией «{0}»:<br>{{0}}",
                this.Version.Caption
                                     ));
        }

        /// <summary>   
        /// Контроль "Наличие показателей качества у мероприятия"
        /// </summary>         
        [ControlInitial(InitialUNK = "0609", InitialCaption = "Наличие показателей качества у мероприятия",
            InitialSkippable = true, InitialManaged = true)]
        public void Control_0609(DataContext context)
        {
            List<string> list = context.PlanActivity_Activity.Where(w =>
                                                                    w.IdOwner == Id &&
                                                                    !context.PlanActivity_IndicatorQualityActivity.Any(
                                                                        a => a.IdMaster == w.Id)
                ).Select(s =>
                         " - " + s.Activity.Caption + (s.Contingent == null ? "" : " - " + s.Contingent.Caption)
                ).OrderBy(o => o).ToList();

            Controls.Check(list, "У следующих мероприятий не указаны показатели качества:<br>{0}");
        }

        /// <summary>   
        /// Контроль "Наличие значений показателей качества у мероприятий"
        /// </summary>         
        [ControlInitial(InitialUNK = "0610", InitialCaption = "Наличие значений показателей качества у мероприятий")]
        public void Control_0610(DataContext context)
        {
            var q = context.PlanActivity_IndicatorQualityActivity.Where(w =>
                                                                        w.IdOwner == Id &&
                                                                        !context
                                                                             .PlanActivity_IndicatorQualityActivityValue
                                                                             .Any(a => a.IdMaster == w.Id)
                ).Select(s => new
                    {
                        CapActivity =
                                  s.Master.Activity.Caption +
                                  (s.Master.Contingent == null ? "" : " - " + s.Master.Contingent.Caption),
                        CapIndicator = " - " + s.IndicatorActivity.Caption
                    }).GroupBy(g => g.CapActivity);

            List<string> list = new List<string>();

            foreach (var a in q.OrderBy(o => o.Key))
            {
                list.Add(a.Key);
                list.AddRange(a.Select(i => i.CapIndicator).OrderBy(o => o));
            }

            Controls.Check(list, "У следующих мероприятий не указаны значения показателей качества:<br>{0}");
        }

        /// <summary>   
        /// Контроль "Наличие значений показателей качества за года без объема мероприятий"
        /// </summary>         
        [ControlInitial(InitialUNK = "0611",
            InitialCaption = "Наличие значений показателей качества за года без объема мероприятий")]
        public void Control_0611(DataContext context)
        {
            var q = context.PlanActivity_IndicatorQualityActivityValue.Where(w =>
                                                                             w.IdOwner == Id
                                                                             &&
                                                                             !context.PlanActivity_ActivityVolume.Any(
                                                                                 a =>
                                                                                 a.IdOwner == Id
                                                                                 && a.IdMaster == w.Master.IdMaster
                                                                                 &&
                                                                                 a.HierarchyPeriod.Year ==
                                                                                 w.HierarchyPeriod.Year
                                                                                  )
                ).Select(s => new
                    {
                        CapActivity =
                                  s.Master.Master.Activity.Caption +
                                  (s.Master.Master.Contingent == null ? "" : " - " + s.Master.Master.Contingent.Caption),
                        CapIndicator = " - " + s.Master.IndicatorActivity.Caption
                    }).GroupBy(g => g.CapActivity);


            List<string> list = new List<string>();

            foreach (var a in q.OrderBy(o => o.Key))
            {
                list.Add(a.Key);
                list.AddRange(a.Select(i => i.CapIndicator).OrderBy(o => o));
            }

            Controls.Check(list,
                           "У следующих показателей качества указаны значения за те года, на которые не был запланирован объем выполнения мероприятия:<br>{0}");
        }

        /// <summary>   
        /// Контроль "Наличие показателей качества учреждения в программах ведомства"
        /// </summary>         
        [ControlInitial(InitialUNK = "0612",
            InitialCaption = "Наличие показателей качества учреждения в программах ведомства")]
        public void Control_0612(DataContext context)
        {
            if (PublicLegalFormation.UsedGMZ ?? false) return;

            //первый СБП-предок с типом «ГРБС» или «РБС» для ШапкаДокумента.Учреждение
            int? nearestParentSBPId =
                context.SBP.NearestParentId(this.IdSBP, idS => idS.Id, idP => idP.IdParent,
                                            find => find.SBPType == SBPType.GeneralManager
                                                    || find.SBPType == SBPType.Manager
                    ).Key;

            var q = context.PlanActivity_IndicatorQualityActivityValue.Where(w => w.IdOwner == Id).Select(s => new
                {
                    s.Master.Master.IdActivity,
                    s.Master.Master.IdContingent,
                    s.Master.IdIndicatorActivity,
                    s.HierarchyPeriod.Year,
                    CapActivity =
                                                                                                                   s
                                                                                                                       .Master
                                                                                                                       .Master
                                                                                                                       .Activity
                                                                                                                       .Caption +
                                                                                                                   (s
                                                                                                                        .Master
                                                                                                                        .Master
                                                                                                                        .Contingent ==
                                                                                                                    null
                                                                                                                        ? ""
                                                                                                                        : " - " +
                                                                                                                          s
                                                                                                                              .Master
                                                                                                                              .Master
                                                                                                                              .Contingent
                                                                                                                              .Caption),
                    CapIndicator = "   - " + s.Master.IndicatorActivity.Caption
                }).Where(w =>
                         !context.TaskIndicatorQuality.Any(a =>
                                                           a.IdVersion == IdVersion
                                                           //&& a.IdBudget == IdBudget
                                                           //&& a.IdPublicLegalFormation == IdPublicLegalFormation
                                                           && !a.IdTerminator.HasValue
                                                           && a.TaskCollection.IdActivity == w.IdActivity
                                                           &&
                                                           (a.TaskCollection.IdContingent ?? 0) == (w.IdContingent ?? 0)
                                                           && a.IdIndicatorActivity_Quality == w.IdIndicatorActivity
                                                           && a.HierarchyPeriod.Year == w.Year
                                                           && a.IdSBP == nearestParentSBPId
                              )
                ).GroupBy(g => g.CapActivity).OrderBy(o => o.Key).ToList();

            List<string> list = new List<string>();

            foreach (var ac in q)
            {
                list.Add(ac.Key);
                foreach (var i in ac.GroupBy(g => g.CapIndicator).OrderBy(o => o.Key))
                {
                    list.Add(i.Key + " - " +
                             i.Select(s => s.Year.ToString(CultureInfo.InvariantCulture))
                              .Distinct()
                              .OrderBy(o => o)
                              .Aggregate((a, b) => a + ", " + b) + " гг");
                }
            }

            Controls.Check(list, string.Format(
                "В документе имеются показатели качества, достижение которых в указанные года не запланировано ни в одной действующей программе ведомства с версией «{0}»:<br>{{0}}",
                this.Version.Caption
                                     ));
        }

        private List<string> Common_0614(DataContext context, DateTime? dateDoc = null)
        {
            //первый СБП-предок с типом «ГРБС» или «РБС» для ШапкаДокумента.Учреждение
            SBP sbpTmp = SBP;
            SBP sbp = null;
            while (sbpTmp.IdParent.HasValue)
            {
                sbpTmp = context.SBP.Single(s => s.Id == sbpTmp.IdParent);
                if (sbpTmp.SBPType == SBPType.GeneralManager || sbpTmp.SBPType == SBPType.Manager)
                {
                    sbp = sbpTmp;
                    break;
                }
            }

            /*
             * Отобрать из всех строк ТЧ Мероприятия поля
             * Наименование + Контингент + Показатель объема.
             * К каждой такой строке добавить сумму объемов в разрезе годов (от периода взять только год) из связанной ТЧ «Объемы периодов»,
             * версию из шапки документа(версия итак известна не берём).           
             * Получим множество строк вида: Наименование + Контингент + Показатель объема + Год + Сумма объемов из документа + (Версия).
            */
            var qTp2 = context.PlanActivity_ActivityVolume.Where(w => w.IdOwner == Id).Select(s => new
                {
                    s.Master.IdActivity,
                    s.Master.IdContingent,
                    s.Master.IdIndicatorActivity,
                    s.HierarchyPeriod.Year,
                    CapActivity =
                                                                                                       " - " +
                                                                                                       s.Master.Activity
                                                                                                        .Caption +
                                                                                                       (s.Master
                                                                                                         .Contingent ==
                                                                                                        null
                                                                                                            ? ""
                                                                                                            : " - " +
                                                                                                              s.Master
                                                                                                               .Contingent
                                                                                                               .Caption),
                    CapIndicator = " - " + s.Master.IndicatorActivity.Caption,
                    DocValue = s.Volume,
                    PlanValue = (decimal) 0,
                    AlreadyValue = (decimal) 0
                });
            /*
            var qTp = context.PlanActivity_ActivityVolume.Where(w => w.IdOwner == Id).GroupBy(g => new
            {
                g.Master.IdActivity,                //ТЧ Мероприятия поля Наименование
                g.Master.IdContingent,              //ТЧ Мероприятия поля Контингент
                g.Master.IdIndicatorActivity,       //ТЧ Мероприятия поля Показатель объема
                g.HierarchyPeriod.Year,   //(от периода взять только год)
                CapActivity = " - " + g.Master.Activity.Caption + (g.Master.Contingent == null ? "" : " - " + g.Master.Contingent.Caption),
                CapIndicator = " - " + g.Master.IndicatorActivity.Caption
            }).Select(s => new
            {
                s.Key,
                DocValue = s.Sum(c => c.Volume ?? 0), //сумму объемов в разрезе годов
                PlanValue = (decimal)0,
                AlreadyValue = (decimal)0
            });
			*/
            /*
                Отобрать из регистра «Объемы задач» все записи, у которых
                    Аннулятор = пусто       +
                    Версия = Версия         +
                    ППО = ППО               +
                    Тип значения = План         +
                    Деятельность АУ/БУ = Ложь   +
                    Набор задач.Мероприятие = Наименование  +
                    Набор задач.Контингент = Контингент     +
                    Показатель объема = Показатель объема   +
                    Период.Год = Год                        +
                СБП = СБП, у которого родителем является первый СБП-предок с типом «ГРБС» или «РБС» для ШапкаДокумента.Учреждение, за исключением СБП = ШапкаДокумента.Учреждение

                Из полученных строк выбрать поля «Значения» и сложить их. Получим сумму «Уже распределено». Если не найдено ни одно строки, то считать Уже распределено = 0

                Далее найти в регистре «Объемы задач» строку, у которой:
                     ---*--*---
                СБП = первый СБП-предок с типом «ГРБС» или «РБС» для ШапкаДокумента.Учреждение 

                Из полученной строки выбрать поле «Значение». Получим сумму «План ведомства». Если не найдено ни одной строки, то считать План ведомства = 0

                Далее проверить:
                Разность = План ведомства – (Уже распределено + Сумма объемов из документа)

                Если Разность < 0, то действие не выполнять и выдать сообщение: 
            */
            if (sbp != null)
            {
                //находим всех потомков SBP для подсчёта суммы «Уже распределено»  (не только дочерних)
                //var sbpDesc = context.SBP.GetDescendantsIds(sbp.Id, idS => idS.Id, idP => idP.IdParent);
                List<int> sbpDesc = context.Database.SqlQuery<int>("select id from dbo.GetChildrens({0}, {1}, 1)",
                                                                   new object[] {sbp.Id, -2013265747}).Where(
                                                                       a => a != IdSBP).ToList();
                var qReg2 = context.TaskVolume.Where(tv =>
                                                     sbpDesc.Contains(tv.IdSBP)
                                                     && tv.IdVersion == IdVersion
                                                     && !tv.IdTerminator.HasValue //Аннулятор = пусто
                                                     && tv.IdValueType == (byte) ValueType.Plan //Тип значения = План
                                                     && !(tv.ActivityAUBU ?? false) //Деятельность АУ/БУ = Ложь
                                                     && (!dateDoc.HasValue || tv.DateCommit <= dateDoc)).
                                    Select(s => new
                                        {
                                            s.TaskCollection.IdActivity, //Набор задач.Мероприятие = Наименование
                                            s.TaskCollection.IdContingent, //Набор задач.Контингент = Контингент
                                            IdIndicatorActivity = s.IdIndicatorActivity_Volume,
                                            //Показатель объема = Показатель объема
                                            s.HierarchyPeriod.Year, //Период.Год = Год
                                            CapActivity =
                                                    " - " + s.TaskCollection.Activity.Caption +
                                                    (s.TaskCollection.Contingent == null
                                                         ? ""
                                                         : " - " + s.TaskCollection.Contingent.Caption),
                                            CapIndicator = " - " + s.IndicatorActivity_Volume.Caption,
                                            DocValue = (decimal) 0,
                                            PlanValue = s.IdSBP == sbp.Id ? s.Value : 0, //сумма «План ведомства»
                                            AlreadyValue = s.IdSBP != sbp.Id ? s.Value : 0 //сумма «Уже распределено»
                                        });
                qTp2 = qTp2.Concat(qReg2);

                /*
				var qReg = context.TaskVolume.Where(tv =>
					sbpDesc.Contains(tv.IdSBP)
					&& tv.IdVersion == IdVersion
					&& !tv.IdTerminator.HasValue                                //Аннулятор = пусто
                    && tv.IdValueType == (byte)ValueType.Plan                //Тип значения = План
                    && !(tv.ActivityAUBU ?? false)                           //Деятельность АУ/БУ = Ложь
                    && (!dateDoc.HasValue || tv.DateCommit <= dateDoc)
                ).GroupBy(g => new
                {
                    g.TaskCollection.IdActivity,                            //Набор задач.Мероприятие = Наименование
                    g.TaskCollection.IdContingent,                          //Набор задач.Контингент = Контингент
                    IdIndicatorActivity = g.IdIndicatorActivity_Volume,     //Показатель объема = Показатель объема
                    g.HierarchyPeriod.Year,                       //Период.Год = Год
                    CapActivity = " - " + g.TaskCollection.Activity.Caption + (g.TaskCollection.Contingent == null ? "" : " - " + g.TaskCollection.Contingent.Caption),
                    CapIndicator = " - " + g.IndicatorActivity_Volume.Caption
                }).Select(b => new
                {
                    b.Key,
                    DocValue = (decimal)0,
                    PlanValue =     b.Sum(c => c.IdSBP == sbp.Id ? c.Value : 0),    //сумма «План ведомства»
                    AlreadyValue =  b.Sum(c => c.IdSBP != sbp.Id ? c.Value : 0)     //сумма «Уже распределено»
                });

				qTp = qTp.Concat(qReg);
				*/
            }

            var tmp12 = qTp2.GroupBy(g => new
                {
                    g.IdActivity,
                    g.IdContingent,
                    g.IdIndicatorActivity,
                    g.Year,
                    g.CapActivity,
                    g.CapIndicator
                }).Select(s => new
                    {
                        s.Key,
                        DocValue = s.Sum(c => c.DocValue),
                        PlanValue = s.Sum(c => c.PlanValue),
                        AlreadyValue = s.Sum(c => c.AlreadyValue)
                    });
            /*
			var tmp1 = qTp.GroupBy(g => g.Key).Select(s => new
                {
                    s.Key,
                    DocValue = s.Sum(c => c.DocValue),
                    PlanValue = s.Sum(c => c.PlanValue),
                    AlreadyValue = s.Sum(c => c.AlreadyValue)
                });
	            
            var tmp2= tmp1.Where(k =>
                         k.DocValue != 0 && (k.PlanValue - k.AlreadyValue - k.DocValue) < 0
                ).ToList();*/
            var tmp22 = tmp12.Where(k =>
                                    k.DocValue != 0 && (k.PlanValue - k.AlreadyValue - k.DocValue) < 0
                ).ToList();

            List<string> list = tmp22.Select(q =>
                                             "<b>" + q.Key.CapActivity + "</b><br>" + q.Key.Year.ToString() + " г"
                                             + "<br>План ведомства = " + q.PlanValue.ToString("0.#####")
                                             + "<br>Уже распределено = " + q.AlreadyValue.ToString("0.#####")
                                             + "<br>Остаток = " + (q.PlanValue - q.AlreadyValue).ToString("0.#####")
                                             + "<br>Объем из документа = " + q.DocValue.ToString("0.#####")
                                             + "<br>Разность = " +
                                             (q.PlanValue - q.AlreadyValue - q.DocValue).ToString("0.#####")
                                             + "<br>"
                ).ToList();

            return list;
        }

        /// <summary>   
        /// Контроль "Распределение объемов мероприятий из программ по учреждениям"
        /// </summary>         
        [ControlInitial(InitialUNK = "0614",
            InitialCaption = "Распределение объемов мероприятий из программ по учреждениям", InitialManaged = true)]
        public void Control_0614(DataContext context)
        {
            if (PublicLegalFormation.UsedGMZ ?? false) return;

            List<string> list = Common_0614(context);

            Controls.Check(list,
                           "У следующих мероприятий указан объем, который превышает нераспределенный остаток объема по ведомству:<br>{0}");
        }

        /// <summary>   
        /// Контроль "Наличие строк финансового обеспечения"
        /// </summary>         
        [ControlInitial(InitialUNK = "0617", InitialCaption = "Наличие строк финансового обеспечения",
            InitialManaged = true)]
        public void Control_0617(DataContext context)
        {
            if (!context.PlanActivity_KBKOfFinancialProvision.Any(a => a.IdOwner == Id))
                Controls.Throw("В документе не указаны объемы финансового обеспечения.");
        }

        private List<PlanActivity_KBKOfFinancialProvision> _linesKbk = null;

        private List<PlanActivity_KBKOfFinancialProvision> GetLinesKbk(DataContext context)
        {
            if (_linesKbk == null)
            {
                _linesKbk = context.PlanActivity_KBKOfFinancialProvision.Where(w => w.IdOwner == Id).ToList();
            }
            return _linesKbk;
        }

        /// <summary>   
        /// Контроль "Проверка заполнения строк фин. обеспечения в соответствии с бланком доведения"
        /// </summary>         
        [ControlInitial(InitialUNK = "0618",
            InitialCaption = "Проверка заполнения строк фин. обеспечения в соответствии с бланком доведения")]
        public void Control_0618(DataContext context)
        {
            var blank = context.SBP_Blank.Single(s => s.IdBudget == IdBudget && (
                                                                                    (SBP.IdSBPType ==
                                                                                     (byte)
                                                                                     SBPType.TreasuryEstablishment &&
                                                                                     s.IdOwner == SBP.IdParent &&
                                                                                     s.IdBlankType ==
                                                                                     (byte) BlankType.BringingKU)
                                                                                    ||
                                                                                    (SBP.IdSBPType ==
                                                                                     (byte)
                                                                                     SBPType.IndependentEstablishment &&
                                                                                     s.IdOwner == SBP.IdParent &&
                                                                                     s.IdBlankType ==
                                                                                     (byte) BlankType.BringingAUBU)
                                                                                    ||
                                                                                    (SBP.IdSBPType ==
                                                                                     (byte) SBPType.BudgetEstablishment &&
                                                                                     s.IdOwner == SBP.IdParent &&
                                                                                     s.IdBlankType ==
                                                                                     (byte) BlankType.BringingAUBU)
                                                                                ));

            var lines = GetLinesKbk(context);
            if (lines.Any(limit => !blank.CheckByBlank(limit)))
                Controls.Throw(string.Format(
                    "В таблице «Финансовое обеспечение» указаны строки, не соответствующие бланку «{0}».",
                    blank.BlankType.Caption()
                                   ));
        }

        // здесь хотели универсальную функцию, но получилась "затычка"
        private static string LineCostToString(object line, List<string> boldFields, string strFormat,
                                               string delim = ", ")
        {
            List<string> repList = new List<string>();

            Match m = Regex.Match(strFormat, @"\[[^\[\]]+\]");
            while (m.Success)
            {
                repList.Add(m.Value);
                m = m.NextMatch();
            }

            string res = string.Empty; //strFormat;

            foreach (var rep in repList)
            {
                string res2 = rep.Replace("[", "").Replace("]", "");

                Match m2 = Regex.Match(res2, @"\{[A-z_0-9.]+\}");
                while (m2.Success)
                {
                    object o = line;

                    string val = m2.Value.Replace("{", "").Replace("}", "");
                    string[] vals = val.Split('.');
                    foreach (var vn in vals)
                    {
                        o = o.GetValue(vn);
                        if (o == null) break;
                    }

                    if (o == null)
                    {
                        res2 = string.Empty;
                    }
                    else
                    {
                        if (boldFields.Contains(vals[0]))
                            res2 = "<b>" + res2 + "</b>";
                        res2 = res2.Replace(m2.Value, o.ToString());
                    }

                    m2 = m2.NextMatch();
                }

                //res = res.Replace(rep, res2);
                if (!string.IsNullOrEmpty(res2)) res += (string.IsNullOrEmpty(res) ? "" : delim) + res2;
            }

            return res;
        }

        /// <summary>   
        /// Контроль "Проверка заполнения строк действующими КБК"
        /// </summary>         
        [ControlInitial(InitialUNK = "0619", InitialCaption = "Проверка заполнения строк действующими КБК")]
        public void Control_0619(DataContext context)
        {
            var verFields = context.EntityField.Where(w =>
                                                      w.IdEntity == PlanActivity_KBKOfFinancialProvision.EntityIdStatic
                                                      &&
                                                      context.Entity.Any(a => a.Id == w.IdEntityLink && a.IsVersioning)
                ).ToList();

            List<string> list = new List<string>();

            var lines = GetLinesKbk(context);
            foreach (var line in lines)
            {
                List<string> errFields = new List<string>();
                foreach (var f in verFields)
                {
                    string fn = f.Name.Substring(2);
                    IVersioning ver = (IVersioning) line.GetValue(fn);
                    if (ver != null)
                    {
                        if ((ver.ValidityFrom ?? DateTime.MinValue) > Date ||
                            (ver.ValidityTo ?? DateTime.MaxValue) < Date)
                            errFields.Add(fn);
                    }
                }
                if (errFields.Any())
                {
                    //  это все страшная затычка временно, нужно переделать!
                    string typRo = (
                                       !line.IdExpenseObligationType.HasValue
                                           ? ""
                                           : (line.IdExpenseObligationType == (byte) ExpenseObligationType.Accepted
                                                  ? ExpenseObligationType.Accepted.Caption()
                                                  : (line.IdExpenseObligationType ==
                                                     (byte) ExpenseObligationType.Existing
                                                         ? ExpenseObligationType.Existing.Caption()
                                                         : "?")
                                             )
                                   );
                    list.Add(LineCostToString(line, errFields,
                                              " - " + (string.IsNullOrEmpty(typRo) ? "" : "[Тип РО " + typRo + "], ") +
                                              // затычка
                                              "[ИФ {FinanceSource.Code}], [КФО {KFO.Code}], [КВСР {KVSR.Caption}], [РзПР {RZPR.Code}], [КЦСР {KCSR.Code}], " +
                                              "[КВР {KVR.Code}], [КОСГУ {KOSGU.Code}], [ДФК {DFK.Code}], [ДКР {DKR.Code}], [ДЭК {DEK.Code}], [Код субсидии {CodeSubsidy.Code}] [Отраслевой код {BranchCode.Code}]"
                                 ));
                }
            }

            Controls.Check(list,
                           "В таблице «Финансовое обеспечение» указаны строки с недействующими КБК (выделены жирным шрифтом):<br>{0}");
        }

        /// <summary>   
        /// Контроль "Проверка кодов БК в строках со средствами АУ/БУ"
        /// </summary>         
        [ControlInitial(InitialUNK = "0620", InitialCaption = "Проверка кодов БК в строках со средствами АУ/БУ")]
        public void Control_0620(DataContext context)
        {
            if (SBP.IdSBPType != (byte) SBPType.TreasuryEstablishment || !SBP.IsFounder)
                return;

            var blank = context.SBP_Blank.Single(s =>
                                                 s.IdBudget == IdBudget
                                                 && s.IdOwner == SBP.IdParent
                                                 && s.IdBlankType == (byte) BlankType.BringingKU
                );

            bool isTestKosgu = blank.IdBlankValueType_KOSGU.HasValue &&
                               blank.IdBlankValueType_KOSGU != (byte) BlankValueType.Empty;
            bool isTestKfo = blank.IdBlankValueType_KFO.HasValue &&
                             blank.IdBlankValueType_KFO != (byte) BlankValueType.Empty;

            if (isTestKfo || isTestKosgu)
            {
                List<string> list = new List<string>();

                var lines = GetLinesKbk(context);

                foreach (var line in lines.Where(w => w.IsMeansAUBU))
                {
                    if (isTestKosgu)
                    {
                        var kosgu = line.KOSGU != null ? line.KOSGU.Code.Replace(".", "") : "";
                        if (kosgu != "530" || kosgu != "241")
                        {
                            list.Add(" - КОСГУ 241 или 530<br>");
                            isTestKosgu = false;
                        }
                    }

                    if (isTestKfo)
                    {
                        var kfo = line.KFO != null ? line.KFO.Code.Replace(".", "") : "";
                        if (kfo != "1")
                        {
                            list.Add(" - КФО 1<br>");
                            isTestKfo = false;
                        }
                    }

                    if (!isTestKfo && !isTestKosgu) break;
                }

                Controls.Check(list,
                               "В строках таблицы «Финансовое обеспечение» указан неверный КБК. Средства, предназначенные для субсидий АУ/БУ, могут быть доведены до учредителя только со следующими кодами:<br>{0}");
            }
        }

        /// <summary>   
        /// Контроль "Проверка кодов БК в строках со средствами АУ/БУ"
        /// </summary>         
        [ControlInitial(InitialUNK = "0621", InitialCaption = "Проверка кодов БК в строках со средствами АУ/БУ")]
        public void Control_0621(DataContext context)
        {
            var q = context.PlanActivity_KBKOfFinancialProvision.Where(w =>
                                                                       w.IdOwner == Id &&
                                                                       !context.PlanActivity_PeriodsOfFinancialProvision
                                                                               .Any(
                                                                                   a =>
                                                                                   a.IdMaster == w.Id &&
                                                                                   (a.Value ?? 0) > 0)
                );

            if (q.Any())
                Controls.Throw(
                    "В таблице «Финансовое обеспечение» имеются строки без сумм. В каждой строке должна быть хотя бы одна сумма.");
        }

        private class ResultKeyRLVA
        {
            public byte? ExpenseObligationType { get; set; }
            public string FinanceSource { get; set; }
            public string KFO { get; set; }
            public string KVSR { get; set; }
            public string RZPR { get; set; }
            public string KCSR { get; set; }
            public string KVR { get; set; }
            public string KOSGU { get; set; }
            public string DFK { get; set; }
            public string DKR { get; set; }
            public string DEK { get; set; }
            public string CodeSubsidy { get; set; }
            public string BranchCode { get; set; }
            public int Year;

            public ResultKeyRLVA()
            {
            }

            public ResultKeyRLVA(ResultKeyRLVA old, string newKosgu)
            {
                ExpenseObligationType = old.ExpenseObligationType;
                FinanceSource = old.FinanceSource;
                KFO = old.KFO;
                KVSR = old.KVSR;
                RZPR = old.RZPR;
                KCSR = old.KCSR;
                KVR = old.KVR;
                KOSGU = string.IsNullOrEmpty(newKosgu) ? old.KOSGU : newKosgu;
                DFK = old.DFK;
                DKR = old.DKR;
                DEK = old.DEK;
                CodeSubsidy = old.CodeSubsidy;
                BranchCode = old.BranchCode;
                Year = old.Year;
            }
        }

        private class ResultGroupByBlank
        {
            public byte? ExpenseObligationType { get; set; }
            public string FinanceSource { get; set; }
            public string KFO { get; set; }
            public string KVSR { get; set; }
            public string RZPR { get; set; }
            public string KCSR { get; set; }
            public string KVR { get; set; }
            public string KOSGU { get; set; }
            public string DFK { get; set; }
            public string DKR { get; set; }
            public string DEK { get; set; }
            public string CodeSubsidy { get; set; }
            public string BranchCode { get; set; }
            public int Year { get; set; }
            public decimal? DocValue { get; set; }
            public decimal? RegValue1 { get; set; }
            public decimal? RegValue2 { get; set; }
        }

        private class ResultRLVA
        {
            public ResultKeyRLVA Key;
            public decimal? DocValue;
            public decimal? RegValue1;
            public decimal? RegValue2;
            public List<ResultRLVA> childRec;

            public string Caption()
            {
                //  это все страшная затычка временно, нужно переделать!
                string typRo = (
                                   !Key.ExpenseObligationType.HasValue
                                       ? ""
                                       : (Key.ExpenseObligationType == (byte) ExpenseObligationType.Accepted
                                              ? ExpenseObligationType.Accepted.Caption()
                                              : (Key.ExpenseObligationType == (byte) ExpenseObligationType.Existing
                                                     ? ExpenseObligationType.Existing.Caption()
                                                     : "?")
                                         )
                               );
                return
                    LineCostToString(Key, new List<string>(),
                                     (string.IsNullOrEmpty(typRo) ? "" : "[Тип РО " + typRo + "], ") + //затычка
                                     "[ИФ {FinanceSource}], [КФО {KFO}], [КВСР {KVSR}], [РзПР {RZPR}], [КЦСР {KCSR}], " +
                                     "[КВР {KVR}], [КОСГУ {KOSGU}], [ДФК {DFK}], [ДКР {DKR}], [ДЭК {DEK}], [Код субсидии {CodeSubsidy}] [Отраслевой код {BranchCode}]"
                        );
            }
        }

        private List<ResultRLVA> GetData_CheckForExcessMeans2(DataContext context, SBP_Blank blank, 
                                                              bool ForAdditionalNeed, bool? isMeanAUBU,
                                                              byte? idValType1, int? idSbp1, bool? isMeanAUBU1,
                                                              byte? idValType2, int? idSbp2, bool? isMeanAUBU2,
                                                              DateTime? dateDoc = null)
        {
            const string textCommand1 = @"
declare @allDocs table (id int);

with cte as (
select id, idParent from doc.PlanActivity where id={2}
union all
select a.id, a.idParent from doc.PlanActivity a inner join cte b on a.id=b.idParent
),
cte2 as (
select id, idParent from doc.PlanActivity where id={2}
union all
select a.id, a.idParent from doc.PlanActivity a inner join cte2 b on a.idParent=b.id
)
insert into @allDocs
select id from cte
union
select id from cte2;

with fromDoc as (
select 
	case when {0}=1 then b.idExpenseObligationType else null end as ExpenseObligationType,
	case when {11}=1 then b.idFinanceSource else null end as idFinanceSource,
	case when {12}=1 then b.idKFO else null end as idKFO,
	case when {13}=1 then b.idKVSR else null end as idKVSR,
	case when {14}=1 then b.idRZPR else null end as idRZPR,
	case when {15}=1 then b.idKCSR else null end as idKCSR,
	case when {16}=1 then b.idKVR else null end as idKVR,
	case when {17}=1 then b.idKOSGU else null end as idKOSGU,
	case when {18}=1 then b.idDFK else null end as idDFK,
	case when {19}=1 then b.idDKR else null end as idDKR,
	case when {20}=1 then b.idDEK else null end as idDEK,
	case when {21}=1 then b.idCodeSubsidy else null end as idCodeSubsidy,
	case when {22}=1 then b.idBranchCode else null end as idBranchCode,
	year(c.DateStart) as [Year],
	a.Value as DocValue, 
	0.00 as RegValue1, 
	0.00 as RegValue2
from tp.PlanActivity_PeriodsOfFinancialProvision a
	inner join tp.PlanActivity_KBKOfFinancialProvision b on b.id=a.idMaster
    inner join ref.FinanceSource fs on fs.id = b.idFinanceSource 
	inner join ref.HierarchyPeriod c on c.id=a.idHierarchyPeriod
where 
    a.idOwner={2} 
    and ({1} IS NULL or b.isMeansAUBU={1})
    and fs.idFinanceSourceType <> {23} 
), fromReg as (
select 
	case when {0}=1 then c.idExpenseObligationType else null end as ExpenseObligationType,
	case when {11}=1 then c.idFinanceSource else null end as idFinanceSource,
	case when {12}=1 then c.idKFO else null end as idKFO,
	case when {13}=1 then c.idKVSR else null end as idKVSR,
	case when {14}=1 then c.idRZPR else null end as idRZPR,
	case when {15}=1 then c.idKCSR else null end as idKCSR,
	case when {16}=1 then c.idKVR else null end as idKVR,
	case when {17}=1 then c.idKOSGU else null end as idKOSGU,
	case when {18}=1 then c.idDFK else null end as idDFK,
	case when {19}=1 then c.idDKR else null end as idDKR,
	case when {20}=1 then c.idDEK else null end as idDEK,
	case when {21}=1 then c.idCodeSubsidy else null end as idCodeSubsidy,
	case when {22}=1 then c.idBranchCode else null end as idBranchCode,
	year(e.DateStart) as [Year],
	0.00 as DocValue,
	case when a.idValueType={3} and c.idSBP={5} then a.Value else 0.00 end as RegValue1,
	case when a.idValueType={4} and c.idSBP={6} then a.Value else 0.00 end as RegValue2
 from reg.LimitVolumeAppropriations a
	inner join reg.EstimatedLine c on c.id=a.idEstimatedLine 
    inner join ref.FinanceSource fs on fs.id = c.idFinanceSource 
	left outer join @allDocs b on a.idRegistratorEntity=-2013265436 and a.idRegistrator=b.id
	inner join ref.HierarchyPeriod e on e.id=a.idHierarchyPeriod
where a.idVersion={7}
and IsNull(a.HasAdditionalNeed,0) = 0
and fs.idFinanceSourceType <> {23} 
and ((c.idSbp={5} AND ({8} IS NULL or a.isMeansAUBU={8}))
	OR (c.idSbp={6} AND ({9} IS NULL or a.isMeansAUBU={9})))
and ({10} IS NULL OR a.DateCommit<={10}) and b.id is null
), 
fromAll as (
select ExpenseObligationType, idFinanceSource, idKFO, idKVSR, idRZPR, idKCSR, idKVR, idKOSGU, idDFK, idDKR, idDEK, idCodeSubsidy, idBranchCode, [Year], DocValue, RegValue1, RegValue2
from fromDoc
union all
select ExpenseObligationType, idFinanceSource, idKFO, idKVSR, idRZPR, idKCSR, idKVR, idKOSGU, idDFK, idDKR, idDEK, idCodeSubsidy, idBranchCode, [Year], DocValue, RegValue1, RegValue2
from fromReg
), 
cte2 as (
select ExpenseObligationType, idFinanceSource, idKFO, idKVSR, idRZPR, idKCSR, idKVR, idKOSGU, idDFK, idDKR, idDEK, idCodeSubsidy, idBranchCode, [Year], SUM(DocValue) as DocValue, SUM(RegValue1) as RegValue1, SUM(RegValue2) as RegValue2
from fromAll
group by ExpenseObligationType, idFinanceSource, idKFO, idKVSR, idRZPR, idKCSR, idKVR, idKOSGU, idDFK, idDKR, idDEK, idCodeSubsidy, idBranchCode, [Year]
)
select a.ExpenseObligationType,
	b1.Code as FinanceSource,
	b2.Code as KFO,
	b3.Caption as KVSR,
	b4.Code as RZPR,
	b5.Code as KCSR,
	b6.Code as KVR,
	b7.Code as KOSGU,
	b8.Code as DFK,
	b9.Code as DKR,
	b10.Code as DEK,
	b11.Code as CodeSubsidy,
	b12.Code as BranchCode,
	[Year],
	a.DocValue,
	a.RegValue1,
	a.RegValue2
 from cte2 a
	left outer join ref.FinanceSource b1 on b1.id=a.idFinanceSource
	left outer join ref.KFO b2 on b2.id=a.idKFO
	left outer join ref.KVSR b3 on b3.id=a.idKVSR
	left outer join ref.RZPR b4 on b4.id=a.idRZPR
	left outer join ref.KCSR b5 on b4.id=a.idKCSR
	left outer join ref.KVR b6 on b4.id=a.idKVR
	left outer join ref.KOSGU b7 on b4.id=a.idKOSGU
	left outer join ref.DFK b8 on b4.id=a.idDFK
	left outer join ref.DKR b9 on b4.id=a.idDKR
	left outer join ref.DEK b10 on b4.id=a.idDEK
	left outer join ref.CodeSubsidy b11 on b4.id=a.idCodeSubsidy
	left outer join ref.BranchCode b12 on b4.id=a.idBranchCode
";

            const string textCommand2 = @"
declare @allDocs table (id int);

with cte as (
select id, idParent from doc.PlanActivity where id={2}
union all
select a.id, a.idParent from doc.PlanActivity a inner join cte b on a.id=b.idParent
),
cte2 as (
select id, idParent from doc.PlanActivity where id={2}
union all
select a.id, a.idParent from doc.PlanActivity a inner join cte2 b on a.idParent=b.id
)
insert into @allDocs
select id from cte
union
select id from cte2;

with fromDoc as (
select 
	case when {0}=1 then b.idExpenseObligationType else null end as ExpenseObligationType,
	case when {11}=1 then b.idFinanceSource else null end as idFinanceSource,
	case when {12}=1 then b.idKFO else null end as idKFO,
	case when {13}=1 then b.idKVSR else null end as idKVSR,
	case when {14}=1 then b.idRZPR else null end as idRZPR,
	case when {15}=1 then b.idKCSR else null end as idKCSR,
	case when {16}=1 then b.idKVR else null end as idKVR,
	case when {17}=1 then b.idKOSGU else null end as idKOSGU,
	case when {18}=1 then b.idDFK else null end as idDFK,
	case when {19}=1 then b.idDKR else null end as idDKR,
	case when {20}=1 then b.idDEK else null end as idDEK,
	case when {21}=1 then b.idCodeSubsidy else null end as idCodeSubsidy,
	case when {22}=1 then b.idBranchCode else null end as idBranchCode,
	year(c.DateStart) as [Year],
	a.AdditionalValue as DocValue, 
	0.00 as RegValue1, 
	0.00 as RegValue2
from tp.PlanActivity_PeriodsOfFinancialProvision a
	inner join tp.PlanActivity_KBKOfFinancialProvision b on b.id=a.idMaster
	inner join ref.HierarchyPeriod c on c.id=a.idHierarchyPeriod
where a.idOwner={2} and ({1} IS NULL or b.isMeansAUBU={1})
), fromReg as (
select 
	case when {0}=1 then c.idExpenseObligationType else null end as ExpenseObligationType,
	case when {11}=1 then c.idFinanceSource else null end as idFinanceSource,
	case when {12}=1 then c.idKFO else null end as idKFO,
	case when {13}=1 then c.idKVSR else null end as idKVSR,
	case when {14}=1 then c.idRZPR else null end as idRZPR,
	case when {15}=1 then c.idKCSR else null end as idKCSR,
	case when {16}=1 then c.idKVR else null end as idKVR,
	case when {17}=1 then c.idKOSGU else null end as idKOSGU,
	case when {18}=1 then c.idDFK else null end as idDFK,
	case when {19}=1 then c.idDKR else null end as idDKR,
	case when {20}=1 then c.idDEK else null end as idDEK,
	case when {21}=1 then c.idCodeSubsidy else null end as idCodeSubsidy,
	case when {22}=1 then c.idBranchCode else null end as idBranchCode,
	year(e.DateStart) as [Year],
	0.00 as DocValue,
	case when a.idValueType={3} and c.idSBP={5} then a.Value else 0.00 end as RegValue1,
	case when a.idValueType={4} and c.idSBP={6} then a.Value else 0.00 end as RegValue2
 from reg.LimitVolumeAppropriations a
	inner join reg.EstimatedLine c on c.id=a.idEstimatedLine 
    inner join ref.FinanceSource fs on fs.id = c.idFinanceSource 
	left outer join @allDocs b on a.idRegistratorEntity=-2013265436 and a.idRegistrator=b.id
	inner join ref.HierarchyPeriod e on e.id=a.idHierarchyPeriod
where a.idVersion={7}
and IsNull(a.HasAdditionalNeed,0) = 1
and fs.idFinanceSourceType <> {23} 
and ((c.idSbp={5} AND ({8} IS NULL or a.isMeansAUBU={8}))
	OR (c.idSbp={6} AND ({9} IS NULL or a.isMeansAUBU={9})))
and ({10} IS NULL OR a.DateCommit<={10}) and b.id is null
), 
fromAll as (
select ExpenseObligationType, idFinanceSource, idKFO, idKVSR, idRZPR, idKCSR, idKVR, idKOSGU, idDFK, idDKR, idDEK, idCodeSubsidy, idBranchCode, [Year], DocValue, RegValue1, RegValue2
from fromDoc
union all
select ExpenseObligationType, idFinanceSource, idKFO, idKVSR, idRZPR, idKCSR, idKVR, idKOSGU, idDFK, idDKR, idDEK, idCodeSubsidy, idBranchCode, [Year], DocValue, RegValue1, RegValue2
from fromReg
), 
cte2 as (
select ExpenseObligationType, idFinanceSource, idKFO, idKVSR, idRZPR, idKCSR, idKVR, idKOSGU, idDFK, idDKR, idDEK, idCodeSubsidy, idBranchCode, [Year], SUM(DocValue) as DocValue, SUM(RegValue1) as RegValue1, SUM(RegValue2) as RegValue2
from fromAll
group by ExpenseObligationType, idFinanceSource, idKFO, idKVSR, idRZPR, idKCSR, idKVR, idKOSGU, idDFK, idDKR, idDEK, idCodeSubsidy, idBranchCode, [Year]
)
select a.ExpenseObligationType,
	b1.Code as FinanceSource,
	b2.Code as KFO,
	b3.Caption as KVSR,
	b4.Code as RZPR,
	b5.Code as KCSR,
	b6.Code as KVR,
	b7.Code as KOSGU,
	b8.Code as DFK,
	b9.Code as DKR,
	b10.Code as DEK,
	b11.Code as CodeSubsidy,
	b12.Code as BranchCode,
	[Year],
	a.DocValue,
	a.RegValue1,
	a.RegValue2
 from cte2 a
	left outer join ref.FinanceSource b1 on b1.id=a.idFinanceSource
	left outer join ref.KFO b2 on b2.id=a.idKFO
	left outer join ref.KVSR b3 on b3.id=a.idKVSR
	left outer join ref.RZPR b4 on b4.id=a.idRZPR
	left outer join ref.KCSR b5 on b4.id=a.idKCSR
	left outer join ref.KVR b6 on b4.id=a.idKVR
	left outer join ref.KOSGU b7 on b4.id=a.idKOSGU
	left outer join ref.DFK b8 on b4.id=a.idDFK
	left outer join ref.DKR b9 on b4.id=a.idDKR
	left outer join ref.DEK b10 on b4.id=a.idDEK
	left outer join ref.CodeSubsidy b11 on b4.id=a.idCodeSubsidy
	left outer join ref.BranchCode b12 on b4.id=a.idBranchCode
";
            List<ResultGroupByBlank> resultQuery =
                context.Database.SqlQuery<ResultGroupByBlank>(
                    ForAdditionalNeed ? textCommand2 : textCommand1,
                    new object[]
                        {
                            blank.IdBlankValueType_ExpenseObligationType,
                            isMeanAUBU, Id, idValType1, idValType2, idSbp1,
                            idSbp2,
                            IdVersion, isMeanAUBU1, isMeanAUBU2, dateDoc,
                            blank.IdBlankValueType_FinanceSource,
                            blank.IdBlankValueType_KFO,
                            blank.IdBlankValueType_KVSR,
                            blank.IdBlankValueType_RZPR,
                            blank.IdBlankValueType_KCSR,
                            blank.IdBlankValueType_KVR,
                            blank.IdBlankValueType_KOSGU,
                            blank.IdBlankValueType_DFK,
                            blank.IdBlankValueType_DKR,
                            blank.IdBlankValueType_DEK,
                            blank.IdBlankValueType_CodeSubsidy,
                            blank.IdBlankValueType_BranchCode,
                            (byte) FinanceSourceType.UnconfirmedFunds
                        }).ToList();
            List<ResultRLVA> result = resultQuery.GroupBy(g => new ResultKeyRLVA
                {
                    ExpenseObligationType = g.ExpenseObligationType,
                    FinanceSource = g.FinanceSource,
                    KFO = g.KFO,
                    KVSR = g.KVSR,
                    RZPR = g.RZPR,
                    KCSR = g.KCSR,
                    KVR = g.KVR,
                    KOSGU = g.KOSGU,
                    DFK = g.DFK,
                    DKR = g.DKR,
                    DEK = g.DEK,
                    CodeSubsidy = g.CodeSubsidy,
                    BranchCode = g.BranchCode,
                    Year = g.Year
                }).Select(s => new ResultRLVA
                    {
                        Key = s.Key,
                        DocValue = s.Sum(c => (decimal?) c.DocValue),
                        RegValue1 = s.Sum(c => (decimal?) c.RegValue1),
                        RegValue2 = s.Sum(c => (decimal?) c.RegValue2),
                    }).ToList();
            return result;
        }

        /// <summary>
        /// Получить фин. данные из документа и из регистра по указанным фильтрам 
        /// получаем в каждой строке, приведенной к бланку, значение по документу и два значения по регистру "Объемы финансовых средств"
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="blank">бланк для приведения</param>
        /// <param name="isMeanAUBU">если null то все данные из тч, иначе только с указанным значением флажка "Средства АУ/БУ"</param>
        /// <param name="idValType1">тип значения для первого значения из регистра</param>
        /// <param name="idSbp1">сбп для первого значения из регистра</param>
        /// <param name="isMeanAUBU1">для первого значения из регистра, если null то все данные из регистра, иначе только с указанным значением флажка "Средства АУ/БУ"</param>
        /// <param name="idValType2">тип значения для второго значения из регистра</param>
        /// <param name="idSbp2">сбп для второго значения из регистра</param>
        /// <param name="isMeanAUBU2">для второго значения из регистра, если null то все данные из регистра, иначе только с указанным значением флажка "Средства АУ/БУ"</param>
        /// <param name="dateDoc">брать из регистра данные актульные на указанную дату, если null то все</param>
        /// <returns>группированный список состоящий из ключа и трех значений</returns>
        private List<ResultRLVA> GetData_CheckForExcessMeans(DataContext context, SBP_Blank blank, bool? isMeanAUBU, bool ForAdditionalNeed,
                                                             byte? idValType1, int? idSbp1, bool? isMeanAUBU1,
                                                             byte? idValType2, int? idSbp2, bool? isMeanAUBU2,
                                                             DateTime? dateDoc = null)
        {
            byte req = (byte)BlankValueType.Mandatory;

            var qTp = context.PlanActivity_PeriodsOfFinancialProvision.Where(w =>
                                                                             w.IdOwner == Id &&
                                                                             w.Master.FinanceSource.IdFinanceSourceType != (byte)FinanceSourceType.UnconfirmedFunds &&
                                                                             (!isMeanAUBU.HasValue ||
                                                                              w.Master.IsMeansAUBU == isMeanAUBU)
                ).Select(y => new
                {
                    y.Master,
                    y.HierarchyPeriod.Year,
                    Value = ForAdditionalNeed ? (y.AdditionalValue ?? 0) : (y.Value ?? 0)
                }).GroupBy(g => new ResultKeyRLVA
                {
                    ExpenseObligationType =
                        (blank.IdBlankValueType_ExpenseObligationType != req ||
                         g.Master.IdExpenseObligationType == null
                             ? null
                             : g.Master.IdExpenseObligationType),
                    FinanceSource =
                        (blank.IdBlankValueType_FinanceSource != req || g.Master.IdFinanceSource == null
                             ? null
                             : g.Master.FinanceSource.Code),
                    KFO =
                        (blank.IdBlankValueType_KFO != req || g.Master.IdKFO == null ? null : g.Master.KFO.Code),
                    KVSR =
                        (blank.IdBlankValueType_KVSR != req || g.Master.IdKVSR == null
                             ? null
                             : g.Master.KVSR.Caption),
                    RZPR =
                        (blank.IdBlankValueType_RZPR != req || g.Master.IdRZPR == null
                             ? null
                             : g.Master.RZPR.Code),
                    KCSR =
                        (blank.IdBlankValueType_KCSR != req || g.Master.IdKCSR == null
                             ? null
                             : g.Master.KCSR.Code),
                    KVR =
                        (blank.IdBlankValueType_KVR != req || g.Master.IdKVR == null ? null : g.Master.KVR.Code),
                    KOSGU =
                        (blank.IdBlankValueType_KOSGU != req || g.Master.IdKOSGU == null
                             ? null
                             : g.Master.KOSGU.Code),
                    DFK =
                        (blank.IdBlankValueType_DFK != req || g.Master.IdDFK == null ? null : g.Master.DFK.Code),
                    DKR =
                        (blank.IdBlankValueType_DKR != req || g.Master.IdDKR == null ? null : g.Master.DKR.Code),
                    DEK =
                        (blank.IdBlankValueType_DEK != req || g.Master.IdDEK == null ? null : g.Master.DEK.Code),
                    CodeSubsidy =
                        (blank.IdBlankValueType_CodeSubsidy != req || g.Master.IdCodeSubsidy == null
                             ? null
                             : g.Master.CodeSubsidy.Code),
                    BranchCode =
                        (blank.IdBlankValueType_BranchCode != req || g.Master.IdBranchCode == null
                             ? null
                             : g.Master.BranchCode.Code),
                    Year = g.Year // g.HierarchyPeriod.Year
                }).Select(s => new ResultRLVA
                {
                    Key = s.Key,
                    DocValue = s.Sum(c => (decimal?)c.Value),
                    RegValue1 = null,
                    RegValue2 = null
                });

            var ids = GetIdAllVersionDoc(context);

            var qReg = context.LimitVolumeAppropriations.Where(w =>
                                                               w.IdVersion == IdVersion
                                                               && (!ForAdditionalNeed && !(w.HasAdditionalNeed.HasValue ? w.HasAdditionalNeed.Value : false) || ForAdditionalNeed && (w.HasAdditionalNeed.HasValue ? w.HasAdditionalNeed.Value : false))
                                                               && w.IdBudget == IdBudget
                                                               && w.IdPublicLegalFormation == IdPublicLegalFormation
                                                               && w.EstimatedLine.FinanceSource.IdFinanceSourceType != (byte)FinanceSourceType.UnconfirmedFunds
                                                               && (
                                                                      (w.EstimatedLine.IdSBP == idSbp1 &&
                                                                       (!isMeanAUBU1.HasValue ||
                                                                        w.IsMeansAUBU == isMeanAUBU1))
                                                                      ||
                                                                      (w.EstimatedLine.IdSBP == idSbp2 &&
                                                                       (!isMeanAUBU2.HasValue ||
                                                                        w.IsMeansAUBU == isMeanAUBU2))
                                                                  )
                                                               && (!dateDoc.HasValue || w.DateCommit <= dateDoc)
                                                               &&
                                                               !(w.IdRegistratorEntity == EntityId &&
                                                                 ids.Contains(w.IdRegistrator))
                // отсекаем движения сделанные веткой этого документа!!!
                ).Select(y => new
                {
                    y.EstimatedLine,
                    y.HierarchyPeriod.Year,
                    y.IdValueType,
                    RegValue1 = (y.IdValueType == idValType1 && y.EstimatedLine.IdSBP == idSbp1) ? (decimal?)y.Value : null,
                    RegValue2 = (y.IdValueType == idValType2 && y.EstimatedLine.IdSBP == idSbp2) ? (decimal?)y.Value : null
                }).GroupBy(g => new ResultKeyRLVA
                {
                    ExpenseObligationType =
                        (blank.IdBlankValueType_ExpenseObligationType != req ||
                         g.EstimatedLine.IdExpenseObligationType == null
                             ? null
                             : g.EstimatedLine.IdExpenseObligationType),
                    FinanceSource =
                        (blank.IdBlankValueType_FinanceSource != req || g.EstimatedLine.IdFinanceSource == null
                             ? null
                             : g.EstimatedLine.FinanceSource.Code),
                    KFO =
                        (blank.IdBlankValueType_KFO != req || g.EstimatedLine.IdKFO == null
                             ? null
                             : g.EstimatedLine.KFO.Code),
                    KVSR =
                        (blank.IdBlankValueType_KVSR != req || g.EstimatedLine.IdKVSR == null
                             ? null
                             : g.EstimatedLine.KVSR.Caption),
                    RZPR =
                        (blank.IdBlankValueType_RZPR != req || g.EstimatedLine.IdRZPR == null
                             ? null
                             : g.EstimatedLine.RZPR.Code),
                    KCSR =
                        (blank.IdBlankValueType_KCSR != req || g.EstimatedLine.IdKCSR == null
                             ? null
                             : g.EstimatedLine.KCSR.Code),
                    KVR =
                        (blank.IdBlankValueType_KVR != req || g.EstimatedLine.IdKVR == null
                             ? null
                             : g.EstimatedLine.KVR.Code),
                    KOSGU =
                        (blank.IdBlankValueType_KOSGU != req || g.EstimatedLine.IdKOSGU == null
                             ? null
                             : g.EstimatedLine.KOSGU.Code),
                    DFK =
                        (blank.IdBlankValueType_DFK != req || g.EstimatedLine.IdDFK == null
                             ? null
                             : g.EstimatedLine.DFK.Code),
                    DKR =
                        (blank.IdBlankValueType_DKR != req || g.EstimatedLine.IdDKR == null
                             ? null
                             : g.EstimatedLine.DKR.Code),
                    DEK =
                        (blank.IdBlankValueType_DEK != req || g.EstimatedLine.IdDEK == null
                             ? null
                             : g.EstimatedLine.DEK.Code),
                    CodeSubsidy =
                        (blank.IdBlankValueType_CodeSubsidy != req || g.EstimatedLine.IdCodeSubsidy == null
                             ? null
                             : g.EstimatedLine.CodeSubsidy.Code),
                    BranchCode =
                        (blank.IdBlankValueType_BranchCode != req || g.EstimatedLine.IdBranchCode == null
                             ? null
                             : g.EstimatedLine.BranchCode.Code),
                    Year = g.Year // g.HierarchyPeriod.Year
                }).Select(s => new ResultRLVA
                {
                    Key = s.Key,
                    DocValue = null,
                    RegValue1 = 
                        s.Sum(c => c.RegValue1),
                    RegValue2 =
                        s.Sum(c => c.RegValue2)
                });

            List<ResultRLVA> data = qTp.Concat(qReg).GroupBy(g => g.Key).Select(s => new ResultRLVA
            {
                Key = s.Key,
                DocValue = s.Sum(c => c.DocValue),
                RegValue1 = s.Sum(c => c.RegValue1),
                RegValue2 = s.Sum(c => c.RegValue2)
            }).ToList();

            return data;
        }

        /// <summary>
        /// Получение данных с превышением для контроля 0623 и контроля 0641
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="isCheckKosgu000">учитывать КОСГУ 000</param>
        /// <param name="forAdditionalNeed">проверять данные по Доп.потребностям</param>
        /// <param name="dateDoc">брать из регистра данные актульные на указанную дату, если null то все</param>
        /// <returns>группированный список состоящий из ключа и трех значений</returns>
        /// <returns></returns>
        private Dictionary<ResultRLVA, string> Common_0623(DataContext context, bool isCheckKosgu000 = false, bool ForAdditionalNeed = false, DateTime? dateDoc = null)
        {
            var list = new Dictionary<ResultRLVA, string>();
            //List<string> list = new List<string>();

            if (!SBP.IdParent.HasValue) return list;

            var rootSbp = SBP.Parent;
            while (rootSbp.IdParent.HasValue)
            {
                rootSbp = context.SBP.Single(s => s.Id == rootSbp.IdParent);
            }

            byte idBlankType = (SBP.Parent.IdSBPType == (byte) SBPType.GeneralManager
                                    ? (byte) BlankType.BringingGRBS
                                    : (byte) BlankType.BringingRBS);

            SBP_Blank blank = context.SBP_Blank.SingleOrDefault(s =>
                                                                s.IdOwner == rootSbp.Id
                                                                && s.IdBudget == IdBudget
                                                                && s.IdBlankType == idBlankType
                );

            if (blank == null)
                return list;

            //List<ResultRLVA> data2 = GetData_CheckForExcessMeans(context, blank, null, (byte)ValueType.Plan, SBP.IdParent, null, (byte)ValueType.Bring, SBP.IdParent, null, dateDoc);
            List<ResultRLVA> data = GetData_CheckForExcessMeans2(context, blank, ForAdditionalNeed, null, (byte)ValueType.Plan,
                                                                 SBP.IdParent, null, (byte) ValueType.Bring,
                                                                 SBP.IdParent, null, dateDoc);

            bool isKosgu000 =
                isCheckKosgu000
                && blank.IdBlankValueType_KOSGU == (byte) BlankValueType.Mandatory
                && data.Any(a => a.RegValue1.HasValue && a.Key.KOSGU == "000");

            List<ResultRLVA> res = new List<ResultRLVA>();
            if (!isKosgu000)
            {
                res.AddRange(data.Where(w => w.DocValue.HasValue).ToList()); // только строки где есть данные документа
            }
            else
            {
                res.AddRange(
                    data.Where(w => w.DocValue.HasValue && (w.RegValue1.HasValue || w.RegValue2.HasValue)).ToList());
                // только строки которые связались с соответсвкющими записями регистра

                var data000 = data.Select(s => new
                    {
                        srcStr = s,
                        newKey = (s.DocValue.HasValue && !s.RegValue1.HasValue && !s.RegValue2.HasValue)
                                     ? new ResultKeyRLVA(s.Key, "000")
                                     : s.Key
                    }).GroupBy(g => g.newKey).Select(s => new ResultRLVA
                        {
                            Key = s.Key,
                            DocValue = s.Sum(c => c.srcStr.DocValue),
                            RegValue1 = s.Sum(c => c.srcStr.RegValue1),
                            RegValue2 = s.Sum(c => c.srcStr.RegValue2),
                            childRec = s.Select(c => c.srcStr).ToList()
                        });

                res.AddRange(
                    data000.Where(w => w.DocValue.HasValue && (w.RegValue1.HasValue || w.RegValue2.HasValue)).ToList());
                // то что удалось сопоставить с косгу 000

                res.AddRange(
                    data000.Where(w => w.DocValue.HasValue && !w.RegValue1.HasValue && !w.RegValue2.HasValue)
                           .SelectMany(m => m.childRec)
                           .ToList()); // не удалось сопоставить вообще
            }

            var qRes = res.Where(w => ((w.RegValue1 ?? 0) - (w.RegValue2 ?? 0) - (w.DocValue ?? 0)) < 0);
            foreach (var r in qRes)
            {
                list.Add(r,
                    String.Format(
                        "{4}, {0} - Нераспределенный остаток = {1}, Объем средств из документа = {2}, Разность = {3}",
                        r.Caption(),
                        (r.RegValue1 ?? 0) - (r.RegValue2 ?? 0),
                        (r.DocValue ?? 0),
                        (r.RegValue1 ?? 0) - (r.RegValue2 ?? 0) - (r.DocValue ?? 0),
                        r.Key.Year));
            }

            return list;
        }

        /// <summary>   
        /// Контроль "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП"
        /// </summary>         
        [ControlInitial(InitialUNK = "0623",
            InitialCaption =
                "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП")]
        public void Control_0623(DataContext context)
        {
            if (SBP.IdSBPType != (byte) SBPType.TreasuryEstablishment)
                return;

            Dictionary<ResultRLVA, string> limitErrors = Common_0623(context, true, false);

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                List<ResultRLVA> errorEstimatedLineResults = limitErrors.Select(l => l.Key).ToList();

                Controls.Throw(
                    String.Format("Объем средств, распределенный по казенным учреждениям, превышает остаток нераспределенных средств вышестоящего СБП «{0}». Превышение обнаружено по строкам:<br>{1}", SBP.Caption, msg));


               // Обрабатываем ЭД «Предельные объемы бюджетных ассигнований» 

                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).Union(context.SBP.Where(w => w.Id == SBP.IdParent).Select(s => s.Id)).ToList())
                {
                    foreach (var docLimitBudgetAllocations in context.LimitBudgetAllocations.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion).ToList())
                    {
                            docLimitBudgetAllocations.IsRequireClarification = true;
                            docLimitBudgetAllocations.ReasonClarification = String.Format(
                                    "«{0}. Объем средств, распределенный по казенным учреждениям, превышает остаток нераспределенных средств вышестоящего СБП «{1}» .Превышение обнаружено по строкам:" +
                                    "{2}", DateTime.Now.ToShortDateString(), SBP.Parent.Caption, msg);
                    }
                }

                // Обрабатываем ЭД «План деятельности»  

                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).ToList())
                {
                    foreach (var docPlanActivity in context.PlanActivity.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion
                                                                                ).ToList())
                    {
                            docPlanActivity.IsRequireClarification = true;
                            docPlanActivity.ReasonClarification = String.Format(
                                    "«{0}. Объем средств, распределенный по казенным учреждениям, превышает остаток нераспределенных средств вышестоящего СБП «{1}» .Превышение обнаружено по строкам:" +
                                    "{2}", DateTime.Now.ToShortDateString(), SBP.Parent.Caption, msg);
                    }
                }

                // Обрабатываем текущий документ

                    this.IsRequireClarification = true;
                    this.ReasonClarification = String.Format(
                                    "«{0}. Объем средств, распределенный по казенным учреждениям, превышает остаток нераспределенных средств вышестоящего СБП «{1}» .Превышение обнаружено по строкам: " +
                                    "{2}", DateTime.Now.ToShortDateString(), SBP.Parent.Caption, msg);
                context.SaveChanges();
   
            }
            else
            {
                //Если контроль УНК 0623 выполнился успешно (т.е. не обнаружено превышение), то выполнить следующие действия:
                //•	Найти документы «План деятельности», у которых «СБП» находятся на одном уровне с СБП из шапки документа, т.е. у них один и тот же родитель, а также документ «План деятельности» на СБП-родитель, для ШапкаДокумента.СБП (с учетом ППО, Бюджет, Версия)
                //•	В найденных документах «План деятельности», а также в текущем документе убрать флажок «Требует уточнения» и очистить поле «Причина уточнения»

                // Обрабатываем ЭД «Предельные объемы бюджетных ассигнований» 

                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).Union(context.SBP.Where(w => w.Id == SBP.IdParent).Select(s => s.Id)).ToList())
                {
                    foreach (var docLimitBudgetAllocations in context.LimitBudgetAllocations.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion).ToList())
                    {
                        docLimitBudgetAllocations.IsRequireClarification = false;
                        docLimitBudgetAllocations.ReasonClarification = "";
                    }
                }


                // Обрабатываем ЭД «План деятельности»  

                foreach (var sbpId in context.SBP.Where(s => s.IdParent.HasValue && s.IdParent == SBP.IdParent).Select(s => s.Id).ToList())
                {

                    foreach (var docPlanActivity in context.PlanActivity.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion
                                                                                ).ToList())
                    {
                        docPlanActivity.IsRequireClarification = false;
                        docPlanActivity.ReasonClarification = "";
                    }
                }

                // Обрабатываем текущий документ
                IsRequireClarification = false;
                ReasonClarification = "";

            }
        }

        /// <summary>   
        /// Контроль "Проверка на превышение сумм доп. потребностей, распределенных по КУ, над остатком средств по доп. потребностям у вышестоящего СБП"
        /// </summary>         
        [ControlInitial(InitialUNK = "0637",
            InitialCaption =
                "Проверка на превышение сумм доп. потребностей, распределенных по КУ, над остатком средств по доп. потребностям у вышестоящего СБП")]
        public void Control_0637(DataContext context)
        {
            if (SBP.IdSBPType != (byte)SBPType.TreasuryEstablishment)
                return;

            var limitErrors = Common_0623(context, true, true);

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                Controls.Throw(
                    String.Format(
                        "Сумма дополнительных потребностей, распределенная по казенным учреждениям, превышает остаток нераспределенных средств по дополнительным поотребностям вышестоящего СБП «{0}». <br>Превышение обнаружено по строкам:<br>{1}",
                        SBP.Caption,
                        msg));
            }
        }


        /// <summary>
        /// Получение данных с превышением для контроля 0624 и контроля 0642
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="dateDoc">брать из регистра данные актульные на указанную дату, если null то все</param>
        /// <returns>группированный список состоящий из ключа и трех значений</returns>
        private Dictionary<ResultRLVA, string> Common_0624(DataContext context, bool ForAdditionalNeed = false, DateTime? dateDoc = null)
        {
            var list = new Dictionary<ResultRLVA, string>();

            if (SBP.IdParent == null) return list;

            var blanks = context.SBP_Blank.Where(s =>
                                                 s.IdBudget == IdBudget
                                                 && (
                                                        (s.IdOwner == SBP.Parent.IdParent &&
                                                         s.IdBlankType == (byte) BlankType.BringingKU)
                                                        ||
                                                        (s.IdOwner == SBP.IdParent &&
                                                         s.IdBlankType == (byte) BlankType.BringingAUBU)
                                                    )
                ).ToList().Select(s => (ISBP_Blank) s).ToList();

            SBP_Blank blank = SBP_BlankHelper.GetReductedBlank(blanks);

            if (blank == null)
                return list;

            List<ResultRLVA> data = GetData_CheckForExcessMeans(context, blank, null, ForAdditionalNeed, (byte) ValueType.Plan,
                                                                SBP.IdParent, true, (byte) ValueType.Bring, SBP.IdParent,
                                                                true, dateDoc);

            var qRes =
                data.Where(w => w.DocValue.HasValue && ((w.RegValue1 ?? 0) - (w.RegValue2 ?? 0) - (w.DocValue ?? 0)) < 0);
            foreach (var r in qRes)
            {
                list.Add(r,
                    String.Format(
                        "{4}, {0} - Нераспределенный остаток = {1}, Сумма из документа = {2}, Разность = {3}",
                        r.Caption(),
                        (r.RegValue1 ?? 0) - (r.RegValue2 ?? 0),
                        (r.DocValue ?? 0),
                        (r.RegValue1 ?? 0) - (r.RegValue2 ?? 0) - (r.DocValue ?? 0),
                        r.Key.Year));
            }

            return list;
        }

        /// <summary>   
        /// Контроль "Проверка на превышение средств, распределенных по АУ/БУ, над остатком средств у учредителя"
        /// </summary>         
        [ControlInitial(InitialUNK = "0624",
            InitialCaption =
                "Проверка на превышение средств, распределенных по АУ/БУ, над остатком средств у учредителя")]
        public void Control_0624(DataContext context)
        {
            if (SBP.IdSBPType != (byte) SBPType.IndependentEstablishment &&
                SBP.IdSBPType != (byte) SBPType.BudgetEstablishment)
                return;

            Dictionary<ResultRLVA, string> limitErrors = Common_0624(context);

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                List<ResultRLVA> errorEstimatedLineResults = limitErrors.Select(l => l.Key).ToList();

                Controls.Throw(
                    string.Format(
                        "Объем средств, распределенный по автономным и бюджетным учреждениям, превышает остаток нераспределенных средств учредителя «{0}». Превышение обнаружено по строкам:<br>{1}",
                        SBP.Parent.Caption, msg
                        ));

                // Обрабатываем ЭД «План деятельности»  

                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).Union(context.SBP.Where(w => w.Id == SBP.IdParent).Select(s => s.Id)).ToList())
                {

                    foreach (var docPlanActivity in context.PlanActivity.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion
                                                                                ).ToList())
                    {
                        docPlanActivity.IsRequireClarification = true;
                        docPlanActivity.ReasonClarification = String.Format(
                                "«{0}. Объем средств, распределенный по автономным и бюджетным учреждениям, превышает остаток нераспределенных средств учредителя «{1}» .Превышение обнаружено по строкам:" +
                                "{2}", DateTime.Now.ToShortDateString(), SBP.Parent.Caption, msg);
                    }
                }

                // Обрабатываем текущий документ


                this.IsRequireClarification = true;
                this.ReasonClarification = String.Format(
                                "«{0}. Объем средств, распределенный по автономным и бюджетным учреждениям, превышает остаток нераспределенных средств учредителя «{1}» .Превышение обнаружено по строкам: " +
                                "{2}", DateTime.Now.ToShortDateString(), SBP.Parent.Caption, msg);
                context.SaveChanges();

            }
            else
            {
                //Если контроль УНК 0624 выполнился успешно (т.е. не обнаружено превышение), то выполнить следующие действия:
                //•	Найти документы «План деятельности», у которых «СБП» находятся на одном уровне с СБП из шапки документа, т.е. у них один и тот же родитель, а также документ «План деятельности» на СБП-родитель, для ШапкаДокумента.СБП (с учетом ППО, Бюджет, Версия)
                //•	В найденных документах «План деятельности», а также в текущем документе убрать флажок «Требует уточнения» и очистить поле «Причина уточнения»

                // Обрабатываем ЭД «План деятельности»  

                foreach (var sbpId in context.SBP.Where(s => s.IdParent == SBP.IdParent).Select(s => s.Id).Union(context.SBP.Where(w => w.Id == SBP.IdParent).Select(s => s.Id)).ToList())
                {

                    foreach (var docPlanActivity in context.PlanActivity.Where(l => l.IdSBP == sbpId &&
                                                                                l.IdPublicLegalFormation == IdPublicLegalFormation &&
                                                                                l.IdBudget == IdBudget &&
                                                                                l.IdVersion == IdVersion
                                                                                ).ToList())
                    {
                        docPlanActivity.IsRequireClarification = false;
                        docPlanActivity.ReasonClarification = "";
                    }
                }

                // Обрабатываем текущий документ
                IsRequireClarification = false;
                ReasonClarification = "";

            }
        }

        /// <summary>   
        /// Контроль "Проверка на превышение сумм доп. потребностей, распределенных по АУ/БУ, над остатком средств по доп. потребностям у учредителя"
        /// </summary>         
        [ControlInitial(InitialUNK = "0638",
            InitialCaption =
                "Проверка на превышение сумм доп. потребностей, распределенных по АУ/БУ, над остатком средств по доп. потребностям у учредителя")]
        public void Control_0638(DataContext context)
        {
            if ((this.IsAdditionalNeed ?? false) &&
                SBP.IdSBPType != (byte)SBPType.IndependentEstablishment &&
                SBP.IdSBPType != (byte)SBPType.BudgetEstablishment)
                return;

            var limitErrors = Common_0624(context, true);


            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                 Controls.Throw(
                    string.Format(
                        "Сумма дополнительных потребностей, распределенная по автономным и бюджетным учреждениям, превышает остаток нераспределенных средств по дополнительным потребностям учредителя «{0}». <br>Превышение обнаружено по строкам:<br>{1}",
                        SBP.Parent.Caption,
                        msg
                        ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка на превышение объемов средств, распределенных по АУ/БУ, над плановым объемом у учредителя"
        /// </summary>         
        [ControlInitial(InitialUNK = "0625",
            InitialCaption =
                "Проверка на превышение объемов средств, распределенных по АУ/БУ, над плановым объемом у учредителя",
            InitialManaged = true, InitialSkippable = true)]
        public void Control_0625(DataContext context)
        {
            if (SBP.IdSBPType != (byte) SBPType.TreasuryEstablishment || !SBP.IsFounder)
                return;

            var blanks = context.SBP_Blank.Where(s =>
                                                 s.IdBudget == IdBudget
                                                 && (
                                                        (s.IdOwner == SBP.IdParent &&
                                                         s.IdBlankType == (byte) BlankType.BringingKU)
                                                        ||
                                                        (s.IdOwner == IdSBP &&
                                                         s.IdBlankType == (byte) BlankType.BringingAUBU)
                                                    )
                ).ToList().Select(s => (ISBP_Blank) s).ToList();

            SBP_Blank blank = SBP_BlankHelper.GetReductedBlank(blanks);

            if (blank == null)
                return;

            List<string> list = new List<string>();

            //List<ResultRLVA> data = GetData_CheckForExcessMeans(context, blank, true, (byte)ValueType.Bring, IdSBP, true, null, null, null);
            List<ResultRLVA> data = GetData_CheckForExcessMeans2(context, blank, false, true, (byte) ValueType.Bring, IdSBP,
                                                                 true, null, null, null);

            var qRes = data.Where(w => w.DocValue.HasValue && ((w.DocValue ?? 0) - (w.RegValue1 ?? 0)) < 0).ToList();

            if (qRes.Any())
            {
                foreach (var r in qRes)
                {
                    list.Add(
                        String.Format(
                            "{4}, {0} - Объем средств АУ/БУ из документа = {1}, Распределено по АУ/БУ = {2}, Разность = {3}",
                            r.Caption(),
                            (r.DocValue ?? 0),
                            (r.RegValue1 ?? 0),
                            (r.DocValue ?? 0) - (r.RegValue1 ?? 0),
                            r.Key.Year));
                }

                Controls.Check(list,
                               "Плановые объемы учредителя по средствам АУ/БУ меньше объема средств, уже распределенных по автономным и бюджетным учреждениям.<br>Несоответствие обнаружено по строкам:<br>{0}"
                    );

                afterControlSkip_0625(context, data, blank);
            }
        }

        private void afterControlSkip_0625(DataContext context, IEnumerable<ResultRLVA> errors, SBP_Blank blank)
        {
            var docPlanActivities =
                context.PlanActivity
                       .Where(l =>
                              ((l.SBP.IdParent.HasValue && l.SBP.IdParent == IdSBP && (l.IdDocStatus == DocStatus.Project || l.IdDocStatus == DocStatus.Approved)) || (l.Id == Id)) &&
                              l.IdPublicLegalFormation == IdPublicLegalFormation &&
                              l.IdBudget == IdBudget &&
                              l.IdVersion == IdVersion
                              )
                       .ToList();

            foreach (var docPlanActivity in docPlanActivities)
            {
                bool hasErrors = false;
                var docKBKOfFinancialProvisions = docPlanActivity.KBKOfFinancialProvisions.ToList();
                var errMsg = new List<string>();

                var ErrorLine = docKBKOfFinancialProvisions.Join(errors,
                    a => new
                    {
                        IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? a.IdExpenseObligationType : 0),
                        IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory && a.IdBranchCode.HasValue ? a.BranchCode.Code : ""),
                        IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory && a.IdCodeSubsidy.HasValue ? a.CodeSubsidy.Code : ""),
                        IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory && a.IdDEK.HasValue ? a.DEK.Code : ""),
                        IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory && a.IdDFK.HasValue ? a.DFK.Code : ""),
                        IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory && a.IdDKR.HasValue ? a.DKR.Code : ""),
                        IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory && a.IdFinanceSource.HasValue ? a.FinanceSource.Code : ""),
                        IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory && a.IdKCSR.HasValue ? a.KCSR.Code : ""),
                        IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory && a.IdKFO.HasValue ? a.KFO.Code : ""),
                        IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory && a.IdKOSGU.HasValue ? a.KOSGU.Code : ""),
                        IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory && a.IdKVR.HasValue ? a.KVR.Code : ""),
                        IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory && a.IdKVSR.HasValue ? a.KVSR.Caption : ""),
                        IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory && a.IdRZPR.HasValue ? a.RZPR.Code : "")
                    },
                    b => new
                    {
                        IdExpenseObligationType = (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? b.Key.ExpenseObligationType : 0),
                        IdBranchCode = (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? b.Key.BranchCode : ""),
                        IdCodeSubsidy = (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? b.Key.CodeSubsidy : ""),
                        IdDEK = (blank.BlankValueType_DEK == BlankValueType.Mandatory ? b.Key.DEK : ""),
                        IdDFK = (blank.BlankValueType_DFK == BlankValueType.Mandatory ? b.Key.DFK : ""),
                        IdDKR = (blank.BlankValueType_DKR == BlankValueType.Mandatory ? b.Key.DKR : ""),
                        IdFinanceSource = (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? b.Key.FinanceSource : ""),
                        IdKCSR = (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? b.Key.KCSR : ""),
                        IdKFO = (blank.BlankValueType_KFO == BlankValueType.Mandatory ? b.Key.KFO : ""),
                        IdKOSGU = (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? b.Key.KOSGU : ""),
                        IdKVR = (blank.BlankValueType_KVR == BlankValueType.Mandatory ? b.Key.KVR : ""),
                        IdKVSR = (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? b.Key.KVSR : ""),
                        IdRZPR = (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? b.Key.RZPR : "")
                    }, (a, b) => a).ToList();

                foreach (var error in ErrorLine.Distinct())
                {
                    hasErrors = true;
                    errMsg.Add(""
                        + (blank.BlankValueType_ExpenseObligationType == BlankValueType.Mandatory ? "Тип РО - " + error.ExpenseObligationType.Caption() + "; " : "")
                        + (blank.BlankValueType_FinanceSource == BlankValueType.Mandatory ? "Источник финансирования - " + error.FinanceSource.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "КФО - " + error.KFO.Code + "; " : "")
                        + (blank.BlankValueType_KVSR == BlankValueType.Mandatory ? "КВСР/КАДБ/КАИФ - " + error.KVSR.Caption + "; " : "")
                        + (blank.BlankValueType_RZPR == BlankValueType.Mandatory ? "РзПр - " + error.RZPR.Code + "; " : "")
                        + (blank.BlankValueType_KCSR == BlankValueType.Mandatory ? "КЦСР - " + error.KCSR.Code + "; " : "")
                        + (blank.BlankValueType_KVR == BlankValueType.Mandatory ? "КВР - " + error.KVR.Code + "; " : "")
                        + (blank.BlankValueType_KOSGU == BlankValueType.Mandatory ? "КОСГУ - " + error.KOSGU.Code + "; " : "")
                        + (blank.BlankValueType_DFK == BlankValueType.Mandatory ? "ДФК - " + error.DFK.Code + "; " : "")
                        + (blank.BlankValueType_DKR == BlankValueType.Mandatory ? "ДКР - " + error.DKR.Code + "; " : "")
                        + (blank.BlankValueType_DEK == BlankValueType.Mandatory ? "ДЕК - " + error.DEK.Code + "; " : "")
                        + (blank.BlankValueType_CodeSubsidy == BlankValueType.Mandatory ? "Код субсидии - " + error.CodeSubsidy.Code + "; " : "")
                        + (blank.BlankValueType_BranchCode == BlankValueType.Mandatory ? "Отраслевой код - " + error.BranchCode.Code + "; " : "")
                        );
                }

                if (hasErrors)
                {
                    docPlanActivity.IsRequireClarification = true;
                    var args = errMsg.Distinct();
                    docPlanActivity.ReasonClarification = String.Format(
                            "«{0}. Плановые объемы учредителя по средствам АУ/БУ меньше объема средств, уже распределенных по автономным и бюджетным учреждениям: \n" +
                            "{1}", DateTime.Now.ToShortDateString(), args.Aggregate((a, b) => a + "\n" + b));
                }
                else
                {
                    docPlanActivity.IsRequireClarification = false;
                    docPlanActivity.ReasonClarification = null;
                }

            }

            context.SaveChanges();
        }

        /// <summary>   
        /// Контроль "Проверка на превышение сумм доп. потребностей, распределенных по АУ/БУ, над плановым объемом доп. потребностей у учредителя"
        /// </summary>         
        [ControlInitial(InitialUNK = "0639",
            InitialCaption =
                "Проверка на превышение сумм доп. потребностей, распределенных по АУ/БУ, над плановым объемом доп. потребностей у учредителя",
            InitialManaged = true)]
        public void Control_0639(DataContext context)
        {
            if (SBP.IdSBPType != (byte)SBPType.TreasuryEstablishment || !SBP.IsFounder)
                return;

            var blanks = context.SBP_Blank.Where(s =>
                                                 s.IdBudget == IdBudget
                                                 && (
                                                        (s.IdOwner == SBP.IdParent &&
                                                         s.IdBlankType == (byte)BlankType.BringingKU)
                                                        ||
                                                        (s.IdOwner == IdSBP &&
                                                         s.IdBlankType == (byte)BlankType.BringingAUBU)
                                                    )
                ).ToList().Select(s => (ISBP_Blank)s).ToList();

            SBP_Blank blank = SBP_BlankHelper.GetReductedBlank(blanks);

            if (blank == null)
                return;

            List<string> list = new List<string>();

            List<ResultRLVA> data = GetData_CheckForExcessMeans2(context, blank, true, true, (byte)ValueType.Bring, IdSBP,
                                                                 true, null, null, null);

            var qRes = data.Where(w => w.DocValue.HasValue && ((w.DocValue ?? 0) - (w.RegValue1 ?? 0)) < 0);
            foreach (var r in qRes)
            {
                list.Add(
                    String.Format(
                        "{4}, {0} - Объем средств АУ/БУ из документа = {1}, Распределено по АУ/БУ = {2}, Разность = {3}",
                        r.Caption(),
                        (r.DocValue ?? 0),
                        (r.RegValue1 ?? 0),
                        (r.DocValue ?? 0) - (r.RegValue1 ?? 0),
                        r.Key.Year));
            }

            Controls.Check(list,
                           "Плановые суммы дополнительных потребностей учредителя по средствам АУ/БУ меньше суммы дополнительных потребностей, уже распределенных по автономным и бюджетным учреждениям.<br>Несоответствие обнаружено по строкам:<br>{0}"
                );
        }

        /// <summary>   
        /// Контроль "Заполнение вкладок «Требования в заданию»"
        /// </summary>         
        [ControlInitial(InitialUNK = "0626", InitialCaption = "Заполнение вкладок «Требования в заданию»",
            InitialSkippable = true, InitialManaged = true)]
        public void Control_0626(DataContext context)
        {
            var q = context.PlanActivity_Activity.Where(w =>
                                                        w.IdOwner == Id &&
                                                        (w.Activity.IdActivityType == (byte) ActivityType.Service ||
                                                         w.Activity.IdActivityType == (byte) ActivityType.Work)
                ).Select(s => new {s.Activity.IdActivityType}).Union(
                    context.PlanActivity_ActivityAUBU.Where(w =>
                                                            w.IdOwner == Id &&
                                                            (w.Activity.IdActivityType == (byte) ActivityType.Service ||
                                                             w.Activity.IdActivityType == (byte) ActivityType.Work)
                        ).Select(s => new {s.Activity.IdActivityType})
                ).Distinct();

            string mess = string.Empty;

            foreach (var at in q.ToList())
            {
                string msg = string.Empty;

                var item =
                    context.PlanActivity_RequirementsForTheTask.SingleOrDefault(
                        a => a.IdOwner == Id && a.IdActivityType == at.IdActivityType);

                if (item == null || string.IsNullOrEmpty(item.ReasonTerminationTask))
                    msg += " - Основание для досрочного прекращения исполнения задания<br>";

                if (item == null || string.IsNullOrEmpty(item.DatesReportingOnExecutionTask))
                    msg += " - Сроки предоставления отчетов об исполнении задания<br>";

                if (
                    !context.PlanActivity_OrderOfControlTheExecutionTasks.Any(
                        a => a.IdOwner == Id && a.Master.IdActivityType == at.IdActivityType))
                    msg += " - Порядок контроля за исполнением задания<br>";

                if (!string.IsNullOrEmpty(msg))
                    mess += string.Format(
                        "В документе указаны мероприятия с типом «{0}», поэтому на вкладке «Требования к заданию» необходимо ввести данные:<br>{1}",
                        ((ActivityType) at.IdActivityType).Caption(), msg
                        );
            }

            if (!string.IsNullOrEmpty(mess))
                Controls.Throw(mess);
        }

        /// <summary>   
        /// Контроль "Распределение объемов мероприятий из программ по учреждениям с учетом даты утверждения"
        /// </summary>         
        [ControlInitial(InitialUNK = "0629",
            InitialCaption = "Распределение объемов мероприятий из программ по учреждениям с учетом даты утверждения")]
        public void Control_0629(DataContext context)
        {
            if (PublicLegalFormation.UsedGMZ ?? false) return;

            List<string> list = Common_0614(context, Date);

            Controls.Check(list, string.Format(
                "Возможно, требуется корректировка даты утверждения текущего документа или утверждение объемов мероприятий в документах «Деятельность ведомства», «ДЦП».<br>На дату {0} возникают несоответствия.<br>"
                +
                "У следующих мероприятий указан объем, который превышает нераспределенный остаток объема по ведомству:<br>{{0}}",
                Date.ToString("dd.MM.yyyy")
                                     ));
        }

        /// <summary>   
        /// Контроль "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП с учетом даты утверждения"
        /// </summary>         
        [ControlInitial(InitialUNK = "0630",
            InitialCaption =
                "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП с учетом даты утверждения"
            )]
        public void Control_0630(DataContext context)
        {
            if (SBP.IdSBPType != (byte) SBPType.TreasuryEstablishment)
                return;

            var limitErrors = Common_0623(context, true, false);

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                Controls.Throw(
                    string.Format(
                        "Возможно, требуется корректировка даты утверждения текущего документа или утверждение средств в документах «Предельные объемы БА».<br>На дату {0} возникают несоответствия.<br>"
                        +
                        "Объем средств, распределенный по казенным учреждениям, превышает остаток нераспределенных средств вышестоящего СБП «{1}». Превышение обнаружено по строкам:<br>{{0}}",
                        Date.ToString("dd.MM.yyyy"),
                        SBP.Parent.Caption
                        ));

            }

        }

        /// <summary>   
        /// Контроль "Проверка на превышение средств, распределенных по АУ/БУ, над остатком средств у учредителя с учетом даты утверждения"
        /// </summary>         
        [ControlInitial(InitialUNK = "0631",
            InitialCaption =
                "Проверка на превышение средств, распределенных по АУ/БУ, над остатком средств у учредителя с учетом даты утверждения"
            )]
        public void Control_0631(DataContext context)
        {
            if (SBP.IdSBPType != (byte) SBPType.IndependentEstablishment &&
                SBP.IdSBPType != (byte) SBPType.BudgetEstablishment)
                return;

            var limitErrors = Common_0624(context, false, Date);

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                Controls.Throw(
                   string.Format(
                        "Возможно, требуется корректировка даты утверждения текущего документа или утверждение средств в документе «План деятельности учредителя».<br>На дату {0} возникают несоответствия.<br>"
                        +
                        "Объем средств, распределенный по автономным и бюджетным учреждениям, превышает остаток нераспределенных средств учредителя «{1}». Превышение обнаружено по строкам:<br>{{0}}",
                        Date.ToString("dd.MM.yyyy"),
                        SBP.Parent.Caption
                       ));
            }
        }

        /// <summary>   
        /// Контроль "Очистка доп. потребностей"
        /// </summary>         
        [ControlInitial(InitialUNK = "0632", InitialCaption = "Очистка доп. потребностей", InitialSkippable = true)]
        public void Control_0632(DataContext context)
        {
            if (IsAdditionalNeed == false)
            {
                bool isExists =
                    context.PlanActivity_ActivityVolume.Any(w => w.IdOwner == Id && (w.AdditionalVolume ?? 0) != 0)
                    ||
                    context.PlanActivity_ActivityVolumeAUBU.Any(w => w.IdOwner == Id && (w.AdditionalVolume ?? 0) != 0)
                    ||
                    context.PlanActivity_IndicatorQualityActivityValue.Any(
                        w => w.IdOwner == Id && (w.AdditionalValue ?? 0) != 0);

                if (isExists)
                {
                    Controls.Throw(
                        "Признак «Вести доп. потребности» отключен. Все доп. потребности в документе будут очищены.");
                    ClearAdditionalValues(context);
                }
            }
        }

        /// <summary>   
        /// Контроль "Проверка признака «Вести доп.потребности»"
        /// </summary>         
        [ControlInitial(InitialUNK = "0633", InitialCaption = "Проверка признака «Вести доп.потребности»",
            InitialSkippable = true)]
        public void Control_0633(DataContext context)
        {
            if (IsAdditionalNeed == true)
                Controls.Throw(
                    "Документ ведется с доп. потребностями. Вы запустили операцию утверждения базовых значений. Будет создана и утверждена новая редакция документа с очищенными данными по доп. потребностям.");
        }

        /// <summary>   
        /// Контроль "Проверка признака «Вести доп.потребности»"
        /// </summary>         
        [ControlInitial(InitialUNK = "0634", InitialCaption = "Проверка признака «Вести доп.потребности»")]
        public void Control_0634(DataContext context)
        {
            if (IsAdditionalNeed != true)
                Controls.Throw(
                    "В документе отсутствуют значения по доп. потребностям. Воспользуйтесь операцией «Утвердить».");
        }

        /// <summary>   
        /// Контроль "Проверка признака «Вести доп.потребности»"
        /// </summary>         
        [ControlInitial(InitialUNK = "0635", InitialCaption = "Проверка признака «Вести доп.потребности»",
            InitialSkippable = true)]
        public void Control_0635(DataContext context)
        {
            if (IsAdditionalNeed == true)
                Controls.Throw(
                    "Будет создана и утверждена новая редакция документа – данные по доп. потребностям будут суммированы с базовыми значениями.");
        }

        /// <summary>   
        /// Контроль "Проверка типа периода планирования"
        /// </summary>         
        [ControlInitial(InitialUNK = "0636", InitialCaption = "Проверка типа периода планирования",
            InitialSkippable = true)]
        public void Control_0636(DataContext context)
        {
            if (SBP.SBPType == SBPType.IndependentEstablishment || SBP.SBPType == SBPType.BudgetEstablishment)
                // Автономное или Бюджетное учреждение
            {
                var fail =
                    (from sbp in context.SBP.Where(w => w.Id == IdSBP)
                     join ppid in context.SBP_PlanningPeriodsInDocumentsAUBU on sbp.IdParent equals ppid.IdOwner
                     where ppid.IdBudget == IdBudget
                           && (ppid.IdDocAUBUPeriodType_OFG != IdDocAUBUPeriodType_OFG
                               || ppid.IdDocAUBUPeriodType_PFG1 != IdDocAUBUPeriodType_PFG1
                               || ppid.IdDocAUBUPeriodType_PFG2 != IdDocAUBUPeriodType_PFG2)
                     select ppid).Any();


                if (fail)
                    Controls.Throw(string.Format(
                        "Изменены типы периодов планирования для учреждения «{0}». В новой редакции документа в таблице «Финансовое обеспечение» необходимо проверить и подкорректировать данные.",
                        SBP.Parent == null ? "" : SBP.Parent.Caption
                                       ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка соответствия текущего бланка доведения с актуальным "
        /// </summary>         
        [ControlInitial(InitialUNK = "0640", InitialCaption = "Проверка соответствия текущего бланка доведения с актуальным ", InitialSkippable = true)]
        public void Control_0640(DataContext context)
        {
            var idchekblanktype = this.SBP.SBPType == DbEnums.SBPType.TreasuryEstablishment
                                      ? (byte) DbEnums.BlankType.BringingKU
                                      : (byte) DbEnums.BlankType.BringingAUBU;

            var newBlanks = context.SBP_Blank.Where(r =>
                                                    r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.IdParent &&
                                                    r.IdBlankType == idchekblanktype);

            var oldBlank = this.SBP_BlankActual;

            var newBlank = newBlanks.FirstOrDefault();

            bool fc;
            if (oldBlank == null)
            {
                fc = true;
            }
            else
            {
                fc = !SBP_BlankHelper.IsEqualBlank(newBlank, oldBlank);
            }

            if (fc)
                Controls.Throw(string.Format("Был изменен бланк «{0}». " +
                                             "Необходимо актуализировать сведения в таблице «Финансовое обеспечение», " +
                                             "в строках будут очищены КБК, не соответствующие бланку доведения, и выполнится группировка сметных строк.",
                                             newBlank.BlankType.Caption()));
        }


        /// <summary>   
        /// Контроль "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП"
        /// </summary>
        [ControlInitial(InitialUNK = "0641", InitialCaption = "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП", InitialSkippable = false, InitialManaged = true)]
        public void Control_0641(DataContext context)
        {
            if (SBP.IdSBPType != (byte)SBPType.TreasuryEstablishment)
                return;

            Dictionary<ResultRLVA, string> limitErrors = Common_0623(context, true, false);

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                List<ResultRLVA> errorEstimatedLineResults = limitErrors.Select(l => l.Key).ToList();

                Controls.Throw(
                    String.Format("Объем средств, распределенный по казенным учреждениям, превышает остаток нераспределенных средств вышестоящего СБП «{0}». Превышение обнаружено по строкам:<br>{1}", SBP.Caption, msg));
            }
        }

        /// <summary>   
        /// Контроль "Проверка на превышение средств, распределенных по АУ/БУ, над остатком средств у учредителя"
        /// </summary>         
        [ControlInitial(InitialUNK = "0642", InitialCaption = "Проверка на превышение средств, распределенных по АУ/БУ, над остатком средств у учредителя", InitialSkippable = false, InitialManaged = true)]
        public void Control_0642(DataContext context)
        {
            if (SBP.IdSBPType != (byte)SBPType.IndependentEstablishment &&
                SBP.IdSBPType != (byte)SBPType.BudgetEstablishment)
                return;

            Dictionary<ResultRLVA, string> limitErrors = Common_0624(context);

            if (limitErrors.Any())
            {
                var msg = new StringBuilder();
                foreach (var limitError in limitErrors)
                    msg.AppendFormat(" - {0}<br/>", limitError.Value);

                List<ResultRLVA> errorEstimatedLineResults = limitErrors.Select(l => l.Key).ToList();

                Controls.Throw(
                    string.Format(
                        "Объем средств, распределенный по автономным и бюджетным учреждениям, превышает остаток нераспределенных средств учредителя «{0}». Превышение обнаружено по строкам:<br>{1}",
                        SBP.Parent.Caption, msg
                        ));
            }
        }

        /// <summary>   
        /// Контроль "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП"
        /// </summary>
        [ControlInitial(InitialUNK = "0643", InitialCaption = "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП", InitialSkippable = false, InitialManaged = true)]
        public void Control_0643(DataContext context)
        {
            Control_0637(context);
        }

        /// <summary>   
        /// Контроль "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП"
        /// </summary>
        [ControlInitial(InitialUNK = "0644", InitialCaption = "Проверка на превышение средств, распределенных по КУ, над остатком средств у вышестоящего СБП", InitialSkippable = false, InitialManaged = true)]
        public void Control_0644(DataContext context)
        {
            Control_0638(context);
        }

        /// <summary>   
        /// Контроль "Проверка на округление до сотен"
        /// </summary>
        [ControlInitial(InitialUNK = "0608", InitialCaption = "Проверка на округление до сотен", InitialManaged = true)]
        public void Control_0608(DataContext context)
        {

            var tp = context.PlanActivity_PeriodsOfFinancialProvision.Where(r => r.IdOwner == this.Id)
                            .Select(s => new { v = s, c = s.Master }).ToList()
                            .Where(r =>
                                (r.v.Value.HasValue && !CommonMethods.IsRound100(r.v.Value.Value)) ||
                                (r.v.AdditionalValue.HasValue && !CommonMethods.IsRound100(r.v.AdditionalValue.Value)))
                            .Select(s => new {s.v, s.c, e = s.c.GetEstimatedLine(context).ToString()});

            if (!tp.Any())
            {
                return;
            }

            var msg = "Для сметных строк указана сумма, не округленная до сотен:<br>{0}";
            var sb = new StringBuilder();

            foreach (var l in tp.Select(s => s.e).Distinct().OrderBy(o => o))
            {
                sb.AppendFormat("    - {0} <br>", l);
            }

            Controls.Throw(string.Format(msg, sb.Replace("Вид бюджетной деятельности Расходы, ", "")));
        }

        ///// <summary>   
        ///// Контроль ""
        ///// </summary>         
        //[ControlInitial(InitialUNK = "", InitialCaption = "")]
        //public void Control_(DataContext context)
        //{
        //}

        #endregion

        #region Методы операций

        #region Общие функции для операций

        private bool SetBlankActual(DataContext context)
        {
            var idchekblanktype = this.SBP.SBPType == DbEnums.SBPType.TreasuryEstablishment
                                      ? (byte)DbEnums.BlankType.BringingKU
                                      : (byte)DbEnums.BlankType.BringingAUBU;

            var newBlanks = context.SBP_BlankHistory.Where(r =>
                                                                         r.IdBudget == this.IdBudget && r.IdOwner == this.SBP.IdParent &&
                                                                         r.IdBlankType == idchekblanktype).
                                                  OrderByDescending(o => o.DateCreate);

            if (!newBlanks.Any())
            {
                return false;
            }

            SBP_BlankHistory oldBlankActual = this.SBP_BlankActual;

            this.SBP_BlankActual = newBlanks.FirstOrDefault();

            return this.SBP_BlankActual != null && (oldBlankActual == null || !SBP_BlankHelper.IsEqualBlank(oldBlankActual, this.SBP_BlankActual));
        }

        /// <summary>
        /// Установление признака «Требует уточнения» в документах «Смета» и «ПФХД»
        /// </summary>
        /// <param name="context"></param>
        private void SetIsRequireClarification(DataContext context)
        {
            var error = new StringBuilder();
            var messageList = new List<string>();
            error.AppendLine(string.Format("{0}. В документе {1}:", DateTime.Now.ToString("dd.MM.yyyy"), Caption));

            if (SBP.SBPType == SBPType.TreasuryEstablishment)
            {
                if (ErrorOnTpActivityCount(ActivityAUBU.ToList(), Parent.ActivityAUBU.ToList()))
                    messageList.Add("Был изменен перечень мероприятий на вкладке «Деятельность АУ/БУ».");
                if (ErrorOnTpActivityVolume(Activity.ToList(), Parent.Activity.ToList(), context, true))
                    messageList.Add("Был изменен объем мероприятий на вкладке «Деятельность АУ/БУ».");
            }

            if (ErrorOnTpActivityCount(Activity.ToList(), Parent.Activity.ToList()))
                messageList.Add("Был изменен перечень мероприятий на вкладке «Собственная деятельность».");
            if (ErrorOnTpActivityVolume(Activity.ToList(), Parent.Activity.ToList(), context, false))
                messageList.Add("Был изменен объем мероприятий на вкладке «Собственная деятельность».");
            if(ErrorLVA(context))
                messageList.Add("Был изменен объем финансового обеспечения");

            if (!messageList.Any()) return;

            var i = 0;
            foreach (var mess in messageList)
            {
                i++;
                error.AppendLine(string.Format("{0}) {1};",i,mess));
            }

            if (SBP.SBPType == SBPType.TreasuryEstablishment)
            {
                var docToChange =
                    context.PublicInstitutionEstimate.FirstOrDefault(
                        w => w.IdBudget == IdBudget
                             && w.IdVersion == IdVersion
                             && w.IdSBP == IdSBP
                             && !w.ChildrenByidParent.Any());

                if (docToChange != null)
                {
                    docToChange.IsRequireClarification = true;
                    docToChange.ReasonClarification = error.ToString();
                }

            }
            if (SBP.SBPType == SBPType.IndependentEstablishment || SBP.SBPType == SBPType.BudgetEstablishment)
            {
                var docToChange =
                    context.FinancialAndBusinessActivities.FirstOrDefault(
                        w => w.IdBudget == IdBudget
                             && w.IdVersion == IdVersion
                             && w.IdSBP == IdSBP
                             && !w.ChildrenByidParent.Any());

                if (docToChange != null)
                {
                    docToChange.IsRequireClarification = true;
                    docToChange.ReasonClarification = error.ToString();
                }
            }
        }

        private bool ErrorOnTpActivityCount(IEnumerable<IPlanActivity_ActivityTps> currentTp,
                                            IEnumerable<IPlanActivity_ActivityTps> parentTp)
        {
            var currentTpList = currentTp.Select(s => new {s.IdActivity, s.IdContingent}).ToList();
            var parrentTpList = parentTp.Select(s => new {s.IdActivity, s.IdContingent}).ToList();

            var currentTpCount = currentTpList.Count();
            var parrentTpCount = parrentTpList.Count();
            var unionCount = currentTpList.Union(parrentTpList).Count();

            return currentTpCount != parrentTpCount || currentTpCount != unionCount;
        }

        private bool ErrorOnTpActivityVolume(IEnumerable<IPlanActivity_ActivityTps> currentTp,
                                             IEnumerable<IPlanActivity_ActivityTps> parentTp, DataContext context,
                                             bool isAuBu)
        {
            var currentTpList = currentTp.Select(s => new {s.IdActivity, s.IdContingent}).ToList();
            var parrentTpList = parentTp.Select(s => new {s.IdActivity, s.IdContingent}).ToList();

            var list =
                currentTpList.Join(parrentTpList, arg => new {arg.IdActivity, arg.IdContingent},
                                   arg2 => new {arg2.IdActivity, arg2.IdContingent}, (arg, arg2) => new
                                       {
                                           arg.IdActivity,
                                           arg.IdContingent
                                       }).ToList();

            foreach (var item in list)
            {
                var check =
                    context.TaskVolume.Where(
                        w => w.IdBudget == IdBudget
                             && w.IdVersion == IdVersion
                             && w.IdSBP == IdSBP
                             && w.TaskCollection.IdActivity == item.IdActivity
                             && w.TaskCollection.IdContingent == item.IdContingent
                             &&
                             ((w.IdRegistrator == Id && w.IdRegistratorEntity == EntityId) ||
                              (w.IdTerminator == Id && w.IdTerminatorEntity == EntityId))).ToList();

                if (check.Any(a => !isAuBu || a.ActivityAUBU == true))
                {
                    return true;
                }
            }

            return false;
        }

        private bool ErrorLVA(DataContext context)
        {
            var error = context.LimitVolumeAppropriations.Any(w => w.IdRegistrator == Id && w.IdRegistratorEntity == EntityId);

            return error;
        }

        /// <summary>
        /// Создать движения по регистру "Объемы задач"
        /// </summary>
        /// <param name="context">контекст</param>
        /// <returns></returns>
        private void Process_TaskVolume(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.PlanActivity_ActivityVolume.Where(w => w.IdOwner == Id).Select(s => new
            {
                s.Master.IdActivity,
                s.Master.IdContingent,
                s.Master.IdIndicatorActivity,
                s.IdHierarchyPeriod,
                isMeanAUBU = false,
                isAdditionalNeed = false,
                Volume = s.Volume
            }).Union(
                context.PlanActivity_ActivityVolumeAUBU.Where(w => w.IdOwner == Id).Select(s => new
                {
                    s.Master.IdActivity,
                    s.Master.IdContingent,
                    s.Master.IdIndicatorActivity,
                    s.IdHierarchyPeriod,
                    isMeanAUBU = true,
                    isAdditionalNeed = false,
                    Volume = s.Volume
                })
            ).Union(
                context.PlanActivity_ActivityVolume.Where(w => w.IdOwner == Id && (w.AdditionalVolume ?? 0) != 0).Select(s => new
                {
                    s.Master.IdActivity,
                    s.Master.IdContingent,
                    s.Master.IdIndicatorActivity,
                    s.IdHierarchyPeriod,
                    isMeanAUBU = false,
                    isAdditionalNeed = true,
                    Volume = s.AdditionalVolume ?? 0
                }).Union(
                    context.PlanActivity_ActivityVolumeAUBU.Where(w => w.IdOwner == Id && (w.AdditionalVolume ?? 0) != 0).Select(s => new
                    {
                        s.Master.IdActivity,
                        s.Master.IdContingent,
                        s.Master.IdIndicatorActivity,
                        s.IdHierarchyPeriod,
                        isMeanAUBU = true,
                        isAdditionalNeed = true,
                        Volume = s.AdditionalVolume ?? 0
                    })
                )
            ).ToList();

            var qReg = context.TaskVolume.Where(w => 
                w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator) 
                && !w.IdTerminator.HasValue
            ).ToList();

            var newItems = qTp.Where(w => 
                !qReg.Any(a => 
                    a.TaskCollection.IdActivity == w.IdActivity
                    && (a.TaskCollection.IdContingent ?? 0) == (w.IdContingent ?? 0)
                    && a.IdIndicatorActivity_Volume == w.IdIndicatorActivity
                    && a.IdHierarchyPeriod == w.IdHierarchyPeriod
                    && (a.ActivityAUBU ?? false) == w.isMeanAUBU
                    && a.IsAdditionalNeed == w.isAdditionalNeed
                    && a.IdValueType == (byte)ValueType.Plan
                    && a.Value == w.Volume
                )
            );
	        List<TaskVolume> items = new List<TaskVolume>();
			foreach (var item in newItems)
            {
				items.Add(new TaskVolume
				{
					DateCreate = DateTime.Now,
					IdRegistratorEntity = EntityId,
					IdRegistrator = Id,
					IdPublicLegalFormation = IdPublicLegalFormation,
					IdBudget = IdBudget,
					IdVersion = IdVersion,
					IdSBP = IdSBP,
					IdTaskCollection = RegisterMethods.FindTaskCollection(context, IdPublicLegalFormation, item.IdActivity, item.IdContingent).Id,
					IdIndicatorActivity_Volume = item.IdIndicatorActivity,
					IdHierarchyPeriod = item.IdHierarchyPeriod,
					IdValueType = (byte)ValueType.Plan,
					ActivityAUBU = item.isMeanAUBU,
					IsAdditionalNeed = item.isAdditionalNeed,
					Value = item.Volume
				});
            }
			context.TaskVolume.InsertAsTableValue(items, context);
			
            var delItems = qReg.Where(a =>
                !qTp.Any(w =>
                    a.TaskCollection.IdActivity == w.IdActivity
                    && (a.TaskCollection.IdContingent ?? 0) == (w.IdContingent ?? 0)
                    && a.IdIndicatorActivity_Volume == w.IdIndicatorActivity
                    && a.IdHierarchyPeriod == w.IdHierarchyPeriod
                    && (a.ActivityAUBU ?? false) == w.isMeanAUBU
                    && a.IsAdditionalNeed == w.isAdditionalNeed
                    && a.IdValueType == (byte)ValueType.Plan
                    && a.Value == w.Volume
                )
            ).ToList();

            foreach (var item in delItems)
            {
                item.IdTerminatorEntity = EntityId;
                item.IdTerminator = Id;
                item.DateTerminate = DateTime.Now;
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Создать движения по регистру "Показатели качества задач"
        /// </summary>
        /// <param name="context">контекст</param>
        /// <returns></returns>
        private void Process_TaskIndicatorQuality(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var qTp = context.PlanActivity_IndicatorQualityActivityValue.Where(w => w.IdOwner == Id).Select(s => new
            {
                s.Master.Master.IdActivity,
                s.Master.Master.IdContingent,
                s.Master.IdIndicatorActivity,
                s.IdHierarchyPeriod,
                IsAdditionalNeed = false,
                Value = s.Value
            }).Union(
                context.PlanActivity_IndicatorQualityActivityValue.Where(w => w.IdOwner == Id && (w.AdditionalValue ?? 0) != 0).Select(s => new
                {
                    s.Master.Master.IdActivity,
                    s.Master.Master.IdContingent,
                    s.Master.IdIndicatorActivity,
                    s.IdHierarchyPeriod,
                    IsAdditionalNeed = true,
                    Value = s.AdditionalValue ?? 0
                })
            ).ToList();

            var qReg = context.TaskIndicatorQuality.Where(w =>
                w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
                && !w.IdTerminator.HasValue
            ).ToList();

            var newItems = qTp.Where(w =>
                !qReg.Any(a =>
                    a.TaskCollection.IdActivity == w.IdActivity
                    && (a.TaskCollection.IdContingent ?? 0) == (w.IdContingent ?? 0)
                    && a.IdIndicatorActivity_Quality == w.IdIndicatorActivity
                    && a.IdHierarchyPeriod == w.IdHierarchyPeriod
                    && a.IdValueType == (byte)ValueType.Plan
                    && a.IsAdditionalNeed == w.IsAdditionalNeed
                    && a.Value == w.Value
                )
            );
	        List<TaskIndicatorQuality> items = new List<TaskIndicatorQuality>();
			foreach (var item in newItems)
            {
				items.Add(new TaskIndicatorQuality
				{
					DateCreate = DateTime.Now,
					IdRegistratorEntity = EntityId,
					IdRegistrator = Id,
					IdPublicLegalFormation = IdPublicLegalFormation,
					IdBudget = IdBudget,
					IdVersion = IdVersion,
					IdSBP = IdSBP,
					IdTaskCollection = RegisterMethods.FindTaskCollection(context, IdPublicLegalFormation, item.IdActivity, item.IdContingent).Id,
					IdIndicatorActivity_Quality = item.IdIndicatorActivity,
					IdHierarchyPeriod = item.IdHierarchyPeriod,
					IdValueType = (byte)ValueType.Plan,
					IsAdditionalNeed = item.IsAdditionalNeed,
					Value = item.Value
				});
            }
			context.TaskIndicatorQuality.InsertAsTableValue(items, context);

	        var delItems = qReg.Where(a =>
	                                  !qTp.Any(w =>
	                                           a.TaskCollection.IdActivity == w.IdActivity
	                                           && (a.TaskCollection.IdContingent ?? 0) == (w.IdContingent ?? 0)
	                                           && a.IdIndicatorActivity_Quality == w.IdIndicatorActivity
	                                           && a.IdHierarchyPeriod == w.IdHierarchyPeriod
	                                           && a.IdValueType == (byte) ValueType.Plan
	                                           && a.IsAdditionalNeed == w.IsAdditionalNeed
	                                           && a.Value == w.Value
		                                   )
		        );
            foreach (var item in delItems)
            {
                item.IdTerminatorEntity = EntityId;
                item.IdTerminator = Id;
                item.DateTerminate = DateTime.Now;
            }
//			context.TaskIndicatorQuality.Update(t => delItems.Any(d => d.Id == t.Id), u => new TaskIndicatorQuality()
//			{
//				IdTerminatorEntity = EntityId,
//				IdTerminator = Id,
//				DateTerminate = DateTime.Now.Date
//			});
            context.SaveChanges();
        }



        /// <summary>
        /// Создать движения по регистру "Объемы финансовых средств"
        /// </summary>
        /// <param name="context">контекст</param>
        /// <returns></returns>
        private void Process_LimitVolumeAppropriations(DataContext context)
        {
            var ids = GetIdAllVersionDoc(context);

            var blank1 = context.SBP_Blank.Single(s => s.IdBudget == IdBudget && (
                (SBP.IdSBPType == (byte)SBPType.TreasuryEstablishment && s.IdOwner == SBP.IdParent && s.IdBlankType == (byte)BlankType.BringingKU)
                || (SBP.IdSBPType == (byte)SBPType.IndependentEstablishment && s.IdOwner == SBP.IdParent && s.IdBlankType == (byte)BlankType.BringingAUBU)
                || (SBP.IdSBPType == (byte)SBPType.BudgetEstablishment && s.IdOwner == SBP.IdParent && s.IdBlankType == (byte)BlankType.BringingAUBU)
            ));

            var blank2 = context.SBP_Blank.Single(s => s.IdBudget == IdBudget && (
                (SBP.Parent.IdSBPType == (byte)SBPType.GeneralManager && s.IdOwner == SBP.IdParent && s.IdBlankType == (byte)BlankType.BringingGRBS)
                || (SBP.Parent.IdSBPType == (byte)SBPType.Manager && s.IdOwner == SBP.Parent.IdParent && s.IdBlankType == (byte)BlankType.BringingRBS)
                || (SBP.Parent.IdSBPType == (byte)SBPType.TreasuryEstablishment && s.IdOwner == SBP.Parent.IdParent && s.IdBlankType == (byte)BlankType.BringingKU)
            ));

            var genericBlank = SBP_BlankHelper.GetReductedBlank(new List<ISBP_Blank> {blank1, blank2});

            var param1 = new FindParamEstimatedLine(context, true, ActivityBudgetaryType.Costs, false, false, IdBudget, IdSBP);
            var param2 = new FindParamEstimatedLine(context, true, ActivityBudgetaryType.Costs, true, genericBlank.IdBlankValueType_KOSGU == (byte)BlankValueType.Mandatory, IdBudget, SBP.IdParent ?? 0);

            List<LimitVolumeAppropriations> lva =
                context.LimitVolumeAppropriations.Where(
                    w => w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)).ToList();

            var q = context.PlanActivity_PeriodsOfFinancialProvision.Where(w => w.IdOwner == Id).Select(s => new { value = s, key = s.Master }).ToList();
			Dictionary<int, int> idsBlank1 = context.PlanActivity_KBKOfFinancialProvision.Where(a => a.IdOwner == Id).GetLinesId(context, Id,
	                                                                                                     EntityId,
	                                                                                                     blank1, param1);
			Dictionary<int, int> idsBlank2 = context.PlanActivity_KBKOfFinancialProvision.Where(a => a.IdOwner == Id).GetLinesId(context, Id,
																										 EntityId,
                                                                                                         genericBlank, param2);
			int idBlank1;
			int idBlank2;
            foreach (var s in q)
            {
	            
	            if (!idsBlank1.TryGetValue(s.key.Id, out idBlank1))
		            idBlank1 = 0;
	            if (!idsBlank2.TryGetValue(s.key.Id, out idBlank2))
		            idBlank2 = 0;
				lva.Add(new LimitVolumeAppropriations
                {
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdBudget = IdBudget,
                    IdVersion = IdVersion,
					IdEstimatedLine = idBlank1,
                    //IdEstimatedLine = s.key.GetLineId(context, Id, EntityId, blank1, param1) ?? 0,
                    IsIndirectCosts = false,
                    IdHierarchyPeriod = s.value.IdHierarchyPeriod ?? 0,
                    IdValueType = (byte)ValueType.Plan,
                    IsMeansAUBU = s.key.IsMeansAUBU,
                    Value = -s.value.Value ?? 0
                });
                lva.Add(new LimitVolumeAppropriations
                {
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdBudget = IdBudget,
                    IdVersion = IdVersion,
					IdEstimatedLine = idBlank2,
                    //IdEstimatedLine = EstimatedLineHelper.GetLineId(s.key, context, Id, EntityId, blank2, param2) ?? 0,
                    IsIndirectCosts = false,
                    IdHierarchyPeriod = s.value.IdHierarchyPeriod ?? 0,
                    IdValueType = (byte)ValueType.Bring,
                    IsMeansAUBU = (SBP.IdSBPType == (byte)SBPType.IndependentEstablishment || SBP.IdSBPType == (byte)SBPType.BudgetEstablishment),
                    Value = -s.value.Value ?? 0
                });

                lva.Add(new LimitVolumeAppropriations
                {
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdBudget = IdBudget,
                    IdVersion = IdVersion,
                    IdEstimatedLine = idBlank1,
                    //IdEstimatedLine = s.key.GetLineId(context, Id, EntityId, blank1, param1) ?? 0,
                    IsIndirectCosts = false,
                    IdHierarchyPeriod = s.value.IdHierarchyPeriod ?? 0,
                    IdValueType = (byte)ValueType.Plan,
                    IsMeansAUBU = s.key.IsMeansAUBU,
                    HasAdditionalNeed = true,
                    Value = -s.value.AdditionalValue ?? 0
                });
                lva.Add(new LimitVolumeAppropriations
                {
                    IdPublicLegalFormation = IdPublicLegalFormation,
                    IdBudget = IdBudget,
                    IdVersion = IdVersion,
                    IdEstimatedLine = idBlank2,
                    //IdEstimatedLine = EstimatedLineHelper.GetLineId(s.key, context, Id, EntityId, blank2, param2) ?? 0,
                    IsIndirectCosts = false,
                    IdHierarchyPeriod = s.value.IdHierarchyPeriod ?? 0,
                    IdValueType = (byte)ValueType.Bring,
                    IsMeansAUBU = (SBP.IdSBPType == (byte)SBPType.IndependentEstablishment || SBP.IdSBPType == (byte)SBPType.BudgetEstablishment),
                    HasAdditionalNeed = true,
                    Value = -s.value.AdditionalValue ?? 0
                });
            }

            var newRecs = lva.GroupBy(g => new 
            {
                IdPublicLegalFormation = g.IdPublicLegalFormation,
                IdBudget = g.IdBudget,
                IdVersion = g.IdVersion,
                IdEstimatedLine = g.IdEstimatedLine,
                IdAuthorityOfExpenseObligation = g.IdAuthorityOfExpenseObligation,
                IdTaskCollection = g.IdTaskCollection,
                IdOKATO = g.IdOKATO,
                IsIndirectCosts = g.IsIndirectCosts,
                IdHierarchyPeriod = g.IdHierarchyPeriod,
                IdValueType = g.IdValueType,
                IsMeansAUBU = g.IsMeansAUBU,
                HasAdditionalNeed = g.HasAdditionalNeed
            }).Select(s => new LimitVolumeAppropriations
            {
                DateCreate = DateTime.Now,
                IdRegistratorEntity = EntityId,
                IdRegistrator = Id,
                IdPublicLegalFormation = s.Key.IdPublicLegalFormation,
                IdBudget = s.Key.IdBudget,
                IdVersion = s.Key.IdVersion,
                IdEstimatedLine = s.Key.IdEstimatedLine,
                IdAuthorityOfExpenseObligation = s.Key.IdAuthorityOfExpenseObligation,
                IdTaskCollection = s.Key.IdTaskCollection,
                IdOKATO = s.Key.IdOKATO,
                IsIndirectCosts = s.Key.IsIndirectCosts,
                IdHierarchyPeriod = s.Key.IdHierarchyPeriod,
                IdValueType = s.Key.IdValueType,
                IsMeansAUBU = s.Key.IsMeansAUBU,
                HasAdditionalNeed = s.Key.HasAdditionalNeed,
                Value = -s.Sum(c => c.Value),
            }).Where(k => k.Value != 0);

            List<LimitVolumeAppropriations> items=new List<LimitVolumeAppropriations>();
			foreach (var s in newRecs)
            {
                items.Add(s);
            }
			context.LimitVolumeAppropriations.InsertAsTableValue(items, context);

            context.SaveChanges();
        }

        /// <summary>
        /// Удалить записи регистров, созданные документом и очистить аннуляцию для аннулированных
        /// </summary>
        /// <param name="context">контекст</param>
        /// <returns></returns>
        private void RemoveAllChangesInRegs(DataContext context)
        {
            foreach (TaskVolume reg in context.TaskVolume.Where(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id))
            {
                context.TaskVolume.Remove(reg);
            }

            foreach (TaskIndicatorQuality reg in context.TaskIndicatorQuality.Where(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id))
            {
                context.TaskIndicatorQuality.Remove(reg);
            }

            foreach (LimitVolumeAppropriations reg in context.LimitVolumeAppropriations.Where(w => w.IdRegistratorEntity == EntityId && w.IdRegistrator == Id))
            {
                context.LimitVolumeAppropriations.Remove(reg);
            }
	       
			

            foreach (TaskVolume reg in context.TaskVolume.Where(w => w.IdTerminatorEntity == EntityId && w.IdTerminator == Id))
            {
                reg.IdTerminatorEntity = null;
                reg.IdTerminator = null;
                reg.DateTerminate = null;
            }

            foreach (TaskIndicatorQuality reg in context.TaskIndicatorQuality.Where(w => w.IdTerminatorEntity == EntityId && w.IdTerminator == Id))
            {
                reg.IdTerminatorEntity = null;
                reg.IdTerminator = null;
                reg.DateTerminate = null;
            }
        }

        /// <summary>
        /// Утверждение записей регистров, аннулирование записей регистров по доп.потребностям
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="d">дата утверждения</param>
        /// <param name="isTermAddNeeds">аннулировать записи регистров по доп.потребностям</param>
        /// <returns></returns>
        private void SetRegsApproved(DataContext context, DateTime d, bool isTermAddNeeds = false)
        {
            var ids = GetIdAllVersionDoc(context);

            var q1 = context.TaskVolume.Where(w =>
                !w.IdTerminator.HasValue && !w.DateCommit.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );
            foreach (TaskVolume reg in q1)
            {
                if (!isTermAddNeeds || !reg.IsAdditionalNeed)
                {
                    reg.DateCommit = d;
                    reg.IdApprovedEntity = EntityId;
                    reg.IdApproved = Id;
                }
                else
                {
                    reg.DateTerminate = d;
                    reg.IdTerminatorEntity = EntityId;
                    reg.IdTerminator = Id;
                }
            }

            var q2 = context.TaskIndicatorQuality.Where(w =>
                !w.IdTerminator.HasValue && !w.DateCommit.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );
            foreach (TaskIndicatorQuality reg in q2)
            {
                if (!isTermAddNeeds || !reg.IsAdditionalNeed)
                {
                    reg.DateCommit = d;
                    reg.IdApprovedEntity = EntityId;
                    reg.IdApproved = Id;
                }
                else
                {
                    reg.DateTerminate = d;
                    reg.IdTerminatorEntity = EntityId;
                    reg.IdTerminator = Id;
                }
            }

            var q3 = context.LimitVolumeAppropriations.Where(w =>
                !w.DateCommit.HasValue
                && w.IdRegistratorEntity == EntityId && ids.Contains(w.IdRegistrator)
            );
            foreach (LimitVolumeAppropriations reg in q3)
            {
                reg.DateCommit = d;
                reg.IdApprovedEntity = EntityId;
                reg.IdApproved = Id;
            }
        }

        /// <summary>
        /// Отмена утверждения записей регистров
        /// </summary>
        /// <param name="context">контекст</param>
        /// <returns></returns>
        private void ClearRegsApproved(DataContext context)
        {
            var q1 = context.TaskVolume.Where(w => w.IdApprovedEntity == EntityId && w.IdApproved == Id);
            foreach (TaskVolume reg in q1)
            {
                reg.DateCommit = null;
                reg.IdApprovedEntity = null;
                reg.IdApproved = null;
            }

            var q2 = context.TaskIndicatorQuality.Where(w => w.IdApprovedEntity == EntityId && w.IdApproved == Id);
            foreach (TaskIndicatorQuality reg in q2)
            {
                reg.DateCommit = null;
                reg.IdApprovedEntity = null;
                reg.IdApproved = null;
            }

            var q3 = context.LimitVolumeAppropriations.Where(w => w.IdApprovedEntity == EntityId && w.IdApproved == Id);
            foreach (LimitVolumeAppropriations reg in q3)
            {
                reg.DateCommit = null;
                reg.IdApprovedEntity = null;
                reg.IdApproved = null;
            }
        }

        /// <summary>
        /// Создать новую версию документа
        /// </summary>
        /// <param name="context">контекст</param>
        /// <returns>новый документ (дочерний)</returns>
        public PlanActivity CreateNextVersion(DataContext context)
        {
            Clone cloner = new Clone(this);
            PlanActivity newDoc = (PlanActivity)cloner.GetResult();
            newDoc.IdDocStatus = DocStatus.Draft;
            newDoc.IdParent = Id;
            newDoc.IsRequireClarification = false;
            newDoc.DateCommit = null;
            newDoc.ReasonTerminate = null;
            newDoc.DateTerminate = null;
            newDoc.DateLastEdit = null;

            newDoc.Date = DateTime.Now.Date;

            var ids = GetIdAllVersionDoc(context);
            var rootNum = context.PlanActivity.Single(w => !w.IdParent.HasValue && ids.Contains(w.Id)).Number;
            newDoc.Number = rootNum + "." + ids.Count().ToString(CultureInfo.InvariantCulture);

            context.Entry(newDoc).State = EntityState.Added;
            context.SaveChanges();

            newDoc.Caption = newDoc.ToString();
            context.SaveChanges();

            return newDoc;
        }

        /// <summary>
        /// Очистка в табличных частях полей доп.потребностей
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="isSumAddVal">добавить перед очисткой доп.потребности к обычным значениям</param>
        /// <returns></returns>
        private void ClearAdditionalValues(DataContext context, bool isSumAddVal = false)
        {
            var q1 = context.PlanActivity_ActivityVolume.Where(w => w.IdOwner == Id && (w.AdditionalVolume ?? 0) != 0);
            foreach (var av in q1)
            {
                if (isSumAddVal) av.Volume += av.AdditionalVolume ?? 0;
                av.AdditionalVolume = null;
            }

            var q2 = context.PlanActivity_ActivityVolumeAUBU.Where(w => w.IdOwner == Id && (w.AdditionalVolume ?? 0) != 0);
            foreach (var av in q2)
            {
                if (isSumAddVal) av.Volume += av.AdditionalVolume ?? 0;
                av.AdditionalVolume = null;
            }

            var q3 = context.PlanActivity_IndicatorQualityActivityValue.Where(w => w.IdOwner == Id && (w.AdditionalValue ?? 0) != 0);
            foreach (var av in q3)
            {
                if (isSumAddVal) av.Value += av.AdditionalValue ?? 0;
                av.AdditionalValue = null;
            }

            var q4 = context.PlanActivity_PeriodsOfFinancialProvision.Where(w => w.IdOwner == Id && (w.AdditionalValue ?? 0) != 0);
            foreach (var av in q4)
            {
                if (isSumAddVal) av.Value += av.AdditionalValue ?? 0;
                av.AdditionalValue = null;
            }

            context.SaveChanges();
        }

        /// <summary>
        /// Получить количество месяцев для указанного типа периода
        /// </summary>
        /// <param name="idDocAubuPeriodType">Тип периода</param>
        /// <returns>количество месяцев для указанного типа периода</returns>
        private int MonthInDocAUBUPeriodType(byte idDocAubuPeriodType)
        {
            return (idDocAubuPeriodType == (byte) DocAUBUPeriodType.Year
                        ? 12
                        : (idDocAubuPeriodType == (byte) DocAUBUPeriodType.Quarter ? 3 : 1));
        }

        /// <summary>
        /// Обновление полей типов периодов и приведение периодов в табличных частях к соответствующим периодам
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="isConvertData">если истина, то конвертировать периоды</param>
        /// <returns></returns>
        public void UpdateDocAUBUPeriodTypes(DataContext context, bool isConvertData)
        {

            var pp = context.SBP_PlanningPeriodsInDocumentsAUBU.Where(w =>
                w.IdBudget == IdBudget
                && context.SBP.Any(a => a.Id == IdSBP && a.IdParent == w.IdOwner)
            ).SingleOrDefault();

            if (pp != null)
            {
                bool isChanged = false;

                if (IdDocAUBUPeriodType_OFG != pp.IdDocAUBUPeriodType_OFG)
                {
                    IdDocAUBUPeriodType_OFG = pp.IdDocAUBUPeriodType_OFG;
                    isChanged = true;
                }
                if (IdDocAUBUPeriodType_PFG1 != pp.IdDocAUBUPeriodType_PFG1)
                {
                    IdDocAUBUPeriodType_PFG1 = pp.IdDocAUBUPeriodType_PFG1;
                    isChanged = true;
                }
                if (IdDocAUBUPeriodType_PFG2 != pp.IdDocAUBUPeriodType_PFG2)
                {
                    IdDocAUBUPeriodType_PFG2 = pp.IdDocAUBUPeriodType_PFG2;
                    isChanged = true;
                }

                if (isConvertData && isChanged && (SBP.SBPType == SBPType.IndependentEstablishment || SBP.SBPType == SBPType.BudgetEstablishment)) // Автономное или Бюджетное учреждение
                {
                    var q =
                        context.PlanActivity_PeriodsOfFinancialProvision.Where(w => w.IdOwner == Id).Select(s => new
                        {
                            Tp = s,
                            Hp = s.HierarchyPeriod,
                            IndexYear = s.HierarchyPeriod.Year - s.Owner.Budget.Year,
                            Months = s.HierarchyPeriod.DateEnd.Month - s.HierarchyPeriod.DateStart.Month + 1
                        });
                    foreach (var item in q)
                    {
                        int months =
                            MonthInDocAUBUPeriodType(item.IndexYear == 0
                                                         ? IdDocAUBUPeriodType_OFG.Value
                                                         : (item.IndexYear == 1
                                                                ? IdDocAUBUPeriodType_PFG1.Value
                                                                : IdDocAUBUPeriodType_PFG2.Value));
                        if (item.Months != months)
                        {
                            item.Tp.IdHierarchyPeriod =
                                context.HierarchyPeriod.First(f =>
                                    f.DateStart == item.Hp.DateStart
                                    && (f.DateEnd.Month - f.DateStart.Month + 1) == months
                                ).Id;
                        }
                    }
                }
            }
        }

        private void TrimKbkByNewActualBlank(DataContext context)
        {
            var gkbktrim =
                (from kbk in context.PlanActivity_KBKOfFinancialProvision.
                                     Where(r => r.IdOwner == this.Id).
                                     ToList().
                                     Select(s =>
                                            new
                                                {
                                                    s.IdOwner,
                                                    s.IsMeansAUBU,
                                                    IdBranchCode = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_BranchCode) ? null : s.IdBranchCode,
                                                    IdCodeSubsidy = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_CodeSubsidy) ? null : s.IdCodeSubsidy,
                                                    IdDEK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DEK) ? null : s.IdDEK,
                                                    IdDFK = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DFK) ? null : s.IdDFK,
                                                    IdDKR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_DKR) ? null : s.IdDKR,
                                                    IdExpenseObligationType = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_ExpenseObligationType) ? null : s.IdExpenseObligationType,
                                                    IdFinanceSource = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_FinanceSource) ? null : s.IdFinanceSource,
                                                    IdKCSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KCSR) ? null : s.IdKCSR,
                                                    IdKOSGU = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KOSGU) ? null : s.IdKOSGU,
                                                    IdKFO = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KFO) ? null : s.IdKFO,
                                                    IdKVR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVR) ? null : s.IdKVR,
                                                    IdKVSR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_KVSR) ? null : s.IdKVSR,
                                                    IdRZPR = SBP_BlankHelper.IsBvtEmpty(SBP_BlankActual.IdBlankValueType_RZPR) ? null : s.IdRZPR,
                                                    s.Id
                                                })
                 join val in context.PlanActivity_PeriodsOfFinancialProvision.Where(r => r.IdOwner == this.Id) on
                     kbk.Id equals val.IdMaster
                 select new {kbk, val}).
                    GroupBy(g =>
                            new
                                {
                                    g.kbk.IdOwner,
                                    g.kbk.IsMeansAUBU,
                                    g.kbk.IdBranchCode,
                                    g.kbk.IdCodeSubsidy,
                                    g.kbk.IdDEK,
                                    g.kbk.IdDFK,
                                    g.kbk.IdDKR,
                                    g.kbk.IdExpenseObligationType,
                                    g.kbk.IdFinanceSource,
                                    g.kbk.IdKCSR,
                                    g.kbk.IdKOSGU,
                                    g.kbk.IdKFO,
                                    g.kbk.IdKVR,
                                    g.kbk.IdKVSR,
                                    g.kbk.IdRZPR,
                                    g.val.IdHierarchyPeriod
                                }).
                    Select(s =>
                           new
                               {
                                   kbk = s.Key,
                                   Value = s.Sum(ss => ss.val.Value),
                                   AdditionalValue = s.Sum(ss => ss.val.AdditionalValue)
                               }).ToList();

            context.PlanActivity_PeriodsOfFinancialProvision.RemoveAll(context.PlanActivity_PeriodsOfFinancialProvision.Where(r => r.IdOwner == this.Id));

            context.PlanActivity_KBKOfFinancialProvision.RemoveAll(context.PlanActivity_KBKOfFinancialProvision.Where(r => r.IdOwner == this.Id));

            var kbks = gkbktrim.Select(g =>
                                       new
                                       {
                                           g.kbk.IdOwner,
                                           g.kbk.IsMeansAUBU,
                                           g.kbk.IdBranchCode,
                                           g.kbk.IdCodeSubsidy,
                                           g.kbk.IdDEK,
                                           g.kbk.IdDFK,
                                           g.kbk.IdDKR,
                                           g.kbk.IdExpenseObligationType,
                                           g.kbk.IdFinanceSource,
                                           g.kbk.IdKCSR,
                                           g.kbk.IdKOSGU,
                                           g.kbk.IdKFO,
                                           g.kbk.IdKVR,
                                           g.kbk.IdKVSR,
                                           g.kbk.IdRZPR
                                       });

            foreach (var kbk in kbks.Distinct())
            {
                var newKbk = new PlanActivity_KBKOfFinancialProvision()
                {
                    IdOwner = kbk.IdOwner,
                    IsMeansAUBU = kbk.IsMeansAUBU,
                    IdBranchCode = kbk.IdBranchCode,
                    IdCodeSubsidy = kbk.IdCodeSubsidy,
                    IdDEK = kbk.IdDEK,
                    IdDFK = kbk.IdDFK,
                    IdDKR = kbk.IdDKR,
                    IdExpenseObligationType = kbk.IdExpenseObligationType,
                    IdFinanceSource = kbk.IdFinanceSource,
                    IdKCSR = kbk.IdKCSR,
                    IdKOSGU = kbk.IdKOSGU,
                    IdKFO = kbk.IdKFO,
                    IdKVR = kbk.IdKVR,
                    IdKVSR = kbk.IdKVSR,
                    IdRZPR = kbk.IdRZPR
                };

                context.PlanActivity_KBKOfFinancialProvision.Add(newKbk);

                var values = gkbktrim.Where(g =>
                                            g.kbk.IdOwner == kbk.IdOwner &&
                                            g.kbk.IsMeansAUBU == kbk.IsMeansAUBU &&
                                            g.kbk.IdBranchCode == kbk.IdBranchCode &&
                                            g.kbk.IdCodeSubsidy == kbk.IdCodeSubsidy &&
                                            g.kbk.IdDEK == kbk.IdDEK &&
                                            g.kbk.IdDFK == kbk.IdDFK &&
                                            g.kbk.IdDKR == kbk.IdDKR &&
                                            g.kbk.IdExpenseObligationType == kbk.IdExpenseObligationType &&
                                            g.kbk.IdFinanceSource == kbk.IdFinanceSource &&
                                            g.kbk.IdKCSR == kbk.IdKCSR &&
                                            g.kbk.IdKOSGU == kbk.IdKOSGU &&
                                            g.kbk.IdKFO == kbk.IdKFO &&
                                            g.kbk.IdKVR == kbk.IdKVR &&
                                            g.kbk.IdKVSR == kbk.IdKVSR &&
                                            g.kbk.IdRZPR == kbk.IdRZPR);

                foreach (var value in values)
                {
                    var newValue = new PlanActivity_PeriodsOfFinancialProvision()
                    {
                        IdOwner = value.kbk.IdOwner,
                        Master = newKbk,
                        IdHierarchyPeriod = value.kbk.IdHierarchyPeriod,
                        Value = value.Value,
                        AdditionalValue = value.AdditionalValue
                    };
                    context.PlanActivity_PeriodsOfFinancialProvision.Add(newValue);
                }
            }
        }

        #endregion

        /// <summary>   
        /// Операция «Создать»   
        /// </summary>  
        public void Create(DataContext context)
        {
            DateLastEdit = DateTime.Now;

            ExecuteControl(e => e.Control_0616(context));

            SetBlankActual(context);
        }
        
        /// <summary>   
        /// Операция «Редактировать»   
        /// </summary>  
        public void BeforeEdit(DataContext context)
        {
            ExecuteControl(e => e.Control_0640(context));

            if (SetBlankActual(context))
            {
                TrimKbkByNewActualBlank(context);
                context.SaveChanges();
            }
        }

        public void Edit(DataContext context)
        {
            DateLastEdit = DateTime.Now;

            ExecuteControl(e => e.Control_0602(context));
            ExecuteControl(e => e.Control_0632(context));
        }

        /// <summary>   
        /// Операция «Обработать»   
        /// </summary>  
        public void Process(DataContext context)
        {
			ReasonCancel = null;
            
			ExecuteControl(e => e.Control_0605(context));
            ExecuteControl(e => e.Control_0606(context));
            ExecuteControl(e => e.Control_0607(context));
            ExecuteControl(e => e.Control_0609(context));
            ExecuteControl(e => e.Control_0610(context));
            ExecuteControl(e => e.Control_0611(context));
            ExecuteControl(e => e.Control_0612(context));
            ExecuteControl(e => e.Control_0614(context));
            ExecuteControl(e => e.Control_0617(context));
            ExecuteControl(e => e.Control_0618(context));
            ExecuteControl(e => e.Control_0619(context));
            ExecuteControl(e => e.Control_0620(context));
            ExecuteControl(e => e.Control_0621(context));
            ExecuteControl(e => e.Control_0623(context));
            ExecuteControl(e => e.Control_0624(context));
            ExecuteControl(e => e.Control_0625(context));
            ExecuteControl(e => e.Control_0626(context));
            ExecuteControl(e => e.Control_0637(context));
            ExecuteControl(e => e.Control_0638(context));
            ExecuteControl(e => e.Control_0639(context));
            ExecuteControl(e => e.Control_0608(context));

            //IsRequireClarification = false;
            //ReasonClarification = null;

			Process_TaskVolume(context);
            Process_TaskIndicatorQuality(context);
            Process_LimitVolumeAppropriations(context);

            if (IdParent.HasValue)
            {
                SetIsRequireClarification(context);
            }

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
            RemoveAllChangesInRegs(context);
            using (new ControlScope())
            {
                context.SaveChanges();
            }

            var prevDoc = GetPrevVersionDoc(context, this);
            if (prevDoc != null)
            {
                prevDoc.ExecuteOperation(e => e.UndoArchive(context));
            }

        }

        /// <summary>   
        /// Операция «Согласование»
        /// </summary>  
        public void Check(DataContext context)
        {
            ExecuteControl(e => e.Control_0641(context));
            ExecuteControl(e => e.Control_0642(context));
            ExecuteControl(e => e.Control_0643(context));
            ExecuteControl(e => e.Control_0644(context));
            IdDocStatus = DocStatus.Checking;
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Отменить согласование»
        /// </summary>  
        public void UndoCheck(DataContext context)
        {
            IdDocStatus = DocStatus.Project;
            context.SaveChanges();
        }

        /// <summary>   
        /// Операция «Утвердить»   
        /// </summary>  
        public void Confirm(DataContext context)
        {
            ExecuteControl(e => e.Control_0633(context));
            ExecuteControl(e => e.Control_0629(context));
            ExecuteControl(e => e.Control_0630(context));
            ExecuteControl(e => e.Control_0631(context));

            if (IsAdditionalNeed == true)
            {
                IdDocStatus = DocStatus.Archive;

                PlanActivity newDoc = CreateNextVersion(context); // потом когда сделают механизм для открытия созданного документа, нужно возвращать что-то
                newDoc.IdDocStatus = DocStatus.Approved;
                newDoc.DateCommit = DateTime.Now.Date;

                newDoc.IsAdditionalNeed = false;
                newDoc.ClearAdditionalValues(context);

                newDoc.SetRegsApproved(context, Date, true);
            }
            else
            {
                DateCommit = DateTime.Now.Date;

                IdDocStatus = DocStatus.Approved;

                SetRegsApproved(context, Date);
            }
        }
        
        /// <summary>   
        /// Операция «Утвердить с доп. потребностями»   
        /// </summary>  
        public void ConfirmWithAddNeed(DataContext context)
        {
            ExecuteControl(e => e.Control_0634(context));
            ExecuteControl(e => e.Control_0635(context));
            ExecuteControl(e => e.Control_0629(context));
            ExecuteControl(e => e.Control_0630(context));
            ExecuteControl(e => e.Control_0631(context));

            PlanActivity newDoc = CreateNextVersion(context); // потом когда сделают механизм для открытия созданного документа, нужно возвращать что-то
            newDoc.IdDocStatus = DocStatus.Approved;
            newDoc.DateCommit = DateTime.Now.Date;

            newDoc.IsAdditionalNeed = false;
            newDoc.ClearAdditionalValues(context, true);

            newDoc.ExecuteControl(e => e.Control_0614(context));

            newDoc.Process_TaskVolume(context);
            newDoc.Process_TaskIndicatorQuality(context);
            newDoc.Process_LimitVolumeAppropriations(context);

            newDoc.SetRegsApproved(context, Date);
        }

        /// <summary>   
        /// Операция «Отменить утверждение»   
        /// </summary>  
        public void UndoConfirm(DataContext context)
        {
            DateCommit = null;

            ClearRegsApproved(context);
        }

        /// <summary>   
        /// Операция «Прекратить»
        /// </summary>  
        public void Terminate(DataContext context)
        {
            ExecuteControl<Control_7001>();
        }

        /// <summary>   
        /// Операция «Отменить прекращение»   
        /// </summary>  
        public void UndoTerminate(DataContext context)
        {
            ReasonTerminate = null;
            DateTerminate = null;
        }

        /// <summary>   
        /// Операция «Изменить»   
        /// </summary>  
        public void Change(DataContext context)
        {
            ExecuteControl(e => e.Control_0636(context));

            PlanActivity newDoc = CreateNextVersion(context); // потом когда сделают механизм для открытия созданного документа, нужно возвращать что-то
            newDoc.UpdateDocAUBUPeriodTypes(context, true); // перечитать поля с типами периодов и привести периоды к новому состоянию
        }

        /// <summary>   
        /// Операция «Отменить изменение»   
        /// </summary>
        public void UndoChange(DataContext context)
        {
            var q = context.PlanActivity.Where(w => w.IdParent == Id);
            foreach (var doc in q)
            {
                context.PlanActivity.Remove(doc);
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

        /// <summary>   
        /// Операция «Согласование МРГ»   
        /// </summary>  
        public void CheckMRG(DataContext context)
        {
            this.IsRequireCheck = false;
        }

        #endregion

        #region Implementation of IColumnFactoryForDenormalizedTablepart

        public ColumnsInfo GetColumns(string tablepartEntityName)
        {
            if (tablepartEntityName == typeof(PlanActivity_KBKOfFinancialProvision).Name)
                return GetColumnsFor_PlanActivity_KBKOfFinancialProvision();

            return null;
        }

        private void AddColumns(DataContext db, List<PeriodIdCaption> columns, int yearStart, int yearEnd, byte period)
        {
            var periods = db.HierarchyPeriod.Where(w => 
                w.DateStart.Year >= yearStart && w.DateStart.Year <= yearEnd
                && (
                    (period == (byte)DocAUBUPeriodType.Year && !w.IdParent.HasValue)
                    || (period == (byte)DocAUBUPeriodType.Quarter && w.IdParent.HasValue && !w.Parent.IdParent.HasValue)
                    || (period == (byte)DocAUBUPeriodType.Month && w.DateStart.Month == w.DateEnd.Month)
                )
            ).OrderBy(o => o.DateStart).Select(p => new PeriodIdCaption { PeriodId = p.Id, Caption = p.Caption });

            columns.AddRange(periods);
        }

        private ColumnsInfo GetColumnsFor_PlanActivity_KBKOfFinancialProvision()
        {
            DataContext db = IoC.Resolve<DbContext>().Cast<DataContext>();

            var columns = new List<PeriodIdCaption>();

            if (SBP.SBPType == SBPType.TreasuryEstablishment) // казенное учереждение
            {
                AddColumns(db, columns, Budget.Year, Budget.Year + 2, (byte)DocAUBUPeriodType.Year);
            }
            else if (SBP.SBPType == SBPType.IndependentEstablishment || SBP.SBPType == SBPType.BudgetEstablishment) // Автономное или Бюджетное учреждение
            {
                if (IdDocAUBUPeriodType_OFG.HasValue && IdDocAUBUPeriodType_PFG1.HasValue &&
                    IdDocAUBUPeriodType_PFG2.HasValue)
                {
                    AddColumns(db, columns, Budget.Year + 0, Budget.Year + 0, IdDocAUBUPeriodType_OFG.Value);
                    AddColumns(db, columns, Budget.Year + 1, Budget.Year + 1, IdDocAUBUPeriodType_PFG1.Value);
                    AddColumns(db, columns, Budget.Year + 2, Budget.Year + 2, IdDocAUBUPeriodType_PFG2.Value);
                }
                else
                {
                    var pp = db.SBP_PlanningPeriodsInDocumentsAUBU.SingleOrDefault(w => w.IdOwner == SBP.IdParent && w.IdBudget == IdBudget);
                    if (pp != null)
                    {
                        AddColumns(db, columns, Budget.Year + 0, Budget.Year + 0, pp.IdDocAUBUPeriodType_OFG);
                        AddColumns(db, columns, Budget.Year + 1, Budget.Year + 1, pp.IdDocAUBUPeriodType_PFG1);
                        AddColumns(db, columns, Budget.Year + 2, Budget.Year + 2, pp.IdDocAUBUPeriodType_PFG2);
                    }
                }
            }

            if (!(IsAdditionalNeed ?? false))
            {
                return new ColumnsInfo()
                {
                    Periods = columns,
                    Resources = new[] { Reflection<ITpAddValue>.Property(ent => ent.Value).Name }
                };
            }
            else
            {
                return new ColumnsInfo()
                {
                    Periods = columns
                };
            }
//            return new ColumnsInfo() { Periods = columns };
        }

        #endregion
    }

}