using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.SessionState;
using System.Xml.Linq;
using BaseApp.Service;
using Newtonsoft.Json;

namespace Platform.Web.Services
{
    /// <summary>
    /// Summary description for ImportXml
    /// </summary>
    public class ImportXml : IHttpHandler, IRequiresSessionState
    {

        public void ProcessRequest(HttpContext context)
        {
			XDocument document = new XDocument();
			if (context.Request.Files.Count == 1)
			{
				document = XDocument.Load(context.Request.Files[0].InputStream);
			}
			int idEntity = Int32.Parse(context.Request.Form["idEntity"]);
	        ServerResponse result;
			try
			{
				string resultMessage = new XmlExchange().SimpleImport(idEntity, document);
				result = new ServerResponse
				 {
					 success = true,
					 msg = resultMessage
				 };

			}
			catch(Exception ex)
			{
				result = new ServerResponse
					{
						success = false,
						msg = ex.Message
					};
			}
	        context.Response.Write(JsonConvert.SerializeObject(result));
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }
    }
}