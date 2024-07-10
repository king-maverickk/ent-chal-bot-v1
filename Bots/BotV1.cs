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
        private readonly BotStateDTO _botState;
        private readonly HubConnection _connection;

        public BotV1(BotStateDTO botState, HubConnection connection)
        {
            _botState = botState;
            _connection = connection;
        }

        public void adaptiveEncircle(int targetArea)
        {
            int currentArea = 0;
            InputCommand direction = InputCommand.LEFT;
            while (currentArea < targetArea)
            {
                if (canSafelyMove(direction))
                {

                }
            }
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
            int botPositionX = 4;
            int botPositionY = 4;

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

            (int x, int y) botPosition = (4, 4); // Bot's position in the 9x9 view
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

            // Check if the new position contains the bot's trail
            // 4 is the cell type for Bot0's trail. Change as needed
            return _botState.HeroWindow[newX][newY] == 4;
        }

        public InputCommand FindOptimalDirection()
        {
            var directions = new[] { InputCommand.UP, InputCommand.RIGHT, InputCommand.DOWN, InputCommand.LEFT };
            return directions.OrderByDescending(scoreDirection).First();
        }

        public int scoreDirection(InputCommand direction)
        {
            int score = 0;
            score += scoreUnclaimedArea(direction);
            score -= scoreDistanceToEnemyTerritory(direction);
            return score;
        }

        // check the land adjacent - in a straight line - to the bot
        public int scoreUnclaimedArea(InputCommand direction)
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
            (int x, int y) spawnPoint

            return commands;
        }
    }
}