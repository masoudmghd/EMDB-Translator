using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMDB_Translator.Model
{
    public class TranslationModel
    {
        public string Id { get; set; }
        public string Source { get; set; }
        
        public string Translation { get; set; }

        public string Info { get; set; }
    }
}
