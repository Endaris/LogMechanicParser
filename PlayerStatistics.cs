using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace LogMechanicParser
{
    class PlayerStatistics
    {
        public string PlayerName { get; set; }
        public Dictionary<string, string> Statistics { get; set; }

        public void UpsertValue(string key, string value) 
        { 
            if (Statistics.ContainsKey(key))
            {
                // should have done this properly with generics but it's just too much effort and for now double works
                Statistics[key] = (double.Parse(Statistics[key]) + double.Parse(value)).ToString();
            }
            else
            {
                Statistics.Add(key, value);
            }
        }

        public static List<PlayerStatistics> GetPlayerStatistics(JsonDocument jsonDocument)
        {
            var playerStatistics = new List<PlayerStatistics>();

            var playersNode = jsonDocument.RootElement.GetProperty("players");

            foreach(var player in playersNode.EnumerateArray())
            {
                var supportNode = player.GetProperty("support")[0];
                var defenseNode = player.GetProperty("defenses")[0];

                var newPlayer = new PlayerStatistics()
                {
                    PlayerName = player.GetProperty("name").GetString(),
                    Statistics = new Dictionary<string, string>(),   
                };       

                newPlayer.Statistics.Add("BoonStrips", supportNode.GetProperty("boonStrips").GetInt32().ToString());
                newPlayer.Statistics.Add("CondiCleanseSelf", supportNode.GetProperty("condiCleanseSelf").GetInt32().ToString());
                newPlayer.Statistics.Add("CondiCleanseOthers", supportNode.GetProperty("condiCleanse").GetInt32().ToString());

                newPlayer.Statistics.Add("ResurrectionTimeInSeconds", supportNode.GetProperty("resurrectTime").GetDouble().ToString());

                newPlayer.Statistics.Add("DodgeCount", defenseNode.GetProperty("dodgeCount").GetInt32().ToString());
                newPlayer.Statistics.Add("EvadedCount", defenseNode.GetProperty("evadedCount").GetInt32().ToString());
                newPlayer.Statistics.Add("BlockedCount", defenseNode.GetProperty("blockedCount").GetInt32().ToString());
                newPlayer.Statistics.Add("InvulnCount", defenseNode.GetProperty("invulnedCount").GetInt32().ToString());

                newPlayer.Statistics.Add("DamageTaken", defenseNode.GetProperty("damageTaken").GetInt32().ToString());
                newPlayer.Statistics.Add("DamageTakenByBarrier", defenseNode.GetProperty("damageBarrier").GetInt32().ToString());

                // not present when the fight had no breakbars
                if (player.TryGetProperty("breakbarDamage1S", out JsonElement breakbarNode))
                {
                    var breakbarArray = breakbarNode[0];
                    newPlayer.Statistics.Add("BreakbarDamageDealt", breakbarArray[breakbarArray.GetArrayLength() - 1].GetDouble().ToString());
                }

                playerStatistics.Add(newPlayer);
            }

            return playerStatistics;
        }

        public static List<PlayerStatistics> CombineByName(List<PlayerStatistics> playerStatistics)
        {
            List<PlayerStatistics> combinedStatistics = new List<PlayerStatistics>();
            foreach(var playerName in playerStatistics.Select(p => p.PlayerName).Distinct())
            {
                var combinedPlayerStatistic = new PlayerStatistics() { PlayerName = playerName, Statistics = new Dictionary<string, string>() };

                foreach(var statistic in playerStatistics.Where(p => p.PlayerName == playerName))
                {
                    foreach(var key in statistic.Statistics.Keys)
                    {
                        combinedPlayerStatistic.UpsertValue(key, statistic.Statistics[key]);
                    }
                }

                combinedStatistics.Add(combinedPlayerStatistic);
            }

            return combinedStatistics;
        }
    }
}
