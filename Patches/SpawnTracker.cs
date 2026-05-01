using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch(typeof(EnemyAI))]
internal class SpawnTracker
{
  [HarmonyPatch(nameof(EnemyAI.Start))]
  [HarmonyPostfix]
  private static void TrackSpawn(EnemyAI __instance)
  {
    if (__instance.enemyType.isOutsideEnemy)
      StatsTracker.DayStats?.NightTimeSpawns.Add(new(__instance.enemyType, HUDManager.Instance.GetClockTimeFormatted(TimeOfDay.Instance.normalizedTimeOfDay, TimeOfDay.Instance.numberOfHours, false)));
    else if (__instance.enemyType.isDaytimeEnemy)
      StatsTracker.DayStats?.DayTimeSpawns.Add(new(__instance.enemyType, HUDManager.Instance.GetClockTimeFormatted(TimeOfDay.Instance.normalizedTimeOfDay, TimeOfDay.Instance.numberOfHours, false)));
    else
      StatsTracker.DayStats?.IndoorSpawns.Add(new(__instance.enemyType, HUDManager.Instance.GetClockTimeFormatted(TimeOfDay.Instance.normalizedTimeOfDay, TimeOfDay.Instance.numberOfHours, false)));

    if (__instance is GiantKiwiAI)
      StatsTracker.DayStats?.BirdInfo = new();
    else if (__instance is RedLocustBees)
    {
      if (StatsTracker.DayStats?.BeeInfo == null)
        StatsTracker.DayStats?.BeeInfo = new();

      StatsTracker.DayStats?.BeeInfo!.AddBeeValue(((RedLocustBees)__instance).hive.scrapValue);
    }
  }
}
