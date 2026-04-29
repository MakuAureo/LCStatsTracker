using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(LungProp))]
internal class LungPropPatches
{
  [HarmonyPatch(nameof(LungProp.Start))]
  [HarmonyPostfix]
  private static void PostStart(LungProp __instance)
  {
    StatsTracker.DayStats?.AppSpawned = true;
    StatsTracker.DayStats?.DungeonInfo.ItemCount += 1;
  }
}
