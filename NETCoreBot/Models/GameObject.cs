﻿using BitchAssBot.Enums;
using System;
using System.Collections.Generic;

namespace BitchAssBot.Models
{
    public class GameObject
    {
        public Guid Id { get; set; }
        public GameObjectType GameObjectType { get; set; }
        public Position Position { get; set; }

        public GameObject(GameObjectType type)
        {
            Id = new Guid();
            GameObjectType = type;
        }

    }
}
