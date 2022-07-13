using BitchAssBot.Enums;
using BitchAssBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitchAssBot.Services
{
    public class BotService
    {
        static bool console = true;
        static bool logging = true;
        static bool everytick = false;
        public bool started = false;

        public Guid Id;
        private BotDto _bot;
        private CommandAction _playerAction;
        private PlayerCommand _playerCommand;
        private GameState _gameState;
        public static EngineConfigDto _engineConfigDto;

        Dictionary<Guid, nodestate> nodes = new Dictionary<Guid, nodestate>();
        Dictionary<Guid,int> towers = new Dictionary<Guid, int>();
        int campfires = 0;
        int Previouscampfires = 0;
        ResourceType Current = ResourceType.Food;
        int cycle = 0;
        BotDto _previousstate;
        int tickstart = 1;

        double foodconsumption = 0;
        double woodconsumption = 0;
        double stoneconsumption = 0.005;
        double goldconsumption = 0.005;
        double heatconsumption = 0;
        double campfirecost = 0;
        double campfirereward = 0;
        double fooddays = 0;
        double wooddays = 0;
        double stonedays = 0;
        double golddays = 0;
        double foodrewards = 0;
        double woodrewards = 0;
        double stonerewards = 0;
        double goldrewards = 0;
        double HeatCap = 6000;
        double UnitScoreCost = 0;
        long totalticks = 0;
        long maxprocticks = 0;
        int maxTicks = 2500;
        double campfiresthiscycle = 0;
        bool Haveeverything = false;
        bool expand = false;
        int lastindex = 0;
        ResourceType[] resourcerotation = new ResourceType[] { ResourceType.Wood, ResourceType.Food,   ResourceType.Food, ResourceType.Wood, ResourceType.Food,  ResourceType.Heat, ResourceType.Food, ResourceType.Stone };
        int MaxPOp = 0;
        bool singleplayer = false;
        bool booming = true;
        int HeatRemaining = 99999999;
        double woodconsuming = 0;
        double woodamount = 0;

        public BotService()
        {
            _playerAction = new CommandAction();
            _playerCommand = new PlayerCommand();
            _gameState = new GameState();
            _bot = new BotDto();
            _engineConfigDto = new EngineConfigDto();

        }

        public BotDto GetBot()
        {
            return _bot;
            
        }
        Random r = new Random();
        bool issueing = false;
        public PlayerCommand GetPlayerCommand()
        {
            System.Diagnostics.Stopwatch wtch = new System.Diagnostics.Stopwatch();
            wtch.Start();
            started = true;
            PlayerCommand playerCommand = new PlayerCommand();
            playerCommand.PlayerId = this._bot.Id;

            try
            {
                var dto = this._gameState.Bots.Find(go => go.Id == _bot.Id);
                /*if (console && (dto.Tick % cycle == 1 || everytick))
                    Console.SetCursorPosition(0, 0);*/
                var _previousstate = this._previousstate;
                this._previousstate = dto;
                if (_previousstate == null)
                    _previousstate = dto;
                if (!issueing && dto.Tick > _previousstate.Tick || dto.Tick == 0)
                {
                    issueing = true;
                    Dictionary<Guid, AvailableNode> abanodes = new Dictionary<Guid, AvailableNode>();
                    Dictionary<Position, AvailableNode> territory = new Dictionary<Position, AvailableNode>();
                    Dictionary<Position, AvailableNode> edge = new Dictionary<Position, AvailableNode>();
                    Dictionary<Position, BuildingObject> buildings = new Dictionary<Position, BuildingObject>();
                    for (int i =0; i< _gameState.World.Map.AvailableNodes.Count;i++)
                    {
                        if (dto.Territory.Contains(_gameState.World.Map.AvailableNodes[i].Position))
                        {
                            abanodes.Add(_gameState.World.Map.AvailableNodes[i].Id, _gameState.World.Map.AvailableNodes[i]);
                            territory.Add(_gameState.World.Map.AvailableNodes[i].Position, _gameState.World.Map.AvailableNodes[i]);
                        }
                    }
                    for (int i = 0; i< dto.Buildings.Count; i++)
                    {
                        buildings.Add(dto.Buildings[i].Position, dto.Buildings[i]);
                    }
                    var CurrentTier = _engineConfigDto.PopulationTiers[dto.CurrentTierLevel];
                    var NextTier = _engineConfigDto.PopulationTiers[Math.Min(dto.CurrentTierLevel+1, _engineConfigDto.PopulationTiers.Count-1)];
                    var tmpNodes = nodes.Values.OrderByDescending(m => m.ResourceValue).ToList();
                    int units = dto.AvailableUnits;
                    var ownNodes = this._gameState.World.Map.Nodes.OrderBy(m => GetDistance(m)).ToList();
                    for  (int i =0; i<ownNodes.Count;i++)
                    {
                        ResourceNode x = ownNodes[i];
                        if (!nodes.ContainsKey(x.Id))
                        {
                            nodes.Add(x.Id, new nodestate(x, GetDistance(x)));
                        }
                        nodes[x.Id].Node = x;
                        nodes[x.Id].checkReturnedUnits(dto.Tick);
                    }

                    if (dto.Population > MaxPOp)
                        MaxPOp = dto.Population;

                    var Woods = tmpNodes.Where(m => m.Node.Type == ResourceType.Wood && m.Node.Amount > 00 /*&& m.ScoreVal >= UnitScoreCost*/).ToList(); ;
                    var Stones = tmpNodes.Where(m => m.Node.Type == ResourceType.Stone && m.Node.Amount > 00 /*&& m.ScoreVal >= UnitScoreCost*/).ToList(); ;
                    var Golds = tmpNodes.Where(m => m.Node.Type == ResourceType.Gold && m.Node.Amount > 00 /*&& m.ScoreVal >= UnitScoreCost*/).ToList(); ;
                    woodamount = Woods.Sum(m => m.Node.Amount);
                    double stoneamount = Stones.Sum(m => m.Remaining);
                    double goldamount = Golds.Sum(m => m.Remaining);
                    if (_gameState.World.Map.Nodes.Count > 250 && booming)
                        booming = woodamount > 1000;


                    if (dto.Tick % cycle == 1 || everytick)
                        Log($"\r\n{DateTime.Now: HH:mm:ss ffff} Population: {dto.Population} Units: {dto.AvailableUnits} Traveling FCUKED NODES { this._gameState.World.Map.Nodes.Count} TICK {dto.Tick} HEATCAP {HeatCap}".PadRight(10) +
                            $"\r\nFood: {dto.Food} Wood: {dto.Wood} Stone: {dto.Stone} gold: {dto.Gold} Heat: {dto.Heat} Heat consume {HeatRemaining}".PadRight(10) +
                            $"\r\nFarm: FCUKED Lumb: FCUKED Mines: FCUKED gold: ???? scout: FCUKED".PadRight(10) +
                            $"\r\nWoodamount {woodamount} {(booming ? "BOOMING" : "NOT     ")}".PadRight(10)

                            );

                    if ((cycle > 0 && dto.Tick % cycle == 1))
                    {
                        campfiresthiscycle = 0;
                    }



                    if (units == 0)
                    {
                        issueing = false;
                        wtch.Stop();
                        totalticks += wtch.ElapsedTicks;
                        if (wtch.ElapsedTicks > maxprocticks)
                            maxprocticks = wtch.ElapsedTicks;
                        return playerCommand;
                    }
                    //playerCommand.PlayerId = this._bot.Id;

                    if (this._gameState.World != null)
                    {
                        if (ownNodes.Count() > 0 && GetDistance(ownNodes[0]) < 5)
                        {

                            if (Haveeverything && dto.Population >= 50 /*|| dto.CurrentTierLevel>0*/)
                            {

                                {
                                    var foods = tmpNodes.Where(m => m.Node.Type == ResourceType.Food && m.Node.Amount > 1000 /*&& m.ScoreVal >= UnitScoreCost*/).ToList();


                                    foodrewards = foods.Count > 0 ? foods.Sum(m => m.Reward) : foodrewards;
                                    fooddays = foods.Count > 0 ? foods.Sum(m => m.TravelTime) : fooddays;
                                    double m = (foodrewards / fooddays * (double)cycle);
                                    double ff = ((double)foodconsumption * (double)_engineConfigDto.ResourceImportance.Food) / m;



                                    woodrewards = Woods.Count > 0 ? Woods.Sum(m => m.Reward) : woodrewards;
                                    wooddays = Woods.Count > 0 ? Woods.Sum(m => m.TravelTime) : wooddays;
                                    double n = woodrewards / wooddays * (double)cycle;
                                    double phaseloading = (dto.Population < _gameState.PopulationTiers[3].MaxPopulation ? //early game, focus on food
                                        0.1 :
                                       dto.CurrentTierLevel == 6 ?
                                       (campfirecost / campfirereward) * 2
                                       :
                                        (campfirecost / campfirereward)

                                        );
                                    double woodcon2 = woodconsumption + phaseloading;
                                    double wf = (woodcon2) / n;

                                    double sf = 0;
                                    if (Stones.Count > 0)
                                    {
                                        stonerewards = Stones.Count > 0 ? Stones.Sum(m => m.Reward) : stonerewards;
                                        stonedays = Stones.Count > 0 ? Stones.Sum(m => m.TravelTime) : stonedays;
                                        double o = 0;
                                        if (stonedays > 0 && stonerewards > 0)

                                        {
                                            o = stonerewards / stonedays * (double)cycle;
                                            sf = (stoneconsumption) / o;
                                        }
                                    }
                                    double gf = 0;
                                    if (Golds.Count > 0)
                                    {
                                        goldrewards = Golds.Count > 0 ? Golds.Sum(m => m.Reward) : goldrewards;
                                        golddays = Golds.Count > 0 ? Golds.Sum(m => m.TravelTime) : golddays;
                                        double o = 0;
                                        if (golddays > 0 && goldrewards > 0)

                                        {
                                            o = goldrewards / golddays * (double)cycle;
                                            gf = (goldconsumption) / o;
                                        }
                                    }


                                    if (foods.Count <= 2 || Woods.Count <= 2 || Stones.Count <= 2 || Golds.Count<=2)
                                    {

                                        int tunits = Math.Max(3, units);
                                        //int sent = tunits;
                                        List<CommandAction> commands = Scout(ref tunits);
                                        if (commands.Count > 0)
                                        {
                                            playerCommand.Actions.AddRange(commands);
                                            units -= commands.Count;
                                        }

                                    }

                                    //var Heats = nodes.Values.Where(m => m.Node.Type == ResourceType.Heat);
                                    double Heatrewards = campfirereward;
                                    double Heatdays = 1;
                                    double p = Heatrewards / Heatdays * (double)cycle;
                                    double hf = (heatconsumption + 0.05) / p;

                                    double TotalWeight = ff + wf + sf + hf+gf;

                                    int foodunits = 0;
                                    int woodunits = 0;
                                    int stoneunits = 0;
                                    int goldunits = 0;
                                    int heatunits = 0;
                                    int buildunits = 0;
                                    var requiredheat = (dto.Population * (10 / _engineConfigDto.ResourceImportance.Heat * CurrentTier.PopulationChangeFactorRange[1])) + dto.Population * 2;
                                    var requiredwood = _engineConfigDto.PopulationTiers[dto.CurrentTierLevel].TierMaxResources.Wood> _engineConfigDto.PopulationTiers[6].TierResourceConstraints.Wood
                                        &&  dto.CurrentTierLevel<6 ?
                                        _engineConfigDto.PopulationTiers[6].TierResourceConstraints.Wood*1.1:
                                          _engineConfigDto.PopulationTiers[dto.CurrentTierLevel + 1].TierResourceConstraints.Wood * 1.1;

                                    if (units >= 4)
                                    {
                                       
                                        heatunits = (int)Math.Ceiling((units * (hf / TotalWeight)));
                                        int MinHeats = (int)Math.Ceiling((((dto.Population * (booming ? 3 : 1.1) / heatconsumption) / 10) / campfirereward));
                                       
                                        if (heatunits < MinHeats)
                                        {
                                            heatunits = MinHeats;
                                        }
                                        if (heatunits > units)
                                            heatunits = units;

                                        //dto.Food
                                        if (dto.Tick < maxTicks)
                                        {
                                            //set the heatcap according to available food

                                            if (dto.Tick > 1000 && UnscoutedTowers() && units > 0)
                                            {
                                                var Result = Scout(ref units);
                                                if (Result.Count > 0)
                                                    playerCommand.Actions.AddRange(Result);
                                            }
                                        }


                                        if (booming)
                                        {
                                            HeatCap = _engineConfigDto.PopulationTiers[_engineConfigDto.PopulationTiers.Count-2].MaxPopulation * 2;
                                            HeatCap = Math.Min(HeatCap, dto.Population * 2.5);
                                        }
                                        else
                                        {
                                            HeatCap = 0;
                                        }
                                        if (dto.Tick >= maxTicks - (cycle))
                                            HeatCap = 0;

                                        /*if (woodamount < 1000 || stoneamount < 1000)
                                            HeatCap = 50;*/
                                        if (dto.Heat >= HeatCap)
                                        {
                                            heatunits = 0;                                            
                                        }
                                        else if (dto.Heat + (heatunits * Heatrewards) > HeatCap)
                                        {
                                            heatunits = (int)((HeatCap - dto.Heat) / Heatrewards);
                                        }
                                        campfiresthiscycle += heatunits;
                                        if (dto.Wood < heatunits * campfirecost)
                                        {
                                            int oldheat = heatunits;
                                            heatunits = (int)Math.Floor(dto.Wood / campfirecost);                                           
                                        }
                                        if (heatunits > units)
                                            heatunits = units;
                                        units -= heatunits;


                                        foodunits = (int)Math.Floor((units * (ff / TotalWeight)));
                                        if (woodamount > 0)
                                            woodunits = (int)Math.Floor((units * (wf / TotalWeight)));
                                        if (stoneamount > 0)
                                            stoneunits = (int)Math.Floor((units * (sf / TotalWeight)));
                                        if (goldamount > 0)
                                            goldunits = (int)Math.Floor((units * (gf / TotalWeight)));


                                        units -= (foodunits + woodunits + stoneunits + goldunits);
                                        if (units > 0 && dto.Population > 100)
                                        {
                                            if (UnscoutedTowers())
                                            {
                                                playerCommand.Actions.AddRange(Scout(ref units));
                                            }
                                            foodunits += units;
                                        }
                                        else if (units > 0)
                                        {
                                            foodunits += units;
                                            units = 0;
                                        }
                                    }
                                    else
                                    {
                                        while (units > 0)
                                        {
                                            switch (resourcerotation[lastindex++])
                                            {
                                                case ResourceType.Food: foodunits++; units--; break;
                                                case ResourceType.Wood: woodunits++; units--; break;
                                                case ResourceType.Heat: heatunits++; units--; break;
                                                case ResourceType.Stone: stoneunits++; units--; break;
                                                case ResourceType.Gold: goldunits++; units--; break;
                                            }
                                            if (lastindex >= resourcerotation.Length)
                                                lastindex = 0;
                                        }
                                       
                                    }

                                    if (dto.Tick % cycle == 1 && dto.Tick > 1000)
                                    {
                                        HeatRemaining = CalcHeatLeft(dto.Tick, dto.Population);
                                    }

                                    if (booming)
                                    {
                                        int index = 0;
                                        //for (int i =0;  foodunits+woodunits+stoneunits > 0 && i<2 && left;i++)
                                        double pendinggold = 0;
                                        double pendingstone = 0;
                                        double pendingwood = 0;
                                        double pendingfood = 0;
                                        for (int i = 0; i < dto.PendingActions.Count;i++ )
                                        {
                                            if (nodes.ContainsKey(dto.PendingActions[i].TargetNodeId))
                                            {
                                                switch (nodes[dto.PendingActions[i].TargetNodeId].Node.Type)
                                                {
                                                    case ResourceType.Food: pendingfood += dto.PendingActions[i].NumberOfUnits * nodes[dto.PendingActions[i].TargetNodeId].Reward; break;
                                                    case ResourceType.Wood: pendingwood += dto.PendingActions[i].NumberOfUnits * nodes[dto.PendingActions[i].TargetNodeId].Reward; break;
                                                    case ResourceType.Stone: pendingstone += dto.PendingActions[i].NumberOfUnits * nodes[dto.PendingActions[i].TargetNodeId].Reward; break;
                                                    case ResourceType.Gold: pendinggold += dto.PendingActions[i].NumberOfUnits * nodes[dto.PendingActions[i].TargetNodeId].Reward; break;
                                                }
                                            }
                                        }
                                        
                                        {
                                            


                                            if (CurrentTier.TierMaxResources.Stone * 0.80 <= (double)dto.Stone + pendingstone && booming)
                                            {
                                                goldunits += stoneunits;
                                                stoneunits = 0;
                                            }
                                            while (stoneunits > 0 && index < Stones.Count)
                                            {
                                                var closestnode = Stones[index];// nodes.Values.Find(m => m.Node.Type == ResourceType.Stone && m.Remaining > 0);
                                                                                //var closestnode = ownNodes.Find(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                                if (closestnode == null)
                                                {
                                                    playerCommand.Actions.AddRange(Scout(ref stoneunits));
                                                    //units = foodunits;
                                                    //stoneunits = 0;
                                                    //closestnode = ownNodes.Find(m => m.Type == ResourceType.Food && m.Amount > 100);
                                                }
                                                if (stoneunits > 0 && closestnode != null)
                                                {
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                                    int uisedunits = stoneunits;
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                                    var tmpAction = MineNode(closestnode.Node, ref uisedunits);
                                                    if (tmpAction != null)
                                                        playerCommand.Actions.Add(tmpAction);
                                                    stoneunits -= uisedunits;
                                                }
                                                index++;
                                            }
                                            if (stoneunits > 0)
                                                goldunits += stoneunits;
                                            index = 0;
                                            if (CurrentTier.TierMaxResources.Gold * 0.80 <= (double)dto.Gold + pendinggold && booming)
                                            {
                                                foodunits += goldunits;
                                                goldunits = 0;
                                            }
                                            while (goldunits > 0 && index < Golds.Count)
                                            {
                                                var closestnode = Golds[index];// nodes.Values.Find(m => m.Node.Type == ResourceType.Stone && m.Remaining > 0);
                                                                               //var closestnode = ownNodes.Find(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                                if (closestnode == null)
                                                {
                                                    playerCommand.Actions.AddRange(Scout(ref goldunits));
                                                    //units = foodunits;
                                                    //stoneunits = 0;
                                                    //closestnode = ownNodes.Find(m => m.Type == ResourceType.Food && m.Amount > 100);
                                                }
                                                if (goldunits > 0 && closestnode != null)
                                                {
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                                    int uisedunits = goldunits;
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                                    var tmpAction = MineNode(closestnode.Node, ref uisedunits);
                                                    if (tmpAction != null)
                                                        playerCommand.Actions.Add(tmpAction);
                                                    goldunits -= uisedunits;
                                                }
                                                index++;
                                            }
                                            if (goldunits > 0)
                                                foodunits += goldunits;
                                            index = 0;
                                            if (CurrentTier.TierMaxResources.Food * 0.80 <= (double)dto.Food + pendingfood&& booming )
                                            {
                                                woodunits += foodunits;
                                                foodunits = 0;
                                            }
                                            while (foodunits > 0 && index < foods.Count)
                                            {
                                                var closestnode = foods[index];// nodes.Values.Find(m => m.Node.Type == ResourceType.Food && m.Remaining > 0);
                                                                               //var closestnode = ownNodes.Find(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                                if (closestnode == null)
                                                {
                                                    playerCommand.Actions.AddRange(Scout(ref foodunits));
                                                    //units = foodunits;
                                                    //foodunits = 0;
                                                    Log("No farms left bitch");
                                                    //closestnode = ownNodes.Find(m => m.Type == ResourceType.Food && m.Amount > 100);
                                                }
                                                if (foodunits > 0 && closestnode != null)
                                                {
                                                    int uisedunits = foodunits;
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                                    var tmpAction = MineNode(closestnode.Node, ref uisedunits);
                                                    if (tmpAction != null)
                                                        playerCommand.Actions.Add(tmpAction);
                                                    foodunits -= uisedunits;
                                                }
                                                index++;
                                            }
                                            if (foodunits > 0)
                                                woodunits += foodunits;
                                            index = 0;
                                            if (dto.Heat > requiredheat)
                                            {
                                                woodunits += heatunits;
                                                //heatunits = 0;
                                            }
                                            if (CurrentTier.TierMaxResources.Wood * 0.8 <= (double)dto.Wood && booming )
                                            {

                                                /*
                                                 
                                                determine how much wood is coming back in 3 ticks
                                                determine how many campfires to make space for that
                                                set heatunits to that.
                                                lumber with everthing else

                                                 */

                                                //double woody = 0;
                                                //for (int i =0; i< dto.Actions.Count;i++)
                                                //{
                                                //    if (nodes.ContainsKey(dto.Actions[i].TargetNodeId))
                                                //    {
                                                //        if (dto.Actions[i].TickActionCompleted == dto.Tick + 1)//???? 3? 2? I DON"T FUCKING KNOW
                                                //        {
                                                //            woody += dto.Actions[i].NumberOfUnits * nodes[dto.Actions[i].TargetNodeId].Reward;
                                                //        }
                                                //    }
                                                //}

                                                //if (dto.Wood + woody > CurrentTier.tierMaxResources.Wood)
                                                //{
                                                //    int newheatunits = (int)((dto.Wood + woody - CurrentTier.tierMaxResources.Wood) / campfirecost);
                                                //    if (newheatunits < heatunits)
                                                //    {
                                                //        woodunits = heatunits - newheatunits;
                                                //        heatunits = newheatunits;
                                                //    }
                                                //}
                                                buildunits = Math.Min(10, woodunits);
                                                woodunits-=buildunits;
                                                heatunits += woodunits;
                                                int oldheat = heatunits;
                                                if (dto.CurrentTierLevel < 6)
                                                {
                                                    if (dto.Wood - (campfires * campfirecost) - (heatunits * campfirecost) <= requiredwood)
                                                    {
                                                        var tmp = (dto.Wood - requiredwood) * campfirecost;
                                                        if (tmp < 0)
                                                            tmp = 0;
                                                        heatunits = (int)tmp;
                                                    }
                                                    heatunits = (int)Math.Min((double)heatunits, (double)((HeatRemaining - dto.Heat) / campfirereward));
                                                    heatunits = (int)Math.Min((double)heatunits, Math.Floor(dto.Wood / campfirecost));
                                                    heatunits = Math.Max(heatunits, 0);
                                                    woodunits = oldheat - heatunits;
                                                }
                                                else
                                                {
                                                    woodunits += heatunits;
                                                }
                                            }
                                            /*if (dto.CurrentTierLevel>=5)
                                            {
                                                woodunits += heatunits;
                                            }*/
                                            int newwood = 0;
                                            while (woodunits > 0 && index < Woods.Count )
                                            {
                                                var closestnode = Woods[index];// nodes.Values.Find(m => m.Node.Type == ResourceType.Wood && m.Remaining > 0);
                                                                               //var closestnode = ownNodes.Find(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                                if (closestnode == null)
                                                {
                                                    playerCommand.Actions.AddRange(Scout(ref woodunits));
                                                    //units = foodunits;
                                                    //woodunits = 0;
                                                    //closestnode = ownNodes.Find(m => m.Type == ResourceType.Food && m.Amount > 100);
                                                }
                                                if (woodunits > 0 && closestnode != null)
                                                {
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                                    int uisedunits = woodunits;
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest

                                                    var tmpAction = MineNode(closestnode.Node, ref uisedunits);
                                                    if (tmpAction != null)
                                                    {
                                                        newwood += tmpAction.Units * closestnode.Reward;
                                                        playerCommand.Actions.Add(tmpAction);
                                                    }
                                                    woodunits -= uisedunits;
                                                }
                                                index++;
                                            }
                                            if (buildunits > 0)
                                            {
                                                //can build?
                                                //determine cheapest building to build
                                                int quarries = 0;
                                                int farms = 0;
                                                int mills = 0;
                                                for (int i=0;i<dto.Buildings.Count;i++)
                                                {
                                                    switch (dto.Buildings[i].Type)
                                                    {
                                                        case BuildingType.FarmersGuild:farms++;break;
                                                        case BuildingType.LumberMill: mills++; break;
                                                        case BuildingType.Quarry: quarries++; break;
                                                    }
                                                }
                                                quarries += dto.PendingActions.Count(m => m.ActionType == ActionType.Quarry) + dto.Actions.Count(m => m.ActionType == ActionType.Quarry);
                                                farms += dto.PendingActions.Count(m => m.ActionType == ActionType.FarmersGuild) + dto.Actions.Count(m => m.ActionType == ActionType.FarmersGuild);
                                                mills += dto.PendingActions.Count(m => m.ActionType == ActionType.LumberMill) + dto.Actions.Count(m => m.ActionType == ActionType.LumberMill);
                                                double quarrycost = 0;
                                                double farmcostcost = 0;
                                                double millcost = 0;
                                                double woodcost = 0;
                                                double stonecost = 0;
                                                double goldcost = 0;
                                                List<Guid> newItems = new List<Guid>();


                                                foreach (var x in abanodes.Values)
                                                {

                                                    var pendingaction = dto.PendingActions.Find(m => m.TargetNodeId == x.Id);
                                                    var action = dto.Actions.Find(m => m.TargetNodeId == x.Id);
                                                    if (pendingaction == null
                                                            && action == null
                                                            && !newItems.Contains(x.Id))
                                                    {
                                                        for (int i = 1; i <= 4; i++)
                                                        {
                                                            var tmpPos = x.Position.checknext(i);
                                                            var ExistingBuilding = dto.Buildings.Find(m => m.Position == tmpPos);
                                                            if (!dto.Territory.Contains(tmpPos)
                                                                && ExistingBuilding == null
                                                                )
                                                            {
                                                                if (!edge.ContainsKey(x.Position))
                                                                    edge.Add(x.Position, x);
                                                                break;

                                                            }
                                                        }
                                                    }
                                                    else
                                                    {

                                                    }
                                                }
                                                


                                                while (buildunits > 0)
                                                {
                                                    BuildingType typetobuild = BuildingType.Base;
                                                    for (int i = 0; i < _engineConfigDto.Buildings.Count; i++)
                                                    {
                                                        double numb = 0;
                                                        switch (_engineConfigDto.Buildings[i].BuildingType)
                                                        {
                                                            case BuildingType.FarmersGuild: numb = farms; break;
                                                            case BuildingType.LumberMill: numb = mills; break;
                                                            case BuildingType.Quarry: numb = quarries; break;
                                                        }
                                                        double woodbase = _engineConfigDto.Buildings[i].Cost.Wood;
                                                        double stonebase = _engineConfigDto.Buildings[i].Cost.Stone;
                                                        double goldbase = _engineConfigDto.Buildings[i].Cost.Gold;
                                                        woodbase += 0.5 * numb * woodbase;
                                                        stonebase += 0.5 * numb * stonebase;
                                                        goldbase += 0.5 * numb * goldbase;
                                                        double totalcost = (woodbase * wf) + (stonebase * sf) + (goldbase + gf);
                                                        switch (_engineConfigDto.Buildings[i].BuildingType)
                                                        {
                                                            case BuildingType.FarmersGuild: farmcostcost = totalcost; break;
                                                            case BuildingType.LumberMill: millcost = totalcost; break;
                                                            case BuildingType.Quarry: quarrycost = totalcost; break;
                                                        }
                                                    }
                                                    double buildingcount = 0;
                                                    if (quarrycost < farmcostcost && quarrycost < millcost)
                                                    {
                                                        typetobuild = BuildingType.Quarry;
                                                        
                                                        buildingcount = quarries;
                                                        quarries++;
                                                    }
                                                    else if (farmcostcost < millcost)
                                                    {
                                                        typetobuild = BuildingType.FarmersGuild;
                                                        buildingcount = farms;
                                                        farms++;
                                                    }
                                                    else
                                                    {
                                                        typetobuild = BuildingType.LumberMill;
                                                        buildingcount = mills;
                                                        mills++;
                                                    }
                                                    var buildingcost = _engineConfigDto.Buildings.FirstOrDefault(m => m.BuildingType == typetobuild).Cost;

                                                    woodcost = buildingcost.Wood + 0.5 * buildingcount * buildingcost.Wood;
                                                    stonecost = buildingcost.Stone + 0.5 * buildingcount * buildingcost.Stone;
                                                    goldcost = buildingcost.Gold + 0.5 * buildingcount * buildingcost.Gold;

                                                    if (dto.Wood - woodcost > NextTier.TierResourceConstraints.Wood*1.1
                                                        && dto.Stone - stonecost > NextTier.TierResourceConstraints.Stone*1.05
                                                        && dto.Gold - goldcost > NextTier.TierResourceConstraints.Gold*1.05
                                                        )
                                                    {
                                                        //alternatively determine the building with the biggest score effect
                                                        //check if thers's enough resources to build it
                                                        //for now assume enough?

                                                        //  Do not dip below next tiers min requirements unless positive there is time to regen resources before then
                                                        //  how determine that?
                                                        //find the closest available node with the most open spots next to it
                                                        //this is going to be a very expensive operation....
                                                        //this calculation does not cater to there being no more space outside of the territory
                                                        //fuck knows
                                                        if (buildunits>1)
                                                        {

                                                        }

                                                        AvailableNode closestEdgenode = null;
                                                        double closestdistance = 0;
                                                        foreach (var x in edge.Values)
                                                        {
                                                            if (closestEdgenode == null)
                                                            {
                                                                closestEdgenode = x;
                                                                closestdistance = GetDistance(x);
                                                            }
                                                            else if (GetDistance(x) < closestdistance)
                                                            {
                                                                closestEdgenode = x;
                                                                closestdistance = GetDistance(x);
                                                            }
                                                        }

                                                        if (closestEdgenode != null)
                                                        {
                                                            newItems.Add(closestEdgenode.Id);
                                                            edge.Remove(closestEdgenode.Position);
                                                            playerCommand.Actions.Add(new CommandAction()
                                                            {
                                                                Type = Enum.Parse<ActionType>(typetobuild.ToString()),
                                                                Id = closestEdgenode.Id,
                                                                Units = 1                                                            
                                                            });
                                                            
                                                            Log($"building {typetobuild.ToString()} at {closestEdgenode.Position.X}, {closestEdgenode.Position.Y}({closestEdgenode.Id}) Costing {woodcost} wood, {stonecost} stone, {goldcost} gold. BuildUnits: { buildunits } EdgePieces { edge.Count}");
                                                        }
                                                        buildunits--;
                                                    }
                                                    else
                                                    {
                                                        heatunits += buildunits;
                                                        buildunits = 0;
                                                        break;
                                                    }
                                                }
                                                
                                            }
                                            index = 0;
                                        }
                                    }
                                    else
                                    {
                                        var Resources = nodes.Values.Where(m => m.Node.Amount > 0).OrderByDescending(m => m.ScoreVal).ToList();
                                        units = dto.AvailableUnits;                                        
                                        
                                        for (int i = 0; i < Resources.Count && units > 0; i++)
                                        {
                                            int uisedunits = units;
                                            //get feedback from minenode as to hoy many are going and re-assign the rest
                                            var tmpAction = MineNode(Resources[i].Node, ref uisedunits);
                                            if (tmpAction != null)
                                            {
                                                playerCommand.Actions.Add(tmpAction);
                                                units -= uisedunits;
                                            }
                                        }
                                    }
                                    heatunits += Math.Min(0, units);
                                    heatunits += Math.Min(0, foodunits);
                                    heatunits += Math.Min(0, woodunits);
                                    heatunits += Math.Min(0, stoneunits);
                                    if (heatunits > 0)
                                    {
                                        /*var NextTier = new */
                                        
                                        if (booming)
                                        {
                                            var tmp2 = dto.Heat + campfires * campfirereward;//total heat
                                            if (tmp2 < requiredheat
                                                &&
                                                dto.Wood- campfires * campfirecost < requiredwood
                                                )
                                            {
                                                double tmp = requiredwood / campfirecost;
                                                if (tmp > 0 && tmp < heatunits)
                                                    heatunits = (int)tmp;
                                            }
                                            if (dto.Wood- campfires * campfirecost-heatunits*campfirecost < requiredwood)
                                            {
                                                heatunits = (int)((requiredwood - (double)dto.Wood - campfires * campfirecost) / campfirecost);
                                            }
                                            if (dto.Wood - campfires * campfirecost  < requiredwood)
                                            {
                                                heatunits = 0;
                                                Log("NEED WOOD!");
                                            }
                                            ///Log($"Total Heat: {tmp2} Heat Required {requiredheat} Tier wood {requiredwood} heatunits {heatunits}");
                                        }

                                        heatunits = (int)Math.Min((double)heatunits, (double)((HeatRemaining - dto.Heat) / campfirereward));
                                        heatunits = (int)Math.Min((double)heatunits, Math.Floor(dto.Wood / campfirecost));

                                        
                                        if (heatunits > 0)
                                        {
                                            playerCommand.Actions.Add(new CommandAction()
                                            {
                                                Type = ActionType.StartCampfire,
                                                Units = heatunits,
                                            });
                                        }
                                        
                                    }
                                    campfires = heatunits;
                                    //Log($"\r\nff: {ff:0.0000}\twf: {wf:0.0000}\tsf: {sf:0.0000}\thf: {hf:0.0000}\r\n" +
                                    //  $"fu: {playerCommand.Actions.Where(m => m.Type == ActionType.Farm).Sum(m => m.Units):00000}\twu: {playerCommand.Actions.Where(m => m.Type == ActionType.Lumber).Sum(m => m.Units):00000}\tsu: {playerCommand.Actions.Where(m => m.Type == ActionType.Mine).Sum(m => m.Units):00000}\thu: {playerCommand.Actions.Where(m => m.Type == ActionType.StartCampfire).Sum(m => m.Units):00000}\r\n");
                                }
                            }
                            else
                            {

                                #region old 
                                if (dto.Wood > 5 && dto.Food > 2 && !expand)
                                {
                                    expand = true;
                                    Current = ResourceType.Food;
                                }
                                if (expand && dto.Heat < dto.Population * 2.5)
                                {
                                    Previouscampfires = campfires;
                                    campfires = Math.Min(Math.Max(dto.Population / 5 * 2, 1), units);

                                    playerCommand.Actions.Add(new CommandAction()
                                    {
                                        Type = ActionType.StartCampfire,
                                        Units = campfires,
                                    });
                                    units -= campfires;
                                }

                                int foodunits = 0;
                                int woodunits = 0;
                                int stoneunits = 0;
                                while (units > 0)
                                {
                                    switch (resourcerotation[lastindex++])
                                    {
                                        case ResourceType.Food: foodunits++; units--; break;
                                        case ResourceType.Wood: woodunits++; units--; break;
                                        case ResourceType.Heat: stoneunits++; units--; break;//actually stone
                                    }
                                    if (lastindex > (dto.Population > 20 ? 6 : 5))
                                        lastindex = 0;
                                }
                                var ClosetstNodes = nodes.Values.OrderBy(m => m.TravelTime).ToList();

                                if (foodunits > 0)
                                {
                                    var closestnode = ClosetstNodes.Find(m => m.Node.Type == ResourceType.Food && m.Node.Amount > foodunits * m.Reward);
                                    var tmpAction = MineNode(closestnode.Node, ref foodunits);
                                    if (tmpAction != null)
                                        playerCommand.Actions.Add(tmpAction);
                                    units -= foodunits;

                                }
                                if (woodunits > 0)
                                {
                                    var closestnode = ClosetstNodes.Find(m => m.Node.Type == ResourceType.Wood && m.Node.Amount > woodunits * m.Reward);
                                    var tmpAction = MineNode(closestnode.Node, ref woodunits);
                                    if (tmpAction != null)
                                        playerCommand.Actions.Add(tmpAction);
                                    units -= woodunits;
                                }
                                if (stoneunits > 0)
                                {
                                    var closestnode = ClosetstNodes.Find(m => m.Node.Type == ResourceType.Stone && m.Node.Amount > stoneunits);
                                    if (closestnode != null)
                                    {
                                        var tmpAction = MineNode(closestnode.Node, ref stoneunits);
                                        if (tmpAction != null)
                                            playerCommand.Actions.Add(tmpAction);
                                        units -= stoneunits;
                                    }
                                    else
                                    {
                                        playerCommand.Actions.AddRange(Scout(ref stoneunits));
                                    }
                                }


                                #endregion
                            }
                        }
                        else
                        {
                            var scouting = dto.PendingActions.Find(m => m.ActionType == ActionType.Scout);
                            if (ownNodes.Count() == 0 && scouting == null)
                                towers.Clear();
                            if (UnscoutedTowers() && (ownNodes.Count() == 0 || (ownNodes.Count() > 0 && GetDistance(ownNodes[0]) > 5)))
                            {
                                if (units > 1)
                                {
                                    int unts = 2;
                                    playerCommand.Actions.AddRange(Scout(ref unts));
                                }
                            }
                        }
                    }
                    issueing = false;
                }

                _previousstate = dto;
            }
            catch (Exception e)
            {
                Log(e.ToString());

            }
            wtch.Stop();
            totalticks += wtch.ElapsedTicks;
            if (wtch.ElapsedTicks > maxprocticks)
                maxprocticks = wtch.ElapsedTicks;
            return playerCommand;
        }


        int CalcHeatLeft(int tick, double currentPop)
        {
            double totalheat = 0;
            double totalwood = 0;
            double onlywood = 0;
            double totalpop = currentPop;
            int tierlevel = _bot.CurrentTierLevel;

            for (int i = tick; i <= _engineConfigDto.MaxTicks-cycle+1; i += 10)
            {
                var Tier = _engineConfigDto.PopulationTiers[tierlevel];
                if (totalpop > Tier.MaxPopulation)
                {
                    tierlevel++;
                    Tier = _engineConfigDto.PopulationTiers[tierlevel];
                }
                totalpop *= (1.0 + Tier.PopulationChangeFactorRange[1]);
                totalheat += totalpop*heatconsumption;
                totalwood += (totalpop * heatconsumption)/campfirereward*campfirecost + (totalpop * woodconsumption);
                onlywood += totalpop += woodconsumption;
            }
            totalheat += totalpop;
            woodconsuming = onlywood;
            return (int)totalheat;            
        }

        internal void PrintFinal()
        {
            //Haveeverything = (cycle > 0 && foodconsumption > 0 && woodconsumption > 0 && campfirecost > 0 && campfirereward > 0 && stoneconsumption > 0 && heatconsumption > 0);
            console = true;
            Log($"Version:{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()}\r\n" +
                $"Measurements: Cycle: {cycle}\r\n" +
                $"foodconsumption: {foodconsumption}\r\n" +
                $"woodconsumption: {woodconsumption}\r\n" +
                $"campfirecost: {campfirecost}\r\n" +
                $"campfirereward: {campfirereward}\r\n" +
                $"stoneconsumption: {stoneconsumption}\r\n" +
                $"heatconsumption: {heatconsumption}\r\n" +
                $"Max Population: {MaxPOp}\r\n" +
                $"Avg Processing Time: {TimeSpan.FromTicks(totalticks / _bot.Tick).TotalMilliseconds}\r\n" +
                $"Max Processing Time: {TimeSpan.FromTicks(maxprocticks).TotalMilliseconds}\r\n" );


            Log($"Total Remaining Resources\r\n" +
                $"Food:  regen {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Food).Sum(m => m.RegenerationRate.Amount/ m.RegenerationRate.Ticks )} remaining {_gameState.World.Map.Nodes.Where(m=>m.Type == ResourceType.Food).Sum(m=>m.Amount)}\r\n" +
                $"Wood: {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Wood).Count()} nodes - {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Wood).Sum(m => m.Amount)}\r\n" +
                $"Stone: {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Stone).Count()} nodes- {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Stone).Sum(m => m.Amount)}\r\n");
            _bot.Actions = null;
            _bot.Map = null;
            _bot.PendingActions = null;            
            Log(Newtonsoft.Json.JsonConvert.SerializeObject(_bot));
            //Log("Useless nodes: " + Newtonsoft.Json.JsonConvert.SerializeObject(nodes.Values.Where(m => m.ScoreVal < UnitScoreCost).OrderBy(m => m.ScoreVal).ToList()));

        }

        int scouts = 0;
        bool UnscoutedTowers()
        {
            for (int i = this._gameState.World.Map.ScoutTowers.Count-1; i >=0; i--)
            {
                if (!towers.ContainsKey( this._gameState.World.Map.ScoutTowers[i].Id))
                {
                    return true;
                }
            }
            return false;
        }
        List<ScoutTower> OrderedTowers = null;

        internal List<CommandAction> Scout(ref int units)
        {
            if ((OrderedTowers == null && _gameState.World.Map.ScoutTowers.Count > 0) || (OrderedTowers != null || OrderedTowers.Count != _gameState.World.Map.ScoutTowers.Count))
                OrderedTowers = _gameState.World.Map.ScoutTowers.OrderBy(m => GetDistance(m)).ToList();
            List<CommandAction> actions = new List<CommandAction>();
            try
            {
                
                var tmpTowers = OrderedTowers;
                for (int i = 0; i < tmpTowers.Count && units > 0; i++)
                {
                    if (!towers.ContainsKey(tmpTowers[i].Id))
                        towers.Add(tmpTowers[i].Id, 0);
                    if (towers[tmpTowers[i].Id] <= scouts)
                    {

                        towers[tmpTowers[i].Id]++;
                        units--;


                        Log($"Sending scouts to {this._gameState.World.Map.ScoutTowers[i].Id}");
                        actions.Add(new CommandAction()
                        {
                            Type = ActionType.Scout,
                            Units = 1,
                            Id = tmpTowers[i].Id,
                        });
                    }
                    if (i + 1 == tmpTowers.Count)
                        scouts++;
                }
                if (scouts > 1)
                    scouts = 0;
                return actions;
            }catch
            {
                return actions;
            }
        }
        CommandAction MineNode(ResourceNode x, ref int Resources)
        {
            nodestate tmpState = null;// new nodestate { Node = x, Units = Resources };
            nodes.TryGetValue(x.Id, out tmpState);
            if (tmpState == null)
            {
                nodes.Add(x.Id, new nodestate(x, GetDistance(x)));
                tmpState = nodes[x.Id];
            }
            tmpState.Node = x;
            
            return tmpState.AddAction(ref Resources, _bot.Tick);
            
        }

        public void SetBot(BotDto bot)
        {
            _bot.Id = bot.Id;
        }

        public void ComputeNextPlayerAction(PlayerCommand playerCommand)
        {

            _playerAction = playerCommand.Actions[0];
        }

        public GameState GetGameState()
        {
            return _gameState;
        }

        public void SetGameState(GameState gameState)
        {
            _gameState = gameState;
            UpdateSelfState();
        }

        private void UpdateSelfState()
        {
            _bot = _gameState.Bots.Find(go => go.Id == _bot.Id);
        }

        private double GetDistance(GameObject location1, GameObject location2)
        {
            Position baseLocation = location1.Position;
            Position nodeLocation = location2.Position;

            double deltaX = baseLocation.X - nodeLocation.X;
            double deltaY = baseLocation.Y - nodeLocation.Y;
            var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            double distance = Math.Sqrt(distanceSquared);

            return distance;
        }
        
        private double GetDistance(GameObject GO)
        {

            Position baseLocation = this._bot.BaseLocation;
            Position nodeLocation = GO.Position;

            double deltaX = baseLocation.X - nodeLocation.X;
            double deltaY = baseLocation.Y - nodeLocation.Y;
            var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            double distance = Math.Sqrt(distanceSquared);

            return distance;
        }
        public EngineConfigDto GetEngineConfigDto()
        {
            return _engineConfigDto;
        }

        public void SetEngineConfigDto(EngineConfigDto engineConfigDto)
        {
            _engineConfigDto = engineConfigDto;
            if (!Haveeverything && engineConfigDto!=null)
            {
                cycle = _engineConfigDto.ProcessTick;
                foodconsumption = _engineConfigDto.UnitConsumptionRatio.Food;
                woodconsumption = 1;
                stoneconsumption = 1;
                goldconsumption = 1;
                heatconsumption = _engineConfigDto.UnitConsumptionRatio.Heat;
                campfirecost = _engineConfigDto.ResourceGenerationConfig.Campfire.ResourceConsumption[ResourceType.Wood][0];
                campfirereward = _engineConfigDto.ResourceGenerationConfig.Campfire.RewardRange[0];
                maxTicks = _engineConfigDto.MaxTicks;
                double heatcost = (campfirecost / campfirereward);
                UnitScoreCost = ((foodconsumption * _engineConfigDto.ResourceScoreMultiplier.Food)
                    + (woodconsumption * _engineConfigDto.ResourceScoreMultiplier.Wood)
                    + (heatconsumption * (heatcost) * _engineConfigDto.ResourceScoreMultiplier.Wood)
                    + (stoneconsumption * _engineConfigDto.ResourceScoreMultiplier.Stone)) / cycle;
                Haveeverything = true;
            }
        }
        
        public static void Log(string s)
        {
            if (console)
            Console.WriteLine(s+"\r\n");

            if (logging)
            {
                try
                {
                    System.IO.File.AppendAllText("log.txt", s + "\r\n");
                }
                catch
                {

                }
            }
        }
    }

    public class nodestate
    {
        //public int ExpectedReturn { get; set; }
        private ResourceNode node;

        public ResourceNode Node
        {
            get { return node; }
            set { node = value; if(maxamount==0) maxamount = Node?.Amount??0; }
        }
        public double ResourceValue { get { return (double)Reward / (double)TravelTime; } }
        public int Reward { get; set; }
        public List<NodeInstruction> PendingInstructions { get; set; }
        public int Units { get; set; }
        ActionType action = ActionType.Error;
        internal int maxamount = 0;
        public int Remaining { get; private set; }
        public int TravelTime { get; set; }
        public double ScoreVal { get { return  Reward * ScoreMultiplier()/(double)TravelTime; } }
        public double Distance { get; set; }
        public double WorkTime { get; set; }
        public nodestate(ResourceNode node, double Distance)
        {
            PendingInstructions = new List<NodeInstruction>();
            this.Node = node;
            this.Reward = node.Reward;
            this.Distance = Distance;
            this.WorkTime = node.WorkTime + 2;
            TravelTime =(int)( Distance + WorkTime);
        }
        int scoremulti = 0;
        double ScoreMultiplier()
        {
            if (scoremulti==0)
                switch (node.Type)
                {
                    case ResourceType.Food: scoremulti = BotService._engineConfigDto.ResourceScoreMultiplier.Food; break;
                    case ResourceType.Gold: scoremulti = BotService._engineConfigDto.ResourceScoreMultiplier.Gold; break;                    
                    case ResourceType.Stone: scoremulti = BotService._engineConfigDto.ResourceScoreMultiplier.Stone; break;
                    case ResourceType.Wood: scoremulti = BotService._engineConfigDto.ResourceScoreMultiplier.Wood; break;
                }
            return scoremulti;
        }
        internal ActionType GetAction()
        {
            if (action == ActionType.Error)
            {
                switch (Node.Type)
                {
                    case ResourceType.Food: action = ActionType.Farm; break;
                    case ResourceType.Gold: action = ActionType.Mine; break;
                    case ResourceType.Heat: action = ActionType.StartCampfire; break;
                    case ResourceType.Stone: action = ActionType.Mine; break;
                    case ResourceType.Wood: action = ActionType.Lumber; break;

                }
            }
            return action;
        }

        

        internal CommandAction AddAction(ref int units, int tick )
        {
            if (units <= 0)
            {
                units = 0;
                return null;
            }
            var tmpInstruction = new NodeInstruction { ExpectedReturn = tick + TravelTime, Type= node.Type };
            if (node.Amount > 0 || node.RegenerationRate != null)
            {
                int Available = node.Amount;
                if (node.RegenerationRate != null)
                {
                    Available += (node.RegenerationRate.Amount / node.RegenerationRate.Ticks * TravelTime);
                }
                if (Available <= 0)
                {
                    units = 0;
                    return null;
                }
                int availableUnits = node.MaxUnits;// - PendingInstructions.Where(m => m.ExpectedReturn > tick + TravelTime).Sum(m => m.Units);
               
                if (availableUnits <= 0)
                {
                    units = 0;
                    return null;
                }
                Available = Available / Reward;
                units = Math.Min(Math.Min(availableUnits, Available), units);
                if (units <= 0)
                {
                    units = 0;
                    return null;
                }
                tmpInstruction.ExpectedResources = Units * Reward;
                tmpInstruction.Units = units;
                Remaining -= tmpInstruction.ExpectedResources;
            }
            else
            {
                units = 0;
                return null;
            }
            PendingInstructions.Add(tmpInstruction);

            //BotService.Log($"\r\nResource {node.Id.ToString()} type {node.Type.ToString()} worktime {node.WorkTime} Amount {node.Amount} Remaining {Remaining} Units {tmpInstruction.Units} Current Units: {node.CurrentUnits} maxunits: {node.MaxUnits}");
            
            return new CommandAction()
            {
                Type = GetAction(),
                Units = tmpInstruction.Units,
                Id = Node.Id,
            };
        }

        internal List<NodeInstruction> checkReturnedUnits(int tick)
        {
            //var pendings = PendingInstructions.Where(m => m.ExpectedReturn <= tick).ToList();
            List<NodeInstruction> pendings = new List<NodeInstruction>();
            int units = 0;
            for (int i =0;i<PendingInstructions.Count;i++)
            {
                if (PendingInstructions[i].ExpectedReturn <= tick)
                {
                    pendings.Add(PendingInstructions[i]);
                    PendingInstructions.RemoveAt(i--);
                }
                else
                {
                    units += PendingInstructions[i].Units;
                }

            }
           
            this.Remaining = Node.Amount - units * Reward;

            return pendings;
        }

    }

    public class NodeInstruction
    {
        public ResourceType Type { get; set; }
        public int Units { get; set; }
        public int ExpectedReturn { get; set; }
        public int ExpectedResources { get; set; }

    }

    class nodesleft
    {
        public bool food { get; set; } = true;
        public bool wood { get; set; } = true;
        public bool stone { get; set; } = true;
        public bool gold { get; set; } = false;
        public static implicit operator bool(nodesleft n)=>n.food||n.wood||n.stone||n.gold;
    }
}