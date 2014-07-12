using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Linq.Dynamic;
using System.Text;
using System.Threading.Tasks;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Common.Interfaces;
using Platform.BusinessLogic.EntityTypes;
using Platform.Common.Exceptions;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Reference;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Logic;
using Sbor.Reference;
using Sbor.Registry;

namespace Sbor
{
    public static class CommonMethods
    {
        /// <summary>
        /// Создает копию с заполненными КБК
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="line"></param>
        /// <returns></returns>
        public static T CloneAsLineCost<T> (this ILineCost line) where T:ILineCost, new()
        {
            var result = new T();

            result.SetLineCostValues(line);

            return result;
        }

        /// <summary>
        /// Установить в result значения для всех кодов КБК как в line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="result"></param>
        /// <typeparam name="T"></typeparam>
        public static void SetLineCostValues<T>(this T result, ILineCost line) where T : ILineCost
        {
            foreach (var property in typeof(ILineCost).GetProperties())
            {
                var value = property.GetValue(line);
                property.SetValue(result, value);
            }
        }

        private static Dictionary<int, int> HierarchyPeriods = new Dictionary<int, int>();

        /// <summary>
        /// Получить idHierarchy из года
        /// </summary>
        /// <param name="year"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// <exception cref="PlatformException"></exception>
        public static int GetIdHierarchyPeriodYear(this int year, DataContext context)
        {
            if (HierarchyPeriods.ContainsKey(year))
                return HierarchyPeriods[year];

            var dateStart = new DateTime(year, 1, 1);
            var dateEnd = dateStart.AddYears(1).AddDays(-1);

            var hierarchyPeriod =
                context.HierarchyPeriod.FirstOrDefault(
                    h => h.DateStart == dateStart && h.DateEnd == dateEnd);

            if (hierarchyPeriod == null)
                throw new PlatformException("В справочнике 'Иерархия периодов' отсутствует год " + year);

            HierarchyPeriods.Add(year, hierarchyPeriod.Id);
            return hierarchyPeriod.Id;
        }

        /// <summary>
        /// Получить номер для вновь создающегося документа
        /// </summary>
        /// <param name="values"></param>
        /// <returns></returns>
        public static string GetNextCode(this IEnumerable<string> values)
        {
            int maxNum = 0;

            foreach (string s in values)
            {
                int i;
                if (Int32.TryParse(s, out i))
                {
                    maxNum = (maxNum < i ? i : maxNum);
                }
            }

            var r = (maxNum + 1).ToString(CultureInfo.InvariantCulture);
            return r;
        }

        /// <summary>
        /// По перечислению полей сгенерировать строку для подстановки в запрос
        /// </summary>
        /// <param name="values">Поля</param>
        /// <param name="alias">Псевдоним</param>
        /// <returns></returns>
        public static string GetQueryString<T>(this IEnumerable<T> values, string alias = "")
        {
            var result = new StringBuilder();

            alias = alias.Trim(' ').TrimEnd('.');
            if (!String.IsNullOrEmpty(alias))
                alias = alias + ".";
            else
                alias = String.Empty;
            
            foreach (var value in values)
                result.AppendFormat(" {0}{1},", alias, value);
            
            if (result.Length > 0)
                result.Remove(result.Length - 1, 1);
            
            return result.ToString().Trim(' ');
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="values">Поля</param>
        /// <param name="separator"></param>
        /// <returns></returns>
        public static string GetString<T>(this IEnumerable<T> values, string separator = " ")
        {
            return String.Join(separator, values);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <param name="entityScheme"></param>
        /// <param name="entityName"></param>
        /// <param name="parentNumber"></param>
        /// <returns></returns>
        public static string GetNextRevisionDb(DataContext context, string entityScheme, string entityName, string parentNumber)
        {
            var textCommand = String.Format(
             @"WITH DocumentTree(id, idParent) AS 
                (SELECT id, idParent AS Document FROM [{0}].[{1}] 
                    WHERE Number = '{2}' 
                    UNION ALL 
                    SELECT d.id, d.idParent FROM [{0}].[{1}] d 
                    INNER JOIN DocumentTree t ON d.idParent = t.id ) 
              SELECT Count(*) FROM DocumentTree DT ", entityScheme, entityName, parentNumber);

            return context.Database.SqlQuery<int>(textCommand).FirstOrDefault().ToString();
        }

        /// <summary>
        /// Получить следующий номер для документа
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static string GetDocNextNumber<T>(this T document, DataContext context) where T : DocumentEntity<T>, IHierarhy
        {
            var baseNumber = document.Number.Split('.').First();
            return baseNumber + "." + GetNextRevisionDb(context, "doc", typeof(T).Name, baseNumber);
        }
        
        /// <summary>
        /// Строим запрос к таблице, содержащей строку КБК
        /// </summary>
        /// <param name="result"></param>
        /// <param name="line"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static IQueryable<T> ApplyWhere<T>(this IQueryable<T> result, ILineCost line ) where T : ILineCost
        {
            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pValue = p.GetValue(line) as int? ?? p.GetValue(line) as byte?;
                if (pValue.HasValue)
                    result = result.Where( String.Format("{0} == {1}", p.Name, pValue.Value) );
            }

            return result;
        }

        /// <summary>
        /// Возвращает Id всех предыдущих версий документа + Id текущей версии
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetIdAllVersion<T>(this T document, DataContext context) where T: DocumentEntity<T>, IHierarhy
        {
            var textCommand = String.Format(
            @"WITH DocumentTree(id, idParent) AS 
                (SELECT id, idParent AS Document 
                    FROM  [{0}].[{1}] 
                    WHERE id = {2} 
                    UNION ALL 
                    SELECT d.id, d.idParent FROM  [{0}].[{1}] d 
                    INNER JOIN DocumentTree t ON t.idParent = d.id ) 
              SELECT id FROM DocumentTree DT", "doc", typeof(T).Name, document.Id);

            return context.Database.SqlQuery<int>(textCommand).ToList();
        }

        /// <summary>
        /// Вовзвращает идентификаторы всех предков
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static IEnumerable<int> GetIdAllPreviousVersion<T>(this T document, DataContext context) where T : DocumentEntity<T>, IHierarhy
        {
            var textCommand = String.Format(
            @"WITH DocumentTree(id, idParent) AS 
                (SELECT id, idParent AS Document 
                    FROM  [{0}].[{1}] 
                    WHERE id = {2} 
                    UNION ALL 
                    SELECT d.id, d.idParent FROM  [{0}].[{1}] d 
                    INNER JOIN DocumentTree t ON t.idParent = d.id )  
              SELECT DT.id FROM DocumentTree DT 
              WHERE DT.id <> {2}", "doc", typeof(T).Name, document.Id);

            return context.Database.SqlQuery<int>(textCommand).ToList();
        }

        /// <summary>
        /// Вовзвращает идентификаторы всех предков
        /// </summary>
        /// <param name="document"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static int GetIdFirstVersion<T>(this T document, DataContext context) where T : DocumentEntity<T>, IHierarhy
        {
            var textCommand = String.Format(
            @"WITH DocumentTree(id, idParent) AS 
                (SELECT id, idParent AS Document 
                    FROM  [{0}].[{1}] 
                    WHERE id = {2} 
                    UNION ALL 
                    SELECT d.id, d.idParent FROM  [{0}].[{1}] d 
                    INNER JOIN DocumentTree t ON t.idParent = d.id )  
              SELECT DT.id FROM DocumentTree DT 
              WHERE DT.idParent IS NULL ", "doc", typeof(T).Name, document.Id);

            return context.Database.SqlQuery<int>(textCommand).FirstOrDefault();
        }

        /// <summary>   
        /// Получение предыдущей версии документа
        /// </summary>         
        public static IHierarhy GetPrevVersionDoc(DataContext context, IHierarhy curdoc, int entityId)
        {
            if (!curdoc.IdParent.HasValue)
                return null;

            return context.Set<IHierarhy>(entityId).FirstOrDefault(w => w.Id == curdoc.IdParent);
        }

        /// <summary>
        /// Подлучение первой версии документа
        /// </summary>
        public static IHierarhy GetFirstVersionDoc<T>(DataContext context, T document, int entityId) where T : DocumentEntity<T>, IHierarhy
        {
            var idFirstVersion = document.GetIdFirstVersion(context);

            return context.Set<IHierarhy>(entityId).FirstOrDefault(w => w.Id == idFirstVersion);
        }

        /// <summary>
        /// Возвращает дату, соответствующую последнему дню указанного года.
        /// </summary>
        /// <param name="vDateEnd"></param>
        /// <returns></returns>
        public static DateTime DateYearEnd(this DateTime vDateEnd)
        {
            return new DateTime(vDateEnd.Year + 1, 1, 1).AddDays(-1);
        }

        /// <summary>
        /// Возвращает дату, соответствующую первому дню указанного года.
        /// </summary>
        /// <param name="vDateStart"></param>
        /// <returns></returns>
        public static DateTime DateYearStart(this DateTime vDateStart)
        {
            return new DateTime(vDateStart.Year, 1, 1);
        }

        /// <summary>
        /// Получить идентификатор последней версии документа
        /// </summary>
        /// <param name="context"></param>
        /// <param name="documentId">Идентификатор документа</param>
        /// <param name="documentEntityId">Идентификатор сущности документа</param>
        /// <returns></returns>
        public static int? FindLastRevisionId(DataContext context, int documentEntityId, int documentId)
        {
            var query = @"Select [dbo].[GetLastVersionId](" + documentEntityId + ", " + documentId + ")";
            var result = context.Database.SqlQuery<int>(query).FirstOrDefault();

            return result;
        }

        /// <summary>
        /// Получаем словарик значений 
        /// Для каждого мероприятия указан список строк ТЧ, в которых имеются неправильные значения КБК.
        /// Сами неправильные значения выделены жЫрным.
        /// </summary>
        /// <param name="document">Документ, для которого хотим проверить версионные КБК в ТЧ</param>
        /// <param name="blank">Бланк для ТЧ</param>
        /// <param name="tpType">Тип ТЧ</param>
        /// <param name="masterName">Имя ТЧ Мастера</param>
        /// <param name="context"></param>
        /// <param name="nonVersionedKBKs">Игнорируемые коды</param>
        /// <param name="optionalText">Дополнительный текст, например имя вкладки</param>
        /// <param name="masterProperty">Свойство, через которое идет связь с мастером</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetWrongVersioningKBK<T>(this T document,
                                                                                ISBP_Blank blank,
                                                                                Type tpType,
                                                                                string masterName,
                                                                                DataContext context,
                                                                                string[] nonVersionedKBKs,
                                                                                string optionalText = "",
                                                                                string masterProperty = "idMaster"
                                                                                )
            where T : DocumentEntity<T>
        {
            if (tpType.GetInterface(typeof(ILineCost).Name) == null)
                throw new PlatformException("ТЧ не является строкой с КБК!");

            var result = new Dictionary<string, List<string>>();

            var versioningKBK = blank.GetVersioningKbk(nonVersionedKBKs);

            var selectQuery = new StringBuilder(@"Select ('" + optionalText + "' + A.Caption + (Case When C.Caption is not null Then ' ( ' + C.Caption + ' ) ' Else '' End)) as [Key], '- Вид бюджетной деятельности " + ActivityBudgetaryType.Costs.Caption() + "' ");

            var fromQuery = new StringBuilder(@"From [doc].[" + typeof(T).Name + @"] D  
                                                Inner Join [tp].[" + tpType.Name + @"] T on (D.[id] = T.[idOwner]) 
                                                Inner Join [tp].[" + masterName + @"] M on (M.[id] = T.[" + masterProperty + @"] )
	                                            Inner Join [ref].[Activity] A on (A.[id] = M.[idActivity] )
	                                            Left Join [ref].[Contingent] C on (C.[id] = M.[idContingent] )");

            var whereQuery = new StringBuilder("Where D.[id] = " + document.Id.ToString() + " ");

            bool hasWhereConditions = false;

            foreach (var p in versioningKBK.MandatoryVersioning.ToList())
            {
                selectQuery.AppendFormat(@" + ( Case When ( ({0}.[ValidityFrom] Is Not Null And {0}.[ValidityFrom] > D.[Date]) 
                                                                OR ( {0}.ValidityTo IS NOT NULL AND {0}.[ValidityTo] <= D.[Date] ))
							                     Then ', <b>{2} ' + {0}.{1} + '</b>'
							                     Else  ', {2} ' + {0}.{1} 
							                     End )", p, KBKHelper.CaptionFields[p], KBKHelper.RussianCaptions[p]);

                fromQuery.AppendFormat(" Inner Join [{1}].[{0}] {0} on ({0}.id = T.[id{0}]) ", p, p == "ExpenseObligationType" ? "enm" : "ref");

                if (!hasWhereConditions)
                    whereQuery.Append("And (");

                whereQuery.AppendFormat(
                    @" {0} ( ({1}.[ValidityFrom] Is Not Null And {1}.[ValidityFrom] > D.[Date]) 
                        Or ({1}.[ValidityTo] Is Not Null And {1}.[ValidityTo] <= D.[Date])) ",
                                                                                                hasWhereConditions ? "Or" : " And ( ",
                                                                                                p);
                hasWhereConditions = true;
            }

            foreach (var p in versioningKBK.OptionalVersioning.ToList())
            {
                selectQuery.AppendFormat(@" + ( Case When ( {0}.{1} Is Not Null ) Then 
						                        Case When ( ({0}.[ValidityFrom] Is Not Null And {0}.[ValidityFrom] > D.[Date]) 
                                                                OR ( {0}.ValidityTo IS NOT NULL AND {0}.[ValidityTo] <= D.[Date] ))
							                         Then ', <b>{2} ' + {0}.{1} + '</b>'
							                         Else  ', {2} ' + {0}.{1} 
							                         End
					                            Else '' End)", p, KBKHelper.CaptionFields[p], KBKHelper.RussianCaptions[p]);

                fromQuery.AppendFormat(" Left Join [{1}].[{0}] {0} on ({0}.id = T.[id{0}]) ", p, p == "ExpenseObligationType" ? "enm" : "ref");

                if (!hasWhereConditions)
                    whereQuery.Append("And (");

                whereQuery.AppendFormat(
                    @" {0} (({1}.[ValidityFrom] Is Not Null And {1}.[ValidityFrom] > D.[Date]) 
                                Or ({1}.[ValidityTo] Is Not Null And {1}.[ValidityTo] <= D.[Date])) ",
                                                                                                hasWhereConditions ? "Or" : "",
                                                                                                p);
                hasWhereConditions = true;
            }

            foreach (var p in versioningKBK.MandatoryNonVersioned.ToList())
            {
                selectQuery.AppendFormat(@" + ( ', {2} ' + {0}.{1} )", p, KBKHelper.CaptionFields[p], KBKHelper.RussianCaptions[p]);
                fromQuery.AppendFormat(" Inner Join [{1}].[{0}] {0} on ({0}.id = T.[id{0}]) ", p, p == "ExpenseObligationType" ? "enm" : "ref");
            }

            foreach (var p in versioningKBK.OptionalNonVersioned.ToList())
            {
                selectQuery.AppendFormat(@"+ ( Case When ( {0}.{1} Is Not Null ) 
                                                Then ', {2} ' + {0}.{1} 
							 					Else '' End)", p, KBKHelper.CaptionFields[p], KBKHelper.RussianCaptions[p]);

                fromQuery.AppendFormat(" Left Join [{1}].[{0}] {0} on ({0}.id = T.[id{0}]) ", p, p == "ExpenseObligationType" ? "enm" : "ref");
            }

            selectQuery.Append(" as Value ");
            if (hasWhereConditions)
                whereQuery.Append(")");

            var query = selectQuery.Append(fromQuery).Append(whereQuery).ToString();

            var errors = context.Database.SqlQuery<ErrorResult>(query).ToList();
            foreach (var error in errors)
            {
                if (result.ContainsKey(error.Key))
                    result[error.Key].Add(error.Value);
                else
                    result.Add(error.Key, new List<string> { error.Value });
            }

            return result;
        }

        /// <summary>
        /// Получаем словарик значений 
        /// Для каждого мероприятия указан список строк ТЧ, в которых имеются неправильные значения КБК.
        /// Сами неправильные значения выделены жЫрным.
        /// </summary>
        /// <param name="document">Документ, для которого хотим проверить версионные КБК в ТЧ</param>
        /// <param name="blank">Бланк для ТЧ</param>
        /// <param name="tpType">Тип ТЧ</param>
        /// <param name="masterName">Имя ТЧ Мастера</param>
        /// <param name="context"></param>
        /// <param name="optionalText">Дополнительный текст, например имя вкладки</param>
        /// <param name="masterProperty">Свойство, через которое идет связь с мастером</param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Dictionary<string, List<string>> GetWrongVersioningKBK<T>(this T document, 
                                                                                ISBP_Blank blank, 
                                                                                Type tpType, 
                                                                                string masterName, 
                                                                                DataContext context, 
                                                                                string optionalText = "", 
                                                                                string masterProperty = "idMaster") where T : DocumentEntity<T>
        {
            return GetWrongVersioningKBK(document, blank, tpType, masterName, context, new string[]{}, optionalText, masterProperty);
        }

        public static bool IsRound100(decimal value)
        {
            return Math.Round(value / 100) * 100 == value;
        }

    }

    public class ErrorResult
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }

    /// <summary>
    /// 
    /// </summary>
    public class RegCommLink
    {
        /// <summary>
        /// 
        /// </summary>
        public Entity RegistratorEntity;
        /// <summary>
        /// 
        /// </summary>
        public int? IdRegistrator;
    }
}
