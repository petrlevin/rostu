using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BaseApp;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Interfaces;
using Sbor.Registry;

namespace Sbor.Logic
{
    /// <summary>
    /// 
    /// </summary>
    public static class RegisterMethods
    {
        public static TaskCollection FindTaskCollection(DataContext context, int publiclegalformation, int activity, int? contingent)
        {
            var tc = FindTaskCollection(context.TaskCollection.Where(r => r.IdPublicLegalFormation == publiclegalformation), activity, contingent);

            if (tc != null)
                return tc;

            var newTaskCollection = new TaskCollection
            {
                IdPublicLegalFormation = publiclegalformation,
                IdActivity = activity,
                IdContingent = contingent
            };

            context.TaskCollection.Add(newTaskCollection);
            context.SaveChanges();
            return newTaskCollection;
        }

        public static TaskCollection FindTaskCollection(List<TaskCollection> taskCollections, int activity, int? contingent)
        {
            return FindTaskCollection(taskCollections.AsQueryable(), activity, contingent);
        }

        public static TaskCollection FindTaskCollection(IQueryable<TaskCollection> taskCollections, int activity, int? contingent)
        {
            var tc = taskCollections.Where(r =>
                                  r.IdActivity == activity &&
                                  (!r.IdContingent.HasValue && !contingent.HasValue || r.IdContingent == contingent));

            return tc.FirstOrDefault();
        }

        public static TaskCollection FindTaskCollection(DataContext context, int publiclegalformation, int activity, int? contingent, out bool newTC, bool isNeedSave = true)
        {
            var tc = FindTaskCollection(context.TaskCollection.Where(r => r.IdPublicLegalFormation == publiclegalformation), activity, contingent);

            if (tc != null)
            {
                newTC = false;
                return tc;
            }

            var newTaskCollection = new TaskCollection
            {
                IdPublicLegalFormation = publiclegalformation,
                IdActivity = activity,
                IdContingent = contingent
            };

            if (isNeedSave)
            {
                context.TaskCollection.Add(newTaskCollection);
                context.SaveChanges();
            }
            newTC = true;
            return newTaskCollection;
        }

        /// <summary>
        ///  Аннулирует записи в регистрах arrRegisters, созданные документом idDoc
        /// </summary>
        public static void SetTerminatorById(DataContext context, int idDoc, int idDocEntity, DateTime dateTerminate, int idTerminator, int idTerminatorEntity, int[] arrRegisters)
        {
            SetTerminatorById(context, new[]{idDoc}, idDocEntity, dateTerminate, idTerminator, idTerminatorEntity, arrRegisters);
        }

        /// <summary>
        ///  Аннулирует записи в регистрах arrRegisters, созданные документами arrId
        /// </summary>
        /// <param name="context"></param>
        /// <param name="arrId">массив документов, являющихся регистраторами аннулируемых записей</param>
        /// <param name="idDocEntity"></param>
        /// <param name="dateTerminate"></param>
        /// <param name="idTerminator">ссылка на док аннулятор</param>
        /// <param name="idTerminatorEntity">сущность аннулятора</param>
        /// <param name="arrRegisters">массив регистров</param>
        public static void SetTerminatorById(DataContext context, int[] arrId, int idDocEntity, DateTime dateTerminate, int idTerminator, int idTerminatorEntity, int[] arrRegisters)
        {
            foreach (var id in arrId)
                SetTerminatorInRegister(context, id, idDocEntity, dateTerminate, idTerminator, idTerminatorEntity, arrRegisters);
        }

        /// <summary>
        /// Убирает у записей в регистрах arrRegisters ссылку на аннулятор idTerminator 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="idTerminator">ссылка на док аннулятор</param>
        /// <param name="idTerminatorEntity">сущность аннулятора</param>
        /// <param name="arrRegisters">массив регистров</param>
        /// <param name="idTerminateOperation">операция, которой производилась аннуляция</param>
        public static void ClearTerminatorByIdDoc(DataContext context, int idTerminator, int idTerminatorEntity, int[] arrRegisters, int idTerminateOperation = 0)
        {
            foreach (var arrRegister in arrRegisters)
                ClearTerminatorInRegister(context, idTerminator, idTerminatorEntity, arrRegister, idTerminateOperation);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="idDoc"></param>
        /// <param name="idTerminatorEntity"></param>
        /// <param name="registerEntityIdStatic"></param>
        /// <param name="idTerminateOperation"></param>
        public static void ClearTerminatorInRegister(DataContext context, int idDoc, int idTerminatorEntity, int registerEntityIdStatic, int idTerminateOperation = 0)
        {
        //todo -- это пиздец((
			//обернули из-за того, что может быть передан registerEntityIdStatic регистра который не реализует IHasCommonTerminator
            try
            {
                var table = context.Set<IHasCommonTerminator>(registerEntityIdStatic);

                foreach (var rec in table.Where(r =>
                                                r.IdTerminator == idDoc &&
                                                r.IdTerminatorEntity == idTerminatorEntity &&
                                                (idTerminateOperation == 0 ||
                                                 r.IdTerminateOperation == idTerminateOperation)))
                {
                    rec.IdTerminator = null;
                    rec.IdTerminatorEntity = null;
                    rec.DateTerminate = null;
                }
            }
            catch
            {
            }
        }

        public static void ClearRegsApproved(DataContext context, int iddoc, int IdApprovedEntity, int[] arrRegisters)
        {
            foreach (var arrRegister in arrRegisters)
            {
                ClearApprovedInRegister(context, iddoc, IdApprovedEntity, arrRegister);
            }
        }

        public static void RemoveFromRegistersByRegistrator(DataContext context, int iddoc, int IdRegistratorEntity, int[] arrRegisters)
        {
            foreach (var arrRegister in arrRegisters)
            {
                DocSGEMethod.RemoveRegRecords(context, arrRegister, iddoc, IdRegistratorEntity);
            }
        }

        /// <summary>
        /// Утверждение записей в регистрах
        /// </summary>
        /// <param name="context"></param>
        /// <param name="idApprovedDoc">Id утверждающего документа</param>
        /// <param name="dateCommit">Дата утверждения</param>
        /// <param name="idRegistratorEntity">Id сущности регистратора</param>
        /// <param name="ids">Ids всех версий документа</param>
        /// <param name="arrRegisters">Идентификаторы регистров, в которых требуется добавить утверждающий документ</param>
        /// <param name="hasAdditional">Доп.потребности</param>
        public static void SetRegsApproved(DataContext context, int idApprovedDoc, DateTime dateCommit, int idRegistratorEntity, int[] ids, int[] arrRegisters, bool? hasAdditional = null)
        {
            foreach (var arrRegister in arrRegisters)
            {
                SetApprovedInReg(context, idApprovedDoc, dateCommit, idRegistratorEntity, ids, arrRegister, hasAdditional);
            }
        }

        public static void SetTerminatorInRegister(DataContext context, int IdDoc, int IdDocEntity, DateTime dateterminate, int IdTerminator, int IdTerminatorEntity, int[] registerEntityIdsStatic)
        {
            foreach (var regId in registerEntityIdsStatic)
            {
                SetTerminatorInRegister(context, IdDoc, IdDocEntity, dateterminate, IdTerminator, IdTerminatorEntity, regId);                
            }
        }


        public static void SetTerminatorInRegister(DataContext context, int IdDoc, int IdDocEntity, DateTime dateterminate, int IdTerminator, int IdTerminatorEntity, int registerEntityIdStatic)
        {
            IQueryableDbSet<ICommonRegister> table = null;
            try
            {
                table = context.Set<ICommonRegister>(registerEntityIdStatic);
            }
            catch
            {
            }

            if (table == null) 
                return;
            

            foreach (var rec in table.Where(r => r.IdRegistrator == IdDoc && r.IdRegistratorEntity == IdDocEntity && !r.IdTerminator.HasValue))
            {
                rec.IdTerminator = IdTerminator;
                rec.IdTerminatorEntity = IdTerminatorEntity;
                rec.DateTerminate = dateterminate;
            }
        }

        

        public static void ClearApprovedInRegister(DataContext context, int iddoc, int IdApprovedEntity, int registerEntityIdStatic)
        {
            var table = context.Set<ICommonRegister>(registerEntityIdStatic);
            foreach (var rec in table.Where(w => w.IdApprovedEntity == IdApprovedEntity && w.IdApproved == iddoc))
            {
                rec.DateCommit = null;
                rec.IdApprovedEntity = null;
                rec.IdApproved = null;
            }
        }

        public static void SetApprovedInReg(DataContext context, int idApprovedDoc, DateTime dateCommit, int idRegistratorEntity, int[] ids,
                                            int registerEntityIdStatic, bool? hasAdditional = null )
        {
            if (hasAdditional.HasValue)
            {
                SetApprovedInReg(context, idApprovedDoc, dateCommit, idRegistratorEntity, ids, registerEntityIdStatic,
                                 hasAdditional.Value);
                return;
            }

            var table = context.Set<ICommonRegister>(registerEntityIdStatic);
            var qq = table.Where(w =>
                                 !w.IdTerminator.HasValue && !w.DateCommit.HasValue
                                 && w.IdRegistratorEntity == idRegistratorEntity && (ids.Contains(w.IdRegistrator) || w.IdRegistrator == idApprovedDoc)
                );
            foreach (var rec in qq)
            {
                rec.DateCommit = dateCommit;
                rec.IdApprovedEntity = idRegistratorEntity;
                rec.IdApproved = idApprovedDoc;
            }
        }

        public static void SetApprovedInReg(DataContext context, int idApprovedDoc, DateTime dateCommit, int idRegistratorEntity, int[] ids,
                                            int registerEntityIdStatic, bool hasAdditional)
        {
            var table = context.Set<IAddRegister>(registerEntityIdStatic);
            var qq = table.Where(w =>
                                 w.IsAdditionalNeed == hasAdditional && 
                                 !w.IdTerminator.HasValue && !w.DateCommit.HasValue
                                 && w.IdRegistratorEntity == idRegistratorEntity && (ids.Contains(w.IdRegistrator) || w.IdRegistrator == idApprovedDoc)
                );

            foreach (var rec in qq)
            {
                rec.DateCommit = dateCommit;
                rec.IdApprovedEntity = idRegistratorEntity;
                rec.IdApproved = idApprovedDoc;
            }
        }

        public static void SetApproveOrTerminateByAddValue(DataContext context, int[] arrIdParent, int idolddoc, int idnewdoc, int IdRegistratorEntity, int registrEntityIdStatic, DateTime date)
        {
            var table = context.Set<IAddRegister>(registrEntityIdStatic);
            var qq = table.Where(w =>
                                 !w.IdTerminator.HasValue && !w.DateCommit.HasValue
                                 && w.IdRegistratorEntity == IdRegistratorEntity && (arrIdParent.Contains(w.IdRegistrator) || w.IdRegistrator == idolddoc)
                );
            foreach (var rec in qq)
            {
                if (rec.IsAdditionalNeed)
                {
                    rec.DateTerminate = date;
                    rec.IdTerminatorEntity = IdRegistratorEntity;
                    rec.IdTerminator = idnewdoc;
                }
                else
                {
                    rec.DateCommit = date;
                    rec.IdApprovedEntity = IdRegistratorEntity;
                    rec.IdApproved = idnewdoc;
                }
            }
        }

        /// <summary>
        /// частный случай Утверждения\Аннулирования записей с Доп.потребностями - для регистра Объёмы финансовых средств
        /// </summary>
        public static void SetApproveOrTerminateByAddValueForLVA(DataContext context, int[] arrIdParent, int idolddoc, int idnewdoc, int IdRegistratorEntity, DateTime date)
        {
            var table = context.LimitVolumeAppropriations;
            var qq = table.Where(w =>
                                 !w.DateCommit.HasValue
                                 && w.IdRegistratorEntity == IdRegistratorEntity && (arrIdParent.Contains(w.IdRegistrator) || w.IdRegistrator == idolddoc)
                );

            List<LimitVolumeAppropriations> iLimitVolumeAppropriations = new List<LimitVolumeAppropriations>();
            foreach (var rec in qq)
            {
                if (rec.HasAdditionalNeed ?? false)
                {
                    iLimitVolumeAppropriations.Add(rec);
                }
                else
                {
                    rec.DateCommit = date;
                    rec.IdApprovedEntity = IdRegistratorEntity;
                    rec.IdApproved = idnewdoc;
                }
            }

            foreach (var rec in iLimitVolumeAppropriations)
            {
                rec.Value = - rec.Value;
                rec.IdRegistrator = idnewdoc;
                rec.DateCommit = date;
                rec.IdApprovedEntity = IdRegistratorEntity;
                rec.IdApproved = idnewdoc;
            }

            context.LimitVolumeAppropriations.InsertAsTableValue(iLimitVolumeAppropriations, context);
            context.SaveChanges();

        }
    }
}
