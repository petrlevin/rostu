using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using BaseApp.Common.Exceptions;
using BaseApp.Common.Interfaces;
using BaseApp.DbEnums;
using BaseApp.Reference;
using BaseApp.Rights.Organizational.Decorators;
using Microsoft.Practices.Unity;
using Platform.BusinessLogic;
using Platform.BusinessLogic.DataAccess;
using Platform.Common;
using Platform.Log;
using Platform.PrimaryEntities;
using Platform.PrimaryEntities.Common.Interfaces;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Utils.Extensions;


namespace BaseApp.Rights.Organizational
{

    /// <summary>
    /// http://conf.rostu-comp.ru/pages/viewpage.action?pageId=13599258
    /// </summary>
    public class OrganizationRights
    {




        #region Конструкторы

        public OrganizationRights(IEntity source)
            : this(
                IoC.Resolve<DbContext>(), IoC.Resolve<IUser>("CurrentUser"),
                IoC.Resolve<SqlConnection>("DbConnection"), source)
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="user"></param>
        public OrganizationRights([Dependency] DbContext dataContext, [Dependency("CurrentUser")] IUser user,
                                  [Dependency("DbConnection")] SqlConnection dbConnection, IEntity source)
            : this(dataContext.Cast<DataContext>(), user, dbConnection, source)
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="dataContext"></param>
        /// <param name="user"></param>
        /// <param name="source"></param>
        public OrganizationRights(DataContext dataContext, IUser user, SqlConnection dbConnection, IEntity source)
        {
            if (dataContext == null) throw new ArgumentNullException("dataContext");
            if (user == null) throw new ArgumentNullException("user");
            if (source == null) throw new ArgumentNullException("source");
            _dataContext = dataContext;
            _user = user;
            _source = source;
            _dbConnection = dbConnection;
        }


        #endregion

        #region Private

        private readonly DataContext _dataContext;
        private readonly IUser _user;
        private readonly IEntity _source;
        private SqlConnection _dbConnection;


        private IEnumerable<OrganizationRightExtension> GetExtensions(bool forEditing)
        {
            if (_user.IsSuperUser())
                return new List<OrganizationRightExtension>();
            var kind = (byte)(forEditing ? OrganizationRightExtensionKind.Write : OrganizationRightExtensionKind.Read);

                return _dataContext.OrganizationRightExtension.Where(oe => oe.Targets.Any(t=>t.Id == _source.Id) && ((oe.IdKind&kind)==kind)).ToList();
        }

        private IEnumerable<IGrouping<IEntity,IGrouping<IEntityField, OrganizationRightInfo>>> GetRights(bool forEditing = false, bool withSelfLinkedFields = false)
        {
            if (_user.IsSuperUser())
                return new List<IGrouping<IEntity, IGrouping<IEntityField, OrganizationRightInfo>>>();
            var kind = (byte)(forEditing ? OrganizationRightExtensionKind.Write : OrganizationRightExtensionKind.Read);
            var efields = 
                _dataContext.EntityField.Where(
                    ef =>
                    (_dataContext.OrganizationRightExtension.Where(oe => oe.Targets.Any(t => t.Id == _source.Id) && ((oe.IdKind&kind)==kind))
                                 .SelectMany(oe => oe.Results)
                                 .Any(e => e.Id == ef.IdEntity)) ||
                    (ef.IdEntity == _source.Id));
            var oiList = efields
                                     .SelectMany(
                                         ef => (_dataContext.Role_OrganizationRight.Where(
                                             or =>
                                             (!or.Disabled)
                                              && (ef.IdEntityLink == or.IdElementEntity)
                                              && (ef.IdCalculatedFieldType == null) &&
                                              or.Owner.Users.Any(u => u.Id == _user.Id) 
                                                   )
                                                            .Select(
                                                                ror =>
                                                                new OrganizationRightInfo()
                                                                    {
                                                                        Field = ef,
                                                                        RoleOrganizationRight =
                                                                            ror
                                                                    })

                                               )).Union(
                                               efields
                                                    .SelectMany(
                                                        ef => (_dataContext.Role_OrganizationRight.Where(
                                                            or => (!or.Disabled) &&
                                                                (ef.Name == "id") &&
                                                                (or.IdElementEntity == ef.IdEntity) &&
                                                                or.Owner.Users.Any(u => u.Id == _user.Id) 
                                                                ).Select(
                                                                ror =>
                                                                new OrganizationRightInfo()
                                                                    {
                                                                        Field = ef,
                                                                        RoleOrganizationRight =
                                                                            ror
                                                                    }))));




            oiList = oiList.Where(
                                                                            efi => 
                                                                        ((!_dataContext.EntityFieldSetting.Any(efs => efs.IdEntityField == efi.Field.Id))
                                                                        || (_dataContext.EntityFieldSetting.Any(efs => (efs.IdEntityField == efi.Field.Id)&&(!efs.IgnoreOrganizationRights)))));
            var list = oiList.ToList();
            if (list.All(oi => oi.Field.IdEntity != _source.Id))
                return new List<IGrouping<IEntity, IGrouping<IEntityField, OrganizationRightInfo>>>();

            IEnumerable<IGrouping<IEntityField, OrganizationRightInfo>> grouped;

            grouped = forEditing
                          ? list.Select(i => i.Field)
                                .Distinct(new Comparer())
                                .GroupJoin(list.Where(i => i.RoleOrganizationRight.EditingFlag).Distinct(), ef => ef,
                                           ror => ror.Field,
                                           (ef, eor) =>
                                               {
                                                   if (!((eor == null) || (!eor.Any())))
                                                       return eor.GroupBy(or => or.Field).Single();
                                                   else
                                                       return new EmptyGrouping(ef);
                                               })
                          : list.Distinct().GroupBy(ori => ori.Field);


            if (!withSelfLinkedFields)
                grouped = WithoutSelfLinked(grouped);
            return grouped.GroupBy(g => ((EntityField)g.Key).Entity); ;
        }

        #region Private Nested

        class EmptyGrouping :IGrouping<IEntityField,OrganizationRightInfo>
        {
            public EmptyGrouping(IEntityField key)
            {
                Key = key;
            }

            public IEnumerator<OrganizationRightInfo> GetEnumerator()
            {
                return new List<OrganizationRightInfo>().GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEntityField Key { get; private set; }
        }


         class Comparer:IEqualityComparer<IEntityField>
        {
            public bool Equals(IEntityField x, IEntityField y)
            {
                return x.Id == y.Id;
            }

            public int GetHashCode(IEntityField obj)
            {
                return obj.Id.GetHashCode();
            }
        }

         class ViolationInfo
         {
             public string For { get; set; }
             public string Field { get; set; }
             public string Value { get; set; }

             public override string ToString()
             {
                 if (Field.ToLower() != "id")
                     return String.Format("{0}:{1}", Field, Value ?? "пустой");
                 else
                     return "Сам элемент";
             }

         }


        #endregion

         private IEnumerable<IGrouping<IEntityField, OrganizationRightInfo>> WithoutSelfLinked(IEnumerable<IGrouping<IEntityField, OrganizationRightInfo>> result)
        {
            result = result.Where(g => g.Key.IdEntityLink != g.Key.IdEntity);
            return result;
        }

        private IEnumerable<IGrouping<IEntityField, OrganizationRightInfo>> WithoutId(IEnumerable<IGrouping<IEntityField, OrganizationRightInfo>> result)
        {
            result = result.Where(g => g.Key.Name.ToLower()!= "id");
            return result;
        }


        private IOrganizationRightData Define()
        {
            return new RightsData(GetRights(), _source,GetExtensions(false));

        }

        private void ValidateWrite(int[] itemId, string operation)
        {
            //var rightData = new RightsData(GetRights(true), _source, GetExtensions(true)); //SBORIII-1844
            var rightData = new RightsData(GetRights(true), _source, new List<OrganizationRightExtension>());
            ValidateWrite(operation,new SelectQueryBuilder<ImplementationRevert>(_source, itemId, rightData));
        }




        private void ValidateWrite(Dictionary<string, object> values, int? itemId, string operationName)
        {
            
            

            var rightsWithSl = GetRights(true, true);
            
            if (!rightsWithSl.Any())
                return;
            var extensions = GetExtensions(true);
            if (itemId.HasValue)
            {
                ValidateWrite(operationName, new SelectQueryBuilder<ImplementationRevert>(_source, itemId.Value, new RightsData(rightsWithSl, WithoutSelfLinked(rightsWithSl.Single(g => g.Key.Id == _source.Id)), extensions)));
                ValidateWrite(operationName, new SelectQueryBuilder<ImplementationRevertStrict>(_source, itemId.Value, new RightsData(rightsWithSl, _source, extensions), values));
            }
            else
            {
                var captionField = _source.RealFields.FirstOrDefault(f => ((f.IsCaption.HasValue) && (f.IsCaption.Value)));
                var caption = captionField != null ? values.ContainsKey(captionField.Name)? values[captionField.Name] : " ": " ";
                ValidateWrite(operationName, new SelectQueryBuilder<ImplementationRevertStrict>(_source, caption.ToString(), new RightsData(rightsWithSl,WithoutId(rightsWithSl.Single(g => g.Key.Id == _source.Id)),extensions), values));
            }
            
            
            
        }

        private void ValidateWrite(string operationName, SelectQueryBuilder builder)
        {
            var cmd = builder.GetSqlCommand(_dbConnection);
            using (var reader = cmd.ExecuteReaderLog())
            {
                var violationsFormat = GetViolationFormat(reader, builder);
				if (violationsFormat != null)
				{
					reader.Close();
					throw new OranizationalRightsException(String.Format(violationsFormat, operationName));
				}
				reader.Close();
            }
            
        }

        private string GetViolationFormat(SqlDataReader reader, SelectQueryBuilder builder)
        {
            var violations = GetViolations(reader, builder);
            if (violations == null)
                return null;

            return "Отсутствуют организационные права на {0} " + violations.GroupBy(vi => vi.For).ToString(
                 g => g.Key  +  g.ToHtmlList());

        }


        private IEnumerable<ViolationInfo> GetViolations(SqlDataReader reader, SelectQueryBuilder builder)
        {
            if (!reader.HasRows)
                return null;
            return reader.AsEnumerable(r => new ViolationInfo()
                                                          {
                                                              Field = r.GetString("field"),
                                                              For = r.GetString(SelectQueryBuilder.CaptionAlias),
                                                              Value = r.GetString("field").ToLower() != "id" ?
                                                                      r.GetString(
                                                                      builder.CaptionAliasFor(r.GetString("field"))) :
                                                                      null
                                                          });


        }


        #endregion

        #region Public Static

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public static IOrganizationRightData For(IEntity source)
        {
            return new OrganizationRights(source).Define();
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public static IOrganizationRightData For(int entityId)
        {
            return For(Objects.ById<Entity>(entityId));
        }


        public static void ValidateMultilinkWrite(IEntity entityMl, int ownerEntityId, int[] itemId, string operation)
        {
            var source = MultilinkHelper.GetRightMultilinkEntity(entityMl, ownerEntityId);
            new OrganizationRights(source).ValidateWrite(itemId, operation);
        }

        public static void ValidateWrite(IEntity entity, int? itemId, Dictionary<String, object> values, string operationName)
        {

            new OrganizationRights(entity).ValidateWrite(values, itemId, operationName);
        }

        public static void ValidateWrite(int entityId, int? itemId, Dictionary<String, object> values, string operationName)
        {

            new OrganizationRights(Objects.ById<Entity>(entityId)).ValidateWrite(values, itemId, operationName);
        }


        public static void ValidateMultilinkWrite(int entityMlId, int ownerEntityId, int[] itemIds, string operation)
        {
            ValidateMultilinkWrite(Objects.ById<Entity>(entityMlId),ownerEntityId, itemIds, operation);
        }

        public static void ValidateWrite(IEntity entity, int[] itemIds, string operation)
        {
            new OrganizationRights(entity).ValidateWrite(itemIds,operation);
        }


        public static void ValidateWrite(int entityId, int[] itemIds, string operation)
        {
            ValidateWrite(Objects.ById<Entity>(entityId), itemIds, operation);
        }

        public static void ValidateWrite(int entityId, int itemIds, string operation)
        {
            ValidateWrite(entityId, new[] { itemIds },operation);
        }

    #endregion
    }

}
