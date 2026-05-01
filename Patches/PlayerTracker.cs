using HarmonyLib;
using GameNetcodeStuff;
using Unity.Netcode;

namespace StatsTracker.Patches;

[HarmonyPatch]
internal class PlayerTracker
{
  [HarmonyPatch(typeof(PlayerControllerB), nameof(PlayerControllerB.KillPlayerClientRpc))]
  [HarmonyPrefix]
  private static void TrackDeath(PlayerControllerB __instance, int causeOfDeath)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    StatsTracker.DayStats?.Players[__instance.playerSteamId]?
      .Kill(StatsTracker.GetCurrentTimeString(),
          ((CauseOfDeath)causeOfDeath).ToString());
  }

  [HarmonyPatch(typeof(StartOfRound), nameof(StartOfRound.OnPlayerDC))]
  [HarmonyPrefix]
  private static void TrackDisconnect(StartOfRound __instance, int playerObjectNumber)
  {
    StatsTracker.DayStats?.Players[__instance.allPlayerScripts[playerObjectNumber].playerSteamId].Disconnect();
  }
}
