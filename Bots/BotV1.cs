using ent_chal_bot_v1.Enums;
using ent_chal_bot_v1.Models;
using Microsoft.AspNetCore.SignalR.Client;

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

        // check if bot is within the grid bounds
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

        public int scoreDirection(InputCommand direction)
        {
            int score = 0;
            score += scoreUnclaimedArea(direction);
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
                        if (view[centerRow][col] == 0) // 255 represents unclaimed land
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
                        if (view[centerRow][col] == 255) // 255 represents unclaimed land
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
                        if (view[row][centerCol] == 255) // 255 represents unclaimed land
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
                        if (view[row][centerCol] == 255) // 255 represents unclaimed land
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
    }
}