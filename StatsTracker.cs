using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;

namespace StatsTracker;

[BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
[BepInDependency("OreoM.HQoL.72", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("OreoM.HQoL.73", BepInDependency.DependencyFlags.SoftDependency)]
public class StatsTracker : BaseUnityPlugin
{
  public static StatsTracker Instance { get; private set; } = null!;
  internal new static ManualLogSource Logger { get; private set; } = null!;
  internal static Harmony? Harmony { get; set; }

  internal static Util.Stats? DayStats;
  internal static Util.HttpSSE LocalServer = new();
  internal static string[] VanillaInteriorNames = { "Facility", "Mansion", "UnusedFacility", "Facility3Exit", "Mineshaft" };

  private void Awake()
  {
    Logger = base.Logger;
    Instance = this;

    Patch();
    if (Chainloader.PluginInfos.ContainsKey("OreoM.HQoL.72") || Chainloader.PluginInfos.ContainsKey("OreoM.HQoL.73"))
      Harmony?.PatchAll(typeof(Patches.HQoLTracker));
    LocalServer.Start();

    Logger.LogInfo($"{MyPluginInfo.PLUGIN_GUID} v{MyPluginInfo.PLUGIN_VERSION} has loaded!");
  }

  internal static void Patch()
  {
    Harmony ??= new Harmony(MyPluginInfo.PLUGIN_GUID);

    Logger.LogDebug("Patching...");

    Harmony.PatchAll();

    Logger.LogDebug("Finished patching!");
  }

  internal static void Unpatch()
  {
    Logger.LogDebug("Unpatching...");

    Harmony?.UnpatchSelf();

    Logger.LogDebug("Finished unpatching!");
  }

  internal static string GetCurrentTimeString()
  {
    return HUDManager.Instance.GetClockTimeFormatted(TimeOfDay.Instance.normalizedTimeOfDay, TimeOfDay.Instance.numberOfHours, false);
  }
}
