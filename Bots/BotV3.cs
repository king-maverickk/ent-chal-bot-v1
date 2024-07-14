using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        public void recordPathVertex(int x, int y)
        {
            paths.Add((x, y));
        }

        public List<(int, int)> cornerVertices(List<(int, int)> pathValues)
        {
            (int top_left_cornerX, int top_left_cornerY) = (0, 0);
            (int top_right_cornerX, int top_right_cornerY) = (0, 0);
            (int bottom_left_cornerX, int bottom_left_cornerY) = (0, 0);
            (int bottom_right_cornerX, int bottom_right_cornerY) = (0, 0);

            foreach (var pathValue in pathValues)
            {
                if (pathValue.Item1 < top_left_cornerX && pathValue.Item2 < top_left_cornerY)
                {
                    top_left_cornerX = pathValue.Item1;
                    top_left_cornerY = pathValue.Item2;
                }
                else if (pathValue.Item1 > top_right_cornerX && pathValue.Item2 < top_right_cornerY)
                {
                    top_right_cornerX = pathValue.Item1;
                    top_right_cornerY = pathValue.Item2;
                }
                else if (pathValue.Item1 < bottom_left_cornerX && pathValue.Item2 > bottom_left_cornerY)
                {
                    bottom_left_cornerX = pathValue.Item1;
                    bottom_left_cornerY = pathValue.Item2;
                }
                else if (pathValue.Item1 > bottom_right_cornerX && pathValue.Item2 > bottom_right_cornerY)
                {
                    bottom_right_cornerX = pathValue.Item1;
                    bottom_right_cornerY = pathValue.Item2;
                }
            }

            List<(int, int)> cornerVerticesList = new List<(int, int)>()
            {
                (top_left_cornerX, top_left_cornerY),
                (top_right_cornerX, top_right_cornerY),
                (bottom_left_cornerX, bottom_left_cornerY),
                (bottom_right_cornerX, bottom_right_cornerY)
            };

            return cornerVerticesList;
        }



        public void calculateAquired()
        {

            // could use track record of x and y
            // then make those as vertices .
            // the furtherest edges
            // BFS flood fill? because the paths are gonna be bad.. crashing into its self, etc. 

            // make a list of x and y movements.
            // take list and compare the values like most_left_corner > coord (<<<< REFINED)
            // BFS flood fill
        }
    }
}
