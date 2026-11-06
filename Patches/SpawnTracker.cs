using System.Collections.Generic;
using HarmonyLib;
using Unity.Netcode;

namespace StatsTracker.Patches;

internal class SpawnTracker
{
    public static void ApplySpawnTrackerPatches(Harmony Harmony)
    {
        Harmony.Patch(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.GenerateNewLevelClientRpc)), prefix: new HarmonyMethod(typeof(SpawnTracker), nameof(ResetSpawnTrackerWhenStartingNewDay)));
        Harmony.Patch(AccessTools.Method(typeof(NetworkBehaviour), nameof(NetworkBehaviour.OnNetworkSpawn)), postfix: new HarmonyMethod(typeof(SpawnTracker), nameof(TrackSpawn)));
        Harmony.Patch(AccessTools.Method(typeof(EnemyAI), nameof(EnemyAI.KillEnemy)), prefix: new HarmonyMethod(typeof(SpawnTracker), nameof(TrackDeath)));
        Harmony.Patch(AccessTools.Method(typeof(EnemyAI), nameof(EnemyAI.OnDestroy)), prefix: new HarmonyMethod(typeof(SpawnTracker), nameof(TrackDespawn)));
    }

    private static readonly Dictionary<EnemyAI, Util.SpawnInfo> EnemySpawns = [];

    private static void ResetSpawnTrackerWhenStartingNewDay(RoundManager __instance)
    {
        if ((GameNetworkManager.Instance.gameVersionNum > 72 && __instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute) || (GameNetworkManager.Instance.gameVersionNum <= 72 && __instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client))
            return;

        EnemySpawns.Clear();
    }

    // CURRENT ISSUE: WILL NOT TRACK MODDED SPAWNS CORRECTLY (PROB NEED STARLANCER_AI_FIX DEPEDENCY)
    private static void TrackSpawn(NetworkBehaviour __instance)
    {
        if (__instance is not EnemyAI enemy || StatsTracker.DayStats == null)
            return;

        Util.SpawnInfo spawn = new(enemy.NetworkObjectId, enemy.enemyType, StatsTracker.GetCurrentTimeString(), enemy.transform.position);
        EnemySpawns[enemy] = spawn;

        if (enemy.enemyType.isDaytimeEnemy)
            StatsTracker.DayStats!.DayTimeSpawns.Add(spawn);
        else if (enemy.enemyType.isOutsideEnemy)
            StatsTracker.DayStats!.NightTimeSpawns.Add(spawn);
        else
            StatsTracker.DayStats!.IndoorSpawns.Add(spawn);
    }

    private static void TrackDeath(EnemyAI __instance)
    {
        if (EnemySpawns.TryGetValue(__instance, out Util.SpawnInfo spawn) == false)
        {
            StatsTracker.Logger.LogWarning($"Enemy killed doesn't have spawn info: {__instance.enemyType.name}");
            return;
        }

        spawn.TimeOfDeath = StatsTracker.GetCurrentTimeString();
        spawn.EndPosition = Util.DungeonMapUtil.ToArray(__instance.transform.position);
    }

    private static void TrackDespawn(EnemyAI __instance)
    {
        if (EnemySpawns.TryGetValue(__instance, out Util.SpawnInfo spawn) && spawn.EndPosition == null)
            spawn.EndPosition = Util.DungeonMapUtil.ToArray(__instance.transform.position);
    }
}
