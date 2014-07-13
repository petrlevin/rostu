using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Platform.PrimaryEntities.Common.Interfaces;

namespace Platform.PrimaryEntities.Reference
{
	public class InterfaceControl : Metadata, IIdentitied
	{
		#region Implementation of IIdentitied

		/// <summary>
		/// идентификатор
		/// </summary>
		public int Id { get; set; }

		#endregion

		public string Caption { get; set; }
		public string Description { get; set; }
		public string ComponentName { get; set; }
		public string Alias { get; set; }
		public string DefaultProperties { get; set; }
		public string LabelProperty { get; set; }
	}
}
