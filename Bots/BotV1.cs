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
            if (_botState.HeroWindow[newX][newY] == 4)
            {
                return true;
            } 
            else
            {
                return false;
            }
        }
    }
}