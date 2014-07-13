using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Platform.ClientInteraction.Actions
{
    public class Eval : ClientActionBase
    {
        public string Code { get; set; }
        
        public Eval()
        {
            ClientHandler = "Eval";
        }
        public Eval(string code): this()
        {
            Code = code;
        }
    }
}
