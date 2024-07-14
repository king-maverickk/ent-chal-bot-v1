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
            // x = 5; y = 10 are example values

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
    }
}
