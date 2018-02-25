using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterPlacement.Entities
{
    public class IterationParameters
    {
        public int StartRowRestriction { get; set; }
        public int EndRowRestriction { get; set; }
        public int StartColumnRestriction { get; set; }
        public int EndColumnRestriction { get; set; }
    }

}
