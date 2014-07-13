//------------------------------------------------------------------------------
// <copyright file="CSSqlClassFile.cs" company="Microsoft">
//	 Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------
using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Text;

namespace Platform.DbClr.Helpers
{
	public static class ReflectionHelper
	{
		public const BindingFlags CommonFlags = BindingFlags.Public | BindingFlags.Instance;

		/// <summary>
		/// </summary>
		public static object CreateInstance(Type type, params object[] args)
		{
			return Activator.CreateInstance(type, CommonFlags | BindingFlags.CreateInstance, null, args,
											CultureInfo.CurrentCulture);
		}
	}
	
}