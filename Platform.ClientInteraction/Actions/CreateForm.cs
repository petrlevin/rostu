using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Platform.ClientInteraction.Actions
{
    public class CreateForm : ClientActionBase
    {
        public JObject Form { get; set; }

        public CreateForm(object form)
            : this(JObject.FromObject(form))
        {
        }

        public CreateForm(JObject form)
        {
            ClientHandler = "CreateForm";
            Form = form;
        }
    }
}
