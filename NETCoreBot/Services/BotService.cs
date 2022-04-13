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
        private BotDto _bot;
        private CommandAction _playerAction;
        private PlayerCommand _playerCommand;
        private GameState _gameState;

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
        int maxTicks = 2500;
        decimal campfiresthiscycle = 0;
        bool Haveeverything = false;
        bool expand = false;
        int lastindex = 0;
        ResourceType[] resourcerotation = new ResourceType[] { ResourceType.Food, ResourceType.Wood, ResourceType.Heat, ResourceType.Stone };
        int MaxPOp = 0;
        int popBoomCalcStart = 2000;
        int popBoom = 9999999;
        bool singleplayer = false;
        bool booming = true;
        public BotService()
        {
            _playerAction = new CommandAction();
            _playerCommand = new PlayerCommand();
            _gameState = new GameState();
            _bot = new BotDto();
           
            
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

            PlayerCommand playerCommand = new PlayerCommand();
            playerCommand.PlayerId = this._bot.Id;
            singleplayer = this._gameState.Bots.Count == 1;
            var dto = this._gameState.Bots.FirstOrDefault(go => go.Id == _bot.Id);
            var _previousstate = this._previousstate;
            this._previousstate = dto;
            if (_previousstate == null)
                _previousstate = dto;
            if (!issueing && dto.Tick>_previousstate.Tick || dto.Tick==0 )
            {
                issueing = true;
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

                var foods2 = nodes.Values.Where(m => m.Node.Type == ResourceType.Food).ToList();
                //foodremaining= foods.Sum(m => m.Remaining) ;
                foodamount = foods2.Sum(m => m.Node.Amount);
                if (_gameState.World.Map.Nodes.Count>250 && booming)
                    booming = foodamount > 50000;


                if (dto.Tick % 10 == 1)
                    Console.WriteLine($"\r\n{DateTime.Now: HH:mm:ss ffff} Population: {dto.Population} Units: {dto.AvailableUnits} Traveling {dto.TravellingUnits} NODES { this._gameState.World.Map.Nodes.Count} TICK {dto.Tick} HEATCAP {HeatCap}\r\n" +
                        $"Food: {dto.Food} Wood: {dto.Wood} Stone: {dto.Stone} gold: ???? Heat: {dto.Heat}\r\n" +
                        $"Farm: {dto.FarmingUnits} Lumb: {dto.LumberingUnits} Mines: {dto.MiningUnits} gold: ???? scout: {dto.ScoutingUnits}\r\n" +
                        $"foodremaining {foodremaining} foodamount {foodamount} {(booming ? "BOOMING" : "")}"

                        );


                if (_bot.Tick - _previousstate.Tick>1)
                {
                    //Console.WriteLine($"MISSED TICKS!!! {_bot.Tick - _previousstate.Tick}");
                }

               //
                
               
                if (cycle==0 || (cycle>0 && dto.Tick%cycle==1) || campfires>0)
                {
                    
                    if (foodconsumption==0 && dto.Food<_previousstate.Food)
                    {
                        //tick start-tick now
                        if (cycle == 0)
                        {
                            cycle = dto.Tick - tickstart;
                            Console.WriteLine($"Cycle {cycle}");
                        }
                        foodconsumption = ((decimal)(_previousstate.Food - dto.Food)) / (decimal)dto.Population;
                        Console.WriteLine($"foodconsumption {foodconsumption}");
                    }
                    
                    if (woodconsumption == 0 && dto.Wood < _previousstate.Wood)
                    {
                        //tick start-tick now
                        //cycle = dto.Tick - tickstart;
                        woodconsumption = ((decimal)(_previousstate.Wood - dto.Wood)) / (decimal)dto.Population;
                        Console.WriteLine($"woodconsumption {woodconsumption}");
                    }
                    if (campfirecost== 0 && dto.Wood < _previousstate.Wood && campfires>0)
                    {
                        decimal woodused = ((decimal)(_previousstate.Wood - dto.Wood));
                        if (dto.Tick % cycle == 1)
                            woodused -= dto.Population * woodconsumption;
                        campfirecost = woodused / (decimal)campfires;
                        
                        Console.WriteLine($"campfirecost {campfirecost}");
                    }
                    if (campfirereward==0 && dto.Heat > _previousstate.Heat && campfirecost>0)
                    {
                        campfirereward = (dto.Heat - _previousstate.Heat) * Previouscampfires;
                        Console.WriteLine($"campfirereward {campfirereward}");
                    }

                    if (stoneconsumption == 0 && dto.Stone < _previousstate.Stone && dto.Population >= 10)
                    {
                        //tick start-tick now
                        //cycle = dto.Tick - tickstart;
                        stoneconsumption = ((decimal)(_previousstate.Stone - dto.Stone)) / (decimal)dto.Population;
                        Console.WriteLine($"stoneconsumption {stoneconsumption}");
                    }

                    if (heatconsumption == 0 && dto.Heat < _previousstate.Heat && dto.Population >= 10)
                    {
                        //tick start-tick now
                        //cycle = dto.Tick - tickstart;
                        heatconsumption = ((decimal)(_previousstate.Heat - dto.Heat)) / (decimal)_previousstate.Population;
                        Console.WriteLine($"heatconsumption {heatconsumption}");
                    }


                    if (!Haveeverything)
                    {
                        Haveeverything = (cycle > 0 && foodconsumption > 0 && woodconsumption > 0 && campfirecost > 0 && campfirereward > 0 && stoneconsumption > 0 && heatconsumption > 0);
                    }
                }
                if ((cycle > 0 && dto.Tick % cycle == 1))
                {
                    campfiresthiscycle = 0;
                }
                
                /*foreach (var x in nodes.Values)
                {
                    x.Node = ownNodes.FirstOrDefault(m=>m.Id==x.Node.Id);
                    var tmpNodes = x.checkReturnedUnits(dto.Tick);
                }*/
               
                if (units == 0)
                {
                    issueing = false;
                    return playerCommand;
                }
                //playerCommand.PlayerId = this._bot.Id;
                
                if (this._gameState.World != null)
                {
                    if (ownNodes.Count()>0 && GetDistance(ownNodes.FirstOrDefault()) < 5)
                    {

                        if (Haveeverything && dto.Population>50)
                        {
                            

                            var foods = tmpNodes.Where(m => m.Node.Type == ResourceType.Food && m.Remaining > 1000).ToList();
                            foodrewards = foods.Count() > 0 ? foods.Sum(m => m.Reward):foodrewards;
                            fooddays = foods.Count() > 0 ? foods.Sum(m => m.TravelTime) : fooddays;
                            decimal m = (foodrewards / fooddays * (decimal)cycle); 
                            decimal ff = ((decimal)foodconsumption*1.2m ) / m;
                            

                            var Woods = tmpNodes.Where(m => m.Node.Type == ResourceType.Wood && m.Remaining > 00).ToList(); ;
                            woodrewards = Woods.Count()>0? Woods.Sum(m => m.Reward) : woodrewards ;
                            wooddays = Woods.Count() > 0 ? Woods.Sum(m => m.TravelTime): wooddays;
                            decimal n = woodrewards / wooddays * (decimal)cycle;
                            decimal woodcon2 = woodconsumption + (dto.Tick<1000?0.1m: (campfirecost / campfirereward));
                            decimal wf = ((decimal)woodcon2) / n;

                            var Stones = tmpNodes.Where(m => m.Node.Type == ResourceType.Stone && m.Remaining > 00).ToList(); ;
                            stonerewards = Stones.Count() > 0 ? Stones.Sum(m => m.Reward) : stonerewards;
                            stonedays = Stones.Count() > 0 ? Stones.Sum(m => m.TravelTime) : stonedays;
                            decimal o =  stonerewards / stonedays * (decimal)cycle;
                            decimal sf = ((decimal)stoneconsumption) / o;

                           
                           

                            if (foods.Count() <= 2 || Woods.Count() <= 2 || Stones.Count() <= 2)
                            {

                                int tunits = Math.Min(3, units);
                                List<CommandAction> commands = Scout(tunits);
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
                            decimal hf = ((decimal)heatconsumption+0.05m) / p;

                            decimal TotalWeight = ff + wf + sf + hf;

                            int foodunits =  0;
                            int woodunits =  0;
                            int stoneunits = 0;
                            int heatunits = 0;

                            if (units >= 4)
                            {
                                //decimal fieldunits = PendingUnits.Sum(m => m.Units);
                                //decimal pendingfood = PendingUnits.Where(m => m.Type == ResourceType.Food).Sum(m => m.Units);
                                //decimal pendingWood = PendingUnits.Where(m => m.Type == ResourceType.Wood).Sum(m => m.Units);
                                //decimal pendingStone = PendingUnits.Where(m => m.Type == ResourceType.Stone).Sum(m => m.Units);
                                //decimal pendingHeat = campfires;
                                //decimal TotalUnits = fieldunits+campfires+units;

                                
                                //decimal Totalheatunits = (int)Math.Floor((double)(TotalUnits * (hf / TotalWeight)));
                                //heatunits =(int)( Totalheatunits - pendingHeat);
                                heatunits = (int)Math.Ceiling((double)(units * (hf / TotalWeight)));
                                int MinHeats = (int)Math.Ceiling((((dto.Population * (booming? 3 :1.1m) / heatconsumption) / 10m) / campfirereward));
                                /*if (dto.Tick >= popBoom)
                                {
                                    //heatunits = (int)Math.Floor((double)(units * (hf / TotalWeight)));
                                    MinHeats = (int)Math.Ceiling((((dto.Population * 1.05m / heatconsumption) / 10m) / (campfirereward * 1.1m)));
                                   
                                }*/
                                if (heatunits < MinHeats)
                                {
                                    //Console.WriteLine("Failed heat check");
                                    heatunits = MinHeats;
                                }
                                if (heatunits > units)
                                    heatunits = units;
                                
                                //dto.Food
                                if (dto.Tick<maxTicks)
                                {
                                    //set the heatcap according to available food

                                    if (dto.Tick>1000 && UnscoutedTowers() && units>0)
                                    {
                                        var Result = Scout(units);
                                        if (Result.Count > 0)
                                            playerCommand.Actions.AddRange(Result);
                                    }

                                    decimal ticketsleft = maxTicks - dto.Tick;
                                    if (ticketsleft > 400 && singleplayer)
                                    {
                                        ticketsleft += 500;
                                    }                                    
                                    decimal availablefood = ownNodes.Where(m => m.Type == ResourceType.Food).Sum(m => m.Amount);
                                    decimal FoodPerTicket = ownNodes.Where(m => m.Type == ResourceType.Food && m.Amount<nodes[m.Id].maxamount).Sum(m => m.RegenerationRate.Amount/m.RegenerationRate.Ticks);
                                    decimal Totalfood = dto.Food+ availablefood+ (singleplayer?(FoodPerTicket * ticketsleft):0);
                                    decimal SustainPop = Totalfood / (ticketsleft / 10); ;
                                   
                                    if (dto.Tick%10==1)
                                        Console.WriteLine($"ticksleft: {ticketsleft} ReadyFood {availablefood} RegenFood {FoodPerTicket} TotalFood {Totalfood} SustainPop {SustainPop}");
                                    HeatCap = SustainPop*2m ;
                                }



                                if (dto.Tick>popBoomCalcStart && dto.Tick%10==0 && popBoom>dto.Tick)
                                {
                                    decimal totalpop = dto.Population;
                                    decimal totalfood = 0;
                                    int i = 0;
                                    for (i = dto.Tick; i<=2500;i+=10)
                                    {
                                        totalfood += totalpop;
                                        totalpop *= 1.05m;
                                        if (totalfood>dto.Food)
                                        {
                                            
                                            break;
                                            
                                        }
                                    }
                                    if (i >= 2500-30)
                                    {
                                        popBoom = dto.Tick;
                                    }
                                }
                                //if (popBoom < dto.Tick)
                                //{
                                //    //popBoom = dto.Tick;
                                //    //Console.WriteLine("----------------------------------------------------------------------------------------------------------------------------------");
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
                                    HeatCap = 60000;
                                if (dto.Heat>= HeatCap)
                                {
                                   
                                    heatunits = 0;
                                    if (!singleplayer)
                                        HeatCap *= 1.015m;
                                    
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
                                campfiresthiscycle += heatunits;
                                if (dto.Wood < heatunits * campfirecost)
                                {
                                    int oldheat = heatunits;
                                    heatunits = (int)Math.Floor(dto.Wood / campfirecost);
                                    //units += oldheat - heatunits;
                                    heatunits += oldheat - heatunits;
                                }
                                units -= heatunits;

                                /* decimal TotalFood = (decimal)Math.Floor((double)(TotalUnits * (ff / TotalWeight)));
                                 decimal Totalwoodunits = (decimal)Math.Floor((double)(TotalUnits * (wf / TotalWeight)));
                                 decimal Totalstoneunits = (decimal)Math.Floor((double)(TotalUnits * (sf / TotalWeight)));
                                 decimal tmpTotal = TotalFood + Totalwoodunits + Totalstoneunits + heatunits;
                                 decimal scale =   Math.Min((decimal)dto.Population/tmpTotal,1) ;
                                 //if (units> TotalFood)
                                 if (units > 0)
                                 {
                                     foodunits = (int)Math.Min(Math.Max(Math.Floor(scale * (TotalFood - pendingfood)), 0),units);
                                     units -= foodunits;
                                 }
                                 if (units > 0)
                                 {
                                     woodunits = (int)Math.Min(Math.Max(Math.Floor(scale * (Totalwoodunits - pendingWood)),0), units);
                                     units -= woodunits;
                                 }
                                 if (units > 0)
                                 {
                                     stoneunits = (int)Math.Min(Math.Max(Math.Floor(scale * (Totalstoneunits - pendingStone)),0), units);
                                     units -= stoneunits;
                                 }*/

                                foodunits = (int)Math.Floor((double)(units * (ff / TotalWeight)));
                                woodunits = (int)Math.Floor((double)(units * (wf / TotalWeight)));
                                stoneunits = (int)Math.Floor((double)(units * (sf / TotalWeight)));
                                units -= foodunits + woodunits + stoneunits;
                                //units -= foodunits + woodunits + stoneunits;
                                if (units > 0)
                                {
                                    if (UnscoutedTowers())
                                    {
                                        playerCommand.Actions.AddRange(Scout(units));
                                    }
                                    else //if (popBoom <= dto.Tick)
                                        foodunits += units;
                                    /*else
                                        woodunits += units;*/
                                }
                                else if (units < 0)
                                {

                                }
                               /* Console.WriteLine($"ff: {ff:0.0000}\twf: {wf:0.0000}\tsf: {sf:0.0000}\thf: {hf:0.0000}\r\n" +
                                $"fu: {foodunits:00000}\tww: {woodunits:00000}\tsf: {stoneunits:00000}\thu: {heatunits:00000}\r\n");
                                */
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

                            if (heatunits > 0)
                            {
                                campfires = heatunits;
                                playerCommand.Actions.Add(new CommandAction()
                                {
                                    Type = ActionType.StartCampfire,
                                    Units = heatunits,
                                });

                            }
                            int index = 0;
                            foods = tmpNodes.Where(m => m.Node.Type == ResourceType.Food && m.Node.Amount > 0).ToList();
                            while (foodunits > 0 && index<foods.Count)
                            {
                                var closestnode = foods[index];// nodes.Values.FirstOrDefault(m => m.Node.Type == ResourceType.Food && m.Remaining > 0);
                                //var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                if (closestnode == null)
                                {
                                    playerCommand.Actions.AddRange(Scout(foodunits));
                                    //units = foodunits;
                                    foodunits = 0;
                                    Console.WriteLine("No farms left bitch");
                                    //closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && m.Amount > 100);
                                }
                                if (foodunits > 0 && closestnode != null)
                                {
                                    int uisedunits = foodunits;
                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                    var tmpAction = MineNode(closestnode.Node, ref uisedunits);
                                    if (tmpAction!=null)
                                        playerCommand.Actions.Add(tmpAction);
                                    foodunits -= uisedunits;
                                }
                                index++;
                            }
                            if (foodunits > 0)
                                woodunits += foodunits;
                            index=0;
                            while (woodunits > 0 && index < Woods.Count)
                            {
                                var closestnode = Woods[index];// nodes.Values.FirstOrDefault(m => m.Node.Type == ResourceType.Wood && m.Remaining > 0);
                                //var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                if (closestnode == null)
                                {
                                    playerCommand.Actions.AddRange(Scout(woodunits));
                                    //units = foodunits;
                                    woodunits = 0;
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
                            if (woodunits > 0)
                                stoneunits += woodunits;
                            index = 0;
                            while (stoneunits > 0 && index < Stones.Count)
                            {
                                var closestnode = Stones[index];// nodes.Values.FirstOrDefault(m => m.Node.Type == ResourceType.Stone && m.Remaining > 0);
                                //var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                if (closestnode == null)
                                {
                                    playerCommand.Actions.AddRange(Scout(stoneunits));
                                    //units = foodunits;
                                    stoneunits = 0;
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

                        }
                        else
                        {

                            #region old 
                            if (expand && dto.Heat < dto.Population * 4)
                            {
                                Previouscampfires = campfires;
                                campfires = Math.Min(Math.Max(dto.Population / 5, 1), units);

                                playerCommand.Actions.Add(new CommandAction()
                                {
                                    Type = ActionType.StartCampfire,
                                    Units = campfires,
                                });
                                units -= campfires;
                            }
                            /*if ((dto.Food < 5 * dto.Population || (Current == ResourceType.Food)) && units > 0 || units > 0)
                            {
                                if (dto.Food < 5 * dto.Population )
                                {
                                    Current = ResourceType.Food;
                                    var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && (m.Amount > 2 * dto.Population || m.Amount > 1000));
                                    if (closestnode == null)
                                    {
                                        playerCommand.Actions.AddRange(Scout(1));
                                        units--;
                                        closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && m.Amount > 100);
                                    }
                                    int usedunits = units;
                                    //get feedback from minenode as to hoy many are going and re-assign the rest
                                    var tmpAction = MineNode(closestnode, ref usedunits);
                                    if (tmpAction != null)
                                        playerCommand.Actions.Add(tmpAction);
                                    units -= usedunits;
                                }
                                else
                                {
                                    Current = ResourceType.Wood;
                                }
                            }
                            if ((Current == ResourceType.Wood) && units > 0 || units > 0)
                            {
                                if (dto.Wood < 4 * dto.Population)
                                {
                                    //Current = ResourceType.Wood;
                                    var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Wood && m.Amount > 0);
                                    if (closestnode == null)
                                    {
                                        playerCommand.Actions.AddRange(Scout(1));
                                        units--;
                                        //closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Wood && m.Amount > 10);
                                    }
                                    else
                                    {
                                        int usedunits = units;
                                        //get feedback from minenode as to hoy many are going and re-assign the rest
                                        var tmpAction = MineNode(closestnode, ref usedunits);
                                        if (tmpAction != null)
                                            playerCommand.Actions.Add(tmpAction);
                                        units -= usedunits;
                                    }
                                }
                                else
                                {
                                    Current = ResourceType.Stone;
                                }
                            }
                            if ((Current == ResourceType.Stone && dto.Stone < 2 * dto.Population) && units > 0 || units > 0)
                            {
                                if (dto.Stone < 2 * dto.Population)
                                {
                                    Current = ResourceType.Stone;
                                    var closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Stone && m.Amount > 10);
                                    if (closestnode == null)
                                    {
                                        //Maybe here use a percentage of what should be allocated to mining?
                                        playerCommand.Actions.AddRange(Scout(1));
                                        units--;
                                        //closestnode = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food && m.Amount > 100);
                                    }
                                    else
                                    {
                                        int usedunits = units;
                                        //get feedback from minenode as to hoy many are going and re-assign the rest
                                        var tmpAction = MineNode(closestnode, ref usedunits);
                                        if (tmpAction != null)
                                            playerCommand.Actions.Add(tmpAction);
                                        units -= usedunits;
                                    }

                                }
                                else
                                {
                                    Current = ResourceType.Food;
                                }
                            }*/
                            int foodunits = 0;
                            int woodunits = 0;
                            int stoneunits = 0;
                            while (units>0)
                            {
                                switch (resourcerotation[lastindex++])
                                {
                                    case ResourceType.Food: foodunits++; units--; break;
                                    case ResourceType.Wood: woodunits++; units--; break;
                                    case ResourceType.Heat: stoneunits++; units--; break;//actually stone
                                }
                                if (lastindex > (dto.Population>20?2:1))
                                    lastindex = 0;
                            }
                            if (foodunits>0)
                            {
                                var closestnode = tmpNodes.FirstOrDefault(m => m.Node.Type == ResourceType.Food && m.Remaining > foodunits);
                                var tmpAction = MineNode(closestnode.Node, ref foodunits);
                                if (tmpAction != null)
                                    playerCommand.Actions.Add(tmpAction);
                                units -= foodunits;

                            }                            
                            if (woodunits>0)
                            {
                                var closestnode = tmpNodes.FirstOrDefault(m => m.Node.Type == ResourceType.Wood && m.Remaining > woodunits);
                                var tmpAction = MineNode(closestnode.Node, ref woodunits);
                                if (tmpAction != null)
                                    playerCommand.Actions.Add(tmpAction);
                                units -= woodunits;
                            }
                            if (stoneunits > 0)
                            {
                                var closestnode = tmpNodes.FirstOrDefault(m => m.Node.Type == ResourceType.Stone && m.Remaining > stoneunits);
                                var tmpAction = MineNode(closestnode.Node, ref stoneunits);
                                if (tmpAction != null)
                                    playerCommand.Actions.Add(tmpAction);
                                units -= stoneunits;
                            }

                            if (dto.Wood > 10 && dto.Food > 10 && !expand && woodconsumption > 0)                                
                            {

                                //create a campfire
                                expand = true;
                                Current = ResourceType.Food;

                            }
                            #endregion
                        }
                    }
                    else
                    {
                        if (UnscoutedTowers() && (ownNodes.Count() == 0  || (ownNodes.Count()>0 && GetDistance(ownNodes.FirstOrDefault())>5)))
                        {
                            playerCommand.Actions.AddRange(Scout(units));
                        }
                    }
                }
                issueing = false;
            }
            _previousstate = dto;
            return playerCommand;
        }

        internal void PrintFinal()
        {
            //Haveeverything = (cycle > 0 && foodconsumption > 0 && woodconsumption > 0 && campfirecost > 0 && campfirereward > 0 && stoneconsumption > 0 && heatconsumption > 0);

            Console.WriteLine($"Measurements: Cycle: {cycle}\r\n" +
                $"foodconsumption: {foodconsumption}\r\n" +
                $"woodconsumption: {woodconsumption}\r\n" +
                $"campfirecost: {campfirecost}\r\n" +
                $"campfirereward: {campfirereward}\r\n" +
                $"stoneconsumption: {stoneconsumption}\r\n" +
                $"heatconsumption: {heatconsumption}\r\n" +
                $"Max Population: {MaxPOp}");
            Console.WriteLine($"Total Remaining Resources\r\n" +
                $"Food:  regen {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Food).Sum(m => m.RegenerationRate.Amount/ m.RegenerationRate.Ticks )} remaining {_gameState.World.Map.Nodes.Where(m=>m.Type == ResourceType.Food).Sum(m=>m.Amount)}\r\n" +
                $"Wood: {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Wood).Count()} - {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Wood).Sum(m => m.Amount)}\r\n" +
                $"Stone: {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Stone).Count()} - {_gameState.World.Map.Nodes.Where(m => m.Type == ResourceType.Stone).Sum(m => m.Amount)}\r\n");
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

        List<CommandAction> Scout(int units)
        {
            List<CommandAction> actions = new List<CommandAction>();
            var tmpTowers = this._gameState.World.Map.ScoutTowers.OrderBy(m => GetDistance(m)).ToList();
            for (int i = 0; i < tmpTowers.Count() && units > 0; i++)
            {
                if (!towers.ContainsKey(tmpTowers[i].Id))
                    towers.Add(tmpTowers[i].Id, 0);
                if (towers[tmpTowers[i].Id]<=scouts)
                {

                    towers[tmpTowers[i].Id]++;
                    units--;


                    Console.WriteLine($"Sending scouts to {this._gameState.World.Map.ScoutTowers[i].Id}");
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
            if (scouts > 3)
                scouts = 0;
            return actions;
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
            Resources = Math.Max(Math.Min(Math.Min(x.MaxUnits, Resources), tmpState.Remaining), 0);
            //Resources = tmpState.Units;
            if (x.Type== ResourceType.Wood)
            {

            }

            //Console.WriteLine($"Resource {x.Id.ToString()} type {x.Type.ToString()} worktime {x.WorkTime} Amount {x.Amount} Remaining {tmpState.Remaining} Units {Resources} Current Units: {x.CurrentUnits}");
            if (Resources > 0)
                return tmpState.AddAction(Resources, (int)Math.Round(GetDistance(x)), _bot.Tick);
            else return null;
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
        public nodestate(ResourceNode node, decimal Distance)
        {
            PendingInstructions = new List<NodeInstruction>();
            this.Node = node;
            this.Reward = node.Reward;
            
            TravelTime = (int)Math.Round( Distance) + (Node.WorkTime) + 2;
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

        

        internal CommandAction AddAction(int units, int distance, int tick )
        {
            
            var tmpInstruction = new NodeInstruction { Units = units, ExpectedReturn = tick + TravelTime, Type= node.Type };
            
            if (Reward>0)
            {
                tmpInstruction.ExpectedResources = Units * Reward;
                Remaining -= tmpInstruction.ExpectedResources;
            }
            PendingInstructions.Add(tmpInstruction);
            
            return new CommandAction()
            {
                Type = GetAction(),
                Units = units,
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
                    //Console.WriteLine($"{x.Units} Units returned from {node.Id} with {x.ExpectedResources} {node.Type.ToString()}. Expected back on tick {x.ExpectedReturn}");
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
}