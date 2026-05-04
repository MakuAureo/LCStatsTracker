using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;
using UnityEngine;

namespace StatsTracker.Patches;

[HarmonyPatch]
internal class ItemAndEventTracker
{
  private static bool appSpawnedThisDay = false;
  private static HashSet<NetworkObjectReference> knivesSpawnedThisDay = new();
  private static HashSet<NetworkObjectReference> shotgunsSpawnedThisDay = new();
  private static HashSet<NetworkObjectReference> hivesSpawnedThisDay = new();
  private static HashSet<NetworkObjectReference> eggsSpawnedThisDay = new();
  private static HashSet<NetworkObjectReference> objectsNaturallySpawnedThisDay = new();
  private static Dictionary<NetworkObjectReference, int> valueFromGiftSpawner = new();
  private static HashSet<Vector3> butlerPopPositionsToTrack = new();

  [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.GenerateNewLevelClientRpc))]
  [HarmonyPrefix]
  private static void ResetTrackerWhenStartingNewDay(RoundManager __instance)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    appSpawnedThisDay = false;
    knivesSpawnedThisDay.Clear();
    shotgunsSpawnedThisDay.Clear();
    hivesSpawnedThisDay.Clear();
    eggsSpawnedThisDay.Clear();
    objectsNaturallySpawnedThisDay.Clear();
    valueFromGiftSpawner.Clear();
    butlerPopPositionsToTrack.Clear();
  }

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

    objectsNaturallySpawnedThisDay = new(spawnedScrap);

    int totalStartScrapValue = 0;
    foreach (int scrapValue in allScrapValue)
      totalStartScrapValue += scrapValue;

    StatsTracker.DayStats?.DungeonInfo = new(spawnedScrap.Length + (appSpawnedThisDay ? 1 : 0), StatsTracker.VanillaInteriorNames[__instance.currentDungeonType]);
    StatsTracker.DayStats?.AppSpawned = appSpawnedThisDay;

    StatsTracker.DayStats?.BottomLine += totalStartScrapValue;
    StatsTracker.DayStats?.BottomLineTrue += totalStartScrapValue;

    StatsTracker.DayStats?.HazardInfo = new(HazardTracker.turretCount, HazardTracker.landmineCount, HazardTracker.spiketrapCount);
    HazardTracker.turretCount = HazardTracker.landmineCount = HazardTracker.spiketrapCount = 0;

    StatsTracker.DayStats?.IndoorFog = __instance.indoorFog.gameObject.activeSelf;
    if (isSid)
      StatsTracker.DayStats?.SIDType = first.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText;
    if (__instance.enemyRushIndex != -1)
      StatsTracker.DayStats?.InfestationType = __instance.currentLevel.Enemies[__instance.enemyRushIndex].enemyType.name;
  }

  [HarmonyPatch(typeof(TimeOfDay), nameof(TimeOfDay.SetBeginMeteorShowerClientRpc))]
  [HarmonyPrefix]
  private static void TrackMeteorShower(TimeOfDay __instance)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    StatsTracker.DayStats?.MeteorShowerTime = StatsTracker.GetCurrentTimeString();
  }

  [HarmonyPatch(typeof(RoundManager), nameof(RoundManager.DespawnPropsAtEndOfRound))]
  [HarmonyPrefix]
  private static void TrackMissedItems(RoundManager __instance)
  {
    VehicleController cruiser = Object.FindAnyObjectByType<VehicleController>();

    GrabbableObject[] rawList = Object.FindObjectsByType<GrabbableObject>(FindObjectsSortMode.None);
    List<GrabbableObject> missedObjs = new(rawList);
    missedObjs.RemoveAll(obj =>
        !obj.itemProperties.isScrap ||
        obj.isInShipRoom ||
        cruiser != null && obj.transform.parent != null && obj.transform.parent.gameObject.GetComponent<VehicleController>() == cruiser && cruiser.magnetedToShip);

    StatsTracker.DayStats?.MissedItems = missedObjs
      .Select<GrabbableObject, Util.MissingItemInfo>
      (obj => new(obj.gameObject.GetComponentInChildren<ScanNodeProperties>() == null ? obj.itemProperties.name : obj.gameObject.GetComponentInChildren<ScanNodeProperties>().headerText, obj.scrapValue, obj.transform.position, obj.scrapPersistedThroughRounds))
      .ToList();
  }

  [HarmonyPatch(typeof(LungProp), nameof(LungProp.Start))]
  [HarmonyPostfix]
  private static void CountApp(LungProp __instance)
  {
    appSpawnedThisDay = true; 
    StatsTracker.DayStats?.BottomLineTrue += __instance.scrapValue;
  }

  [HarmonyPatch(typeof(GiftBoxItem), nameof(GiftBoxItem.InitializeAfterPositioning))]
  [HarmonyPostfix]
  private static void TrackTrueValueFromGiftBox(GiftBoxItem __instance)
  {
    StatsTracker.DayStats?.BottomLineTrue += __instance.objectInPresentValue - __instance.scrapValue;
  }

  [HarmonyPatch(typeof(RedLocustBees), nameof(RedLocustBees.SpawnHiveClientRpc))]
  [HarmonyPrefix]
  private static void TrackHiveItem(RedLocustBees __instance, NetworkObjectReference hiveObject)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;
    
    hivesSpawnedThisDay.Add(hiveObject);
  }

  [HarmonyPatch(typeof(GiantKiwiAI), nameof(GiantKiwiAI.SpawnEggsClientRpc))]
  [HarmonyPrefix]
  private static void TrackEggItems(GiantKiwiAI __instance, NetworkObjectReference[] eggNetworkReferences)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    foreach (NetworkObjectReference eggNetRef in eggNetworkReferences)
      eggsSpawnedThisDay.Add(eggNetRef);
  }

  [HarmonyPatch(typeof(NutcrackerEnemyAI), nameof(NutcrackerEnemyAI.InitializeNutcrackerValuesClientRpc))]
  [HarmonyPrefix]
  private static void TrackShogtun(NutcrackerEnemyAI __instance, NetworkObjectReference gunObject)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    shotgunsSpawnedThisDay.Add(gunObject);
  }

  [HarmonyPatch(typeof(ButlerEnemyAI), nameof(ButlerEnemyAI.KillEnemy))]
  [HarmonyPrefix]
  private static void TrackButlerPopPosition(ButlerEnemyAI __instance)
  {
    butlerPopPositionsToTrack.Add(__instance.transform.position);
  }

  [HarmonyPatch(typeof(GrabbableObject), nameof(GrabbableObject.Start))]
  [HarmonyPrefix]
  private static void TrackKnife(GrabbableObject __instance)
  {
    if (__instance is not KnifeItem knife) 
      return;

    Vector3 pos = butlerPopPositionsToTrack.FirstOrDefault(p => Vector3.Distance(p + Vector3.up * 0.5f, knife.transform.position) < 0.1f);
    if (pos != Vector3.zero)
    {
      knivesSpawnedThisDay.Add(__instance.NetworkObject);
      butlerPopPositionsToTrack.Remove(pos);
    }
  }

  [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.SetItemInElevator))]
  [HarmonyPrefix]
  private static void TrackItemWhenDropped(PlayerControllerB __instance, bool droppedInShipRoom, GrabbableObject gObject)
  {
    if (!gObject.IsSpawned || StartOfRound.Instance.inShipPhase || gObject.isInShipRoom == droppedInShipRoom || gObject.scrapPersistedThroughRounds)
      return;

    if (droppedInShipRoom)
    {
      if (objectsNaturallySpawnedThisDay.Contains(gObject.NetworkObject))
      {
        StatsTracker.DayStats?.CollectedNoExtra += gObject.scrapValue;
        StatsTracker.DayStats?.CollectedTotal += gObject is GiftBoxItem ? ((GiftBoxItem)gObject).objectInPresentValue : gObject.scrapValue;
      }
      else if (valueFromGiftSpawner.TryGetValue(gObject.NetworkObject, out int parentGiftValue))
      {
        StatsTracker.DayStats?.CollectedNoExtra += parentGiftValue;
        StatsTracker.DayStats?.CollectedTotal += gObject.scrapValue;
      }
      else if (shotgunsSpawnedThisDay.Contains(gObject.NetworkObject))
      {
        StatsTracker.DayStats?.CollectedTotal += gObject.scrapValue;
        StatsTracker.DayStats?.ShotgunsCollected += 1;
      }
      else if (knivesSpawnedThisDay.Contains(gObject.NetworkObject))
      {
        StatsTracker.DayStats?.CollectedTotal += gObject.scrapValue;
        StatsTracker.DayStats?.KnivesCollected += 1;
      }
      else if (hivesSpawnedThisDay.Contains(gObject.NetworkObject) || eggsSpawnedThisDay.Contains(gObject.NetworkObject))
      {
        StatsTracker.DayStats?.CollectedTotal += gObject.scrapValue;
      }
    }
    else
    {
      if (objectsNaturallySpawnedThisDay.Contains(gObject.NetworkObject))
      {
        StatsTracker.DayStats?.CollectedNoExtra -= gObject.scrapValue;
        StatsTracker.DayStats?.CollectedTotal -= gObject is GiftBoxItem ? ((GiftBoxItem)gObject).objectInPresentValue : gObject.scrapValue;
      }
      else if (valueFromGiftSpawner.TryGetValue(gObject.NetworkObject, out int parentGiftValue))
      {
        StatsTracker.DayStats?.CollectedNoExtra -= parentGiftValue;
        StatsTracker.DayStats?.CollectedTotal -= gObject.scrapValue;
      }
      else if (shotgunsSpawnedThisDay.Contains(gObject.NetworkObject))
      {
        StatsTracker.DayStats?.CollectedTotal -= gObject.scrapValue;
        StatsTracker.DayStats?.ShotgunsCollected -= 1;
      }
      else if (knivesSpawnedThisDay.Contains(gObject.NetworkObject))
      {
        StatsTracker.DayStats?.CollectedTotal -= gObject.scrapValue;
        StatsTracker.DayStats?.KnivesCollected -= 1;
      }
      else if (hivesSpawnedThisDay.Contains(gObject.NetworkObject) || eggsSpawnedThisDay.Contains(gObject.NetworkObject))
      {
        StatsTracker.DayStats?.CollectedTotal -= gObject.scrapValue;
      }
    }
  }

  [HarmonyPatch(typeof(GiftBoxItem), nameof(GiftBoxItem.OpenGiftBoxClientRpc))]
  [HarmonyPrefix]
  private static void AddNewlySpawnedGiftItemToItemTracker(GiftBoxItem __instance, NetworkObjectReference netObjectRef)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    if (StartOfRound.Instance.inShipPhase)
      return;

    // Using StartOfRound to make sure the coroutine doesn't get interrupted early if the gift instance is destroyed somehow
    StartOfRound.Instance.StartCoroutine(WaitForGiftItemToFullySpawnBeforeTracking(netObjectRef, __instance.scrapValue));
  }

  private static IEnumerator WaitForGiftItemToFullySpawnBeforeTracking(NetworkObjectReference netObjRef, int giftScrapValue)
  {
    NetworkObject netObject = null!;
		float startTime = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup - startTime < 8f && !netObjRef.TryGet(out netObject))
		{
			yield return new WaitForSeconds(0.03f);
		}
		if (netObject == null)
		{
			StatsTracker.Logger.LogWarning("No network object found for giftbox");
			yield break;
		}

    // Make sure the items were already set to Elevator before tracking (this isn't guaranteed to wait for long enough, but like yk)
    yield return new WaitForSeconds(0.3f);

    valueFromGiftSpawner[netObjRef] = giftScrapValue;
  }
}
