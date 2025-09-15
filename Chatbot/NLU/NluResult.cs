using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chatbot.NLU {
    public class NluResult {
        public string Intent { get; set; }
        public Dictionary<string, string> Entities { get; set; }

        public NluResult(string intent, Dictionary<string, string> entities = null) {
            Intent = intent;
            Entities = entities ?? new Dictionary<string, string>();
        }
    }
}
