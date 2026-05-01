using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(SpikeRoofTrap))]
internal class SpikeRoofTrapPatches
{
  public static int spiketrapCount = 0;

  [HarmonyPatch(nameof(SpikeRoofTrap.Start))]
  [HarmonyPostfix]
  private static void PostStart(SpikeRoofTrap __instance)
  {
    spiketrapCount++;
  }
}

