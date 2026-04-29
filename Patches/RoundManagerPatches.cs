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

    StatsTracker.LocalServer.Reset();
    StatsTracker.DayStats = new(
        __instance.playersManager.randomMapSeed,
        __instance.currentLevel.PlanetName,
        __instance.currentLevel.currentWeather == LevelWeatherType.None ? "Mild" : (__instance.currentLevel.currentWeather.ToString() ?? "Null?"),
        spawnedScrap.Length,
        __instance.indoorFog.gameObject.activeSelf,
        StatsTracker.InteriorNames[__instance.currentDungeonType],
        is_sid ? first.itemProperties.name : null,
        __instance.enemyRushIndex != -1 ? __instance.currentLevel.Enemies[__instance.enemyRushIndex].enemyType.name : null);
  }
}
