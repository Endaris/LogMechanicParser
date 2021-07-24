using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace LogMechanicParser
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Count() == 1)
            {
                string logDirectory = args[0];

                var mechanics = new List<LogMechanic>();
                var playerStatistics = new List<PlayerStatistics>();
                foreach (var file in Directory.EnumerateFiles(logDirectory).
                                              Where(f => Path.GetExtension(f) == ".json"))
                {
                    var jsonDocument = GetJsonDocument(file);
                    mechanics.AddRange(LogMechanic.GetLogMechanics(jsonDocument));
                    playerStatistics.AddRange(PlayerStatistics.GetPlayerStatistics(jsonDocument));               
                }

                playerStatistics = PlayerStatistics.CombineByName(playerStatistics);
                var playerMechanics = new List<PlayerMechanics>();

                foreach (var playerName in GetPlayerNames(mechanics))
                {
                    foreach (var mechanicName in GetMechanicNames(mechanics))
                    {
                        playerMechanics.Add(new PlayerMechanics()
                        {
                            PlayerName = playerName,
                            MechanicName = mechanicName,
                            MechanicValue = GetMechanicCountForPlayer(mechanics, playerName, mechanicName).ToString()
                        });
                    }

                    var stats = playerStatistics.FirstOrDefault(p => p.PlayerName == playerName);
                    if (stats != null)
                    {
                        foreach(var stat in stats.Statistics)
                        {
                            playerMechanics.Add(new PlayerMechanics()
                            {
                                PlayerName = playerName,
                                MechanicName = stat.Key,
                                MechanicValue = stat.Value
                            });
                        }
                    }
                }

                WriteFile(playerMechanics, logDirectory);
            }                
        }

        private static int GetMechanicCountForPlayer(List<LogMechanic> mechanics, string playerName, string mechanicName)
        {
            if (IsPersistentMechanic(mechanicName))
            {
                var playerMechanics = mechanics.Where(m => m.AffectedActor == playerName && m.MechanicName == mechanicName).ToList();
                return playerMechanics.Where(m => !playerMechanics.Any(t => t.Time >= m.Time - 1000 && t.Time < m.Time)).Count();
            }
            else
            {
                return mechanics.Where(m => m.AffectedActor == playerName && m.MechanicName == mechanicName).Count();
            }
        }

        private static bool IsPersistentMechanic(string mechanicName)
        {
            switch (mechanicName)
            {
                case "Echo PU":
                case "Dip":
                    return true;
            }
            return false;
        }

        private static List<string> GetMechanicNames(List<LogMechanic> mechanics)
        {
            return mechanics.Select(m => m.MechanicName).Distinct().ToList();
        }

        private static List<string> GetPlayerNames(List<LogMechanic> mechanics)
        {
            return mechanics.Select(m => m.AffectedActor).Distinct().ToList();
        }

        private static JsonDocument GetJsonDocument(string filePath)
        {
            if (File.Exists(filePath))
            {
                var json = File.ReadAllText(filePath);
                var jsonDocument = JsonDocument.Parse(json);
                return jsonDocument;
            }
            else
            {
                throw new FileNotFoundException($"Found no file at {filePath}");
            }
        }

        private static void WriteFile(List<PlayerMechanics> playerMechanics, string directory)
        {
            var file = new List<string>();
            var header = string.Concat("PlayerName;", string.Join(";", playerMechanics.Select(m => m.MechanicName).Distinct()));
            file.Add(header);

            foreach(var player in playerMechanics.Select(m  => m.PlayerName).Distinct())
            {
                file.Add(string.Concat(player, ";", string.Join(";", playerMechanics.Where(m => m.PlayerName == player)
                                                          //stupid but makes things easier with excel etc. interpreting the output regardless of which decimal sign your country uses
                                                          .Select(m => Math.Round(double.Parse(m.MechanicValue)).ToString()))));
            }

            var targetPath = Path.Combine(directory, "mechanicsSummary.csv");

            if (File.Exists(targetPath))
            {
                File.Delete(targetPath);
            }
            File.AppendAllLines(targetPath, file);
        }
    }
}
