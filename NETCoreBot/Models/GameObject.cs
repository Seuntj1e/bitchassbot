﻿using System;
using System.Collections.Generic;
using BitchAssBot.Enums;

namespace BitchAssBot.Models
{
    public class GameObject
    {
        
        public Guid Id { get; set; }
        public GameObjectType GameObjectType { get; set; }
        public Position Position { get; set; }

        public GameObject() { }

        public GameObject(GameObjectType type, Position position)
        {
            Id = Guid.NewGuid();
            GameObjectType = type;
            Position = position;
        }

        // Todo: please fix this because it is breaking the logger
        public List<int> ToStateList() =>
            new List<int>
            {
                // (int) GameObjectType,
                // Position.X,
                // Position.Y
            };

        public static GameObject FromStateList(Guid id, List<int> stateList) =>
            new GameObject
            {
                Id = id,
                GameObjectType = (GameObjectType)stateList[3],
                Position = new Position
                {
                    X = stateList[4],
                    Y = stateList[5]
                }
            };
    }
}