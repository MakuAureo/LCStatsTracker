using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(LungProp))]
internal class LungPropPatches
{
  [HarmonyPatch(nameof(LungProp.Start))]
  [HarmonyPostfix]
  private static void PostStart(LungProp __instance)
  {
    StatsTracker.DayStats?.has_app = true;
    StatsTracker.DayStats?.moon_info.item_count += 1;
  }
}
