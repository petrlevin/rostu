using System;
using System.Collections.Generic;
using System.IO;
using Platform.Web.Common;
using Platform.Web.Services.Extensions;

namespace Platform.Web.Services
{
	public partial class Import : System.Web.UI.Page
	{
        private int idEntity;
        private int? idOwner;
        private int? idTemplate;
        private string fieldValues;
        private string ownerFieldName;
        private int? tpFiledId;
        private string searchStr;
        private int? ownerItemId;

        private List<string> visibleColumnsList;
        private string visibleColumns
        {
            set { visibleColumnsList = new List<string>(value.Split(',')); }
        }
        private string getFileType()
        {
            return Path.GetExtension(this.Request.Files[0].FileName).Remove(0,1);
        }


        private void readParameters()
        {
//            idEntity = this.GetInt("idEntity");
//            idOwner = this.GetNullableInt("idOwner");
//            idTemplate = this.GetNullableInt("idTemplate");
//            fieldValues = this.Request.Params["fieldValues"];
//            ownerFieldName = this.Request.Params["ownerFieldName"];
//            tpFiledId = this.GetNullableInt("tpFiledId");
//            visibleColumns = this.Request.Params["visibleColumns"];
//            searchStr = this.Request.Params["searchStr"];
//            ownerItemId = this.GetNullableInt("ownerItemId");
        }

		protected void Page_Load(object sender, EventArgs e)
		{
		    readParameters();
			int? ignoreRows = this.GetNullableInt("ignoreRows");
			int? idOwner = this.GetNullableInt("idOwner");
			int idTemplate = this.GetInt("idTemplate");
            var dep = this.Request.Params["fieldvalues"];
			var file = Request.GetFirstFile();
		    var fileType = getFileType();
			var import = new ImportService();

            var resultMsg = import.Import(file, idTemplate, idOwner, dep, ignoreRows ?? 0, fileType);
            
			Response.Write(resultMsg);
			Response.End();
		}
	}
}