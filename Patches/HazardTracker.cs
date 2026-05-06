using System;
using System.Reflection;
using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch]
internal class HazardTracker
{
  public static int turretCount = 0;
  public static int landmineCount = 0;
  public static int spiketrapCount = 0;

  [HarmonyPatch(typeof(Landmine), nameof(Landmine.Start))]
  [HarmonyPostfix]
  private static void CountLandmine(Landmine __instance)
  {
    landmineCount++;
  }

  [HarmonyPatch(typeof(Turret), nameof(Turret.Start))]
  [HarmonyPostfix]
  private static void CountTurret(Turret __instance)
  {
    turretCount++;
  }

  [HarmonyPatch]
  private static class CountSpiketrap
  {
    private static Type? SpikeRookTrapType = null;
    private static bool Prepare()
    {
      SpikeRookTrapType = AccessTools.TypeByName(nameof(SpikeRoofTrap));
      return (SpikeRookTrapType != null);
    }
    private static MethodBase TargetMethod() => AccessTools.Method(SpikeRookTrapType, nameof(SpikeRoofTrap.Start));
    private static void Postfix(object __instance) { spiketrapCount++; }
  }
}
