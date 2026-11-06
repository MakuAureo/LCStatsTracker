using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace StatsTracker.Patches;

internal class MapTracker
{
    internal static Util.DungeonMap? CurrentMap;

    internal static List<Util.DoorInfo> Doors = [];
    internal static List<Util.BigDoorInfo> BigDoors = [];
    internal static List<Util.MapHazardInfo> MapHazards = [];

    private static readonly Dictionary<DoorLock, Util.DoorInfo> doors = [];
    private static readonly List<(GameObject tree, Vector3 position)> treesBeforeHazards = [];

    public static void ApplyMapTrackerPatches(Harmony Harmony)
    {
        Harmony.Patch(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.FinishGeneratingLevel)), postfix: new HarmonyMethod(typeof(MapTracker), nameof(MapTracker.BuildDungeonMapGraph)));
        Harmony.Patch(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.SpawnOutsideHazards)), prefix: new HarmonyMethod(typeof(MapTracker), nameof(MapTracker.RememberOutsideBeforeHazards)), postfix: new HarmonyMethod(typeof(MapTracker), nameof(MapTracker.TrackOutsideObjects)));
        Harmony.Patch(AccessTools.Method(typeof(RoundManager), nameof(RoundManager.SetBigDoorCodes)), postfix: new HarmonyMethod(typeof(MapTracker), nameof(MapTracker.TrackDoorsAndHazards)));
        Harmony.Patch(AccessTools.Method(typeof(DoorLock), nameof(DoorLock.UnlockDoor)), prefix: new HarmonyMethod(typeof(MapTracker), nameof(MapTracker.TrackDoorUnlock)));
    }

    private static void RememberOutsideBeforeHazards(RoundManager __instance, out HashSet<Transform> __state)
    {
        __state = [];
        treesBeforeHazards.Clear();
        try
        {
            if (__instance.mapPropsContainer != null)
                foreach (Transform child in __instance.mapPropsContainer.transform)
                    __state.Add(child);

            // Throws if the tag doesn't exist, which it might not on older versions
            foreach (GameObject tree in GameObject.FindGameObjectsWithTag("Tree"))
                treesBeforeHazards.Add((tree, tree.transform.position));
        }
        catch (Exception e)
        {
            StatsTracker.Logger.LogDebug($"Not tracking removed trees: {e.Message}");
        }
    }

    private static void TrackOutsideObjects(RoundManager __instance, HashSet<Transform> __state)
    {
        try
        {
            if (StatsTracker.DayStats == null || __instance.mapPropsContainer == null)
                return;

            foreach (Transform child in __instance.mapPropsContainer.transform)
                if (!__state.Contains(child))
                    StatsTracker.DayStats.MoonInfo.OutsideObjects.Add(new(Util.DungeonMapUtil.CleanName(child.name), child.position, child.eulerAngles.y));
        }
        catch (Exception e)
        {
            StatsTracker.Logger.LogError($"Failed to track outside objects: {e}");
        }
    }

    private static void TrackDoorsAndHazards()
    {
        try
        {
            if (StatsTracker.DayStats != null)
            {
                foreach (EntranceTeleport entrance in UnityEngine.Object.FindObjectsOfType<EntranceTeleport>())
                    if (entrance.isEntranceToBuilding)
                        StatsTracker.DayStats.MoonInfo.Entrances.Add(new(entrance.transform.position, entrance.entranceId == 0));

                // Destroy() waits for the end of the frame, so by now the removed trees are gone
                foreach ((GameObject tree, Vector3 position) in treesBeforeHazards)
                    if (tree == null)
                        StatsTracker.DayStats.MoonInfo.RemovedTrees.Add(Util.DungeonMapUtil.ToArray(position));
            }

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
