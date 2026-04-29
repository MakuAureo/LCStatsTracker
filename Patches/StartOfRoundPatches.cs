using HarmonyLib;
using System.Text.Json;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatches 
{
  [HarmonyPatch(nameof(StartOfRound.PassTimeToNextDay))]
  [HarmonyPostfix]
  private static void PostPassTimeToNextDay()
  {
    StatsTracker.LocalServer.PublishStats(JsonSerializer.Serialize(StatsTracker.DayStats));
  }
}
