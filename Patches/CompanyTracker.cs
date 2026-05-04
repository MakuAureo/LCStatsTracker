using HarmonyLib;

namespace StatsTracker.Patches;

[HarmonyPatch]
internal class CompanyTracker
{
  // Despite the name this run on every client, just early returns on client
  [HarmonyPatch(typeof(DepositItemsDesk), nameof(DepositItemsDesk.SellItemsOnServer))]
  [HarmonyPrefix]
  private static void CalculateAmountSold(DepositItemsDesk __instance)
  {
    for (int i = 0; i < __instance.itemsOnCounter.Count; i++)
    {
      if (!__instance.itemsOnCounter[i].itemProperties.isScrap)
        continue;
      else
        StatsTracker.DayStats?.ValueSold += __instance.itemsOnCounter[i].scrapValue;
    }
  }
}
