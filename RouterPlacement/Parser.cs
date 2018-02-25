using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RouterPlacement.Entities;

namespace RouterPlacement
{
    public class Parser
    {
        public static Building Parse(string input)
        {
            var building = new Building();

            string[] lines = input.Split('\n');

            //string[] stringSeparators = new string[] { "\r\n" };
            //string[] lines = input.Split(stringSeparators, StringSplitOptions.None);

            var firstLineDescription = lines[0].Split(' ');
            var rowsNumber = int.Parse(firstLineDescription[0]);
            var columnsNumber = int.Parse(firstLineDescription[1]);
            var routerRadius = int.Parse(firstLineDescription[2]);

            building.RouterRadius = routerRadius;

            building.cells = new Cell[rowsNumber, columnsNumber];

            var secondLineDescription = lines[1].Split(' ');
            var connectingPriceForOneCell = int.Parse(secondLineDescription[0]);
            var routerPrice = int.Parse(secondLineDescription[1]);
            var budgetLimit = int.Parse(secondLineDescription[2]);

            building.CellPrice = connectingPriceForOneCell;
            building.RouterPrice = routerPrice;
            building.Budget = budgetLimit;

            var thirdLineDescription = lines[2].Split(' ');
            var initialBackboneCellRow = int.Parse(thirdLineDescription[0]);
            var initialBackboneCellColumn = int.Parse(thirdLineDescription[1]);

            for (int i = 3; i < rowsNumber + 3; i++)
            {
                var gridRow = lines[i].ToCharArray();
                for (int j = 0; j < columnsNumber; j++)
                {
                    var currentCell = new Cell();
                    currentCell.Row = i - 3;
                    currentCell.Column = j;

                    switch (gridRow[j])
                    {
                        case '-':
                            currentCell.Type = "void";
                            building.voidCells.Add(currentCell);
                            break;
                        case '#':
                            currentCell.Type = "wall";
                            building.wallCells.Add(currentCell);
                            break;
                        case '.':
                            currentCell.Type = "target";
                            building.targetCells.Add(currentCell);
                            break;
                        default:
                            throw new Exception("Wrong cell type");
                    }
                    building.cells[i - 3, j] = currentCell;
                }
            }

            building.backboneCells.Add(building.cells[initialBackboneCellRow, initialBackboneCellColumn]);

            return building;
        }
    }
}
