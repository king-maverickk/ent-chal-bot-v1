using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ent_chal_bot_v1.Models;

namespace ent_chal_bot_v1
{
    internal class TrianglePlan
    {
        public bool IsOnTerritoryEdge(BotStateDTO botState)
        {
            int[][] view = botState.HeroWindow;

            // Check the four adjacent cells
            if (view[3][4] == 255) // Left
            {
                return true;
            }
            if (view[5][4] == 255) // Right
            {
                return true;
            }
            if (view[4][3] == 255) // Down
            {
                return true;
            }
            if (view[4][5] == 255) // Up
            {
                return true;
            }

            return false;
        }
    }

}
