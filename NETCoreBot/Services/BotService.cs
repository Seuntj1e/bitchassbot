using Domain.Models;
using BitchAssBot.Enums;
using BitchAssBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BitchAssBot.Services
{
    public class BotService
    {
        static bool console = false;
        static bool logging = false;
        static bool everytick = true;
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

        decimal foodconsumption = 0;
        decimal woodconsumption = 0;
        decimal stoneconsumption = 0.005m;
        decimal heatconsumption = 0;
        decimal campfirecost = 0;
        decimal campfirereward = 0;
        decimal fooddays = 0;
        decimal wooddays = 0;
        decimal stonedays = 0;
        decimal foodrewards = 0;
        decimal woodrewards = 0;
        decimal stonerewards = 0;
        decimal HeatCap = 6000;
        decimal UnitScoreCost = 0;
        long totalticks = 0;
        int maxTicks = 2500;
        decimal campfiresthiscycle = 0;
        bool Haveeverything = false;
        bool expand = false;
        int lastindex = 0;
        ResourceType[] resourcerotation = new ResourceType[] { ResourceType.Wood, ResourceType.Food,   ResourceType.Food, ResourceType.Wood, ResourceType.Food,  ResourceType.Heat, ResourceType.Food, ResourceType.Stone };
        int MaxPOp = 0;
        int popBoomCalcStart = 2000;
        int popBoom = 9999999;
        bool singleplayer = false;
        bool booming = true;
        int HeatRemaining = 99999999;
        decimal woodconsuming = 0;
        decimal woodamount = 0;

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
        int foodremaining = 0;
        int foodamount = 0;
        public PlayerCommand GetPlayerCommand()
        {
            System.Diagnostics.Stopwatch wtch = new System.Diagnostics.Stopwatch();
            wtch.Start();
            started = true;
            PlayerCommand playerCommand = new PlayerCommand();
            playerCommand.PlayerId = this._bot.Id;

            try
            {
                singleplayer = this._gameState.Bots.Count == 1;
                var dto = this._gameState.Bots.FirstOrDefault(go => go.Id == _bot.Id);
                if (console && dto.Tick % cycle == 1 || everytick)
                    Console.SetCursorPosition(0, 0);
                var _previousstate = this._previousstate;
                this._previousstate = dto;
                if (_previousstate == null)
                    _previousstate = dto;
                if (!issueing && dto.Tick > _previousstate.Tick || dto.Tick == 0)
                {
                    issueing = true;
                    var CurrentTier = _engineConfigDto.PopulationTiers.FirstOrDefault(m => m.Level == dto.CurrentTierLevel);
                    var tmpNodes = nodes.Values.OrderByDescending(m => m.ResourceValue);
                    int units = dto.AvailableUnits;
                    var ownNodes = this._gameState.World.Map.Nodes.OrderBy(m => GetDistance(m));
                    foreach (var x in ownNodes)
                    {
                        if (!nodes.ContainsKey(x.Id))
                        {
                            nodes.Add(x.Id, new nodestate(x, GetDistance(x)));
                        }
                        nodes[x.Id].Node = x;
                        nodes[x.Id].checkReturnedUnits(dto.Tick);
                    }
                    List<NodeInstruction> PendingUnits = nodes.Values.SelectMany(m => m.PendingInstructions).ToList();


                    if (dto.Population > MaxPOp)
                        MaxPOp = dto.Population;

                    var foods2 = nodes.Values.Where(m => m.Node.Type == ResourceType.Food && m.ScoreVal >= UnitScoreCost).ToList();
                    foodremaining = foods2.Sum(m => m.Remaining);
                    foodamount = foods2.Sum(m => m.Node.Amount);

                    var Woods = tmpNodes.Where(m => m.Node.Type == ResourceType.Wood && m.Node.Amount > 00 /*&& m.ScoreVal >= UnitScoreCost*/).ToList(); ;
                    var Stones = tmpNodes.Where(m => m.Node.Type == ResourceType.Stone && m.Node.Amount > 00 /*&& m.ScoreVal >= UnitScoreCost*/).ToList(); ;
                    woodamount = Woods.Sum(m => m.Node.Amount) + dto.Wood;
                    decimal stoneamount = Stones.Sum(m => m.Remaining);
                    if (_gameState.World.Map.Nodes.Count > 250 && booming)
                        booming = foodamount > 150000 && woodamount > 1000;


                    if (dto.Tick % cycle == 1 || everytick)
                        Log($"\r\n{DateTime.Now: HH:mm:ss ffff} Population: {dto.Population} Units: {dto.AvailableUnits} Traveling FCUKED NODES { this._gameState.World.Map.Nodes.Count} TICK {dto.Tick} HEATCAP {HeatCap}".PadRight(Console.WindowWidth - 2) +
                            $"\r\nFood: {dto.Food} Wood: {dto.Wood} Stone: {dto.Stone} gold: ???? Heat: {dto.Heat} Heat consume {HeatRemaining}".PadRight(Console.WindowWidth - 2) +
                            $"\r\nFarm: FCUKED Lumb: FCUKED Mines: FCUKED gold: ???? scout: FCUKED".PadRight(Console.WindowWidth - 2) +
                            $"\r\nfoodremaining {foodremaining} foodamount {foodamount} woodamount {woodamount} {(booming ? "BOOMING" : "NOT     ")}".PadRight(Console.WindowWidth - 2)

                            );




                    if ((cycle > 0 && dto.Tick % cycle == 1))
                    {
                        campfiresthiscycle = 0;
                    }



                    if (units == 0)
                    {
                        issueing = false;
                        return playerCommand;
                    }
                    //playerCommand.PlayerId = this._bot.Id;

                    if (this._gameState.World != null)
                    {
                        if (ownNodes.Count() > 0 && GetDistance(ownNodes.FirstOrDefault()) < 5)
                        {

                            if (Haveeverything && dto.Population >= 50 /*|| dto.CurrentTierLevel>0*/)
                            {

                                {
                                    var foods = tmpNodes.Where(m => m.Node.Type == ResourceType.Food && m.Node.Amount > 1000 /*&& m.ScoreVal >= UnitScoreCost*/).ToList();


                                    foodrewards = foods.Count() > 0 ? foods.Sum(m => m.Reward) : foodrewards;
                                    fooddays = foods.Count() > 0 ? foods.Sum(m => m.TravelTime) : fooddays;
                                    decimal m = (foodrewards / fooddays * (decimal)cycle);
                                    decimal ff = ((decimal)foodconsumption * (decimal)_engineConfigDto.ResourceImportance.Food) / m;



                                    woodrewards = Woods.Count() > 0 ? Woods.Sum(m => m.Reward) : woodrewards;
                                    wooddays = Woods.Count() > 0 ? Woods.Sum(m => m.TravelTime) : wooddays;
                                    decimal n = woodrewards / wooddays * (decimal)cycle;
                                    decimal phaseloading = (dto.Population < _gameState.PopulationTiers[3].MaxPopulation ? //early game, focus on food
                                        0.1m :
                                       dto.CurrentTierLevel == 6 ?
                                       (campfirecost / campfirereward) * 2
                                       :
                                        (campfirecost / campfirereward)

                                        );
                                    decimal woodcon2 = woodconsumption + phaseloading;
                                    decimal wf = ((decimal)woodcon2) / n;

                                    decimal sf = 0;
                                    if (dto.Stone <= _gameState.PopulationTiers.Max(m => m.TierResourceConstraints.Stone) && Stones.Count > 0)
                                    {
                                        stonerewards = Stones.Count() > 0 ? Stones.Sum(m => m.Reward) : stonerewards;
                                        /*if (stonedays == 0)
                                            stonedays = _engineConfigDto.ResourceGenerationConfig.Stone.RewardRange.Min();*/
                                        stonedays = Stones.Count() > 0 ? Stones.Sum(m => m.TravelTime) : stonedays;
                                        /*if (stonedays == 0)
                                            stonedays = _engineConfigDto.ResourceGenerationConfig.Stone.WorkTimeRange.Max() * 2;*/
                                        decimal o = 0;
                                        if (stonedays > 0 && stonerewards > 0)

                                        {
                                            o = stonerewards / stonedays * (decimal)cycle;
                                            sf = (stoneconsumption) / o;
                                        }
                                    }



                                    if (foods.Count() <= 2 || Woods.Count() <= 2 || Stones.Count() <= 2)
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
                                    decimal Heatrewards = campfirereward;
                                    decimal Heatdays = 1;
                                    decimal p = Heatrewards / Heatdays * (decimal)cycle;
                                    decimal hf = ((decimal)heatconsumption + 0.05m) / p;

                                    decimal TotalWeight = ff + wf + sf + hf;

                                    int foodunits = 0;
                                    int woodunits = 0;
                                    int stoneunits = 0;
                                    int heatunits = 0;

                                    if (units >= 4)
                                    {
                                        //decimal fieldunits = PendingUnits.Sum(m => m.Units);
                                        //decimal pendingfood = PendingUnits.Where(m => m.Type == ResourceType.Food).Sum(m => m.Units);
                                        //decimal pendingWood = PendingUnits.Where(m => m.Type == ResourceType.Wood).Sum(m => m.Units);
                                        //decimal pendingStone = PendingUnits.Where(m => m.Type == ResourceType.Stone).Sum(m => m.Units);
                                        //decimal pendingHeat = campfires;
                                        //decimal TotalUnits = fieldunits + campfires + units;


                                        //decimal Totalheatunits = (int)Math.Floor((double)(TotalUnits * (hf / TotalWeight)));
                                        //heatunits =(int)( Totalheatunits - pendingHeat);
                                        heatunits = (int)Math.Ceiling((double)(units * (hf / TotalWeight)));
                                        int MinHeats = (int)Math.Ceiling((((dto.Population * (booming ? 3 : 1.1m) / heatconsumption) / 10m) / campfirereward));
                                        /*if (dto.Tick >= popBoom)
                                        {
                                            //heatunits = (int)Math.Floor((double)(units * (hf / TotalWeight)));
                                            MinHeats = (int)Math.Ceiling((((dto.Population * 1.05m / heatconsumption) / 10m) / (campfirereward * 1.1m)));

                                        }*/
                                        if (heatunits < MinHeats)
                                        {
                                            //Log("Failed heat check");
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

                                            decimal ticketsleft = maxTicks - dto.Tick;
                                            if (ticketsleft > 400 && singleplayer)
                                            {
                                                ticketsleft += 500;
                                            }
                                            decimal availablefood = foods2.Sum(m => m.Node.Amount);

                                            decimal FoodPerTicket = ownNodes.Where(m => m.Type == ResourceType.Food && m.Amount < nodes[m.Id].maxamount).Sum(m => m.RegenerationRate.Amount / m.RegenerationRate.Ticks);
                                            decimal Totalfood = dto.Food + availablefood + (FoodPerTicket * ticketsleft);
                                            decimal SustainPopFood = Totalfood / (ticketsleft / 10); ;

                                            decimal availablewood = Woods.Sum(m => m.Node.Amount);

                                            decimal TotalWood = dto.Wood + availablewood;
                                            decimal SustainPopWood = TotalWood / (ticketsleft / 10); ;

                                            decimal SustainPop = Math.Min(SustainPopFood, SustainPopWood);
                                            /*if (dto.Tick%10==1)
                                                Log($"ticksleft: {ticketsleft} ReadyFood {availablefood} RegenFood {FoodPerTicket} TotalFood {Totalfood} SustainPop {SustainPop}");*/
                                            HeatCap = SustainPop * 2m;
                                        }



                                        //if (dto.Tick>popBoomCalcStart && dto.Tick%10==0 && popBoom>dto.Tick)
                                        //{
                                        //    decimal totalpop = dto.Population;
                                        //    decimal totalfood = 0;
                                        //    int i = 0;
                                        //for (i = dto.Tick; i <= 2500; i += 10)
                                        //{
                                        //    totalfood += totalpop;
                                        //    totalpop *= 1.05m;
                                        //    if (totalfood > dto.Food)
                                        //    {

                                        //        break;

                                        //    }
                                        //}
                                        //if (i >= 2500 - 30)
                                        //{
                                        //    popBoom = dto.Tick;
                                        //}
                                        //}
                                        //if (popBoom < dto.Tick)
                                        //{
                                        //    //popBoom = dto.Tick;
                                        //    //Log("----------------------------------------------------------------------------------------------------------------------------------");
                                        //    HeatCap *= 1.05m;
                                        //    if (popBoom == dto.Tick)
                                        //    {
                                        //        foodunits = (int)Math.Floor((double)(units * (ff / TotalWeight)));
                                        //        heatunits += foodunits;
                                        //        foodunits = 0;
                                        //    }
                                        //    ff = 0;
                                        //    if (dto.Stone >= 1043)
                                        //        sf = 0;
                                        //    TotalWeight = hf + wf + sf;
                                        //}
                                        //TotalUnits -= heatunits;
                                        if (booming)
                                        {
                                            HeatCap = _engineConfigDto.PopulationTiers.OrderByDescending(m => m.Level).FirstOrDefault(m => m.MaxPopulation > 0).MaxPopulation * 2;
                                            HeatCap = Math.Min(HeatCap, dto.Population * 2.5m);
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
                                            if (!singleplayer)
                                                HeatCap *= 1m + (decimal)CurrentTier.PopulationChangeFactorRange.Max();

                                            //units += heatunits;
                                            //TotalUnits += heatunits;

                                            /*if (campfiresthiscycle > populationCap / campfirereward)
                                            {
                                                units += heatunits;
                                                heatunits = 0;
                                            }
                                            else if (campfiresthiscycle + heatunits >= populationCap / campfirereward)
                                            {
                                                int newheatunits = (int)((populationCap / campfirereward) - (decimal)campfiresthiscycle);
                                                units += heatunits - newheatunits;
                                                heatunits = newheatunits;
                                            }*/
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
                                            //units += oldheat - heatunits;
                                            heatunits += oldheat - heatunits;
                                        }
                                        if (heatunits > units)
                                            heatunits = units;
                                        units -= heatunits;


                                        foodunits = (int)Math.Floor((double)(units * (ff / TotalWeight)));
                                        if (woodamount > 0)
                                            woodunits = (int)Math.Floor((double)(units * (wf / TotalWeight)));
                                        if (stoneamount > 0)
                                            stoneunits = (int)Math.Floor((double)(units * (sf / TotalWeight)));


                                        units -= (foodunits + woodunits + stoneunits);
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
                                        }



                                    }
                                    else
                                    {
                                        switch (resourcerotation[lastindex++])
                                        {
                                            case ResourceType.Food: foodunits += units; units = 0; break;
                                            case ResourceType.Wood: woodunits += units; units = 0; break;
                                            case ResourceType.Heat: heatunits += units; units = 0; break;
                                            case ResourceType.Stone: stoneunits += units; units = 0; break;
                                        }
                                        if (lastindex >= resourcerotation.Length)
                                            lastindex = 0;
                                    }

                                    if (dto.Tick % cycle == 1)
                                    {
                                        HeatRemaining = CalcHeatLeft(dto.Tick, dto.Population);
                                    }

                                    if (booming)
                                    {
                                        var left = new nodesleft();
                                        int index = 0;
                                        foods = tmpNodes.Where(m => m.Node.Type == ResourceType.Food && m.Node.Amount > 0 /*&& m.ScoreVal >= UnitScoreCost*/).ToList();
                                        bool first = true;

                                        //for (int i =0;  foodunits+woodunits+stoneunits > 0 && i<2 && left;i++)
                                        {
                                            if (CurrentTier.tierMaxResources.Stone * 0.80m <= (decimal)dto.Stone + dto.PendingActions.Where(m => m.ActionType == ActionType.Lumber).Sum(m => m.NumberOfUnits * nodes[m.TargetNodeId].Reward) + dto.Actions.Where(m => m.ActionType == ActionType.Lumber).Sum(m => m.NumberOfUnits * nodes[m.TargetNodeId].Reward) && booming && first)
                                            {
                                                foodunits += stoneunits;
                                                stoneunits = 0;
                                            }
                                            while (stoneunits > 0 && index < Stones.Count)
                                            {
                                                var closestnode = Stones[index];// nodes.Values.FirstOrDefault(m => m.Node.Type == ResourceType.Stone && m.Remaining > 0);
                                                                                //var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                                if (closestnode == null)
                                                {
                                                    playerCommand.Actions.AddRange(Scout(ref stoneunits));
                                                    //units = foodunits;
                                                    //stoneunits = 0;
                                                    //closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && m.Amount > 100);
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
                                                foodunits += stoneunits;
                                            left.stone = index < Stones.Count;
                                            index = 0;
                                            if (CurrentTier.tierMaxResources.Food * 0.80m <= (decimal)dto.Food + dto.PendingActions.Where(m => m.ActionType == ActionType.Farm).Sum(m => m.NumberOfUnits * nodes[m.TargetNodeId].Reward) + dto.Actions.Where(m => m.ActionType == ActionType.Farm).Sum(m => m.NumberOfUnits * nodes[m.TargetNodeId].Reward) && booming && first)
                                            {
                                                woodunits += foodunits;
                                                foodunits = 0;
                                            }
                                            while (foodunits > 0 && index < foods.Count)
                                            {
                                                var closestnode = foods[index];// nodes.Values.FirstOrDefault(m => m.Node.Type == ResourceType.Food && m.Remaining > 0);
                                                                               //var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                                if (closestnode == null)
                                                {
                                                    playerCommand.Actions.AddRange(Scout(ref foodunits));
                                                    //units = foodunits;
                                                    //foodunits = 0;
                                                    Log("No farms left bitch");
                                                    //closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && m.Amount > 100);
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
                                            left.food = index < foods.Count;
                                            index = 0;
                                            var requiredheat = (dto.Population * (10m / (decimal)_engineConfigDto.ResourceImportance.Heat * (decimal)CurrentTier.PopulationChangeFactorRange.Max())) + dto.Population * 2;
                                            if (dto.Heat > requiredheat)
                                                woodunits += heatunits;
                                            if (CurrentTier.tierMaxResources.Wood * 0.8m <= (decimal)dto.Wood && booming && first)
                                            {
                                                heatunits += woodunits;
                                                int oldheat = heatunits;
                                                if (dto.CurrentTierLevel < 6)
                                                {
                                                    if (dto.Wood - heatunits * campfirecost <= _engineConfigDto.PopulationTiers[dto.CurrentTierLevel + 1].TierResourceConstraints.Wood * 2)
                                                    {
                                                        var tmp = (dto.Wood - _engineConfigDto.PopulationTiers[dto.CurrentTierLevel + 1].TierResourceConstraints.Wood * 2) * campfirecost;
                                                        if (tmp < 0)
                                                            tmp = 0;
                                                        heatunits = (int)tmp;
                                                    }
                                                }
                                                heatunits = (int)Math.Min((decimal)heatunits, (decimal)((HeatRemaining - dto.Heat) / campfirereward));
                                                heatunits = (int)Math.Min((decimal)heatunits, Math.Floor(dto.Wood / campfirecost));

                                                //woodunits = 0;
                                                woodunits = oldheat - heatunits;
                                            }
                                            while (woodunits > 0 && index < Woods.Count)
                                            {
                                                var closestnode = Woods[index];// nodes.Values.FirstOrDefault(m => m.Node.Type == ResourceType.Wood && m.Remaining > 0);
                                                                               //var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                                if (closestnode == null)
                                                {
                                                    playerCommand.Actions.AddRange(Scout(ref woodunits));
                                                    //units = foodunits;
                                                    //woodunits = 0;
                                                    //closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && m.Amount > 100);
                                                }
                                                if (woodunits > 0 && closestnode != null)
                                                {
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                                    int uisedunits = woodunits;
                                                    //get feedback from minenode as to hoy many are going and re-assign the rest

                                                    var tmpAction = MineNode(closestnode.Node, ref uisedunits);
                                                    if (tmpAction != null)
                                                        playerCommand.Actions.Add(tmpAction);
                                                    woodunits -= uisedunits;
                                                }
                                                index++;
                                            }
                                            left.wood = index < Woods.Count;
                                            if (woodunits > 0)
                                                heatunits += woodunits;
                                            index = 0;

                                            //if (first )//&& foodunits + woodunits + stoneunits > 0)
                                            //{
                                            //    first = false;


                                            //    heatunits += Math.Min(0, units);
                                            //    heatunits += Math.Min(0, foodunits);
                                            //    heatunits += Math.Min(0, woodunits);
                                            //    heatunits += Math.Min(0, stoneunits);
                                            //    int oldheat = heatunits;
                                            //    var tmp2 = dto.Heat + campfires * campfirereward;//total heat
                                            //    var tmp3 = (dto.Population * (10m / (decimal)_engineConfigDto.ResourceImportance.Heat * (decimal)CurrentTier.PopulationChangeFactorRange.Max())) + dto.Population * 2;//required heat
                                            //    var tmp4 = _engineConfigDto.PopulationTiers[dto.CurrentTierLevel + 1].TierResourceConstraints.Wood * 2.5;//min wood for next level

                                            //    if (dto.CurrentTierLevel < 6)
                                            //    {
                                            //        if (tmp2 < tmp3
                                            //            &&
                                            //            dto.Wood < tmp4
                                            //            )
                                            //        {
                                            //            decimal tmp = (_engineConfigDto.PopulationTiers[dto.CurrentTierLevel + 1].TierResourceConstraints.Wood * 2 - dto.Wood) / campfirecost;
                                            //            if (tmp > 0 && tmp < heatunits)
                                            //                heatunits = (int)tmp;
                                            //        }
                                            //        else if (dto.Wood < tmp4)
                                            //        {
                                            //            heatunits = 0;
                                            //            Log("NEED WOOD!");
                                            //        }
                                            //        Log($"Total Heat: {tmp2} Heat Required {tmp3} Tier wood {tmp4} heatunits {heatunits}");
                                            //    }
                                            //    heatunits = (int)Math.Min((decimal)heatunits, (decimal)((HeatRemaining - dto.Heat) / campfirereward));
                                            //    heatunits = (int)Math.Min((decimal)heatunits, Math.Floor((dto.Wood-(decimal)tmp4) / campfirecost));

                                            //    foodunits += oldheat - heatunits;
                                            //    Log("Recycling Units: " + foodunits);
                                            //    //determine heat unit requirements and recycle units for food as nescecary
                                            //}

                                        }
                                    }
                                    else
                                    {
                                        Console.WriteLine("NOT BOOMING");
                                        var Resources = nodes.Values.Where(m => m.Remaining > 0).OrderByDescending(m => m.ScoreVal).ToList();
                                        units = dto.AvailableUnits;
                                        if (dto.Wood > 100 && dto.Heat < HeatRemaining)
                                        {
                                            //test heat vs current allocated heat units
                                            //reallocate units to creating heat here

                                        }
                                        
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

                                    if (heatunits > 0)
                                    {
                                        /*var NextTier = new */
                                        heatunits += Math.Min(0, units);
                                        heatunits += Math.Min(0, foodunits);
                                        heatunits += Math.Min(0, woodunits);
                                        heatunits += Math.Min(0, stoneunits);
                                        if (dto.CurrentTierLevel < 6)
                                        {
                                            var tmp2 = dto.Heat + campfires * campfirereward;//total heat
                                            var tmp3 = (dto.Population * (10m / (decimal)_engineConfigDto.ResourceImportance.Heat * (decimal)CurrentTier.PopulationChangeFactorRange.Max())) + dto.Population * 2;//required heat
                                            var tmp4 = _engineConfigDto.PopulationTiers[dto.CurrentTierLevel + 1].TierResourceConstraints.Wood * 2.5;//min heat for next level
                                            if (tmp2 < tmp3
                                                &&
                                                dto.Wood < tmp4
                                                )
                                            {
                                                decimal tmp = (_engineConfigDto.PopulationTiers[dto.CurrentTierLevel + 1].TierResourceConstraints.Wood * 2 - dto.Wood) / campfirecost;
                                                if (tmp > 0 && tmp < heatunits)
                                                    heatunits = (int)tmp;
                                            }
                                            else if (dto.Wood < tmp4)
                                            {
                                                heatunits = 0;
                                                Log("NEED HEAT!");
                                            }
                                            Log($"Total Heat: {tmp2} Heat Required {tmp3} Tier wood {tmp4} heatunits {heatunits}");
                                        }

                                        heatunits = (int)Math.Min((decimal)heatunits, (decimal)((HeatRemaining - dto.Heat) / campfirereward));
                                        heatunits = (int)Math.Min((decimal)heatunits, Math.Floor(dto.Wood / campfirecost));

                                        campfires = heatunits;
                                        if (heatunits > 0)
                                        {
                                            playerCommand.Actions.Add(new CommandAction()
                                            {
                                                Type = ActionType.StartCampfire,
                                                Units = heatunits,
                                            });
                                        }
                                    }
                                    Log($"\r\nff: {ff:0.0000}\twf: {wf:0.0000}\tsf: {sf:0.0000}\thf: {hf:0.0000}\r\n" +
                                      $"fu: {playerCommand.Actions.Where(m => m.Type == ActionType.Farm).Sum(m => m.Units):00000}\twu: {playerCommand.Actions.Where(m => m.Type == ActionType.Lumber).Sum(m => m.Units):00000}\tsu: {playerCommand.Actions.Where(m => m.Type == ActionType.Mine).Sum(m => m.Units):00000}\thu: {playerCommand.Actions.Where(m => m.Type == ActionType.StartCampfire).Sum(m => m.Units):00000}\r\n");
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
                                tmpNodes = nodes.Values.OrderBy(m => m.TravelTime);

                                if (foodunits > 0)
                                {
                                    var closestnode = tmpNodes.FirstOrDefault(m => m.Node.Type == ResourceType.Food && m.Remaining > foodunits * m.Reward);
                                    var tmpAction = MineNode(closestnode.Node, ref foodunits);
                                    if (tmpAction != null)
                                        playerCommand.Actions.Add(tmpAction);
                                    units -= foodunits;

                                }
                                if (woodunits > 0)
                                {
                                    var closestnode = tmpNodes.FirstOrDefault(m => m.Node.Type == ResourceType.Wood && m.Remaining > woodunits * m.Reward);
                                    var tmpAction = MineNode(closestnode.Node, ref woodunits);
                                    if (tmpAction != null)
                                        playerCommand.Actions.Add(tmpAction);
                                    units -= woodunits;
                                }
                                if (stoneunits > 0)
                                {
                                    var closestnode = tmpNodes.FirstOrDefault(m => m.Node.Type == ResourceType.Stone && m.Remaining > stoneunits);
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
                            var scouting = dto.PendingActions.FirstOrDefault(m => m.ActionType == ActionType.Scout);
                            if (ownNodes.Count() == 0 && scouting == null)
                                towers.Clear();
                            if (UnscoutedTowers() && (ownNodes.Count() == 0 || (ownNodes.Count() > 0 && GetDistance(ownNodes.FirstOrDefault()) > 5)))
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
            return playerCommand;
        }


        int CalcHeatLeft(int tick, decimal currentPop)
        {
            decimal totalheat = 0;
            decimal totalwood = 0;
            decimal onlywood = 0;
            decimal totalpop = currentPop;
            
            for (int i = tick; i <= _engineConfigDto.MaxTicks-cycle+1; i += 10)
            {
                var Tier = _engineConfigDto.PopulationTiers.FirstOrDefault(m => m.MaxPopulation > totalpop);
                totalpop *= (1m + (decimal)Tier.PopulationChangeFactorRange.Max());
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

            Log($"Measurements: Cycle: {cycle}\r\n" +
                $"foodconsumption: {foodconsumption}\r\n" +
                $"woodconsumption: {woodconsumption}\r\n" +
                $"campfirecost: {campfirecost}\r\n" +
                $"campfirereward: {campfirereward}\r\n" +
                $"stoneconsumption: {stoneconsumption}\r\n" +
                $"heatconsumption: {heatconsumption}\r\n" +
                $"Max Population: {MaxPOp}\r\n" +
                $"Avg Processing Time: {TimeSpan.FromTicks(totalticks / _bot.Tick).TotalMilliseconds}\r\n" );
                //$"Furthest node cost: {nodes.Values.OrderBy(m=>m.ScoreVal).First().ScoreVal} vs {UnitScoreCost} unit cost");
            
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
            for (int i = this._gameState.World.Map.ScoutTowers.Count()-1; i >=0; i--)
            {
                if (!towers.ContainsKey( this._gameState.World.Map.ScoutTowers[i].Id))
                {
                    return true;
                }
            }
            return false;
        }

        internal List<CommandAction> Scout(ref int units)
        {
            List<CommandAction> actions = new List<CommandAction>();
            try
            {
                
                var tmpTowers = this._gameState.World.Map.ScoutTowers.OrderBy(m => GetDistance(m)).ToList();
                for (int i = 0; i < tmpTowers.Count() && units > 0; i++)
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
                    if (i + 1 == tmpTowers.Count())
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
            //Resources = Math.Max(Math.Min(Math.Min(x.MaxUnits, Resources), tmpState.Remaining), 0);
            //Resources = tmpState.Units;
            if (x.Type== ResourceType.Wood)
            {

            }

            
            //if (Resources > 0)
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
            _bot = _gameState.Bots.FirstOrDefault(go => go.Id == _bot.Id);
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
        
        private decimal GetDistance(GameObject GO)
        {

            Position baseLocation = this._bot.BaseLocation;
            Position nodeLocation = GO.Position;

            double deltaX = baseLocation.X - nodeLocation.X;
            double deltaY = baseLocation.Y - nodeLocation.Y;
            var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            double distance = Math.Sqrt(distanceSquared);

            return (decimal)distance;
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
                foodconsumption = (decimal)_engineConfigDto.UnitConsumptionRatio.Food;
                woodconsumption = (decimal)_engineConfigDto.UnitConsumptionRatio.Wood;
                stoneconsumption = (decimal)_engineConfigDto.UnitConsumptionRatio.Stone;
                heatconsumption = (decimal)_engineConfigDto.UnitConsumptionRatio.Heat;
                campfirecost = _engineConfigDto.ResourceGenerationConfig.Campfire.ResourceConsumption[ResourceType.Wood].First();
                campfirereward = (decimal)_engineConfigDto.ResourceGenerationConfig.Campfire.RewardRange[0];
                maxTicks = _engineConfigDto.MaxTicks;
                decimal heatcost = (campfirecost / campfirereward);
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
        public decimal ResourceValue { get { return (decimal)Reward / (decimal)TravelTime; } }
        public int Reward { get; set; }
        public List<NodeInstruction> PendingInstructions { get; set; }
        public int Units { get; set; }
        ActionType action = ActionType.Error;
        internal int maxamount = 0;
        public int Remaining { get; private set; }
        public int TravelTime { get; set; }
        public decimal ScoreVal { get { return  Reward * ScoreMultiplier()/(decimal)TravelTime; } }
        public decimal Distance { get; set; }
        public decimal WorkTime { get; set; }
        public nodestate(ResourceNode node, decimal Distance)
        {
            PendingInstructions = new List<NodeInstruction>();
            this.Node = node;
            this.Reward = node.Reward;
            this.Distance = Distance;
            this.WorkTime = node.WorkTime + 2;
            TravelTime =(int)( Distance + WorkTime);
        }
        int scoremulti = 0;
        decimal ScoreMultiplier()
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
            if (Remaining > 0 || node.RegenerationRate != null)
            {
                int Available = Remaining;
                if (node.RegenerationRate != null)
                {
                    Available += (int)((decimal)node.RegenerationRate.Amount / (decimal)node.RegenerationRate.Ticks * (decimal)TravelTime);
                }
                if (Available <= 0)
                {
                    units = 0;
                    return null;
                }
                int availableUnits = node.MaxUnits;// - PendingInstructions.Where(m => m.ExpectedReturn > tick + TravelTime).Sum(m => m.Units);
                if (availableUnits > node.MaxUnits)
                    availableUnits = node.MaxUnits;
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

            BotService.Log($"\r\nResource {node.Id.ToString()} type {node.Type.ToString()} worktime {node.WorkTime} Amount {node.Amount} Remaining {Remaining} Units {tmpInstruction.Units} Current Units: {node.CurrentUnits} maxunits: {node.MaxUnits}");
            
            return new CommandAction()
            {
                Type = GetAction(),
                Units = tmpInstruction.Units,
                Id = Node.Id,
            };
        }

        internal List<NodeInstruction> checkReturnedUnits(int tick)
        {
            var pendings = PendingInstructions.Where(m => m.ExpectedReturn <= tick || (m.ExpectedReturn % 2500 <= tick && tick < 2000)).ToList();
            if (pendings.Count() > 0)
            {
                if (this.Reward == 0)
                {
                    this.Reward = (maxamount - Node.Amount) / pendings.Sum(m => m.Units);
                }
                foreach (var x in pendings)                
                {
                    if (x.ExpectedResources == 0 && Reward > 0)
                        x.ExpectedResources = x.Units * Reward;
                    //Log($"{x.Units} Units returned from {node.Id} with {x.ExpectedResources} {node.Type.ToString()}. Expected back on tick {x.ExpectedReturn}");
                    PendingInstructions.Remove(x);

                }
                
            }
            this.Remaining = Node.Amount - (PendingInstructions.Sum(m => m.Units) * Reward);

            return pendings.ToList();
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