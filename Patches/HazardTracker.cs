using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch]
internal class HazardTracker
{
  public static int turretCount = 0;
  public static int landmineCount = 0;
  public static int spiketrapCount = 0;

  [HarmonyPatch(typeof(Landmine))]
  [HarmonyPatch(nameof(Landmine.Start))]
  [HarmonyPostfix]
  private static void CountLandmine(Landmine __instance)
  {
    landmineCount++;
  }

  [HarmonyPatch(typeof(Turret))]
  [HarmonyPatch(nameof(Turret.Start))]
  [HarmonyPostfix]
  private static void CountTurret(Turret __instance)
  {
    turretCount++;
  }

  [HarmonyPatch(typeof(SpikeRoofTrap), nameof(SpikeRoofTrap.Start))]
  [HarmonyPostfix]
  private static void CountSpiketrap(SpikeRoofTrap __instance)
  {
    spiketrapCount++;
  }
}
