using System;
using System.Collections.Generic;

namespace BitchAssBot.Models
{
    public class GameState
    {
        public World World { get; set; }
        public List<BotDto> Bots { get; set; }
        public List<PopulationTier> PopulationTiers { get; set; }

    }
}