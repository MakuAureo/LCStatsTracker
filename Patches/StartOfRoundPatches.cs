using HarmonyLib;
using Newtonsoft.Json;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(StartOfRound))]
internal class StartOfRoundPatches 
{
  [HarmonyPatch(nameof(StartOfRound.StartGame))]
  [HarmonyPostfix]
  private static void PostStartGame(StartOfRound __instance)
  {
    StatsTracker.LocalServer.Reset();
    StatsTracker.DayStats = new(__instance.randomMapSeed, __instance.currentLevel.PlanetName,
        __instance.currentLevel.currentWeather == LevelWeatherType.None ? "Mild" : __instance.currentLevel.currentWeather.ToString());
  }

  [HarmonyPatch(nameof(StartOfRound.EndOfGameClientRpc))]
  [HarmonyPrefix]
  private static void EndOfGameClientRpc(StartOfRound __instance, int bodiesInsured, int daysPlayersSurvived, int connectedPlayersOnServer, int scrapCollectedOnServer)
  {
    StatsTracker.DayStats?.topline = scrapCollectedOnServer;
  }

  [HarmonyPatch(nameof(StartOfRound.PassTimeToNextDay))]
  [HarmonyPostfix]
  private static void PostPassTimeToNextDay()
  {
    StatsTracker.LocalServer.PublishStats(JsonConvert.SerializeObject(StatsTracker.DayStats));
  }

}
