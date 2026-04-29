using HarmonyLib;
using Newtonsoft.Json;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatches 
{
  [HarmonyPatch(nameof(StartOfRound.PassTimeToNextDay))]
  [HarmonyPostfix]
  private static void PostPassTimeToNextDay()
  {
    StatsTracker.LocalServer.PublishStats(JsonConvert.SerializeObject(StatsTracker.DayStats));
  }
}
