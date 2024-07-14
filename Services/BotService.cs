using ent_chal_bot_v1.Enums;
using ent_chal_bot_v1.Models;

namespace ent_chal_bot_v1.Services;
public class BotService
{
    private Guid BotId;
    public BotCommand ProcessState(BotStateDTO botState)
    {
        return new BotCommand
        {
            BotId = BotId,
            Action = InputCommand.RIGHT
        };
    }
    public void SetBotId(Guid botId)
    {
        BotId = botId;
    }
}
