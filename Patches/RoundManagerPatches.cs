using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(RoundManager))]
internal class RoundManagerPatches
{
  [HarmonyPatch(nameof(RoundManager.SyncScrapValuesClientRpc))]
  [HarmonyPrefix]
  private static void PreSyncScrapValuesClientRpc(RoundManager __instance, NetworkObjectReference[] spawnedScrap, int[] allScrapValue)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    spawnedScrap[0].TryGet(out var firstNetObj);
    GrabbableObject first = firstNetObj.GetComponent<GrabbableObject>();
    if (first == null) 
    {
      StatsTracker.Logger.LogWarning("Unable to retrieve first GrabbableObject from the spawned objects");
      return;
    }

    bool is_sid = true;
    foreach (NetworkObjectReference netObjRef in spawnedScrap)
    {
      netObjRef.TryGet(out var netObj);
      GrabbableObject component = netObj.GetComponent<GrabbableObject>();
      if (component == null)
      {
        StatsTracker.Logger.LogWarning("Unable to retrieve some GrabbableObject from the spawned objects");
        return;
      }

      if (component.itemProperties.name != first.itemProperties.name)
      {
        is_sid = false;
        break;
      }
    }

    int totalStartScrapValue = 0;
    foreach (int scrapValue in allScrapValue)
      totalStartScrapValue += scrapValue;

    StatsTracker.DayStats?.DungeonInfo = new(spawnedScrap.Length + (LungPropPatches.AppSpawnedThisDay ? 1 : 0), StatsTracker.InteriorNames[__instance.currentDungeonType]);

    StatsTracker.DayStats?.AppSpawned = LungPropPatches.AppSpawnedThisDay;
    StatsTracker.DayStats?.BottomLine = totalStartScrapValue;
    StatsTracker.DayStats?.BottomLineTrue = totalStartScrapValue + (LungPropPatches.AppSpawnedThisDay ? 80 : 0);
    LungPropPatches.AppSpawnedThisDay = false;

    StatsTracker.DayStats?.HazardInfo = new(TurretPatches.turretCount, LandminePatches.landmineCount, SpikeRoofTrapPatches.spiketrapCount);
    TurretPatches.turretCount = LandminePatches.landmineCount = SpikeRoofTrapPatches.spiketrapCount = 0;

    StatsTracker.DayStats?.SIDType = is_sid ? first.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText : "";
    StatsTracker.DayStats?.IndoorFog = __instance.indoorFog.gameObject.activeSelf;
    StatsTracker.DayStats?.InfestationType = __instance.enemyRushIndex != -1 ? __instance.currentLevel.Enemies[__instance.enemyRushIndex].enemyType.name : "";
  }

  [HarmonyPatch(nameof(RoundManager.DespawnPropsAtEndOfRound))]
  [HarmonyPrefix]
  private static void PreDespawnPropsAtEndOfRound(RoundManager __instance)
  {
    VehicleController cruiser = Object.FindAnyObjectByType<VehicleController>();

    List<GrabbableObject> missedObjs = new(Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None));
    missedObjs.RemoveAll(obj => 
        !obj.itemProperties.isScrap ||
        obj.isInShipRoom ||
        cruiser != null && obj.transform.parent != null && obj.transform.parent.gameObject.GetComponent<VehicleController>() == cruiser && cruiser.magnetedToShip);

    StatsTracker.DayStats?.MissedItems = missedObjs
      .Select<GrabbableObject, Util.MissingItemInfo>
      (obj => new(obj.gameObject.GetComponentInChildren<ScanNodeProperties>() == null ? obj.itemProperties.name : obj.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText, obj.scrapValue, obj.transform.position))
      .ToList();
  }
}
