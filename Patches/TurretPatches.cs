using HarmonyLib;

namespace StatsTracker.Patches;

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

