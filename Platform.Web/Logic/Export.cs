using System.Collections.Generic;
using System.Collections.Specialized;
using BaseApp.Export;
using BaseApp.Service.Common;
using Platform.PrimaryEntities.Factoring;
using Platform.PrimaryEntities.Reference;
using Platform.Web.Common;
using Platform.Web.Interfaces;

namespace Platform.Web.Logic
{
    public class Export : IFileDownload
    {
        private int _idEntity;
        private int? _idOwner;
        private string _fieldValues;
        private string _ownerFieldName;
        private int? _tpFiledId;
        private string _searchStr;
        private int? _ownerItemId;
        private string _exportType;

        private List<string> _visibleColumnsList;
        private string VisibleColumns
        {
            set { _visibleColumnsList = new List<string>(value.Split(',')); }
        }

        public FileDataInfo GetFile()
        {
            const string extension = "xls";

            var entity = Objects.ById<Entity>(_idEntity);
            var export = new EntitiesListExport(_fieldValues, _tpFiledId, entity, _searchStr, _visibleColumnsList, _ownerItemId)
            {
                IdOwner = _idOwner,
                OwnerFieldName = _ownerFieldName
            };
            var result = export.BuildReport();

            string contentType = null;
            if (!string.IsNullOrEmpty(_exportType))
                contentType = "application/vnd.ms-" + _exportType;

            return new FileDataInfo
                {
                    ContentType = contentType,
                    FileName = "export." + extension,
                    FileData = result
                };
        }

        public void SetParams(NameValueCollection values)
        {
            _exportType = values["type"];

            _idEntity = values.GetInt("idEntity");
            _idOwner = values.GetNullableInt("idOwner");
            values.GetNullableInt("idTemplate");
            _fieldValues = values["fieldValues"];
            _ownerFieldName = values["ownerFieldName"];
            _tpFiledId = values.GetNullableInt("tpFiledId");
            VisibleColumns = values["visibleColumns"];
            _searchStr = values["searchStr"];
            _ownerItemId = values.GetNullableInt("ownerItemId");
        }
    }
}