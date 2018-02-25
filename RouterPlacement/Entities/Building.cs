using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RouterPlacement.Entities
{
    public class Building
    {
        public int CellPrice { get; set; }
        public int RouterPrice { get; set; }
        public int RouterRadius { get; set; }
        public int Budget { get; set; }
        public Cell[,] cells;
        public List<Cell> targetCells = new List<Cell>();
        public List<Cell> wallCells = new List<Cell>();
        public List<Cell> voidCells = new List<Cell>();

        public List<Cell> backboneCells = new List<Cell>();
        public List<Cell> routerCells = new List<Cell>();
        public int totalCoverage;
        public int totalBackboneUsed;
    }
}
