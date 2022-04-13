using Domain.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BitchAssBot.Models
{
    public class GameStateDto
    {
        public World World { get; set; }

        public List<BotDto> Bots { get; set; }

        public Guid BotId { get; set; }
        public List<PopulationTier> PopulationTiers { get; set; }

    }
}
