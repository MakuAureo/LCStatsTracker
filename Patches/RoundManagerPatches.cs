using HarmonyLib;
using Unity.Netcode;

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

    StatsTracker.DayStats?.DungeonInfo = new(spawnedScrap.Length + (LungPropPatches.AppSpawnedThisDay ? 1 : 0), StatsTracker.InteriorNames[__instance.currentDungeonType]);
    StatsTracker.DayStats?.AppSpawned = LungPropPatches.AppSpawnedThisDay;
    StatsTracker.DayStats?.SIDType = is_sid ? first.itemProperties.name : "";
    StatsTracker.DayStats?.IndoorFog = __instance.indoorFog.gameObject.activeSelf;
    StatsTracker.DayStats?.InfestationType = __instance.enemyRushIndex != -1 ? __instance.currentLevel.Enemies[__instance.enemyRushIndex].enemyType.name : "";
    LungPropPatches.AppSpawnedThisDay = false;
  }
}
