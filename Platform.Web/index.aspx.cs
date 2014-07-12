using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Platform.Web
{
	public partial class index : System.Web.UI.Page
	{
		protected void Page_Load(object sender, EventArgs e)
		{
		}

        public string GetScriptTags()
        {
#if DEBUG

            return @"<script type='text/javascript' charset='utf-8' src='ext\ext-dev.js'></script>" +
                   @"<script type='text/javascript' charset='utf-8' src='app.js'></script>";
#else
            return String.Format(@"<script type='text/javascript' charset='utf-8' src='ext\all-classes.js?{0}'></script>", Assembly.GetExecutingAssembly().GetName().Version.Revision);
#endif
        }
	}

}

namespace ExtensionHandlers
{

}
