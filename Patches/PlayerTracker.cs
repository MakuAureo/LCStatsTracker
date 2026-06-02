using System.Collections.Generic;
using GameNetcodeStuff;
using HarmonyLib;
using Unity.Netcode;

namespace StatsTracker.Patches;

internal class PlayerTracker
{
  private static readonly Dictionary<int, ulong> allConnectedPlayersIDToSteamIDs = [];

  public static void ApplyPlayerTrackerPatches(Harmony Harmony)
  {
    Harmony.Patch(AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.ResetPlayersLoadedValueClientRpc)), prefix: new HarmonyMethod(typeof(PlayerTracker), nameof(RegisterAllConnectedPlayers)));
    Harmony.Patch(AccessTools.Method(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc)), prefix: new HarmonyMethod(typeof(PlayerTracker), nameof(TrackDeath)));
    Harmony.Patch(AccessTools.Method(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC)), prefix: new HarmonyMethod(typeof(PlayerTracker), nameof(TrackDisconnect)));
  }

  private static void RegisterAllConnectedPlayers(StartOfRound __instance)
  {
    if ((GameNetworkManager.Instance.gameVersionNum > 72 && __instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute) || (GameNetworkManager.Instance.gameVersionNum <= 72 && __instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client))
      return;

    allConnectedPlayersIDToSteamIDs.Clear();

    for (int i = 0; i < __instance.allPlayerScripts.Length; i++)
    {
      PlayerControllerB? playerScript = __instance.allPlayerScripts[i];
      if (playerScript == null) continue;

      allConnectedPlayersIDToSteamIDs.Add(i, playerScript.playerSteamId);
    }
  }

  private static void TrackDeath(PlayerControllerB __instance, int causeOfDeath)
  {
    if ((GameNetworkManager.Instance.gameVersionNum > 72 && __instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute) || (GameNetworkManager.Instance.gameVersionNum <= 72 && __instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client))
      return;

    StatsTracker.DayStats!.Players[__instance.playerSteamId]!
      .Kill(StatsTracker.GetCurrentTimeString(),
          ((CauseOfDeath)causeOfDeath).ToString());
  }

  private static void TrackDisconnect(StartOfRound __instance, int playerObjectNumber)
  {
    if (!StatsTracker.dayHasStarted) return;

    StatsTracker.DayStats!.Players[allConnectedPlayersIDToSteamIDs[playerObjectNumber]].Disconnect();
  }
}
