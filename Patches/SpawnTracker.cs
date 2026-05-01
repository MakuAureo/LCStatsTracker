using HarmonyLib;
using Unity.Netcode;

namespace StatsTracker.Patches;

[HarmonyPatch]
internal class SpawnTracker
{
  [HarmonyPatch(typeof(EnemyAI), nameof(EnemyAI.Start))]
  [HarmonyPostfix]
  private static void TrackSpawn(EnemyAI __instance)
  {
    if (__instance.enemyType.isDaytimeEnemy)
      StatsTracker.DayStats?.DayTimeSpawns.Add(new(__instance.enemyType, StatsTracker.GetCurrentTimeString()));
    else if (__instance.enemyType.isOutsideEnemy)
      StatsTracker.DayStats?.NightTimeSpawns.Add(new(__instance.enemyType, StatsTracker.GetCurrentTimeString()));
    else
      StatsTracker.DayStats?.IndoorSpawns.Add(new(__instance.enemyType, StatsTracker.GetCurrentTimeString()));
  }

  [HarmonyPatch(typeof(RedLocustBees), nameof(RedLocustBees.SpawnHiveClientRpc))]
  [HarmonyPrefix]
  private static void TrackHive(RedLocustBees __instance, int hiveScrapValue)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    StatsTracker.DayStats?.BeeInfo.AddBeeValue(hiveScrapValue);
    StatsTracker.DayStats?.BottomLineTrue += hiveScrapValue;
  }

  [HarmonyPatch(typeof(GiantKiwiAI), nameof(GiantKiwiAI.SpawnEggsClientRpc))]
  [HarmonyPrefix]
  private static void TrackEggs(GiantKiwiAI __instance, int[] eggScrapValues)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    StatsTracker.DayStats?.BirdInfo.AddEggValue(eggScrapValues);
    foreach (int eggValue in eggScrapValues)
      StatsTracker.DayStats?.BottomLineTrue += eggValue;
  }
}
