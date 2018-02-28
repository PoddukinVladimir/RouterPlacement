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
            SystemUtils.Maximize(); 
            var path = Environment.CurrentDirectory + "\\Input\\charleston_road.in";
            var input = File.ReadAllText(path);
            var building = Parser.Parse(input);

            var solver = new Solver(building);

            solver.FindOptimalRouterPlacement();

            var score = solver.CalculateScore();

            solver.CreateOutput();

            //Console.WriteLine("Your score is " + score);
            Console.ReadKey();
        }
    }
}
