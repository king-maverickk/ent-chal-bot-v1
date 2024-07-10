using System;
using ent_chal_bot_v1.Enums;
using ent_chal_bot_v1.Models;

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

    public bool IsOnBoundaryEdge(BotStateDTO botState)
    {
        int x = botState.X;
        int y = botState.Y;

        if (x == 0) return true;  // Left edge
        if (y == 0) return true;  // Bottom edge
        if (x == 49) return true; // Right edge
        if (y == 49) return true; // Top edge

        return false;
    }

    public Queue<InputCommand> DecideNextMoves(BotStateDTO botState)
    {
        bool onEdge = IsOnTerritoryEdge(botState);
        Queue<InputCommand> commands = new Queue<InputCommand>();

        if (onEdge)
        {
            string direction = GetEdgeDirection(botState);

            switch (direction)
            {
                case "left":
                    commands.Enqueue(InputCommand.LEFT);
                    break;
                case "right":
                    commands.Enqueue(InputCommand.RIGHT);
                    break;
                case "down":
                    commands.Enqueue(InputCommand.DOWN);
                    break;
                case "up":
                    commands.Enqueue(InputCommand.UP);
                    break;
            }
            return commands;
        }
        else
        {
            Console.WriteLine("The bot is not on the edge of its territory. Finding path to nearest edge.");
            commands.Enqueue(InputCommand.LEFT);
            return commands;
        }
    }

    public List<Tuple<int, int>> GeneratePath(int startX, int startY, int plotSize)
    {
        List<Tuple<int, int>> path = new List<Tuple<int, int>>();
        path.Add(new Tuple<int, int>(startX, startY)); // Start position

        // Move down
        for (int i = 0; i < plotSize; i++)
            path.Add(new Tuple<int, int>(startX, startY + i + 1));

        // Move left
        for (int i = 0; i < plotSize; i++)
            path.Add(new Tuple<int, int>(startX - i - 1, startY + plotSize));

        // Move up
        for (int i = 0; i < plotSize; i++)
            path.Add(new Tuple<int, int>(startX - plotSize, startY + plotSize - i - 1));

        // Move right (back to start)
        for (int i = 0; i < plotSize; i++)
            path.Add(new Tuple<int, int>(startX - plotSize + i + 1, startY));

        return path;
    }

    public InputCommand GetNextCommand(Tuple<int, int> current, Tuple<int, int> next)
    {
        if (next.Item1 > current.Item1)
            return InputCommand.RIGHT;
        else if (next.Item1 < current.Item1)
            return InputCommand.LEFT;
        else if (next.Item2 > current.Item2)
            return InputCommand.DOWN;
        else
            return InputCommand.UP;
    }

}
