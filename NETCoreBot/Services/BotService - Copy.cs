using Domain.Models;
using NETCoreBot.Enums;
using NETCoreBot.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace NETCoreBot.Services
{
    public class BotService2
    {
        private BotDto _bot;
        private CommandAction _playerAction;
        private PlayerCommand _playerCommand;
        private GameState _gameState;

        Dictionary<Guid, nodestate> nodes = new Dictionary<Guid, nodestate>();
        Dictionary<ResourceType, int> PreviousValues = new Dictionary<ResourceType, int>();

        int[] unitsAssigned = new int[] {0,0,0,0};//food,wood,stone,gold
        int NextAssignIndex = 0;
        int nextindex = 0;
        bool attemptedFire = true;
        int turns = 0;
        
        public BotService2()
        {
            _playerAction = new CommandAction();
            _playerCommand = new PlayerCommand();
            _gameState = new GameState();
            _bot = new BotDto();
            PreviousValues.Add(ResourceType.Food, _bot.Food);
            PreviousValues.Add(ResourceType.Wood, _bot.Wood);
            PreviousValues.Add(ResourceType.Stone, _bot.Stone);
            PreviousValues.Add(ResourceType.Heat, _bot.Heat);
            
        }

        public BotDto GetBot()
        {
            return _bot;
            
        }
        bool issueing = false;
        public PlayerCommand GetPlayerCommand()
        {

            PlayerCommand playerCommand = new PlayerCommand();

            if (!issueing) 
            {
                issueing = true;

                var dto = this._gameState.Bots.FirstOrDefault(go => go.Id == _bot.Id);
                int units = dto.AvailableUnits;

                Dictionary<ResourceType, int> CurrentValues = new Dictionary<ResourceType, int>();
                CurrentValues.Add(ResourceType.Food, _bot.Food);
                CurrentValues.Add(ResourceType.Wood, _bot.Wood);
                CurrentValues.Add(ResourceType.Stone, _bot.Stone);
                CurrentValues.Add(ResourceType.Heat, _bot.Heat);
                Dictionary<ResourceType, int> TypeChanges = new Dictionary<ResourceType, int>();
                foreach (var x in PreviousValues.Keys)
                {
                    TypeChanges.Add(x, CurrentValues[x] - PreviousValues[x]);

                }
                if (_bot.Tick % 10 == 0)
                {

                }
                int food = nodes.Values.Where(x => x.Node.Type == ResourceType.Food).Sum(x => x.Units);
                int wood = nodes.Values.Where(x => x.Node.Type == ResourceType.Wood).Sum(x => x.Units);
                int stone = nodes.Values.Where(x => x.Node.Type == ResourceType.Stone).Sum(x => x.Units);
                int heat = nodes.Values.Where(x => x.Node.Type == ResourceType.Heat).Sum(x => x.Units);


                if (dto.Tick % 50 == 0)
                    Console.WriteLine($"Population: {dto.Population} Units: {dto.AvailableUnits} Traveling {dto.TravellingUnits}\r\n" +
                        $"Food: {dto.Food} Wood: {dto.Wood} Stone: {dto.Stone} gold: ???? Heat: {dto.Heat}\r\n" +
                        $"Farm: {dto.FarmingUnits} Lumb: {dto.LumberingUnits} Mines: {dto.MiningUnits} gold: ???? scout: {dto.ScoutingUnits}"
                        );

                if (units == 0)
                {
                    issueing = false;
                    return playerCommand;
                }

                if (this._gameState.World != null)
                {
                    if (nodes.Count > 0)
                    {
                        var tmpnode = nodes.ElementAt(nextindex).Value;
                        //foreach (var x in nodes.Values)
                        {
                           if (dto.Food>50 && dto.Wood>10 && !attemptedFire)
                            {
                                attemptedFire = true;
                                playerCommand.Actions.Add(new CommandAction()
                                {
                                    Type = ActionType.StartCampfire,
                                    Units = tmpnode.Units,
                                    Id = tmpnode.Node.Id,
                                });
                            }
                            else
                            {
                                playerCommand.Actions.Add(new CommandAction()
                                {
                                    Type = tmpnode.GetAction(),
                                    Units = tmpnode.Units,
                                    Id = tmpnode.Node.Id,
                                });
                                //tmpnode.ExpectedReturn = dto.Tick + ((int)GetDistance(tmpnode.Node)) * 2;
                                units--;
                                if (dto.Tick % 50 == 0)
                                    Console.WriteLine($"Resource {tmpnode.Node.Id.ToString()} type {tmpnode.Node.Type.ToString()} worktime {tmpnode.Node.WorkTime} remaining {tmpnode.Node.Amount}");

                                if (++turns==6)
                                {
                                    nextindex = 1;
                                }
                                else if (turns>7)
                                {
                                    nextindex = 0;
                                    turns = 0;
                                }
                            }
                        }
                            

                    }
                    var ownNodes = this._gameState.World.Map.Nodes.OrderBy(m => GetDistance(m));


                    while (units > 0 && nodes.Count < this._gameState.World.Map.Nodes.Count)
                    {
                        //assign work to someone

                        //for (int i = 0; i < this._gameState.World.Map.Nodes.Count && units > 0; i++)
                        if (this._gameState.World.Map.Nodes.Count > 0)
                        {
                            ResourceNode x = null;

                            while (x == null)
                            {
                                switch (NextAssignIndex)
                                {
                                    case 0: x = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Food); break;
                                    case 1: x = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Wood); break;
                                    case 2: x = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Stone); break;
                                    case 3: x = ownNodes.FirstOrDefault(m => m.Type == ResourceType.Gold); break;
                                }

                                if (++NextAssignIndex > 3)
                                    NextAssignIndex = 0;


                            }
                            //var x = this._gameState.World.Map.Nodes[i];
                            if (x != null && x.Amount > 0)
                            {
                                nodestate tmpState = null;
                                nodes.TryGetValue(x.Id, out tmpState);
                                if (tmpState == null)
                                {
                                    nodes.Add(x.Id, new nodestate(x, GetDistance(x) ));
                                    tmpState = nodes[x.Id];
                                }
                                tmpState.Units = 1;


                                units -= tmpState.Units;
                                playerCommand.PlayerId = this._bot.Id;
                                Console.WriteLine($"Resource {x.Id.ToString()} type {x.Type.ToString()} worktime {x.WorkTime} remaining {x.Amount} Distance: {GetDistance(x)}");

                                playerCommand.Actions.Add(new CommandAction()
                                {
                                    Type = tmpState.GetAction(),
                                    Units = tmpState.Units,
                                    Id = this._gameState.World.Map.Nodes[0].Id,
                                });
                            }
                        }
                    }

                    if (units > 0)
                    {
                        playerCommand.Actions.Add(new CommandAction()
                        {
                            Type = ActionType.Scout,
                            Units = 1,
                            Id = this._gameState.World.Map.ScoutTowers[0].Id,
                        });

                        playerCommand.PlayerId = this._bot.Id;
                        Console.WriteLine($"Sending scouts to {this._gameState.World.Map.ScoutTowers[0].Id}");
                    }
                }
                issueing = false;
            }
            
            return playerCommand;
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

        private double GetDistance(Node node)
        {

            Position baseLocation = this._bot.BaseLocation;
            Position nodeLocation = node.Position;

            double deltaX = baseLocation.X - nodeLocation.X;
            double deltaY = baseLocation.Y - nodeLocation.Y;
            var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            double distance = Math.Sqrt(distanceSquared);

            return distance;
        }
        private decimal GetDistance(ResourceNode node)
        {

            Position baseLocation = this._bot.BaseLocation;
            Position nodeLocation = node.Position;

            double deltaX = baseLocation.X - nodeLocation.X;
            double deltaY = baseLocation.Y - nodeLocation.Y;
            var distanceSquared = (deltaX * deltaX) + (deltaY * deltaY);

            double distance = Math.Sqrt(distanceSquared);

            return (decimal) distance;
        }

    }
}