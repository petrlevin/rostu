using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic;
using System.Reflection;
using System.Text;
using Platform.BusinessLogic;
using Platform.BusinessLogic.Activity.Controls;
using Platform.BusinessLogic.Common.Exceptions;
using Platform.BusinessLogic.EntityTypes.Interfaces;
using Platform.Common.Extensions;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Sbor.DbEnums;
using Sbor.Interfaces;
using Sbor.Registry;

namespace Sbor.Logic
{
    public static class EstimatedLineHelper
    {
		public static string ToStringAsEstimatedLine(this ILineCost line, DataContext context)
		{
			List<string> inputValues = new List<string>();
			StringBuilder outputValues = new StringBuilder();
			StringBuilder joins = new StringBuilder();
			int i = 1;
			bool isFirst = true;
			foreach (PropertyInfo propertyInfo in typeof(ILineCost).GetProperties())
			{
				string fieldName = propertyInfo.Name;
				string tableName = fieldName.Substring(2);
				object value = propertyInfo.GetValue(line);
				if (value != null)
				{
					inputValues.Add(string.Format("{0} as [{1}]", value, fieldName));
					joins.AppendFormat(" INNER JOIN [{3}].[{0}] AS [a{2}] ON [a{2}].[id]=[a].[{1}]", tableName, fieldName, i, fieldName != "IdExpenseObligationType" ? "ref" : "enm");
					outputValues.AppendFormat("{3}'{0} '+[a{1}].[{2}]", KBKHelper.RussianCaptions[tableName], i,
					                          KBKHelper.CaptionFields[tableName], isFirst ? "" : "+' '+");
					isFirst = false;
					i++;
				}
			}
			string commandText = string.Format("SELECT {0} FROM (SELECT {1}) a {2}", outputValues, string.Join(",", inputValues), joins);
			return Convert.ToString(context.ExecuteScalarCommand(commandText));
		}

		public static Dictionary<int, int> GetEstimatedLineSet(ISBP_Blank blank, FindParamEstimatedLine param, string tpName)
        {
            var result = new Dictionary<int, int>();

            return result;
        }


        /// <summary>
        /// Получение сметной строки для документа (должна вызываться в транзакции, т.е. из операции документа)
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="iddoc">ид документа</param>
        /// <param name="identitydoc">ид сущности документа</param>
        /// <param name="blank">бланк по которому будем создавать/искать сметную строку</param>
        /// <param name="line">строка с кодами КБК</param>
        /// <param name="param">параметры для поиска и создания сметной строки</param>
        /// <returns></returns>
        /// <exception cref="ControlResponseException"></exception>
        public static EstimatedLine GetLine(this ILine line, DataContext context, int iddoc, int identitydoc, ISBP_Blank blank, FindParamEstimatedLine param)
        {
            var lineId = line.GetLineId(context, iddoc, identitydoc, blank, param);

            var estimatedLine = context.EstimatedLine.FirstOrDefault(e => e.Id == lineId);

            return estimatedLine;

        }

        /// <summary>
        /// Получение списка сметных строк в виде словаря
        /// </summary>
        /// <param name="input">Набор на основе которого ищутся сметные строки</param>
        /// <param name="context">Контекст</param>
        /// <param name="iddoc">Идентификатор документа</param>
        /// <param name="identitydoc">Идентификатор сущности документа</param>
        /// <param name="blank">Бланк</param>
        /// <param name="param">Параметры поиска</param>
        /// <typeparam name="T">Тип элементов набора</typeparam>
        /// <returns></returns>
        public static Dictionary<int, int> GetLinesId<T>(this IQueryable<T> input, DataContext context, int iddoc, int identitydoc, ISBP_Blank blank, FindParamEstimatedLine param) where T : class, ILine
		{
			Dictionary<int, int> result = new Dictionary<int, int>();
			if (param.TypeLine == ActivityBudgetaryType.Costs) // расходы
			{
				if (!(input is IQueryable<ILineCost>))
					return null;
			    if (!input.Any())
			        return null;
				result = input.GetLinesCostId(context, iddoc, identitydoc, blank, param);
			}
			else
			{
				Controls.Throw(string.Format(
					"Не реализовано создание сметных строк для вида бюджетной деятельности {0}",
					param.TypeLine.ToString()
				));
			}
			return result;
		}

        /// <summary>
        /// Получение сметной строки для документа (должна вызываться в транзакции, т.е. из операции документа)
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="iddoc">ид документа</param>
        /// <param name="identitydoc">ид сущности документа</param>
        /// <param name="blank">бланк по которому будем создавать/искать сметную строку</param>
        /// <param name="line">строка с кодами КБК</param>
        /// <param name="param">параметры для поиска и создания сметной строки</param>
        /// <returns></returns>
        /// <exception cref="ControlResponseException"></exception>
        public static int? GetLineId(this ILine line, DataContext context, int iddoc, int identitydoc, ISBP_Blank blank, FindParamEstimatedLine param)
        {
            int? res = null;

            if (param.TypeLine == ActivityBudgetaryType.Costs) // расходы
            {
                if (!(line is ILineCost))
                    return null;

                res = (line as ILineCost).GetLineCostId(context, iddoc, identitydoc, blank, param);
            }
            else
            {
                Controls.Throw(string.Format(
                    "Не реализовано создание сметных строк для вида бюджетной деятельности {0}",
                    param.TypeLine.ToString()
                ));
            }

            return res;
        }

		private const string _prefixSpbBlankPropertie = "IdBlankValueType_";

        private static Dictionary<int, int> GetLinesCostId<T>(this IQueryable<T> input, DataContext context, int iddoc, int identitydoc,
                                         ISBP_Blank blank, FindParamEstimatedLine param) where T : class
        {
	        Dictionary<int, int> result = new Dictionary<int, int>();
	        string typeName = typeof (T).Name;
	        Entity entity = Objects.ByName<Entity>(typeName);
	        StringBuilder baseSelect = new StringBuilder();
			baseSelect.AppendFormat("FROM [{0}].[{1}] a LEFT OUTER JOIN [reg].[EstimatedLine] el ON", entity.Schema, entity.Name);
			StringBuilder insertCommand = new StringBuilder("INSERT INTO [reg].[EstimatedLine] ([idPublicLegalFormation], [idBudget], [idSBP], [idActivityBudgetaryType]");

			var notNullFields = new List<string>();
			var bvtyp = new List<byte?> { (byte)BlankValueType.Mandatory };
			
			if (!param.IsRequired)
				bvtyp.Add((byte)BlankValueType.Optional);
	        baseSelect.AppendFormat(" [el].[idBudget]={0} AND [el].[idSBP]={1} AND [el].[idActivityBudgetaryType]={2}",
	                                param.IdBudget, param.IdSbp, (byte) param.TypeLine);
			foreach (var p in typeof(ILineCost).GetProperties())
			{
				var pName = p.Name.Substring(2);

				var blankPropertie = typeof (ISBP_Blank).GetProperty(_prefixSpbBlankPropertie + pName);
				var blankValue = (byte?) blankPropertie.GetValue(blank);

				if (bvtyp.Contains(blankValue))
				{
				    if (blankValue == (byte) BlankValueType.Mandatory)
				    {
				        baseSelect.AppendFormat(" AND [a].[{0}]=[el].[{0}]", "id" + pName);
				    }
				    else
				    {
                        baseSelect.AppendFormat(" AND ([a].[{0}]=[el].[{0}] OR ([a].[{0}] IS NULL AND [el].[{0}] IS NULL))", "id" + pName);
				    }
				    notNullFields.Add("id" + pName);
				}
				else
				{
					baseSelect.AppendFormat(" AND [el].[{0}] IS NULL", "id" + pName);

				}
			}
	        insertCommand.Append(", " + string.Join(", ", notNullFields.Select(a => "[" + a + "]")));
	        
			baseSelect.AppendFormat(" WHERE [a].[idOwner]={0}", iddoc);

            if(input.Any())
            {
                var ids = (input as IQueryable<IIdentitied>).Select(s => s.Id).ToList();
                baseSelect.AppendFormat(" AND [a].[Id] IN ({0})", string.Join(",",ids));
            }

            using (SqlCommand command = (SqlCommand)context.Database.Connection.CreateCommand())
			{
				if (context.Database.Connection.State != ConnectionState.Open)
					context.Database.Connection.Open();
				if (param.IsCreate)
				{
					command.CommandText = insertCommand.ToString() +
					                      string.Format(") SELECT DISTINCT {0}, {1}, {2}, {3}, ", param.IdPublicLegalFormation, param.IdBudget,
					                                    param.IdSbp, (byte) param.TypeLine) +
					                      string.Join(", ", notNullFields.Select(a => "[a].[" + a + "] ")) + baseSelect +
					                      " AND [el].[id] IS NULL";
					int count = command.ExecuteNonQuery();
				}
				command.CommandText = "SELECT [a].[id], [el].[id] " + baseSelect + " AND [el].[id] IS NOT NULL";
				using (SqlDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						result.Add(reader.GetInt32(0), reader.GetInt32(1));
					}
					reader.Close();
				}
				context.Database.Connection.Close();
			}
	        return result;
        }
        
		
		/// <summary>
        /// Получение сметной строки для документа расходов
        /// </summary>
        /// <param name="context">контекст</param>
        /// <param name="iddoc">ид документа</param>
        /// <param name="identitydoc">ид сущности документа</param>
        /// <param name="blank">бланк по которому будем создавать/искать сметную строку</param>
        /// <param name="line">строка с кодами КБК</param>
        /// <param name="param">параметры для поиска и создания сметной строки</param>
        /// <returns></returns>
        /// <exception cref="ControlResponseException"></exception>
        public static int? GetLineCostId(this ILineCost line, DataContext context, int iddoc, int identitydoc,
                                         ISBP_Blank blank, FindParamEstimatedLine param)
        {
            int? res = null;


            StringBuilder textCommand = new StringBuilder(string.Format(
                "SELECT el.[id] FROM [reg].[EstimatedLine] AS el {0} WHERE [idBudget] = {1} AND el.[idSBP] = {2} AND el.[idActivityBudgetaryType] = {3} ",
                (param.IsKosgu000 ? "LEFT JOIN ref.KOSGU AS kosgu ON kosgu.[id] = el.[idKOSGU]" : ""),
                param.IdBudget,
                param.IdSbp,
                (byte)param.TypeLine
                                                              ));

            Dictionary<string, int?> values = blank.GetLineReductedToBlank(line, param.IsRequired);

            foreach (var li in values)
            {
                if (param.IsKosgu000 && li.Key.Equals("idKOSGU", StringComparison.OrdinalIgnoreCase))
                {
                    textCommand.AppendLine(string.Format(" AND (el.[{0}] = {1} OR Replace(kosgu.[Code],'0','') = '') ",
                                                         li.Key, li.Value));
                }
                else
                {
                    textCommand.AppendLine(string.Format(" AND el.[{0}] {1} ", li.Key,
                                                         li.Value.HasValue ? " = " + li.Value : "IS NULL"));
                }
            }

            if (param.IsKosgu000)
                textCommand.AppendLine(" ORDER BY CASE WHEN Replace(kosgu.[Code],'0','') = '' THEN 1 ELSE 0 END");

            res = context.Database.SqlQuery<int?>(textCommand.ToString()).FirstOrDefault();

            if (param.IsCreate && !res.HasValue)
            {
                EstimatedLine newEstLine = context.EstimatedLine.Create();

                foreach (var p in typeof(ILineCost).GetProperties())
                {
                    int? value = null;
                    if (values.TryGetValue(p.Name, out value))
                    {
                        var estLinePropertie = typeof(EstimatedLine).GetProperty(p.Name);
                        if (estLinePropertie.PropertyType == typeof(byte?))
                        {
                            estLinePropertie.SetValue(newEstLine, (byte?)value);
                        }
                        else
                            estLinePropertie.SetValue(newEstLine, value);
                    }
                }

                newEstLine.IdActivityBudgetaryType = (byte)param.TypeLine;
                newEstLine.IdBudget = param.IdBudget;
                newEstLine.IdSBP = param.IdSbp;
                newEstLine.IdPublicLegalFormation = param.IdPublicLegalFormation;
                newEstLine.Caption = Guid.NewGuid().ToString();

                context.EstimatedLine.Add(newEstLine);
                context.SaveChanges();

                res = newEstLine.Id;
            }

            return res;
        }
        
        /// <summary>
        /// Получить EF сущность, точно соответствующую набору КБК из line.
        /// !!!В базе такая сущность может отсутствовать!!!
        /// </summary>
        /// <param name="line"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static EstimatedLine GetEstimatedLine(this ILineCost line, DataContext context)
        {
            var result = (IQueryable<EstimatedLine>)context.EstimatedLine;

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pValue = p.GetValue(line) as int? ?? p.GetValue(line) as byte?;

                //Строки в регистре с параметрами == свернутым КБК по бланку может не быть. Находим какую-нибудь удовлетворяющую не пустым КБК
                if (pValue.HasValue)
                    result = result.Where(String.Format("{0} == {1}", p.Name, pValue.Value));
                else
                    result = result.Where(String.Format("{0} == {1}", p.Name, "Null"));
            }

            var estimatedLine = result.FirstOrDefault();

            return estimatedLine;
        }

        /// <summary>
        /// Получить EF сущность, точно соответствующую набору КБК из line.
        /// !!!В базе такая сущность может отсутствовать!!!
        /// </summary>
        /// <param name="line"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public static EstimatedLine GetEstimatedLineNotExisted(this ILineCost line, DataContext context)
        {
            var result = (IQueryable<EstimatedLine>)context.EstimatedLine;

            var estimatedLine = new EstimatedLine();

            foreach (var p in typeof(ILineCost).GetProperties())
            {
                var pName = p.Name.Substring(2);
                var pValue = p.GetValue(line) as int? ?? p.GetValue(line) as byte?;

                var estimatedLineProperty = typeof(EstimatedLine).GetProperty(p.Name);
                var estimatedLinePropertyRelation = typeof(EstimatedLine).GetProperty(pName);
                var relationEntityId = estimatedLinePropertyRelation.PropertyType.GetEntity();
                var isMapped = estimatedLinePropertyRelation.GetCustomAttributes(true).All(a => a.GetType() != typeof (NotMappedAttribute));
                
                if (pValue.HasValue)
                {
                    estimatedLineProperty.SetValue(estimatedLine, p.GetValue(line));

                    if (isMapped)
                    {
                        var relationObject = context.Set<IReferenceEntity>(relationEntityId).FirstOrDefault(s => s.Id == pValue);
                        estimatedLinePropertyRelation.SetValue(estimatedLine, relationObject);
                    }
                }
            }

            return estimatedLine;
        }

    }
}
