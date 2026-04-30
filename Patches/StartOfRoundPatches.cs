using System;
using System.Collections.Generic;
using HarmonyLib;
using Newtonsoft.Json;
using Unity.Netcode;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatches 
{
  [HarmonyPatch(nameof(StartOfRound.ResetPlayersLoadedValueClientRpc))]
  [HarmonyPostfix]
  private static void PostResetPlayersLoadedValueClientRpc(StartOfRound __instance)
  {
    if (__instance.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Execute)
      return;

    StatsTracker.LocalServer.Reset();
    StatsTracker.DayStats = new(__instance.randomMapSeed, __instance.currentLevel.PlanetName,
        __instance.currentLevel.currentWeather == LevelWeatherType.None ? "Mild" : __instance.currentLevel.currentWeather.ToString(),
        new List<GameNetcodeStuff.PlayerControllerB>(new ArraySegment<GameNetcodeStuff.PlayerControllerB>(__instance.allPlayerScripts, 0, __instance.connectedPlayersAmount + 1)).ConvertAll(pcb => pcb.playerSteamId).ToArray());
  }

  [HarmonyPatch(nameof(StartOfRound.PassTimeToNextDay))]
  [HarmonyPostfix]
  private static void PostPassTimeToNextDay()
  {
    StatsTracker.LocalServer.PublishStats(JsonConvert.SerializeObject(StatsTracker.DayStats));
  }

}
