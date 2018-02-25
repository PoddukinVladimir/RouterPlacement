using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterPlacement.Entities
{
    public class Cell
    {
        public int Row { get; set; }
        public int Column { get; set; }
        public string Type { get; set; }
        public bool HasRouter { get; set; }
        public bool IsCovered { get; set; }
    }
}
