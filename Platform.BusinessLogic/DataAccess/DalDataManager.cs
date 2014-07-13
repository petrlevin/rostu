using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Platform.Common.Exceptions;
using Platform.Dal.QueryBuilders;
using Platform.Dal.QueryBuilders.QueryParts;
using Platform.Log;
using Platform.PrimaryEntities.Common.DbEnums;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Collections;

namespace Platform.BusinessLogic.DataAccess
{
    /// <summary>
    /// менеджер использует DAL
    /// </summary>
    public class DalDataManager : DataManager
    {
        #region
        
        /// <summary>
        /// Конструктор дата менеджера для сущности <see cref="source"/>
        /// </summary>
        /// <param name="dbConnection">Соединение с SQL</param>
        /// <param name="source">Сущность</param>
        public DalDataManager(SqlConnection dbConnection, IEntity source)
            : base(dbConnection, source)
        {
        }

        /// <summary>
        /// Конструктор дата менеджера для сущности <see cref="source"/> с визуальной формой <see cref="form"/>
        /// </summary>
        /// <param name="dbConnection">Соединение с SQL</param>
        /// <param name="source">Сущность</param>
        /// <param name="form">Форма сущности</param>
        public DalDataManager(SqlConnection dbConnection, IEntity source, Form form)
            : base(dbConnection, source, form)
        {
        }
        #endregion

        #region Update

        protected override int DoUpdateEntry(int itemId, Dictionary<string, object> values)
        {
            var filter = new FilterConditions() { Field = "id", Operator = ComparisionOperator.Equal, Value = itemId };
            UpdateEntries(filter, values);
            return itemId;
        }

        protected override void DoUpdateEntries(int[] itemIds, Dictionary<string, object> values)
        {
            var filter = new FilterConditions() { Field = "id", Operator = ComparisionOperator.InList, Value = itemIds };
            UpdateEntries(filter, values);
        }

        /// <summary>
        /// Изменить несколько элементов
        /// </summary>
        /// <param name="filter">Условия отбора изменяемых элементов</param>
        /// <param name="values">Значения параметров</param>
        /// <exception cref="PlatformException"></exception>
        public int UpdateEntries(FilterConditions filter, Dictionary<string, object> values)
        {
            CheckValues(values);
            var builder = new UpdateQueryBuilder(Source, values)
            {
                Conditions = filter
            };


            SqlCommand sqlCmd = builder.GetSqlCommand(DbConnection);
            return sqlCmd.ExecuteNonQueryLog();

        }


        #endregion

        #region Delete

        /// <summary>
        /// Удалить элементы
        /// </summary>
        /// <param name="filter">Условия отбора удаляемых элементов</param>
        /// <returns></returns>
        public bool DeleteItem(FilterConditions filter)
        {
            try
            {
                using (TransactionScope transaction = CreateTransaction())
                {
                    var result = DoDeleteItem(filter);
                    transaction.Complete();
                    return result;
                }

            }
            catch (PlatformException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new PlatformException("Ошибка при удалении экземпляров сущности. Обращайтесь к разработчику. ", ex);
            }
        }

        protected bool DoDeleteItem(FilterConditions filter)
        {
            var builder = new DeleteQueryBuilder(Source);

            builder.Conditions = filter;
            SqlCommand sqlCmd = builder.GetSqlCommand(DbConnection);
            int result = sqlCmd.ExecuteNonQueryLog();
            return result == 1;

        }

        protected override bool DoDeleteItem(int[] itemIds)
        {
            if (itemIds.Length == 0)
                return false;

            var filter = new FilterConditions()
            {
                Field = "id", // ToDo: использовать константу
                Operator = ComparisionOperator.InList,
                Value = itemIds
            };

            return DoDeleteItem(filter);
        }
        #endregion

        public override int CreateNewVersionEntityItem(int idItem)
        {
            throw new NotImplementedException();
            SelectQueryBuilder selectQueryBuilder = new SelectQueryBuilder(Source)
                                                        {
                                                            Conditions = new FilterConditions()
                                                                             {
                                                                                 Field = "id",
                                                                                 Value = idItem
                                                                             }
                                                        };
            
            IDictionary<string, object> currentItem = GetDataSet(selectQueryBuilder).Rows.FirstOrDefault();
            DateTime date = DateTime.Now;
            if (currentItem == null)
                throw new Exception("Не найден элемент сущности '" + Source.Name + "' с идентификатором - " + idItem.ToString());
            if (currentItem.ContainsKey("ValidityTo") && currentItem["ValidityTo"] != null)
                throw new Exception("Можно создавать тоолько на основе актуального элемента");
            if (currentItem.ContainsKey("ValidityTo") && currentItem["ValidityTo"] == null)
                currentItem["ValidityTo"] = date;
            int idRoot;
            if (currentItem.ContainsKey("idRoot") && currentItem["idRoot"] != null)
                idRoot = (int)currentItem["idRoot"];
            else
                idRoot = (int)currentItem["id"];

            IgnoreCaseDictionary<object> newItem =
                new IgnoreCaseDictionary<object>(
                    currentItem.Where(
                        valuePair =>
                        !(new string[] { "id", "tstamp", "validityto", "validityfrom", "idroot" }).Contains(valuePair.Key)).
                        ToDictionary(valuePair => valuePair.Key, valuePair => valuePair.Value)) { { "idRoot", idRoot }, { "ValidityFrom", date } };

            int result;
            using (var transaction = CreateTransaction())
            {
                int resultUpdate = DoUpdateEntry(idItem, new Dictionary<string, object> { { "ValidityTo", date } });
                int? resultInsert = DoCreateEntry(newItem);

                if (!resultInsert.HasValue || resultUpdate == 0)
                    throw new Exception("Идентификатор новой или обновляенной записи не был получен");

                result = resultInsert.Value;
                transaction.Complete();
            }
            return result;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values"></param>
        /// <returns>Возвращает null в случае ошибки.</returns>
        protected override int? DoCreateEntry(Dictionary<String, object> values)
        {
            base.DoCreateEntry(values);
            var builder = new InsertQueryBuilder(Source, values) { ReturnIdentityValue = true };
            SqlCommand sqlCmd = builder.GetSqlCommand(DbConnection);
            //sqlCmd.ExecuteScalar();

            // ToDo: переписать правильно
            //sqlCmd = new SqlCommand("SELECT SCOPE_IDENTITY();", DbConnection);
            object newId = sqlCmd.ExecuteScalarLog();
            return Convert.ToInt32(newId);
        }

        /// <summary>
        /// Копирование подчиненных ТЧ и мультилинков из Source в Target
        /// </summary>
        /// <param name="sourceItem"> </param>
        /// <param name="targetItem"> </param>
        /// <returns></returns>
        protected override int? DoCloneInternalPartEntry(object sourceItem, object targetItem)
        {
            ///получить все ТЧ и мультлинки, который ссылаются только на документ
            List<EntityField> fieldsTpMl =
                Source.Fields.Cast<EntityField>().Where(
                    a => a.EntityFieldType == EntityFieldType.Tablepart || a.EntityFieldType == EntityFieldType.Multilink).ToList();
            List<int> entitiesId = fieldsTpMl.Select(a => a.IdEntityLink.Value).ToList();
            var fieldsTpMlStart =
                fieldsTpMl.Where(
                    a => !a.EntityLink.Fields.Any(b => entitiesId.Where(c => c != a.IdEntityLink).Contains(b.IdEntityLink.Value)));
            throw new NotImplementedException();
        }
    }
}
