using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace LogMechanicParser
{
    public class LogMechanic
    {
        public string MechanicName { get; set; }
        public string AffectedActor { get; set; }
        public int Time { get; set; }

        public static List<LogMechanic> GetLogMechanics(JsonDocument jsonDocument)
        {
            var logMechanics = new List<LogMechanic>();

            var mechanicsNode = jsonDocument.RootElement.GetProperty("mechanics");
            foreach (var mechanicType in mechanicsNode.EnumerateArray())
            {
                foreach (var mechanicHit in mechanicType.GetProperty("mechanicsData").EnumerateArray())
                {
                    logMechanics.Add(new LogMechanic()
                    {
                        AffectedActor = mechanicHit.GetProperty("actor").GetString(),
                        MechanicName = mechanicType.GetProperty("name").GetString(),
                        Time = mechanicHit.GetProperty("time").GetInt32(),
                    });
                }
            }

            return logMechanics;
        }
    }
}
