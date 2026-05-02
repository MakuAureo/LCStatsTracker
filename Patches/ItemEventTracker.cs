using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace StatsTracker.Patches;

[HarmonyPatch]
internal class ItemEventTracker
{
  private static bool appSpawnedThisDay = false;

  [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.SyncScrapValuesClientRpc))]
  [HarmonyPrefix]
  private static void TrackSpawnedItemsAndDayEvents(RoundManager __instance, NetworkObjectReference[] spawnedScrap, int[] allScrapValue)
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

    bool isSid = true;
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
        isSid = false;
        break;
      }
    }

    int totalStartScrapValue = 0;
    foreach (int scrapValue in allScrapValue)
      totalStartScrapValue += scrapValue;

    StatsTracker.DayStats?.DungeonInfo = new(spawnedScrap.Length + (appSpawnedThisDay ? 1 : 0), StatsTracker.InteriorNames[__instance.currentDungeonType]);

    StatsTracker.DayStats?.AppSpawned = appSpawnedThisDay;
    StatsTracker.DayStats?.BottomLine += totalStartScrapValue;
    StatsTracker.DayStats?.BottomLineTrue += totalStartScrapValue + (appSpawnedThisDay ? 80 : 0);
    appSpawnedThisDay = false;

    StatsTracker.DayStats?.HazardInfo = new(HazardTracker.turretCount, HazardTracker.landmineCount, HazardTracker.spiketrapCount);
    HazardTracker.turretCount = HazardTracker.landmineCount = HazardTracker.spiketrapCount = 0;

    StatsTracker.DayStats?.IndoorFog = __instance.indoorFog.gameObject.activeSelf;
    if (isSid)
      StatsTracker.DayStats?.SIDType = first.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText;
    if (__instance.enemyRushIndex != -1)
      StatsTracker.DayStats?.InfestationType = __instance.currentLevel.Enemies[__instance.enemyRushIndex].enemyType.name;
  }

  [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
  [HarmonyPrefix]
  private static void TrackMissedItems(RoundManager __instance)
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

  [HarmonyPatch(typeof(LungProp), nameof(LungProp.Start))]
  [HarmonyPostfix]
  private static void CountApp(LungProp __instance)
  {
    appSpawnedThisDay = true;
  }

  // Prob gotta scrap all this in favor of an actual object tracker to make it easier to know all the edge cases cuz this solution lowk sucks
  [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.CollectNewScrapForThisRound))]
  [HarmonyPrefix]
  private static void TrackCollectedItem(RoundManager __instance, GrabbableObject scrapObject)
  {
    StatsTracker.DayStats?.CollectedNoExtra += scrapObject.scrapValue;
    StatsTracker.DayStats?.TotalCollected += (scrapObject is GiftBoxItem) ? ((GiftBoxItem)scrapObject).objectInPresentValue : scrapObject.scrapValue;
  }

  [HarmonyPatch(typeof(GiftBoxItem), nameof(GiftBoxItem.InitializeAfterPositioning))]
  [HarmonyPostfix]
  private static void TrackTrueBottomLineFromGiftBox(GiftBoxItem __instance)
  {
    StatsTracker.DayStats?.BottomLineTrue += __instance.objectInPresentValue - __instance.scrapValue;
  }

  [HarmonyPatch(typeof(GiftBoxItem), nameof(GiftBoxItem.OpenGiftBoxClientRpc))]
  [HarmonyPrefix]
  private static void TrackOpenGiftBox(GiftBoxItem __instance, int presentValue)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;
    
    if (__instance.isInShipRoom)
      return;

    StatsTracker.DayStats?.CollectedNoExtra -= presentValue;
    StatsTracker.DayStats?.TotalCollected -= presentValue;
  }
}
