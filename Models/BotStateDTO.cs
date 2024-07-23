﻿using ent_chal_bot_v1.Enums; // from page 37 of instruction manuals

namespace ent_chal_bot_v1.Models
{
    public class BotStateDTO {
        public int DirectionState { get; set; }
        public required string ElapsedTime { get; set; }
        public int GameTick { get; set; }
        public int PowerUp { get; set; }
        public int SuperPowerUp { get; set; }
        public required Dictionary<Guid, int> LeaderBoard { get; set; }
        public required Location[] BotPostions { get; set; }
        public required PowerUpLocation[] PowerUpLocations { get; set; }
        public required bool[][] Weeds { get; set; }
        public required int[][] HeroWindow { get; set; }
        public int X { get; set; }
        public int Y { get; set; }
        public override string ToString() {
            // Early return if we don't have a window.
            if (HeroWindow == null || HeroWindow[0] == null)
            {
                return "";
            }
            String result = "";

            // Print metadata.
            result += $"Elapsed Time: {ElapsedTime}\tGame Tick: {GameTick}\n";
            result += $"Position: ({X}, {Y})\tDirectionState: {(InputCommand)DirectionState}\n";
            // Print hero window.
            for (int y = 0; y < HeroWindow[0].Length; y++)
            {
                for (int x = 0; x < HeroWindow.Length; x++)
                {
                    result += " " + HeroWindow[x][y];
                }
                result += "\n";
            } // hero window is a 2D array (called jagged array)
              // the outer arrays are the y coords. the inner arrays are x coords
              // ...remember line int[][] HeroWindow

            result += "Weeds: \n";
            // Print the weeds status.
            for (int y = 0; y < Weeds[0].Length; y++)
            {
                for (int x = 0; x < Weeds.Length; x++)
                {
                    result += " " + Weeds[x][y];
                }
                result += "\n";
            }

            result += "Power Up Locations:\n";
            foreach (var powerUp in PowerUpLocations)
            {
                String type;
                switch (powerUp.Type)
                {
                    case 1:
                        type = "TerritoryImmunity";
                        break;
                    case 2:
                        type = "Unprunable";
                        break;
                    case 3:
                        type = "Freeze";
                        break;
                    default:
                        type = powerUp.Type.ToString(); // returns the int of the powerup 
                        break;
                }

                result += $"({powerUp.Location.X}, {powerUp.Location.Y}) - Type: {type}\t PowerUp Int: {powerUp.Type}\n";
            }

            // Print power ups.
            result += $"Power Up: {PowerUp}\n";
            result += $"Super Power Up: {SuperPowerUp}\n";
            /*
             * SAME AS X, Y above
            result += "Bot Positions:\n";
            foreach (var pos in BotPostions)
            {
                result += $"({pos.X}, {pos.Y})\n";
            }
            */

            return result;
        }


        // Cell types
        enum CellTypes
        {
            Bot0Territory = 0,
            Bot1Territory = 1,
            Bot2Territory = 2,
            Bot3Territory = 3,
            Bot0Trail = 4,
            Bot1Trail = 5,
            Bot2Trail = 6,
            Bot3Trail = 7,
            OutOfBounds = 254,
            Unclaimed = 255,
        }
    }
    public struct PowerUpLocation
    {
        public Location Location { get; set; }
        public int Type { get; set; }
    }
    public struct Location
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

}
