using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using BitchAssBot.Enums;
using BitchAssBot.Models;
using BitchAssBot.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Reflection;

namespace BitchAssBot
{
    public class Program
    {
        static bool train = true;
        public static IConfigurationRoot Configuration;

        private static async Task Main(string[] args)
        {
            Console.WriteLine(Assembly.GetExecutingAssembly().GetName().Version.ToString());
            //if (train)
            //{
            //    Process process; 
            //    string strCmdText = @"bash C:\Users\seunt\Downloads\starter-pack\starter-pack\run.sh"; 
            //    process = new ProcessstrCmdText);                 
            //    ///process.StartInfo.FileName = strCmdText; 
            //    process.StartInfo.UseShellExecute = true;                 
            //    //process.StartInfo.CreateNoWindow = true;
            //    process.Start();
            //    process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;                  
            //    Thread.Sleep(7000); 
            //} 
                                                                                                                                                                                                                         // Set up configuration sources.
                var builder = new ConfigurationBuilder().AddJsonFile($"appsettings.json", optional: false);

            Configuration = builder.Build();
            var registrationToken = Environment.GetEnvironmentVariable("Token");
            var environmentIp = Environment.GetEnvironmentVariable("RUNNER_IPV4")?? "http://localhost";
            var ip = !string.IsNullOrWhiteSpace(environmentIp) ? environmentIp : Configuration.GetSection("RunnerIP").Value;
            ip = ip.StartsWith("http://") ? ip : "http://" + ip;
            bool started = false;
            var port = Configuration.GetSection("RunnerPort");

            var url = ip + ":" + port.Value + "/runnerhub";

            var connection = new HubConnectionBuilder()
                                .WithUrl($"{url}")
                                .ConfigureLogging(logging =>
                                {
                                    logging.SetMinimumLevel(LogLevel.Debug);
                                })
                                .WithAutomaticReconnect()
                                .Build();

            var botService = new BotService();

            await connection.StartAsync()
                .ContinueWith(
                    task =>
                    {
                        Console.WriteLine("Connected to Runner");
                        /* Clients should disconnect from the server when the server sends the request to do so. */
                        connection.On<Guid>(
                            "Disconnect",
                            (id) =>
                            {
                                Console.WriteLine("Disconnected:");
                                botService.PrintFinal();
                                connection.StopAsync();
                                connection.DisposeAsync();                                
                            });
                        connection.On<Guid>(
                            "Registered",
                            (id) =>
                            {
                                Console.WriteLine("Registered Bot with the runner");
                                botService.SetBot(
                                    new BotDto()
                                    {
                                        Id = id
                                    });
                                botService.Id = id;
                                Console.WriteLine($"Bot Id: {id}");
                            });

                        /* Get the current WorldState along with the last known state of the current client. */
                        connection.On<GameStateDto>(
                            "ReceiveBotState",
                            (gameStateDto) =>
                            {
                                //Console.WriteLine("GameStateDTO hit: " + gameStateDto?.World.CurrentTick);
                                var gameState = new GameState { World = null, Bots = new List<BotDto>() };
                                gameState.World = gameStateDto.World;
                                gameState.Bots = gameStateDto.Bots;
                                gameState.PopulationTiers = gameStateDto.PopulationTiers;
                                botService.SetGameState(gameState);
                                var bot = botService.GetBot();
                                if (botService.GetGameState().World != null)
                                {
                                    //botService.ComputeNextPlayerAction(botService.GetPlayerCommand());
                                    connection.InvokeAsync("SendPlayerCommand", botService.GetPlayerCommand());
                                }
                            });
                        connection.On<EngineConfigDto>(
                           "ReceiveConfigValues",
                           (engineConfigDto) =>
                           {
                               Console.WriteLine("engineConfigDto hit");
                               botService.SetEngineConfigDto(engineConfigDto);
                               
                           });
                        var token = Environment.GetEnvironmentVariable("REGISTRATION_TOKEN");
                        token = !string.IsNullOrWhiteSpace(token) ? token : Guid.NewGuid().ToString();
                        //Console.WriteLine(token);
                        Thread.Sleep(1000);
                        Console.WriteLine("Registering with the runner...");
                        connection.SendAsync("Register", token, "Seuntj1e");

                        while (connection.State == HubConnectionState.Connected)
                        {
                            Thread.Sleep(30);
                            
                            /*if (!botService.started)
                            {
                                try
                                {
                                    Console.WriteLine("Attempting early scout");
                                    int units = 1;
                                    var tmp = botService.Scout(ref units);
                                    if (tmp != null && tmp.Count > 0)
                                    {
                                        PlayerCommand playerCommand = new PlayerCommand();
                                        playerCommand.PlayerId = botService.Id;
                                        playerCommand.Actions.AddRange(tmp);
                                        connection.InvokeAsync("SendPlayerCommand", botService.GetPlayerCommand());
                                    }
                                }
                                catch (Exception e)
                                {

                                }
                            }*/
                           // Console.WriteLine($"ConState: {connection.State}");
                           // Console.WriteLine($"Bot: {botService.GetBot()?.Id.ToString()}");

                            //var bot = botService.GetBot();
                            //if (bot == null)
                            //{
                            //    continue;
                            //}

                            //if (botService.GetGameState().World != null)
                            //{
                            //    //botService.ComputeNextPlayerAction(botService.GetPlayerCommand());
                            //    connection.InvokeAsync("SendPlayerCommand", botService.GetPlayerCommand());
                            //}
                        }
                    });
        }
    }
}
