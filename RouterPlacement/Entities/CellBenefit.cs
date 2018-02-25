using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterPlacement.Entities
{
    public class CellBenefit
    {
        public int CellCoverage { get; set; }
        public int CellCost { get; set; }
        public Cell cell;
    }
}
