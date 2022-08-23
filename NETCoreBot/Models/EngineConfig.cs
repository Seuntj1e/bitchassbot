﻿using BitchAssBot.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitchAssBot.Models
{
    public class EngineConfigDto
    {
        public string RunnerUrl { get; set; }
        public string RunnerPort { get; set; }
        public int BotCount { get; set; }
        public int MaxTicks { get; set; }
        public int ScoutWorkTime { get; set; }
        public int TickRate { get; set; }
        public int WorldLength => RegionSize * NumberOfRegionsInMapLength;
        public int WorldArea => WorldLength ^ 2;
        public int RegionSize { get; set; }
        public int ProcessTick { get; set; }
        public int BaseZoneSize { get; set; }
        public int NumberOfRegionsInMapLength { get; set; }
        public int ResourceWorldCoverage { get; set; }
        public double PopulationDecreaseRatio { get; set; }
        public int WorldSeed { get; set; }
        public Dictionary<GameObjectType, decimal> ConsumptionRatio { get; set; }
        public Dictionary<GameObjectType, int> ScoreRates { get; set; }
        public ResourceScoreMultiplier ResourceScoreMultiplier { get; set; }
        public UnitConsumptionRatio UnitConsumptionRatio { get; set; }
        public IList<PopulationTier> PopulationTiers { get; set; }
        public int StartingFood { get; set; }
        public int StartingUnits { get; set; }
        public ResourceGenerationConfig ResourceGenerationConfig { get; set; }
        public Seeds Seeds { get; set; }
        public int MinimumPopulation { get; set; }
        public ResourceImportance ResourceImportance { get; set; }
        public int MinimumUnits { get; set; }
        public IList<Building> Buildings { get; set; }
    }

    public class ResourceGenerationConfig
    {
        public RenewableResourceConfig Farm { get; set; }
        public NonRenewableResourceConfig Wood { get; set; }
        public NonRenewableResourceConfig Stone { get; set; }
        public NonRenewableResourceConfig Gold { get; set; }
        public ConsumptionResourceConfig Campfire { get; set; }
    }

    public class ResourceConfig
    {
        public int ProximityDistance { get; set; }
        public IList<string> DistributionZones { get; set; }
        public IList<int> QuantityRangePerRegion { get; set; }
        public IList<int> RewardRange { get; set; }
        public IList<int> WorkTimeRange { get; set; }
        public IList<int> MaxUnitsRange { get; set; }
    }

    public class ConsumptionResourceConfig : ResourceConfig
    {
        public Dictionary<ResourceType, IList<int>> ResourceConsumption { get; set; }
    }

    public class RenewableResourceConfig : ResourceConfig
    {
        public RegenerationRateRange RegenerationRateRange { get; set; }
        public IList<int> AmountRange { get; set; }
    }

    public class NonRenewableResourceConfig : ResourceConfig
    {
        public IList<int> AmountRange { get; set; }
    }

    public class RegenerationRateRange
    {
        public IList<int> TickRange { get; set; }
        public IList<int> AmountRange { get; set; }
    }

    public class Seeds
    {
        public List<int> PlayerSeeds { get; set; }
        public int MaxSeed { get; set; }
        public int MinSeed { get; set; }
    }

    public class ResourceScoreMultiplier
    {
        public int Population { get; set; }
        public int Food { get; set; }
        public int Wood { get; set; }
        public int Stone { get; set; }
        public int Gold { get; set; }
    }

    public class UnitConsumptionRatio
    {
        public double Food { get; set; }
        public double Wood { get; set; }
        public double Stone { get; set; }
        public double Gold { get; set; }
        public double Heat { get; set; }
    }

    public class UnitActionDuration
    {
        public int Farm { get; set; }
        public int Scout { get; set; }
        public int Lumber { get; set; }
        public int Mine { get; set; }
    }

    public class UnitActionReward
    {
        public int Farm { get; set; }
        public int Lumber { get; set; }
        public int Stone { get; set; }
        public int Gold { get; set; }
    }

    public class ResourceImportance
    {
        public double Food { get; set; }
        public double Heat { get; set; }
    }

    public class Building
    {
        public BuildingType BuildingType { get; set; }
        public int StatusEffectMultiplier { get; set; }
        public int TerritorySquare { get; set; }
        public int ScoreMultiplier { get; set; }
        public int BuildTime { get; set; }
        public Cost Cost { get; set; }
        Cost actualcost;
        public Cost ActualCost
        {
            get
            {
                if (actualcost == null)
                {
                    actualcost = new Cost { Gold=Cost.Gold, Wood=Cost.Wood, Stone=Cost.Stone };
                }
                return actualcost;
            }
        }
        public int NumBuildings { get; private set; } = 0;
        public void UpdateCosts(int numberofbuildings)
        {
            
            if (numberofbuildings != NumBuildings)
            {
                NumBuildings = numberofbuildings;
                ActualCost.Wood = GetBuildingCost(Cost.Wood, NumBuildings);
                ActualCost.Stone = GetBuildingCost(Cost.Stone, NumBuildings);
                ActualCost.Gold = GetBuildingCost(Cost.Gold, NumBuildings);
            }
        }
        private static int GetBuildingCost(int numberOfBuildingsPerType, int cost) => cost + ((numberOfBuildingsPerType * cost) / 2);
        public double weightedCost { get; private set; }
        public void UpdateWeightedCost(double Wood, double Stone, double Gold)
        {
            weightedCost = ActualCost.Wood * Wood + ActualCost.Stone * Stone + ActualCost.Gold * Gold;
        }
    }

    public class Cost
    {
        public int Wood { get; set; }
        public int Stone { get; set; }
        public int Gold { get; set; }
    }
}