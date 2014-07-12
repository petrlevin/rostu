using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Platform.PrimaryEntities.DbEnums;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Tools.MigrationHelper.DbManager;
using Tools.MigrationHelper.Helpers;

namespace Tools.MigrationHelper.EnumsProcessing
{
    /// <summary>
    /// Функционал объединения списков энумераторов в набор данных (DataSet)
    /// </summary>
    public class EnumsMerger
    {
		private class FieldsForEnumBuilder
		{
			public int IdEntity;
			//public int IdProject;
			public int Id;
			
			private EntityField CreateFieldForEnum(string fieldName, string fieldCaption, byte idEntityFieldType, bool readOnly = false, byte size = 0, bool allowNull = false, bool isCaption = false)
			{
				var result = Objects.Create<EntityField>();
				result.Id = Id++;
				result.IdEntity = IdEntity;
				result.Name = fieldName;
				result.Caption = fieldCaption;
				result.IdEntityFieldType = idEntityFieldType;
				//result.IdProject = IdProject; Убрали по CORE-68
				result.Size = size;
				result.AllowNull = allowNull;
				result.IsCaption = isCaption;
				result.IdForeignKeyType = (byte)ForeignKeyType.WithOutForeignKey;
				result.ReadOnly = readOnly;
				return result;
			}

			public List<EntityField> Build()
			{
				return new List<EntityField>()
                {
                    CreateFieldForEnum(Names.Id, "Идентификатор", 15, true),
                    CreateFieldForEnum(Names.Name, "Системное имя", 2, false, 50),
                    CreateFieldForEnum(Names.Caption, "Наименование", 2, false, 100, true, true),
                    CreateFieldForEnum(Names.Description, "Описание", 2, false, 200, true)
                };
			}
		}



        /// <summary>
        /// Приращение к идентификаторам енумераторов, чтобы не мешались
        /// </summary>
        private const int IdEn = 100;

        private const int IdenF = 1000;

        /// <summary>
        /// Функционал объединения списков энумераторов в набор данных (DataSet)
        /// </summary>
        /// <param name="dataSet">Набор данных, с которым требуется объединить списки энумераторов</param>
        public EnumsMerger(Metadata metadata)
        {
            this.Meta = metadata;
        }

        /// <summary>
        /// Набор данных (а ОММ по сути им и является), с которым требуется объединить списки энумераторов
        /// </summary>
        public Metadata Meta { get; private set; }

        /// <summary>
        /// Сокращенная форма для доступа к таблицам из набора данных ОММ. 
        /// </summary>
        private DataTableCollection Tables
        {
            get { return Meta.DataSet.Tables; }
        }

        /// <summary>
        /// Объединяет списки перечислений из <paramref name="enumsTables"/> в DataSet
        /// </summary>
        /// <param name="enumsTables">Коллекция dataTable с енумераторами и их сущностями</param>
        public void Merge(List<DataTable> enumsTables)
        {
			#if DEBUG
			checkEnumFields();
			#endif

            var enumsEntityList = enumsTables.FirstOrDefault(w => w.TableName == Names.RefEntity);

            if (enumsEntityList == null)
                throw new Exception("Отсутсвует таблица Entity");

            int newEntityId = Meta.GetNewId(Names.RefEntity);
            //Увеличиваем id на константу чтобы перечисления не мешались
            if (newEntityId < IdEn)
                newEntityId += IdEn;
            //Вставка ref.Entity в DataSet
            foreach (DataRow enumInfo in enumsEntityList.Rows)
            {
                // получаем id из DataSet по имени сущности
                var entityIdFromMetadata = GetIdByNameFromEntity(enumInfo[Names.Name].ToString());

                // если для данного имени нет совпадений значит перечисление новое, создаем запись в ref.Entity и ref.EntityField
                if (entityIdFromMetadata == null)
                {
                    DataRow newRow = Tables[Names.RefEntity].NewRow();
                    newRow[Names.Id] = newEntityId;
                    newRow[Names.Name] = enumInfo[Names.Name];
					newRow[Names.Caption] = enumInfo[Names.Caption];
					newRow[Names.Description] = enumInfo[Names.Description];
                    newRow[Names.IdEntityType] = enumInfo[Names.IdEntityType];
                    newRow[Names.IdProject] = enumInfo[Names.IdProject];
	                newRow[Names.IsVersioning] = false;
                    newRow[Names.GenerateEntityClass] = false;
                    Tables[Names.RefEntity].Rows.Add(newRow);
                    AddEntityFieldsForEnum(newEntityId);
                    newEntityId++;
                }
                else
                {
                    var entityRow = Tables[Names.RefEntity].AsEnumerable().SingleOrDefault(a => a.Field<int>(Names.Id) == entityIdFromMetadata);
                    entityRow[Names.Name] = enumInfo[Names.Name];
					entityRow[Names.Caption] = enumInfo[Names.Caption];
					entityRow[Names.Description] = enumInfo[Names.Description];
                    entityRow[Names.IdEntityType] = enumInfo[Names.IdEntityType];
                    entityRow.AcceptChanges();
                }
            }

            //Все полученные таблицы по Enums, кроме таблицы Entity
            var enumTables = enumsTables.Where(w => w.TableName != Names.RefEntity).ToList();

            CheckDataSet(enumTables);

            //Все полученные таблицы по Enums, кроме таблицы Entity мерджаться c DataSet
            foreach (var dataTable in enumTables)
            {
                if (Tables.Contains(dataTable.TableName))
                {
                    Meta.DataSet.Merge(dataTable);
                }
                else
                {
                    Tables.Add(dataTable);
                }
            }
            Meta.DataSet.AcceptChanges();
        }

		/// <summary>
		/// Для каждой строки из Meta.DataSet.Tables[Names.RefEntity], соответствующей перечислению, 
		/// должны существовать записи в таблице EntityField соответствующие полям перечислений.
		/// </summary>
		/// <remarks>
		/// Мотивом создания данной проверки послужил случай, когда в репозиторий было закоммичено перечисление (запись в Entity.xml) без полей.
		/// </remarks>
	    private void checkEnumFields()
	    {
			//Получаем количество полей которые создаются для новой сущности перечисления
			var builder = new FieldsForEnumBuilder { Id = 0, IdEntity = 0 };
			var listFields = builder.Build();
			var fieldsCount = listFields.Count;
			
			//кол-во перечислений полученных из xml
			var entities = Tables[Names.RefEntity].Select("idEntityType = 1").AsEnumerable().Select(row => row.Field<int>("id")).ToArray();
			//кол-во Полей перечислений полученных из xml
			var entityFieldsCount = Tables[Names.RefEntityField].Select("idEntity in (" + string.Join(",",entities) + ")").Count();

			if (fieldsCount * entities.Count() != entityFieldsCount)
			{
				throw new Exception("Ошибка! В xml файлах, для сущностей типа перечисление, не хватает строк в справочнике Поля сущности. Ожидалось: " + fieldsCount * entities.Count() + " В xml файлах: " + entityFieldsCount);
			}
	    }

	    /// <summary>
		/// Проверка на то что для каждого перечисления в таблице ref.entity есть таблица с таким же именем
        /// </summary>
        /// <param name="enumsTables"></param>
		private void CheckDataSet(List<DataTable> enumsTables)
        {
            var builder = new StringBuilder();
            foreach (var entityRow in Tables[Names.RefEntity].Select("idEntityType = 1"))
            {
                if (!enumsTables.Any(w => w.TableName.Split(new[] { '.' }, 2)[1] == entityRow[Names.Name].ToString()))
                {
                    builder.Append(entityRow[Names.Name] + "\r\n");
                }
            }

            var mess = builder.ToString();

            if (!string.IsNullOrEmpty(mess))
            {
                throw new Exception("Ошибка! Найдены перечисления по которым есть запись в Entity.xml, а в коде отсутсвует. Возможно перечисление в КОДЕ было удалено или переименовано. Перечисления:" + mess);
            }
        }

        /// <summary>
        /// Возвращает идентификатор сущности по системному наименованию, в случае отсутствия возвращает NULL
        /// </summary>
        /// <param name="name">Системное наименование</param>
        /// <returns>int?</returns>
        private int? GetIdByNameFromEntity(string name)
        {
            if (Tables["ref.Entity"] == null)
                throw new Exception("В DataSet не загружена таблица ref.Entity");
            int? result = null;
            DataRow row = Tables["ref.Entity"].AsEnumerable().SingleOrDefault(a => a.Field<string>("Name") == name);
            if (row != null)
                result = Convert.ToInt32(row["id"]);
            return result;
        }

        /// <summary>
        /// Добавляет строки в EntityField для описания сущности с типом Енумератор
        /// </summary>
        /// <param name="idEntity">Идентификатор сущности</param>
        private void AddEntityFieldsForEnum(int idEntity)
		{
			int id = Meta.GetNewId(Names.RefEntityField);
			//Увеличиваем id на константу чтобы перечисления не мешались
			if (id < IdenF)
				id += IdenF;

            var builder = new FieldsForEnumBuilder {Id = id, IdEntity = idEntity};
            var listFields = builder.Build();
			foreach (var field in listFields)
			{
				Tables[Names.RefEntityField].Rows.Add(GetNewEntityFieldRow(field));
			}
		}

        /// <summary>
		/// Получить DataRow, соответствующую полю перечсиления
        /// </summary>
        /// <param name="field"></param>
        /// <remarks>
        /// ToDo: Было бы удобнее написать и использовать метод базового класса сущностей - ToDataRow. 
        /// Поля сущностного класса будут помечены атрибутами, из которых можно прочесть имена соответствующих столбцов.
        /// </remarks>
        /// <returns></returns>
        private DataRow GetNewEntityFieldRow(EntityField field)
        {
            DataRow newRow = Tables[Names.RefEntityField].NewRow();
            newRow[Names.Id] = field.Id;
            newRow[Names.Name] = field.Name;
			newRow[Names.Caption] = field.Name;
            newRow[Names.IdEntity] = field.IdEntity;
            newRow[Names.IdEntityFieldType] = field.IdEntityFieldType;
            newRow["Size"] = (object)field.Size ?? DBNull.Value;
            newRow["AllowNull"] = field.AllowNull;
            newRow["idForeignKeyType"] = field.IdForeignKeyType;
            newRow["isCaption"] = field.IsCaption;
            newRow[Names.ReadOnly] = field.ReadOnly;
			newRow["isHidden"] = false;
            return newRow; 
        }
    }
}
