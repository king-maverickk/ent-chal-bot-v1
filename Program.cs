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

        private static BotStateDTO _botState;
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
                _botState = botState; // get the state and update it
            });

            // Register bot.
            connection.InvokeAsync("Register", token, botNickname).Wait();

            // Loop while connected.
            while (connection.State == HubConnectionState.Connected || connection.State == HubConnectionState.Connecting || connection.State == HubConnectionState.Reconnecting)
            {
                // use while to make moves, assess situtation etc.
                if (_botState != null)
                {
                    MoveBot(InputCommand.RIGHT, BotId, connection);
                }
                Thread.Sleep(300);
            }
        }
        public static void MoveBot(InputCommand direction, Guid botId, HubConnection connection)
        {
            BotCommand command = new BotCommand() { Action = direction, BotId = botId };
            connection.InvokeAsync("SendPlayerCommand", command).Wait();
        }
    }
}