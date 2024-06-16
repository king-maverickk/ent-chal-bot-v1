using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ent_chal_bot_v1.Enums;

namespace ent_chal_bot_v1.Models
{
    // ... p. 37
    public class BotCommand
    {
        public Guid BotId { get; set; }
        public InputCommand Action { get; set; }
    }

}
