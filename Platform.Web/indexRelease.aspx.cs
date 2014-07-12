using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace Platform.Web
{
    public partial class indexRelease : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }
        public string GetScriptTags()
        {
            return @"<script type='text/javascript' charset='utf-8' src='ext\all-classes.js'></script>";
        }
    }
}