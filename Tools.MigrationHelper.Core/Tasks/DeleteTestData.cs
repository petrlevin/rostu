using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Practices.ObjectBuilder2;
using NAnt.Core.Attributes;
using Platform.PrimaryEntities.Common.DbEnums;
using Tools.MigrationHelper.Core.DeleteTestData;

namespace Tools.MigrationHelper.Core.Tasks
{
    [TaskName("deletetestdata")]
    public class DeleteTestData : SourceTask
    {

        public bool SaveAndRestore { get; set; }

        protected override void ExecuteTask()
        {
            try
            {
                if (SaveAndRestore)
                    CopySource();
                try
                {
                    Delete();
                }
                catch (Exception)
                {
                    if (SaveAndRestore)
                        RestoreSource();
                    throw; 
                }

            }
            catch (Exception ex)
            {
                Fatal("Фатальная ошибка при удалении тестовых данных", ex);
            }
        }

        private void RestoreSource()
        {
            var tempPath = GetTempPath();
            var regex = new Regex("^" + tempPath.Replace(@"\", @"\\") + "\\\\(?'relativepath'.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            foreach (var enumerateFile in Directory.EnumerateFiles(tempPath, "*.xml", SearchOption.AllDirectories))
            {
                var match = regex.Match(enumerateFile);
                if (match.Success)
                {
                    var relPath = match.Groups["relativepath"].Value;
                    var destPath = Path.Combine(SourcePath, relPath); 
                    File.Copy(enumerateFile, destPath, true);
                }
            }

        }

        public DeleteTestData()
        {
            SaveAndRestore = true;
        }

        private const string NameDistributiveData = "DistributiveData";
        private const string NameDistributiveDataMultiLink = "DistributiveDataMultiLink";


        private void Delete()
        {
            var allEntities = new Entities(SourcePath).Get();
            var entities = allEntities.Where(ei => (ei.Name != NameDistributiveData) && (ei.Name != NameDistributiveDataMultiLink) &&                                                                    
                                                                      (
                                                                        (ei.EntityType == EntityType.Reference) || 
                                                                        (ei.EntityType == EntityType.Document) ||
                                                                        (ei.EntityType == EntityType.Tablepart) || 
                                                                        (ei.EntityType == EntityType.Tool)
                                                                      )
                                                                ).ToList();

            var multilinks = allEntities.Where(ei => (ei.EntityType == EntityType.Multilink)).ToList();

            var entityEntity = allEntities.Single(ei => ei.Name == "Entity");
            var entityEntityField = allEntities.Single(ei => ei.Name == "EntityField");
            var distributivaDataId = allEntities.Single(ei => ei.Name == NameDistributiveData).Id;
            var distributiveDataMultiLinkId = allEntities.Single(ei => ei.Name == NameDistributiveDataMultiLink).Id;

            var distributiveData = new DistributivaDatas(SourcePath).Get();
            var distributiveDataMultiLink = new DistributivaDatasMultilink(SourcePath).Get();

            Func<EntityInfo, Int32, bool> condition =
                (ei, id) =>
                (((id == distributivaDataId) || (id == distributiveDataMultiLinkId)) && (ei.Id == entityEntity.Id)) ||
                !distributiveData.Any(dt => dt.IdElement == id && dt.IdElementEntity == ei.Id);


            Func<EntityInfo, Int32,Int32, bool> multilinkCondition =
                (ei, left,right) =>
                !distributiveDataMultiLink.Any(dt => (dt.IdLeft == left) && (dt.IdRight== right) && (dt.IdEntity == ei.Id));

            DeleteMultilinkData(multilinks, multilinkCondition);

            DeleteData(entities.Where(e => e.Id != entityEntityField.Id), condition);

            DeleteEntityFields(condition, entityEntity);

            DeleteFiles(distributiveData, entityEntity, entities, multilinks);

            DeleteRegistry(allEntities);


            new DistributivaDatas(SourcePath).DeleteWholeFile();
            new DistributivaDatasMultilink(SourcePath).DeleteWholeFile();

        }

        private void DeleteRegistry(List<EntityInfo> allEntities)
        {
            var entities = allEntities.Where(ei => ei.EntityType == EntityType.Registry).Where(ei => ei.Name != "ItemsDependencies");
            entities.ForEach(
                e => new Data(SourcePath, e.Id).DeleteWholeFile());
        

        }

        private void DeleteFiles(IEnumerable<DistributivaDataInfo> distributiveData, EntityInfo entityEntity, List<EntityInfo> entities, List<EntityInfo> multilinks)
        {
            var distributiveDataEntities = distributiveData.Where(di => di.IdElementEntity == entityEntity.Id);

            entities.AddRange(multilinks);
            entities.ForEach(
                e =>
                    {
                        if (distributiveDataEntities.Any(dt => dt.IdElement == e.Id))
                            return;
                        new Data(SourcePath, e.Id).DeleteWholeFile();
                    }
                );
        }

        private void DeleteEntityFields(Func<EntityInfo, int, bool> condition, EntityInfo entityEntity)
        {
            new EntityFields(SourcePath).DeleteByEntity(id => condition(entityEntity, id));
        }

        private void DeleteMultilinkData(IEnumerable<EntityInfo> multilinks, Func<EntityInfo, int, int, bool> multilinkCondition)
        {
            foreach (EntityInfo entityInfo in multilinks)
            {
                EntityInfo info = entityInfo;
                new MultilinkData(SourcePath, entityInfo.Id).Delete(
                    (left, right) => multilinkCondition(info, left, right)
                    );
            }
        }

        private void DeleteData(IEnumerable<EntityInfo> entities, Func<EntityInfo, int, bool> condition)
        {
            foreach (EntityInfo entityInfo in entities)
            {
                EntityInfo info = entityInfo;
                new Data(SourcePath, entityInfo.Id).Delete(
                    id => condition(info, id)
                    );
            }
        }

        private void CopySource()
        {
            var tempPath = GetTempPath();
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
            Directory.CreateDirectory(tempPath);
            var regex = new Regex("^" + SourcePath.Replace(@"\", @"\\") + "\\\\(?'relativepath'.+)$", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var regexIsData = new Regex(@"DbStructure\\[\w]+\\[\w]+\.xml$", RegexOptions.Compiled | RegexOptions.IgnoreCase);


            foreach (var enumerateFile in Directory.EnumerateFiles(SourcePath, "*.xml", SearchOption.AllDirectories))
            {
                if (!regexIsData.IsMatch(enumerateFile))
                    continue;
                var match = regex.Match(enumerateFile);
                if (match.Success)
                {
                    var relPath = match.Groups["relativepath"].Value;
                    var destPath = Path.Combine(tempPath, relPath);
                    var dir = Path.GetDirectoryName(destPath);
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                    }

                    File.Copy(enumerateFile, destPath);
                }
            }
        }

        private static string GetTempPath()
        {
            var tempPath = Path.Combine(System.IO.Path.GetTempPath(), "rostu_metadata");
            return tempPath;
        }
    }

}
