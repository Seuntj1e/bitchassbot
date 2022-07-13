﻿using System;
using BitchAssBot.Enums;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitchAssBot.Models
{
    public class Node : GameObject
    {
        public ResourceType Type { get; set; }
        public int MaxUnits { get; set; }
        public int CurrentUnits { get; set; }
        //TODO: can remove
        public Guid TerritoryBase { get; set; }

        public Node(GameObjectType gameObjectType, Position position) : base(gameObjectType, position)
        {
            CurrentUnits = 0;
        }
    }
}
