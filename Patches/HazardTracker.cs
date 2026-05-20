using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using Unity.Netcode;

namespace StatsTracker.Patches;

internal class HazardTracker
{
  public static int turretCount = 0;
  public static int landmineCount = 0;
  public static int spiketrapCount = 0;

  public static void ApplyHazardTrakcerPatches(Harmony Harmony)
  {
    Harmony.Patch(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.GenerateNewLevelClientRpc)), prefix: new HarmonyMethod(typeof(HazardTracker), nameof(ResetHazardTrackerWhenStartingNewDay)));

    Harmony.Patch(AccessTools.Method(typeof(Landmine), nameof(Landmine.Start)), postfix: new HarmonyMethod(typeof(Patches.HazardTracker), nameof(Patches.HazardTracker.CountLandmine)));
    Harmony.Patch(AccessTools.Method(typeof(Turret), nameof(Turret.Start)), postfix: new HarmonyMethod(typeof(Patches.HazardTracker), nameof(Patches.HazardTracker.CountTurret)));
    Type? SpikeRoofTrapType = AccessTools.TypeByName(nameof(SpikeRoofTrap));
    if (SpikeRoofTrapType != null)
      SpikeRoofTrapTypePath();

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    void SpikeRoofTrapTypePath() => Harmony.Patch(AccessTools.Method(SpikeRoofTrapType, nameof(SpikeRoofTrap.Start)), postfix: new HarmonyMethod(typeof(Patches.HazardTracker), nameof(Patches.HazardTracker.CountSpiketrap)));
  }

  private static void ResetHazardTrackerWhenStartingNewDay(RoundManager __instance)
  {
    if ((GameNetworkManager.Instance.gameVersionNum > 72 && __instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute) || (GameNetworkManager.Instance.gameVersionNum <= 72 && __instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client))
      return;

    HazardTracker.turretCount = HazardTracker.landmineCount = HazardTracker.spiketrapCount = 0;
  }

  private static void CountLandmine(Landmine __instance)
  {
    landmineCount++;
  }

  private static void CountTurret(Turret __instance)
  {
    turretCount++;
  }

  private static void CountSpiketrap(SpikeRoofTrap __instance)
  {
    spiketrapCount++;
  }
}
