using System.Runtime.CompilerServices;
using HarmonyLib;

namespace StatsTracker.Patches;

internal class ShopTracker
{
  public static void ApplyShopTrackerPatches(Harmony Harmony)
  {
    Harmony.Patch(AccessTools.Method(typeof(Terminal), nameof(Terminal.SetItemSales)), postfix: new HarmonyMethod(typeof(ShopTracker), nameof(ShopTracker.TrackCurrentSales)));
    Harmony.Patch(AccessTools.Method(typeof(Terminal), nameof(Terminal.SetItemSales)), postfix: new HarmonyMethod(typeof(ShopTracker), nameof(ShopTracker.TrackCurrentFurniture)));
  }

  private static void TrackCurrentSales(Terminal __instance)
  {
    if (StatsTracker.DayStats == null) return;

    int i = 0;
    foreach (Item item in __instance.buyableItemsList) StatsTracker.DayStats!.ShopSales[item.itemName] = 100 - __instance.itemSalesPercentages[i++];

    if (StatsTracker.BuyableVehicleType != null) vehicleControllerPath();
    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    void vehicleControllerPath()
    {
      foreach (var vehicle in Traverse.Create(__instance).Field(nameof(Terminal.buyableVehicles)).GetValue<System.Collections.IEnumerable>())
        StatsTracker.DayStats!.ShopSales[Traverse.Create(vehicle).Field(nameof(BuyableVehicle.vehicleDisplayName)).GetValue<string>()] = 100 - __instance.itemSalesPercentages[i++];
    }
  }

  private static void TrackCurrentFurniture(Terminal __instance)
  {
    foreach (UnlockableItem furniture in StartOfRound.Instance.unlockablesList.unlockables)
    {
      if (furniture.shopSelectionNode == null) continue;
      StatsTracker.DayStats!.FurnitureInfo[furniture.unlockableName] = new(furniture, __instance);
    }
  }
}
