
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Tools.MigrationHelper.Logger
{
	public class Message
	{
		public Message(MethodBase sender, MessageType type, string text)
		{
			this.Sender = sender;
			this.Type = type;
			this.Text = text;
		}

		public MethodBase Sender { get; set; }
		public MessageType Type { get; set; }
		public string Text { get; set; }
		
		public override string ToString()
		{
			return string.Format("Sender: {0}; Type: {1}{2}{3}", getSenderName(), Type.ToString(), Environment.NewLine, Text);
		}

		/// <summary>
		/// Возвращает имя класса и метода, где было сформировано данное сообщение
		/// </summary>
		private string getSenderName()
		{
			return string.Format("{0}.{1}", Sender.DeclaringType.Name, Sender.Name);
		}
	}
}
