using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ent_chal_bot_v1.Models;

namespace ent_chal_bot_v1.Bots
{
    internal class BotV3
    {
        public void printStuff(List<int> a)
        {
            foreach (var coord in a)
            {
                Console.Write(coord + " ");
            }
            Console.WriteLine();
        }
        public List<int> randomPath(int x, int y)
        {
            // Initialize variables
            // x = 12; y = 1 are example values

            // Create a dictionary with string keys and List<int> values
            var path = new Dictionary<string, List<int>>
            {
                { "path1", new List<int> { x + 1, y + 1, x - 1, y - 1 } },
                { "path2", new List<int> { x + 2, y + 2, x - 2, y - 2 } },
                { "path3", new List<int> { x + 3, y + 3, x - 3, y - 3 } }
            };

            // Create a Random object
            Random rand = new Random();

            // Get a list of keys from the dictionary
            List<string> keys = new List<string>(path.Keys);

            // Pick a random key
            string randomKey = keys[rand.Next(keys.Count)];

            // Get the corresponding list
            List<int> randomPath = path[randomKey];

            return randomPath;
        }


        private List<(int, int)> paths = new List<(int, int)>();
        private double acquiredArea; // new area
        public void recordPosition(BotStateDTO _botState)
        {
            int x = _botState.X;
            int y = _botState.Y;
            paths.Add((x, y));

            if (isOnTerritory(_botState))
            {
                calculateAquired();
                
            }
        }

        public bool isOnTerritory(BotStateDTO _botState)
        {
            return _botState.HeroWindow[5][5] == 0;
        }

        public double calculateArea(List<(int, int)> pathValues) // shoelace method/function to calculate area.
        {
            int n = pathValues.Count;
            if (n < 3) return 0; // not a polygon

            double area = 0;
            for (int i = 0; i < n; i++)
            {
                int x1 = pathValues[i].Item1, y1 = pathValues[i].Item2;
                int x2 = pathValues[(i + 1) % n].Item1, y2 = pathValues[(i + 1) % n].Item2;

                area += (x1 * y2 - x2 * y1);
            }
            area = Math.Abs(area) / 2.0;
            return area;
        }

        public void calculateAquired()
        {
            double area = calculateArea(paths);
            acquiredArea += area;
            Console.WriteLine($"Enclosed Area: {area} units");
        }
    }
}
