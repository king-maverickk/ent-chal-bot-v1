using ent_chal_bot_v1.Enums;
using ent_chal_bot_v1.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ent_chal_bot_v1.Bots
{
    public class BotV1
    {
        private BotStateDTO _botState;
        private readonly HubConnection _connection;

        public BotV1(BotStateDTO botState, HubConnection connection)
        {
            _botState = botState;
            _connection = connection;
        }

        public void updateState(BotStateDTO botState)
        {
            _botState = botState;
        }

        public InputCommand AdaptiveEncircle(int targetArea)
        {
            int currentArea = 0;
            InputCommand direction = InputCommand.LEFT;
            while (currentArea < targetArea)
            {
                if (canSafelyMove(direction))
                {
                    return direction;
                }
                else
                {
                    return FindOptimalDirection();
                }
            }
            return FindOptimalDirection();
        }

        // check if bot is within the grid bounds
        public List<(int, int)> getCellTypes(CellType cellType)
        {
            int[][] view = _botState.HeroWindow;
            List<(int, int)> unclaimedPlots = new List<(int, int)>();

            for (int y = view[0].Length - 1; y >= 0; y--)
            {
                for (int x = 0; x < view.Length; x++)
                {
                    if (view[x][y] == (int)cellType)
                    {
                        unclaimedPlots.Add((x, y));
                    }
                }
            }
            return unclaimedPlots;
        }

        public List<(int, int)> distanceToCellType(List<(int, int)> cellCoordinates)
        {
            List<(int, int)> relativePositions = new List<(int, int)>();
            int botPositionX = 5;
            int botPositionY = 5;

            foreach (var coordinate in cellCoordinates)
            {
                int x = coordinate.Item1 - botPositionX;
                int y = coordinate.Item2 - botPositionY;
                relativePositions.Add((x, y));
            }
            return relativePositions;
        }

        public InputCommand GetNextMove()
        {
            List<(int, int)> targetCells = getCellTypes(CellType.Unclaimed); // Adjust CellType as needed
            List<(int, int)> distances = distanceToCellType(targetCells);

            (int x, int y) botPosition = (5, 5); // Bot's position in the 11x11 view
            (int x, int y) closestCell = (0, 0);
            int shortestDistance = int.MaxValue;

            for (int i = 0; i < distances.Count; i++)
            {
                int distance = Math.Abs(distances[i].Item1) + Math.Abs(distances[i].Item2); // Manhattan distance
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    closestCell = targetCells[i];
                }
            }

            return GetMovementCommand(botPosition, closestCell);
        }

        private InputCommand GetMovementCommand((int x, int y) botPos, (int x, int y) targetPos)
        {
            if (targetPos.x > botPos.x)
            {
                return InputCommand.RIGHT;
            }
            else if (targetPos.x < botPos.x)
            {
                return InputCommand.LEFT;
            }
            else if (targetPos.y > botPos.y)
            {
                return InputCommand.UP;
            }
            else
            {
                return InputCommand.DOWN;
            }
        }
        
        public bool canSafelyMove(InputCommand direction)
        {
            return IsWithinBounds(direction) && WillHitOwnTrail(direction);
        }
        
        public bool IsWithinBounds(InputCommand direction)
        {
            int newX = _botState.X;
            int newY = _botState.Y;

            switch (direction)
            {
                case InputCommand.LEFT:
                    newX--;
                    break;
                case InputCommand.RIGHT:
                    newX++;
                    break;
                case InputCommand.DOWN:
                    newY++;
                    break;
                case InputCommand.UP:
                    newY--;
                    break;
            }

            // Check if new position is within the 50x50 bounds
            return newX >= 0 && newX < 50 && newY >= 0 && newY < 50;
        }

        public bool WillHitOwnTrail(InputCommand direction)
        {
            int newX = 5;
            int newY = 5;

            switch (direction)
            {
                case InputCommand.LEFT:
                    newX--;
                    break;
                case InputCommand.RIGHT:
                    newX++;
                    break;
                case InputCommand.DOWN:
                    newY++;
                    break;
                case InputCommand.UP:
                    newY--;
                    break;
            }

            // Check if the new position contains the bot's trail
            // 4 is the cell type for Bot0's trail. Change as needed
            return _botState.HeroWindow[newX][newY] == 4;
        }

        public InputCommand FindOptimalDirection()
        {
            var directions = new[] { InputCommand.DOWN, InputCommand.UP, InputCommand.LEFT, InputCommand.RIGHT };
            return directions.OrderByDescending(scoreDirection).First();
        }

        public int scoreDirection(InputCommand direction)
        {
            return ScoreConeShapedArea(direction, 5, 5);
        }

        public int CalculateManhattanDistance(int x1, int y1, int x2, int y2)
        {
            // This calculates the Manhattan distance between two points
            return Math.Abs(x1 - x2) + Math.Abs(y1 - y2);
        }

        public int ScoreConeShapedArea(InputCommand direction, int coneWidth, int coneDepth)
        {
            int score = 0;
            int[][] view = _botState.HeroWindow;
            int centerX = 5; // Assuming the bot is at the center of an 11x11 grid
            int centerY = 5;

            // Define the direction vectors
            int dx = 0, dy = 0;
            switch (direction)
            {
                case InputCommand.UP: dy = -1; break;
                case InputCommand.DOWN: dy = 1; break;
                case InputCommand.LEFT: dx = -1; break;
                case InputCommand.RIGHT: dx = 1; break;
            }

            for (int depth = 1; depth <= coneDepth; depth++)
            {
                // Calculate the width of the cone at this depth
                int widthAtDepth = (depth * coneWidth) / coneDepth;

                for (int offset = -widthAtDepth / 2; offset <= widthAtDepth / 2; offset++)
                {
                    int x = centerX + (depth * dx) + (dy != 0 ? offset : 0);
                    int y = centerY + (depth * dy) + (dx != 0 ? offset : 0);

                    // Check if the cell is within the view
                    if (x >= 0 && x < view.Length && y >= 0 && y < view[0].Length)
                    {
                        int cellType = view[x][y];
                        int distance = CalculateManhattanDistance(centerX, centerY, x, y);

                        // Score the cell based on its type and distance
                        score += ScoreCellByTypeAndDistance(cellType, distance);
                    }
                }
            }

            return score;
        }

        public int ScoreCellByTypeAndDistance(int cellType, int distance)
        {
            int baseScore;
            switch (cellType)
            {
                case (int)CellType.Unclaimed:
                    baseScore = 10;
                    break;
                case (int)CellType.Bot0Territory: // Assuming Bot0 is this bot
                    baseScore = 5;
                    break;
                case (int)CellType.Bot1Territory:
                case (int)CellType.Bot2Territory:
                case (int)CellType.Bot3Territory:
                    baseScore = -5;
                    break;
                case (int)CellType.OutOfBounds:
                    baseScore = -15;
                    break;
                case (int)CellType.Bot0Trail:
                    baseScore = -10;
                    break;
                default:
                    baseScore = 0;
                    break;
            }

            // Adjust score based on distance. Closer cells have more impact.
            return baseScore / (distance + 1);
        }

        public int ScoreCornerPriority(InputCommand direction)
        {
            int score = 0;
            int gridSize = 50; // Assuming a 50x50 grid
            int cornerThreshold = 5; // Consider the bot "in a corner" if within 5 cells of a corner

            // Get the bot's current position
            int currentX = _botState.X;
            int currentY = _botState.Y;

            // Calculate new position after move
            int newX = currentX;
            int newY = currentY;
            switch (direction)
            {
                case InputCommand.LEFT:
                    newX--;
                    break;
                case InputCommand.RIGHT:
                    newX++;
                    break;
                case InputCommand.UP:
                    newY--;
                    break;
                case InputCommand.DOWN:
                    newY++;
                    break;
            }

            // Check if the move brings us closer to any corner
            if (IsCloserToCorner(newX, newY, currentX, currentY))
            {
                // Base score for moving towards a corner
                score += 50;

                // Additional score based on how close to a corner we are
                int distanceToNearestCorner = DistanceToNearestCorner(newX, newY, gridSize);
                if (distanceToNearestCorner < cornerThreshold)
                {
                    // Bonus points for being very close to a corner
                    score += (cornerThreshold - distanceToNearestCorner) * 10;
                }
            }

            return score;
        }

        private bool IsCloserToCorner(int newX, int newY, int currentX, int currentY)
        {
            int gridSize = 50;
            return DistanceToNearestCorner(newX, newY, gridSize) < DistanceToNearestCorner(currentX, currentY, gridSize);
        }

        private int DistanceToNearestCorner(int x, int y, int gridSize)
        {
            // Calculate distances to all four corners
            int topLeft = Math.Abs(x) + Math.Abs(y);
            int topRight = Math.Abs(gridSize - 1 - x) + Math.Abs(y);
            int bottomLeft = Math.Abs(x) + Math.Abs(gridSize - 1 - y);
            int bottomRight = Math.Abs(gridSize - 1 - x) + Math.Abs(gridSize - 1 - y);

            // Return the minimum distance
            return Math.Min(Math.Min(topLeft, topRight), Math.Min(bottomLeft, bottomRight));
        }

        // check the land adjacent - in a straight line - to the bot
        public int scoreUnclaimedArea(InputCommand direction)
        {
            int score = 0;
            int[][] view = _botState.HeroWindow;

            int centerRow = 5;
            int centerCol = 5;

            switch (direction)
            {
                case InputCommand.LEFT:
                    for (int col = centerCol - 1; col >= 0; col--)
                    {
                        if (view[centerRow][col] == (int)CellType.Unclaimed)
                        {
                            score+= 2;
                        }
                        else
                        {
                            break; // Stop if we hit a claimed land
                        }
                    }
                    break;
                case InputCommand.RIGHT:
                    for (int col = centerCol + 1; col < view[centerRow].Length; col++)
                    {
                        if (view[centerRow][col] == (int)CellType.Unclaimed)
                        {
                            score += 2;
                        }
                        else
                        {
                            break; // Stop if we hit a claimed land
                        }
                    }
                    break;
                case InputCommand.DOWN:
                    for (int row = centerRow + 1; row < view.Length; row++)
                    {
                        if (view[row][centerCol] == (int)CellType.Unclaimed)
                        {
                            score += 2;
                        }
                        else
                        {
                            break; // Stop if we hit a claimed land
                        }
                    }
                    break;
                case InputCommand.UP:
                    for (int row = centerRow - 1; row >= 0; row--)
                    {
                        if (view[row][centerCol] == (int)CellType.Unclaimed)
                        {
                            score += 2;
                        }
                        else
                        {
                            break; // Stop if we hit a claimed land
                        }
                    }
                    break;
            }
            return score;
        }

        // this needs tweaking because I have a feeling it may avoid territories that are far but come closer to territories that are close
        public int scoreDistanceToEnemyTerritory(InputCommand direction)
        {
            int score = 0;
            int[][] view = _botState.HeroWindow;

            int centerRow = 4;
            int centerCol = 4;

            switch (direction)
            {
                case InputCommand.LEFT:
                    for (int col = centerCol - 1; col >= 0; col--)
                    {
                        if (view[centerRow][col] == (int)CellType.Bot1Territory ||
                            view[centerRow][col] == (int)CellType.Bot2Territory ||
                            view[centerRow][col] == (int)CellType.Bot3Territory)
                        {
                            score -= 2;
                        }
                        else
                        {
                            break; // stop if the land is not territory
                        }
                    }
                    break;
                case InputCommand.RIGHT:
                    for (int col = centerCol + 1; col < view[centerRow].Length; col++)
                    {
                        if (view[centerRow][col] == (int)CellType.Bot1Territory ||
                            view[centerRow][col] == (int)CellType.Bot2Territory ||
                            view[centerRow][col] == (int)CellType.Bot3Territory)
                        {
                            score -= 2;
                        }
                        else
                        {
                            break; // stop if the land is not territory
                        }
                    }
                    break;
                case InputCommand.DOWN:
                    for (int row = centerRow + 1; row < view.Length; row++)
                    {
                        if (view[row][centerCol] == (int)CellType.Bot1Territory ||
                            view[row][centerCol] == (int)CellType.Bot2Territory ||
                            view[row][centerCol] == (int)CellType.Bot3Territory)
                        {
                            score -= 2;
                        }
                        else
                        {
                            break; // stop if the land is not territory
                        }
                    }
                    break;
                case InputCommand.UP:
                    for (int row = centerRow - 1; row >= 0; row--)
                    {
                        if (view[row][centerCol] == (int)CellType.Bot1Territory ||
                            view[row][centerCol] == (int)CellType.Bot2Territory ||
                            view[row][centerCol] == (int)CellType.Bot3Territory)
                        {
                            score -= 2;
                        }
                        else
                        {
                            break; // stop if the land is not territory
                        }
                    }
                    break;
            }

            return score;
        }

        public Queue<InputCommand> ReturnToTerritoryQueue()
        {
            Queue<InputCommand> commands = new Queue<InputCommand>();
            (int x, int y) spawnPoint;

            return commands;
        }
    }
}