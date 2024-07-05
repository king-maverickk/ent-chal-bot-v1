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

        public string GetEdgeDirection(BotStateDTO botState)
        {
            int[][] view = botState.HeroWindow;

            if (view[3][4] == 255) return "left";
            if (view[5][4] == 255) return "right";
            if (view[4][3] == 255) return "down";
            if (view[4][5] == 255) return "up";

            return "not_on_edge";
        }
    }

    /* 
     example usage:

    public Queue<InputCommand> DecideNextMoves(BotStateDTO botState)
    {
        Queue<InputCommand> commands = new Queue<InputCommand>();

        bool onEdge = IsOnTerritoryEdge(botState);

        if (onEdge == true)
        {
            string edgeDirection = GetEdgeDirection(botState);
            Console.WriteLine($"The bot is on the {edgeDirection} edge of its territory.");
            
            // Add logic for what to do based on the edge direction
            switch (edgeDirection)
            {
                case "Left":
                    commands.Enqueue(InputCommand.LEFT);
                    break;
                case "Right":
                    commands.Enqueue(InputCommand.RIGHT);
                    break;
                case "Up":
                    commands.Enqueue(InputCommand.UP);
                    break;
                case "Down":
                    commands.Enqueue(InputCommand.DOWN);
                    break;
            }
        }
        else
        {
            Console.WriteLine("The bot is not on the edge of its territory.");
            // Add logic for what to do when not on the edge
        }

     */

}
