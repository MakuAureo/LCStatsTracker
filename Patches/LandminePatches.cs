using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(Landmine))]
internal class LandminePatches
{
  public static int landmineCount = 0;

  [HarmonyPatch(nameof(Landmine.Start))]
  [HarmonyPostfix]
  private static void PostStart(Landmine __instance)
  {
    landmineCount++;
  }
}
