using HarmonyLib;
using GameNetcodeStuff;
using Unity.Netcode;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(PlayerControllerB))]
internal class PlayerControllerBPatches
{
  [HarmonyPatch(nameof(PlayerControllerB.KillPlayerClientRpc))]
  [HarmonyPrefix]
  private static void PreKillPlayerClientRpc(PlayerControllerB __instance, int causeOfDeath)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    StatsTracker.DayStats?.Players[__instance.playerSteamId]?
      .Kill(HUDManager.Instance.GetClockTimeFormatted(TimeOfDay.Instance.normalizedTimeOfDay, TimeOfDay.Instance.numberOfHours, false),
          ((CauseOfDeath)causeOfDeath).ToString());
  }
}
