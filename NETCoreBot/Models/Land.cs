using System;
using System.Collections.Generic;

namespace BitchAssBot.Models
{
    public class Land : Position
    {
        public Guid Owner { get; set; }
        public Guid NodeOnLand { get; set; }

        public List<Occupants> Occupants { get; set; }
        private readonly Dictionary<Guid, Occupants> occupantsByBot;

        public Land(Position position, Guid owner, Guid nodeOnLand) : base(position.X, position.Y)
        {
            Owner = owner;
            NodeOnLand = nodeOnLand;
            Occupants = new List<Occupants>();
            occupantsByBot = new Dictionary<Guid, Occupants>();
        }

        public Land() { }

        public Dictionary<Guid, Occupants> GetOccupantsByBotDictionary()
        {
            return occupantsByBot;
        }
        public bool EnemyBorder { get; set; }
        public bool Occupied { get; set; }
    }

    public class Occupants
    {
        public Guid BotId { get; set; }
        public int Count { get; set; }

        public int Pressure { get; set; }
        public double HomeGroundWeight { get; set; }
        public int RadialWeight { get; set; }
        public bool ownsLand;


        public Occupants(double distanceFromBotBase, Guid botId)
        {
            HomeGroundWeight = 1.1;
            BotId = botId;
            RadialWeight = (int)Math.Floor(1 + 10 / (distanceFromBotBase + 0.01)); // todo: add these to the config
            CalculatePressure();
        }

        public Occupants() { }

        public void SetOwnsLand(bool value)
        {
            ownsLand = value;
            CalculatePressure();
        }

        public bool GetOwnsLand()
        {
            return ownsLand;
        }

        public void CalculatePressure()
        {
            var tempPressure = (Count + 1) * RadialWeight * (ownsLand ? HomeGroundWeight : 1);
            Pressure = (int)Math.Floor(tempPressure);
        }

        public void Add(int newOccupantAmount)
        {
            Count += newOccupantAmount;
            CalculatePressure();
        }

        public int Vacate()
        {
            int unitsLeaving = Count;
            Count = 0;
            CalculatePressure();
            return unitsLeaving;
        }
    }
}