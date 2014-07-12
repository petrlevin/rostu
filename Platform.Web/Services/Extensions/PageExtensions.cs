using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Platform.Web.Services.Extensions
{
	public static class PageExtensions
	{
		public static int? GetNullableInt(this System.Web.UI.Page page, string paramName)
		{
			return !string.IsNullOrEmpty(page.Request.Params[paramName]) ? int.Parse(page.Request.Params[paramName]) : (int?)null;
		}

		public static int GetInt(this System.Web.UI.Page page, string paramName)
		{
			return int.Parse(page.Request.Params[paramName]);
		}
	}
}