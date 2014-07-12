using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Platform.Web
{
    public partial class indexDebug : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public string GetScriptTags()
        {
            return @"<script type='text/javascript' charset='utf-8' src='ext\ext-dev.js'></script>" +
                   @"<script type='text/javascript' charset='utf-8' src='app.js'></script>";
        }
    }
}