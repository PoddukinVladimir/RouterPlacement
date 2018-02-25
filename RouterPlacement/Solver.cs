using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RouterPlacement.Entities;

namespace RouterPlacement
{
    public class Solver
    {
        private Building building;
        private readonly int routerRadius;
        private int bestCoverageRate;

        public Solver(Building _building)
        {
            building = _building;
            routerRadius = _building.RouterRadius;
        }

        public void CreateOutput()
        {
            using (StreamWriter sw = File.CreateText("charleston_road.out.txt"))
            {
                sw.WriteLine(building.backboneCells.Count);

                var result = "";

                // writing backbones
                foreach (var backboneCell in building.backboneCells)
                {
                    result += backboneCell.Row + " " + backboneCell.Column;

                    sw.WriteLine(result);
                    result = "";
                }

                sw.WriteLine(building.routerCells.Count);

                // writing routers`
                foreach (var routerCell in building.routerCells)
                {
                    result += routerCell.Row + " " + routerCell.Column;

                    sw.WriteLine(result);
                    result = "";
                }
            }
        }

        public int CalculateScore()
        {
            var score = 0;
            score = 1000 * building.totalCoverage + building.Budget;
            return score;
        }

        public void FindOptimalRouterPlacement()
        {
            while (building.Budget > 0 || building.Budget > ((building.targetCells.Count - building.totalCoverage) * 1000))
            {
                var allRouterOptionsSortedByBenefit = FindBestCoverageCellsForRouters();

                // if there are no options left
                // if (allRouterOptionsSortedByBenefit.Count == 0) break;

                var bestOption = allRouterOptionsSortedByBenefit[0];

                // check if we can afford best option to put router into
                if (bestOption.CellCost + building.RouterPrice > building.Budget)
                {
                    // if not, check other options available
                    for (int i = 1; i < allRouterOptionsSortedByBenefit.Count; i++)
                    {
                        if (allRouterOptionsSortedByBenefit[i].CellCost + building.RouterPrice < building.Budget)
                        {
                            bestOption = allRouterOptionsSortedByBenefit[i];
                            break;
                        }
                    }
                    // if we can't afford
                    break;
                }

                // all target cells have been covered
                if (bestOption.CellCoverage == 0) break;

                PutRouterIntoCell(bestOption.cell);

                // Updating resources used and statistics
                building.Budget -= building.RouterPrice;
                building.Budget -= bestOption.CellCost * building.CellPrice;
                building.totalCoverage += bestOption.CellCoverage;
                building.totalBackboneUsed += bestOption.CellCost;
            }
        }

        private List<CellBenefit> FindBestCoverageCellsForRouters()
        {
            var cellsSortedByBenefit = new List<CellBenefit>();
            int cost;

            foreach (var targetCell in building.targetCells)
            {
                // decreases time spent on calculations by 5-7 times, but score decreases as well
                // if (targetCell.IsCovered) continue;

                var wallCellsCoveredByRouter = CreateWallCellsListForTargetCell(targetCell, out var iterationP);

                var coveredTargetCellsCount = CountNumberOfTargetCellsCovered(iterationP, targetCell, wallCellsCoveredByRouter);

                // TODO implement a time saving check for doubtlessly best solution
                // At first, check top 1 options by best coverage possible, then
                // if no such exists any longer, decrement the criteria (or use
                // info about best coverage from previous iterations)

                // when doubtlessly best option is found, iterate no more()
                //if (coveredTargetCellsCount == bestCoverageRate && bestCoverageRate != 0)
                //{
                //    cost = CalculateCostOfSettingRouterInTargetCell(targetCell);

                //    cellsSortedByBenefit.Add(new CellBenefit()
                //    {
                //        cell = targetCell,
                //        CellCost = cost,
                //        CellCoverage = coveredTargetCellsCount
                //    });
                //    return cellsSortedByBenefit.OrderByDescending(cb => cb.CellCoverage).ThenBy(cb => cb.CellCost).ToList();
                //}

                cost = CalculateCostOfSettingRouterInTargetCell(targetCell);

                cellsSortedByBenefit.Add(new CellBenefit()
                {
                    cell = targetCell,
                    CellCost = cost,
                    CellCoverage = coveredTargetCellsCount
                });
            }

            bestCoverageRate = cellsSortedByBenefit.OrderByDescending(cb => cb.CellCoverage).ThenBy(cb => cb.CellCost).ToList()[0].CellCoverage;

            return cellsSortedByBenefit.OrderByDescending(cb => cb.CellCoverage).ThenBy(cb => cb.CellCost).ToList();
        }

        private List<Cell> CreateWallCellsListForTargetCell(Cell targetCell, out IterationParameters iterationP)
        {
            var wallCellsCoveredByRouter = new List<Cell>();
            var targetCellRowPosition = targetCell.Row;
            var targetCellColumnPosition = targetCell.Column;

            // find restrictions, if any, for further calculations
            var startRowRestriction = targetCellRowPosition - routerRadius;

            if (startRowRestriction < 0)
            {
                startRowRestriction = 0;
            }

            var endRowRestriction = targetCellRowPosition + routerRadius;

            if (endRowRestriction >= building.cells.GetLength(0))
            {
                endRowRestriction = building.cells.GetLength(0) - 1;
            }

            var startColumnRestriction = targetCellColumnPosition - routerRadius;

            if (startColumnRestriction < 0)
            {
                startColumnRestriction = 0;
            }

            var endColumnRestriction = targetCellColumnPosition + routerRadius;

            if (endColumnRestriction >= building.cells.GetLength(1))
            {
                endColumnRestriction = building.cells.GetLength(1) - 1;
            }

            iterationP = new IterationParameters
            {
                StartRowRestriction = startRowRestriction,
                EndRowRestriction = endRowRestriction,
                StartColumnRestriction = startColumnRestriction,
                EndColumnRestriction = endColumnRestriction
            };

            // Considering restrictions calculated above, iterating over all cells that router would cover if was 
            // connected to a backbone in given coordinates
            for (int i = startRowRestriction; i <= endRowRestriction; i++)
            {
                for (int j = startColumnRestriction; j <= endColumnRestriction; j++)
                {
                    if (building.cells[i, j].Type == "wall")
                    {
                        wallCellsCoveredByRouter.Add(building.cells[i, j]);
                    }
                }
            }
            return wallCellsCoveredByRouter;
        }

        private int CountNumberOfTargetCellsCovered(IterationParameters iterationP, Cell routerCell, List<Cell> wallCells)
        {
            var count = 0;
            for (int i = iterationP.StartRowRestriction; i <= iterationP.EndRowRestriction; i++)
            {
                for (int j = iterationP.StartColumnRestriction; j <= iterationP.EndColumnRestriction; j++)
                {
                    if (building.cells[i, j].Type == "target")
                    {
                        if (IsTargetCellCovered(building.cells[i, j], routerCell, wallCells))
                        {
                            // if target cell is already covered it shouldn't be taken into account anymore
                            if (building.cells[i, j].IsCovered) continue;
                            count++;
                        }
                    }
                }
            }
            return count;
        }

        private bool IsTargetCellCovered(Cell targetCell, Cell routerCell, List<Cell> wallCells)
        {
            foreach (var wallCell in wallCells)
            {
                if (Math.Min(routerCell.Row, targetCell.Row) <= wallCell.Row 
                    && wallCell.Row <= Math.Max(routerCell.Row, targetCell.Row) 
                    && Math.Min(routerCell.Column, targetCell.Column) <= wallCell.Column 
                    && wallCell.Column <= Math.Max(routerCell.Column, targetCell.Column))
                {
                    return false;
                }
            }
            return true;
        }

        private int CalculateCostOfSettingRouterInTargetCell(Cell targetCell)
        {
            var nearestBackboneCellWithDistance = FindNearestBackboneCellToTargetCell(targetCell);

            return nearestBackboneCellWithDistance.distance;
        }

        private BackboneDistance FindNearestBackboneCellToTargetCell(Cell targetCell)
        {
            var listOfBackboneCellsWithAccordingDistancesToTargetCell = new List<BackboneDistance>();

            foreach (var backboneCell in building.backboneCells)
            {
                var distance = CalculateDistanceFromCellToCell(targetCell, backboneCell);

                listOfBackboneCellsWithAccordingDistancesToTargetCell.Add(new BackboneDistance() {
                    backboneCell = backboneCell,
                    distance = distance
                });
            }

            return listOfBackboneCellsWithAccordingDistancesToTargetCell.OrderBy(bc => bc.distance).ToList()[0];
        }

        private int CalculateDistanceFromCellToCell(Cell cell1, Cell cell2)
        {
            double distance = Math.Sqrt(Math.Pow((cell2.Row - cell1.Row), 2) + Math.Pow((cell2.Column - cell1.Column), 2));
            return (int)Math.Floor(distance);
        }

        private void PutRouterIntoCell(Cell routerCell)
        {
            routerCell.HasRouter = true;
            building.routerCells.Add(routerCell);

            var wallListForRouterCell = CreateWallCellsListForTargetCell(routerCell, out var iterationP);

            MarkTargetCellsAsCoveredForRouterCellCoverRange(routerCell, iterationP, wallListForRouterCell);
            FillConnectedBackboneCellList(routerCell);
        }

        private void MarkTargetCellsAsCoveredForRouterCellCoverRange(Cell routerCell, IterationParameters iterationP, List<Cell> wallCells)
        {
            for (int i = iterationP.StartRowRestriction; i <= iterationP.EndRowRestriction; i++)
            {
                for (int j = iterationP.StartColumnRestriction; j <= iterationP.EndColumnRestriction; j++)
                {
                    if (building.cells[i, j].Type == "target")
                    {
                        if (IsTargetCellCovered(building.cells[i, j], routerCell, wallCells))
                        {
                            building.cells[i, j].IsCovered = true;
                        }
                    }
                }
            }
        }

        private void FillConnectedBackboneCellList(Cell routerCell)
        {
            var nearestBackboneCell = FindNearestBackboneCellToTargetCell(routerCell).backboneCell;

            var rRow = routerCell.Row;
            var bRow = nearestBackboneCell.Row;
            var rCol = routerCell.Column;
            var bCol = nearestBackboneCell.Column;

            // Defining in which direction the backbone has to be installed depending on coordinates

            // First case - coordinates are the same (routerCell is already connected to a backbone)
            if (routerCell.Row == nearestBackboneCell.Row && routerCell.Column == nearestBackboneCell.Column) return;

            // Second case - If backbone should be installed in straight line
            if (routerCell.Row == nearestBackboneCell.Row || routerCell.Column == nearestBackboneCell.Column)
            {
                ConnectBackboneInStraightLine(rRow, rCol, bRow, bCol);
            }
            
            // Other cases (moving diagonally)
            if (routerCell.Row > nearestBackboneCell.Row && routerCell.Column > nearestBackboneCell.Column)
            {
                do
                {
                    building.backboneCells.Add(building.cells[++bRow, ++bCol]);
                } while (rRow != bRow && rCol != bCol);

                // in case router can be installed straightly diagonally
                if (rRow == bRow && rCol == bCol) return;

                ConnectBackboneInStraightLine(rRow, rCol, bRow, bCol);
            }
            if (routerCell.Row < nearestBackboneCell.Row && routerCell.Column > nearestBackboneCell.Column)
            {
                do
                {
                    building.backboneCells.Add(building.cells[--bRow, ++bCol]);
                } while (rRow != bRow && rCol != bCol);

                // in case router can be installed straightly diagonally
                if (rRow == bRow && rCol == bCol) return;

                ConnectBackboneInStraightLine(rRow, rCol, bRow, bCol);
            }
            if (routerCell.Row < nearestBackboneCell.Row && routerCell.Column < nearestBackboneCell.Column)
            {
                do
                {
                    building.backboneCells.Add(building.cells[--bRow, --bCol]);
                } while (rRow != bRow && rCol != bCol);

                // in case router can be installed straightly diagonally
                if (rRow == bRow && rCol == bCol) return;

                ConnectBackboneInStraightLine(rRow, rCol, bRow, bCol);
            }
            if (routerCell.Row > nearestBackboneCell.Row && routerCell.Column < nearestBackboneCell.Column)
            {
                do
                {
                    building.backboneCells.Add(building.cells[++bRow, --bCol]);
                } while (rRow != bRow && rCol != bCol);

                // in case router can be installed straightly diagonally
                if (rRow == bRow && rCol == bCol) return;

                ConnectBackboneInStraightLine(rRow, rCol, bRow, bCol);
            }
        }

        private void ConnectBackboneInStraightLine(int rRow, int rCol, int bRow, int bCol)
        {
            // Row coordinates are the same (backbone should be installed from either 
            // left to right to a router cell or vice versa)
            if (rRow == bRow)
            {
                if (rCol > bCol)
                {
                    // from left to right
                    ConnectBackboneFromLeftToRight(rCol, bRow, bCol);
                }
                else
                {
                    // from right to left
                    ConnectBackboneFromRightToLeft(rCol, bRow, bCol);
                }
            }
            // Columns are equal
            if (rCol == bCol)
            {
                if (rRow > bRow)
                {
                    // from top to bottom
                    ConnectBackboneFromTopToBottom(rRow, bRow, bCol);
                }
                else
                {
                    // from bottom to top
                    ConnectBackboneFromBottomToTop(rRow, bRow, bCol);
                }
            }
        }

        private void ConnectBackboneFromLeftToRight(int rCol, int bRow, int bCol)
        {
            do
            {
                building.backboneCells.Add(building.cells[bRow, ++bCol]);
            } while (rCol != bCol);
        }

        private void ConnectBackboneFromRightToLeft(int rCol, int bRow, int bCol)
        {
            do
            {
                building.backboneCells.Add(building.cells[bRow, --bCol]);
            } while (rCol != bCol);
        }

        private void ConnectBackboneFromTopToBottom(int rRow, int bRow, int bCol)
        {
            do
            {
                building.backboneCells.Add(building.cells[++bRow, bCol]);
            } while (rRow != bRow);
        }

        private void ConnectBackboneFromBottomToTop(int rRow, int bRow, int bCol)
        {
            do
            {
                building.backboneCells.Add(building.cells[--bRow, bCol]);
            } while (rRow != bRow);
        }
    }
}
