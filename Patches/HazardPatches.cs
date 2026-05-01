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

[HarmonyPatch(typeof(Turret))]
internal class TurretPatches
{
  public static int turretCount = 0;

  [HarmonyPatch(nameof(Turret.Start))]
  [HarmonyPostfix]
  private static void PostStart(Turret __instance)
  {
    turretCount++;
  }
}
