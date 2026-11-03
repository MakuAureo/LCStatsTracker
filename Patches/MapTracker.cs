using System;
using System.Collections.Generic;
using HarmonyLib;

namespace StatsTracker.Patches;

internal class MapTracker
{
    internal static Util.DungeonMap? CurrentMap;

    internal static List<Util.DoorInfo> Doors = [];
    internal static List<Util.BigDoorInfo> BigDoors = [];
    internal static List<Util.MapHazardInfo> MapHazards = [];

    private static readonly Dictionary<DoorLock, Util.DoorInfo> doors = [];

    public static void ApplyMapTrackerPatches(Harmony Harmony)
    {
        Harmony.Patch(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.FinishGeneratingLevel)), postfix: new HarmonyMethod(typeof(MapTracker), nameof(MapTracker.BuildDungeonMapGraph)));
        Harmony.Patch(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.SetBigDoorCodes)), postfix: new HarmonyMethod(typeof(MapTracker), nameof(MapTracker.TrackDoorsAndHazards)));
        Harmony.Patch(AccessTools.Method(typeof(DoorLock), nameof(DoorLock.UnlockDoor)), prefix: new HarmonyMethod(typeof(MapTracker), nameof(MapTracker.TrackDoorUnlock)));
    }

    private static void TrackDoorsAndHazards()
    {
        try
        {
            foreach (DoorLock door in UnityEngine.Object.FindObjectsOfType<DoorLock>())
            {
                doors[door] = new(door.transform.position, door.isLocked);
                Doors.Add(doors[door]);
            }

            Type? spikeTrap = AccessTools.TypeByName("SpikeRoofTrap");
            foreach (TerminalAccessibleObject obj in UnityEngine.Object.FindObjectsOfType<TerminalAccessibleObject>())
            {
                if (obj.isBigDoor)
                    BigDoors.Add(new(obj.transform.position, obj.objectCode, obj.isDoorOpen));
                else if (obj.GetComponent<Turret>() != null)
                    MapHazards.Add(new("Turret", obj.transform.position, obj.objectCode));
                else if (obj.GetComponent<Landmine>() != null)
                    MapHazards.Add(new("Landmine", obj.transform.position, obj.objectCode));
                else if (spikeTrap != null && obj.transform.parent?.GetComponentInChildren(spikeTrap) != null)
                    MapHazards.Add(new("Spike Trap", obj.transform.position, obj.objectCode));
            }
        }
        catch (Exception e)
        {
            StatsTracker.Logger.LogError($"Failed to track doors and hazards: {e}");
        }
    }

    private static void TrackDoorUnlock(DoorLock __instance)
    {
        if (__instance.isLocked && doors.TryGetValue(__instance, out Util.DoorInfo info))
            info.UnlockTime = StatsTracker.GetCurrentTimeString();
    }

    private static void BuildDungeonMapGraph(RoundManager __instance)
    {
        Doors = [];
        BigDoors = [];
        MapHazards = [];
        doors.Clear();

        try
        {
            object? generator = Traverse.Create(__instance).Field("dungeonGenerator").Field("Generator").GetValue();
            object? dungeon = generator == null ? null : Util.DunGenAccess.Property<object>(generator, "CurrentDungeon");
            if (dungeon == null)
            {
                StatsTracker.Logger.LogWarning("Couldn't find the generated dungeon, skipping map graph");
                return;
            }

            CurrentMap = new(generator!, dungeon);
            if (StatsTracker.DayStats?.DungeonInfo != null)
                StatsTracker.DayStats.DungeonInfo.DungeonMap = CurrentMap;
        }
        catch (Exception e)
        {
            StatsTracker.Logger.LogError($"Failed to build dungeon map graph: {e}");
        }
    }
}
