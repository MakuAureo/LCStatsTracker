using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(LungProp))]
internal class LungPropPatches
{
  public static bool AppSpawnedThisDay = false;

  [HarmonyPatch(nameof(LungProp.Start))]
  [HarmonyPostfix]
  private static void PostStart(LungProp __instance)
  {
    AppSpawnedThisDay = true;
  }
}
