using System;
using ent_chal_bot_v1.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ent_chal_bot_v1.Enums;
using ent_chal_bot_v1.Bots;
using ent_chal_bot_v1.Services;

namespace ent_chal_bot_v1
{
    public class Program
    {
        private static IConfigurationRoot Configuration;
        private static Guid BotId;
        private static Queue<InputCommand> commandQueue = new Queue<InputCommand>();

        private static BotStateDTO _botState;
        private static BotV3 _botV3 = new BotV3();
        private static void Main(string[] args)
        {
            // function is below
            // printStuff();

            // Setup configuration builder with appsettings.json
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: false);

            // Read appsettings.json into Configuration
            Configuration = builder.Build();

            // Retrieve RUNNER_IPV4 environment variable.
            var environmentIp = Environment.GetEnvironmentVariable("RUNNER_IPV4");
            // Use RUNNER_IPV4 environment variable if it exists,
            // else read the value in appsettings.json
            var runnerIp = !string.IsNullOrWhiteSpace(environmentIp)
                ? environmentIp
                : Configuration.GetSection("RunnerIP").Value!;
            // Add http:// to start if not already present.
            if (!runnerIp.StartsWith("http://"))
            {
                runnerIp = $"http://{runnerIp}";
            }

            // Retrieve bot nickname from either environment variable or appsettings.json
            var botNickname = Environment.GetEnvironmentVariable("BOT_NICKNAME")
                ?? Configuration.GetSection("BotNickname").Value;

            // Get registration token from environment.
            var token = Environment.GetEnvironmentVariable("Token");
            // Get runner hub port
            var port = Configuration.GetSection("RunnerPort").Value;
            // Build runner hub connection URL.
            var runnerHubUrl = $"{runnerIp}:{port}/runnerhub";

            // Build SignalR connection
            var connection = new HubConnectionBuilder()
                .WithUrl(runnerHubUrl)
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Debug);
                })
                .WithAutomaticReconnect()
                .Build();

            // Start connection
            connection.StartAsync().Wait();
            Console.WriteLine("Connected to Runner!");

            // Listen for registered event.
            connection.On<Guid>("Registered", (id) =>
            {
                Console.WriteLine($"Bot Registered with ID: {id}");
                BotId = id;
            });

            // Using to process stuff
            BotService botService = new();


            // Print bot state.
            connection.On<BotStateDTO>("ReceiveBotState", (botState) =>
            {
                Console.WriteLine(botState.ToString()); // VERY IMPORTANT to get information
                _botState = botState; // get the state and update it
            });

            // Register bot.
            connection.InvokeAsync("Register", token, botNickname).Wait();

            // Loop while connected.
            while (connection.State == HubConnectionState.Connected || connection.State == HubConnectionState.Connecting || connection.State == HubConnectionState.Reconnecting)
            {
                // use while-loop to make moves, assess situtation etc.
                if (_botState != null)
                {
                    EncircleTiles(200, _botState, connection); // Example: Encircle 10 tiles
                    _botV3.recordPosition(_botState);
                    _botV3.calculateAquired();
                }
                Thread.Sleep(300);
            }
        }
        public static void MoveBot(InputCommand direction, Guid botId, HubConnection connection)
        {
            BotCommand command = new BotCommand() { Action = direction, BotId = botId };
            connection.InvokeAsync("SendPlayerCommand", command).Wait();
        }

        public static void printStuff()
        {
            Console.WriteLine("=======RANDOM PATH===========");
            Console.WriteLine("");
            Console.WriteLine("");
            List<int> randomPath = _botV3.randomPath(12, 1);
            _botV3.printStuff(randomPath);
        }

        private static void EncircleTiles(int n, BotStateDTO botState, HubConnection connection)
        {
            int startX = botState.X;
            int startY = botState.Y;

            // Calculate the side length of the square to encircle at least n tiles
            int sideLength = (int)Math.Ceiling(Math.Sqrt(n));

            // If the side length is odd, adjust it to be even
            if (sideLength % 2 != 0)
            {
                sideLength++;
            }

            // Calculate half of the side length
            int halfSide = sideLength / 2;

            // Store the path to encircle and return to start point
            List<InputCommand> path = new List<InputCommand>();

            // Move UP to create the upper boundary
            for (int i = 0; i < halfSide; i++)
            {
                path.Add(InputCommand.UP);
            }


            // Move LEFT to create the left boundary
            for (int i = 0; i < sideLength; i++)
            {
                path.Add(InputCommand.LEFT);
            }

            // Move DOWN to create the bottom boundary
            for (int i = 0; i < sideLength; i++)
            {
                path.Add(InputCommand.DOWN);
            }

            // Move RIGHT to create the right boundary
            for (int i = 0; i < sideLength; i++)
            {
                path.Add(InputCommand.RIGHT);
            }

            

            // Move UP to return to the starting row
            for (int i = 0; i < halfSide; i++)
            {
                path.Add(InputCommand.UP);
            }

            // Execute the path to encircle the tiles and return to the start point
            foreach (var command in path)
            {
                MoveBot(command, BotId, connection);
            }
        }
    }
}