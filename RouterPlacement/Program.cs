using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RouterPlacement.Entities;

namespace RouterPlacement
{
    class Program
    {
        static void Main(string[] args)
        {
            var input = File.ReadAllText("C:\\Users\\1\\Source\\Repos\\ConsoleApp1\\ConsoleApp1\\Input\\charleston_road.in");
            var building = Parser.Parse(input);

            var solver = new Solver(building);

            solver.FindOptimalRouterPlacement();

            var score = solver.CalculateScore();

            solver.CreateOutput();

            Console.WriteLine("Your score is " + score);
            Console.ReadKey();
        }
    }
}
