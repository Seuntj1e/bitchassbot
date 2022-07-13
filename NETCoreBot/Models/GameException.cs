﻿using System;

namespace BitchAssBot.Models
{
    public class GameException
    {
        public int CurrentTick { get; set; }
        public Guid? BotId { get; set; }
        public string ExceptionMessage { get; set; }
    }
}