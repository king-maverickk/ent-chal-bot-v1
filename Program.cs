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
                Console.WriteLine(botState.HeroWindow.GetType()); // System.Int32[][]

                // If there are commands in the queue, send the next one
                if (commandQueue.Count > 0)
                {
                    InputCommand nextCommand = commandQueue.Dequeue();
                    BotCommand command = new BotCommand() { Action = nextCommand, BotId = BotId };
                    connection.InvokeAsync("SendPlayerCommand", command).Wait();
                }

                // Respond with simple command.
                // BotCommand command = new BotCommand() { Action = Enums.InputCommand.DOWN, BotId = BotId };
                
                //connection.InvokeAsync("SendPlayerCommand", command).Wait();
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