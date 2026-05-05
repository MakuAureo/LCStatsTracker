using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch]
internal class ShipTracker
{
  [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.ShipLeave))]
  [HarmonyPostfix]
  private static void RegisterTakeOffTime(StartOfRound __instance)
  {
    StatsTracker.DayStats?.TakeOffTime = StatsTracker.GetCurrentTimeString();
  }
}
