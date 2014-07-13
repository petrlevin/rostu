using System.Linq;
using System.Collections.Generic;

namespace AutoClient.FixLimitVolumeAppropriations
{
    partial class Main
    {
        class SqlTpl
        {
            #region Templates (Private)

            private static string findLeafs = @"
                select {0}
                from 
	                doc.{1} doc
	                inner join ref.DocStatus on doc.idDocStatus = ref.DocStatus.id
	                left join doc.{1} child on doc.id = child.idParent
                where
	                child.id is null
	                {2}
            ";

            private static string findDrafts = @"
                    doc.Number
                    ,ref.DocStatus.Caption                    
                    ,doc.Caption	                
            ";

            private static string leafStatuses = @"distinct ref.DocStatus.Caption";

            private static string entityIdByName = @"select id, Name from ref.Entity where name in ({0})";

            private static string drafts = @"select id from doc.{0} where idDocStatus = -2147483615 and idParent is not null"; // черновик

            private static string draftsParents = @"select idParent as id from doc.{0} where idDocStatus = -2147483615 and idParent is not null"; // черновик порожденный

            /// <summary>
            /// {0} = имя сущности документа
            /// </summary>
            private static string draftsInEditMode = @"
                select
	                doc.id
                from 
	                doc.{0} doc
	                inner join reg.StartedOperation on (
		                reg.StartedOperation.idRegistratorEntity = (select id from ref.Entity where name = '{0}')
		                and reg.StartedOperation.idRegistrator = doc.id
	                )
	                inner join ref.EntityOperation on reg.StartedOperation.idEntityOperation = ref.EntityOperation.id
	                inner join ref.Operation on ref.EntityOperation.idOperation = ref.Operation.id
                where
	                ref.Operation.Name = 'Edit'
            ";

            private static string documentsOperations = @"
                select
	                ref.Entity.Name
	                ,ref.EntityOperation.id
                from
	                ref.EntityOperation
	                inner join ref.Entity on ref.EntityOperation.[idEntity] = ref.Entity.id
	                inner join ref.Operation on ref.EntityOperation.idOperation = ref.Operation.id
                where
	                ref.Operation.Name = '{0}'
	                and ref.Entity.Name in ({1})
            ";

            #endregion

            #region Sql Statement Getters (Public)

            public static string FindDrafts(string docName)
            {
                return string.Format(findLeafs, findDrafts, docName);
            }

            public static string LeafStatuses(string docName)
            {
                return string.Format(findLeafs, leafStatuses, docName, string.Empty);
            }

            public static string LeafDocuments(string docName)
            {
                return string.Format(findLeafs, "doc.id", docName, "and ref.DocStatus.name not in ('Draft', 'Archive')");
            }

            public static string GetEntityIdsByName(IEnumerable<string> names)
            {
                var n = names.Select(a => string.Format("'{0}'", a)).Aggregate((a, b) => string.Format("{0}, {1}", a, b));
                return string.Format(entityIdByName, n);
            }

            public static string GetDrafts(string docName)
            {
                return string.Format(drafts, docName);
            }

            public static string GetDraftsParents(string docName)
            {
                return string.Format(draftsParents, docName);
            }

            public static string GetDraftsInEditMode(string docName)
            {
                return string.Format(draftsInEditMode, docName);
            }

            public static string GetUndoChangeOperations(IEnumerable<string> docNames)
            {
                return getEntityOperations(docNames, "UndoChange");
            }

            public static string GetChangeOperations(IEnumerable<string> docNames)
            {
                return getEntityOperations(docNames, "Change");
            }

            public static string GetProcessOperations(IEnumerable<string> docNames)
            {
                return getEntityOperations(docNames, "Process");
            }

            #endregion

            #region Private

            private static string getEntityOperations(IEnumerable<string> docNames, string operationName)
            {
                string n = docNames.Select(a => string.Format("'{0}'", a)).Aggregate((a, b) => string.Format("{0}, {1}", a, b));
                return string.Format(documentsOperations, operationName, n);
            }

            #endregion
        }
         
    }
}
