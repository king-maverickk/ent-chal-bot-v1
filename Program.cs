using System;
using ent_chal_bot_v1.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using ent_chal_bot_v1.Enums; // for using Queue

namespace ent_chal_bot_v1
{
    public class Program
    {
        private static IConfigurationRoot Configuration;
        private static Guid BotId;
        private static Queue<InputCommand> commandQueue = new Queue<InputCommand>();

        private static void Main(string[] args)
        {
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

            // Print bot state.
            connection.On<BotStateDTO>("ReceiveBotState", (botState) =>
            {
                Console.WriteLine(botState.ToString()); // VERY IMPORTANT to get information

                EncircleTiles(10, botState, connection); // Example: Encircle 10 tiles
            });

            // Register bot.
            connection.InvokeAsync("Register", token, botNickname).Wait();

            // use the movement command queue
            MovementCommandQueue();

            // Loop while connected.
            while (connection.State == HubConnectionState.Connected || connection.State == HubConnectionState.Connecting || connection.State == HubConnectionState.Reconnecting)
            {
                Thread.Sleep(300);
            }


        }
        public static void MoveBot(InputCommand direction, Guid botId, HubConnection connection)
        {
            BotCommand command = new BotCommand() { Action = direction, BotId = botId };
            connection.InvokeAsync("SendPlayerCommand", command).Wait();
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

            // Move RIGHT to create the right boundary
            for (int i = 0; i < sideLength; i++)
            {
                path.Add(InputCommand.RIGHT);
            }

            // Move DOWN to create the bottom boundary
            for (int i = 0; i < sideLength; i++)
            {
                path.Add(InputCommand.DOWN);
            }

            // Move LEFT to create the left boundary
            for (int i = 0; i < sideLength; i++)
            {
                path.Add(InputCommand.LEFT);
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
            path.Add(InputCommand.RIGHT);
            path.Add(InputCommand.RIGHT);
        }
        
            private static void MovementCommandQueue()
        {
            commandQueue.Enqueue(Enums.InputCommand.DOWN);
            commandQueue.Enqueue(Enums.InputCommand.DOWN);
            commandQueue.Enqueue(Enums.InputCommand.DOWN);
            commandQueue.Enqueue(Enums.InputCommand.DOWN);
            commandQueue.Enqueue(Enums.InputCommand.RIGHT);
            commandQueue.Enqueue(Enums.InputCommand.RIGHT);
            commandQueue.Enqueue(Enums.InputCommand.RIGHT);
            commandQueue.Enqueue(Enums.InputCommand.DOWN);
            commandQueue.Enqueue(Enums.InputCommand.DOWN);
            commandQueue.Enqueue(Enums.InputCommand.DOWN);
            commandQueue.Enqueue(Enums.InputCommand.LEFT);
            commandQueue.Enqueue(Enums.InputCommand.LEFT);
            commandQueue.Enqueue(Enums.InputCommand.LEFT);
            commandQueue.Enqueue(Enums.InputCommand.LEFT);
            commandQueue.Enqueue(Enums.InputCommand.LEFT);
            commandQueue.Enqueue(Enums.InputCommand.LEFT);
            commandQueue.Enqueue(Enums.InputCommand.UP);
            commandQueue.Enqueue(Enums.InputCommand.UP);
            commandQueue.Enqueue(Enums.InputCommand.UP);
            commandQueue.Enqueue(Enums.InputCommand.RIGHT);
            commandQueue.Enqueue(Enums.InputCommand.RIGHT);
            // Add more movement commands as needed
        }
    }
}