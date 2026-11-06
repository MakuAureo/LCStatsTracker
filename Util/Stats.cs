using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace StatsTracker.Util;

internal class OutsideObjectInfo(string Name, Vector3 Position, float Yaw)
{
    public string Name = Name;
    public double[] Position = DungeonMapUtil.ToArray(Position);
    public double Yaw = Math.Round(Yaw, 1);
}

internal class MoonInfo(string Name, string Weather)
{
    public string Name = Name;
    public string Weather = Weather;
    public List<DungeonEntrance> Entrances = [];
    public List<OutsideObjectInfo> OutsideObjects = [];
    public List<double[]> RemovedTrees = [];
}

internal class DungeonInfo(int ItemCount, string Interior)
{
    public int ItemCount = ItemCount;
    public string Interior = Interior;
    public DungeonMap DungeonMap = new();
    public List<DoorInfo> Doors = [];
    public List<BigDoorInfo> BigDoors = [];
}

internal class HazardInfo(int TurretCount, int LandmineCount, int SpiketrapCount)
{
    public int TurretCount = TurretCount;
    public int LandmineCount = LandmineCount;
    public int SpiketrapCount = SpiketrapCount;
    public List<MapHazardInfo> MapHazards = [];
}

internal class PerformanceInfo()
{
    public int CollectedNoExtra = 0;
    public int CollectedTotal = 0;
    public int InitialAvailableValue = 0;
    public int TotalAvailableValue = 0;
    public int ExtraFromOldGift = 0;
}

internal class SpecialItemInfo()
{
    public List<int> Available = [];
    public List<int> Collected = [];

    public void AddToAvailable(int value)
    {
        Available.Add(value);
    }

    public void AddToCollected(int value)
    {
        Collected.Add(value);
    }
}

internal class QuotaInfo()
{
    public int ValueSold = 0;
    public int NewQuota = 0;
}

internal class EventInfo()
{
    public bool AppSpawned = false;
    public bool IndoorFog = false;
    public string TakeOffTime = "";
    public string SIDType = "";
    public string InfestationType = "";
    public string MeteorShowerTime = "";
}

internal class PlayerStats(string name)
{
    public string Name = name;
    public bool Alive = true;
    public bool Disconnected = false;
    public string TimeOfDeath = "";
    public string CauseOfDeath = "";

    public void Kill(string TimeOfDeath, string CauseOfDeath)
    {
        Alive = false;
        this.TimeOfDeath = TimeOfDeath;
        this.CauseOfDeath = CauseOfDeath;
    }

    public void Disconnect()
    {
        Disconnected = true;
    }
}

internal class SpawnInfo(ulong Id, EnemyType EnemyType, string Time, Vector3 SpawnPosition)
{
    public ulong Id = Id;
    public string Enemy = EnemyType.enemyName;
    public string SpawnTime = Time;
    public string TimeOfDeath = "";
    public double[] SpawnPosition = DungeonMapUtil.ToArray(SpawnPosition);
    public double[]? EndPosition = null;
}

internal class FurnitureInfo(UnlockableItem Furniture, Terminal Terminal)
{
    public bool InStock = Furniture.alwaysInStock || Terminal.ShipDecorSelection.Contains(Furniture.shopSelectionNode);
    public bool Owned = Furniture.alreadyUnlocked || Furniture.hasBeenUnlockedByPlayer;
    public bool Stored = Furniture.inStorage;
    public int ApparentPrice = Furniture.shopSelectionNode.itemCost;
    public int RealPrice = Furniture.shopSelectionNode.terminalOptions[0].result.itemCost;
    public float Luck = (float?)StatsTracker.LuckValueField?.GetValue(Furniture) ?? 0;
}

internal class GiftBoxInfo(int newScrapValue, int GiftScrapValue, int GiftBoxAge)
{
    public int NewScrapValue = newScrapValue;
    public int GiftScrapValue = GiftScrapValue;
    public int GiftBoxAge = GiftBoxAge;
    public bool Collected = false;
}

internal class MissingItemInfo(ulong Id, string Name, int Value, Vector3? SpawnPosition, Vector3 DespawnPosition, bool CollectedOnPreviousDay, int ScrapInsideGiftValue = 0)
{
    public ulong Id = Id;
    public int Value = Value;
    public string ItemType = Name;
    public double[]? SpawnPosition = SpawnPosition != null ? [Math.Round(SpawnPosition.Value.x, 1), Math.Round(SpawnPosition.Value.y, 1), Math.Round(SpawnPosition.Value.z, 1)] : null;
    public double[] DespawnPosition = [Math.Round(DespawnPosition.x, 1), Math.Round(DespawnPosition.y, 1), Math.Round(DespawnPosition.z, 1)];
    public bool CollectedOnPreviousDay = CollectedOnPreviousDay;
    public int ScrapInsideGiftValue = ScrapInsideGiftValue;
}

internal class SpawnedItemInfo(ulong Id, string Name, Vector3 SpawnPosition)
{
    public ulong Id = Id;
    public int Value = 0;
    public string ItemType = Name;
    public double[] SpawnPosition = [Math.Round(SpawnPosition.x, 1), Math.Round(SpawnPosition.y, 1), Math.Round(SpawnPosition.z, 1)];
    public int ScrapInsideGiftValue = 0;
}

internal static class DungeonMapUtil
{
    public static double[] ToArray(Vector3 v) => [Math.Round(v.x, 2), Math.Round(v.y, 2), Math.Round(v.z, 2)];
    // Yaw (clockwise from above) relative to how the tile looks in the editor's axis view of its up axis.
    // Most tiles are Y-up (Top view: +X right, +Z up), some are built Z-up and tipped upright by DunGen
    // (Back view, clicking the Z arrow: -X right, +Y up), so eulerAngles.y alone is meaningless for those
    public static double Yaw(Transform t)
    {
        Vector3 up = t.InverseTransformDirection(Vector3.up);
        Vector3 abs = new(Mathf.Abs(up.x), Mathf.Abs(up.y), Mathf.Abs(up.z));
        up = abs.x > abs.y && abs.x > abs.z ? new(Mathf.Sign(up.x), 0, 0)
           : abs.z > abs.y ? new(0, 0, Mathf.Sign(up.z))
           : new(0, Mathf.Sign(up.y), 0);

        // Pose matching that editor view: tile up axis to world up, tile +Y (screen up in the view) to world +Z
        Quaternion upright = up.y != 0 ? Quaternion.identity
            : Quaternion.LookRotation(Vector3.up, Vector3.forward) * Quaternion.Inverse(Quaternion.LookRotation(up, Vector3.up));
        double yaw = Math.Round((t.rotation * Quaternion.Inverse(upright)).eulerAngles.y, 1);
        return yaw >= 360 ? 0 : yaw;
    }
    public static string CleanName(string name) => name.Replace("(Clone)", "").Trim();

    // Path below root, the overlay keys in the tile images' manifest.json use the same format. DunGen has already destroyed what it
    // didn't pick by now, so same-named siblings get numbered by their order in the untouched prefab. Without one (nothing gets
    // destroyed inside spawned prefabs) the live siblings are used
    public static string PathFrom(Transform root, Transform? prefabRoot, Transform t) => Resolve(root, prefabRoot, t).path;

    private static (string path, Transform? prefab) Resolve(Transform root, Transform? prefabRoot, Transform t)
    {
        if (t == root) return ("", prefabRoot);
        (string parentPath, Transform? prefabParent) = Resolve(root, prefabRoot, t.parent);

        List<Transform> twins = [];
        foreach (Transform child in prefabParent ?? t.parent)
            if (child.name == t.name) twins.Add(child);
        Transform? match = prefabParent == null ? t : twins.OrderBy(c => (c.localPosition - t.localPosition).sqrMagnitude + Quaternion.Angle(c.localRotation, t.localRotation)).FirstOrDefault();

        string segment = twins.Count > 1 ? $"{t.name}#{twins.IndexOf(match)}" : t.name;
        return (parentPath == "" ? segment : $"{parentPath}/{segment}", prefabParent == null ? null : match);
    }

    public static IEnumerable<GameObject> VariableSceneObjects(Transform tileRoot)
    {
        foreach (Component dw in tileRoot.GetComponentsInChildren(DunGenAccess.Doorway, true))
        {
            foreach (GameObject o in DunGenAccess.Field<List<GameObject>>(dw, "ConnectorSceneObjects")) if (o != null) yield return o;
            foreach (GameObject o in DunGenAccess.Field<List<GameObject>>(dw, "BlockerSceneObjects")) if (o != null) yield return o;
        }
        foreach (Component set in tileRoot.GetComponentsInChildren(DunGenAccess.LocalPropSet, true))
            foreach (object w in Traverse.Create(set).Field("Props").Field("Weights").GetValue<IEnumerable>())
                if (DunGenAccess.Field<GameObject>(w, "Value") is GameObject o && o != null) yield return o;
        foreach (Component gp in tileRoot.GetComponentsInChildren(DunGenAccess.GlobalProp, true))
            if (gp.gameObject.activeSelf) yield return gp.gameObject;
    }

    // Closest variable object above t (itself included), or null if t is part of the static tile
    public static GameObject? Owner(Transform root, Transform t, HashSet<GameObject> variable)
    {
        for (; t != null && t != root; t = t.parent)
            if (variable.Contains(t.gameObject)) return t.gameObject;
        return null;
    }
}

// DunGen is its own assembly on recent game versions but part of Assembly-CSharp on older ones, so using its types directly
// would crash on one or the other. All of the map goes through here instead
internal static class DunGenAccess
{
    public static readonly Type Doorway = AccessTools.TypeByName("DunGen.Doorway");
    public static readonly Type LocalPropSet = AccessTools.TypeByName("DunGen.LocalPropSet");
    public static readonly Type GlobalProp = AccessTools.TypeByName("DunGen.GlobalProp");

    public static T Field<T>(object o, string name) => Traverse.Create(o).Field(name).GetValue<T>();
    public static T Property<T>(object o, string name) => Traverse.Create(o).Property(name).GetValue<T>();

    // Tile.Prefab only exists on recent versions, the dungeon's flow has them on all of them
    public static Dictionary<string, GameObject> TilePrefabs(object generator)
    {
        Dictionary<string, GameObject> prefabs = [];
        object flow = Field<object>(generator, "DungeonFlow");
        foreach (object tileSet in Traverse.Create(flow).Method("GetUsedTileSets").GetValue<IEnumerable>())
            foreach (object weight in Traverse.Create(tileSet).Field("TileWeights").Field("Weights").GetValue<IEnumerable>())
                if (Field<GameObject>(weight, "Value") is GameObject prefab && prefab != null)
                    prefabs[prefab.name] = prefab;
        return prefabs;
    }

    // Older versions don't keep a reference to the blocker they spawn, it's the doorway's clone that isn't the door
    public static IEnumerable<GameObject> SpawnedBlockers(Component doorway)
    {
        GameObject door = Property<GameObject>(doorway, "UsedDoorPrefabInstance");
        foreach (Transform child in doorway.transform)
            if (child.name.EndsWith("(Clone)") && child.gameObject != door)
                yield return child.gameObject;
    }
}

internal class DungeonEntrance(Vector3 Position, bool Main)
{
    public double[] Position = DungeonMapUtil.ToArray(Position);
    public bool Main = Main;
}

internal class DungeonNode(Component tile, Dictionary<string, GameObject> tilePrefabs)
{
    public string Name = DungeonMapUtil.CleanName(tile.name);
    public double[] Position = DungeonMapUtil.ToArray(tile.transform.position);
    // World space, axis aligned. Tile.Bounds and not Placement.Bounds, which ignores the tile's scale
    public double[] BoundsMin = DungeonMapUtil.ToArray(DunGenAccess.Property<Bounds>(tile, "Bounds").min);
    public double[] BoundsMax = DungeonMapUtil.ToArray(DunGenAccess.Property<Bounds>(tile, "Bounds").max);
    public double Yaw = DungeonMapUtil.Yaw(tile.transform);
    public List<string> Overlays = GetOverlays(tile, tilePrefabs);
    public List<DungeonEntrance> Entrances = GetEntrances(tile);

    // Unpicked fire exits are inactive and these aren't spawned yet, so this is only the ones that will be. The main entrance
    // is the one with id 0 on the prefab, fire exits all have 1 until the game numbers them
    private static List<DungeonEntrance> GetEntrances(Component tile)
    {
        List<DungeonEntrance> entrances = [];
        foreach (SpawnSyncedObject sso in tile.GetComponentsInChildren<SpawnSyncedObject>())
            if (sso.spawnPrefab != null && sso.spawnPrefab.GetComponentInChildren<EntranceTeleport>(true) is EntranceTeleport entrance)
                entrances.Add(new(sso.transform.position, entrance.entranceId == 0));
        return entrances;
    }

    private static List<string> GetOverlays(Component tile, Dictionary<string, GameObject> tilePrefabs)
    {
        List<string> overlays = [];
        Transform? tilePrefab = tilePrefabs.TryGetValue(DungeonMapUtil.CleanName(tile.name), out GameObject prefab) ? prefab.transform : null;
        string KeyOf(GameObject go, string? spawned = null)
        {
            string path = DungeonMapUtil.PathFrom(tile.transform, tilePrefab, go.transform);
            return spawned == null ? path : $"{path}|{DungeonMapUtil.CleanName(spawned)}";
        }

        // A connection's door is parented to one of its two doorways, the tile holding it is the one that draws it
        List<Component> doorways = DunGenAccess.Field<IEnumerable>(tile, "AllDoorways").Cast<Component>().ToList();
        List<(Component doorway, GameObject instance)> spawnedOnDoorways = [];
        foreach (Component dw in doorways)
        {
            foreach (GameObject blocker in DunGenAccess.SpawnedBlockers(dw))
                spawnedOnDoorways.Add((dw, blocker));
            GameObject door = DunGenAccess.Property<GameObject>(dw, "UsedDoorPrefabInstance");
            if (door != null && door.transform.parent == dw.transform)
                spawnedOnDoorways.Add((dw, door));
        }
        HashSet<GameObject> spawned = [.. spawnedOnDoorways.Select(s => s.instance)];

        // Whatever else is inside a spawned prefab is already drawn in that prefab's image
        bool InsideSpawned(Transform t) => DungeonMapUtil.Owner(tile.transform, t, spawned) != null;
        HashSet<GameObject> variable = [.. DungeonMapUtil.VariableSceneObjects(tile.transform).Where(go => !InsideSpawned(go.transform))];

        // Connector/blocker lists also hold non-visual things (AI nodes, radar sprites, empty prop set containers).
        // Only keep variable objects that own a visible renderer not belonging to a nested variable/spawned object
        HashSet<GameObject> owners = [.. variable, .. spawned];
        HashSet<GameObject> visible = [];
        int mapRadarLayer = LayerMask.NameToLayer("MapRadar");
        // Not checking Renderer.enabled: room culling has it off for every room the player isn't near
        foreach (Renderer r in tile.GetComponentsInChildren<Renderer>())
            if (r.gameObject.layer != mapRadarLayer && DungeonMapUtil.Owner(tile.transform, r.transform, owners) is GameObject owner)
                visible.Add(owner);

        foreach (GameObject go in variable)
            if (visible.Contains(go))
                overlays.Add(KeyOf(go));

        // Global props nested in a spawned prefab are their own overlay (blockers all hold a fire exit, only the picked ones stay on)
        foreach ((Component dw, GameObject instance) in spawnedOnDoorways)
        {
            string key = KeyOf(dw.gameObject, instance.name);
            overlays.Add(key);
            foreach (Component gp in instance.GetComponentsInChildren(DunGenAccess.GlobalProp))
                if (gp.gameObject != instance)
                    overlays.Add($"{key}/{DungeonMapUtil.PathFrom(instance.transform, null, gp.transform)}");
        }

        // The game spawns these as network objects (vents, etc.) after generation, outside the tile, at the anchor's transform
        foreach (SpawnSyncedObject sso in tile.GetComponentsInChildren<SpawnSyncedObject>())
            if (sso.spawnPrefab != null && !InsideSpawned(sso.transform))
                overlays.Add(KeyOf(sso.gameObject, sso.spawnPrefab.name));

        return overlays;
    }
}

internal class DungeonEdge(object connection, Dictionary<Component, int> tileIds)
{
    public int[] From = Side(connection, "A", tileIds);
    public int[] To = Side(connection, "B", tileIds);

    private static int[] Side(object connection, string side, Dictionary<Component, int> tileIds)
    {
        Component doorway = DunGenAccess.Property<Component>(connection, side);
        Component tile = DunGenAccess.Property<Component>(doorway, "Tile");
        return [tileIds[tile], DunGenAccess.Field<IList>(tile, "AllDoorways").IndexOf(doorway)];
    }
}

// Drawing it from the tile images, where <Name>/manifest.json has pivot, pixelsPerUnit, floors and overlays:
//   every image of a tile goes pivot on (Position.x, -Position.z) * pixelsPerUnit, turned clockwise by Yaw around the pivot
//   draw each floor at height Position.y + floor.height, lowest first
//   for each key in Overlays, draw manifest.overlays[key] on its floor, a key with no entry has nothing to draw
//   Entrances are world positions, they aren't in the images
// Edges aren't needed for drawing, they're [tile index, doorway index] pairs for walking the dungeon as a graph
internal class DungeonMap()
{
    public List<DungeonNode> Tiles = [];
    public List<DungeonEdge> Edges = [];

    public DungeonMap(object generator, object dungeon) : this()
    {
        List<Component> tiles = DunGenAccess.Property<IEnumerable>(dungeon, "AllTiles").Cast<Component>().ToList();
        Dictionary<Component, int> tileIds = tiles.Select((t, i) => (t, i)).ToDictionary(x => x.t, x => x.i);
        Dictionary<string, GameObject> tilePrefabs = DunGenAccess.TilePrefabs(generator);

        foreach (Component tile in tiles)
            Tiles.Add(new(tile, tilePrefabs));

        foreach (object connection in DunGenAccess.Property<IEnumerable>(dungeon, "Connections"))
            Edges.Add(new(connection, tileIds));
    }
}

internal class DoorInfo(Vector3 Position, bool StartedLocked)
{
    public double[] Position = DungeonMapUtil.ToArray(Position);
    public bool StartedLocked = StartedLocked;
    public string UnlockTime = "";
}

internal class BigDoorInfo(Vector3 Position, string Code, bool StartedOpen)
{
    public double[] Position = DungeonMapUtil.ToArray(Position);
    public string Code = Code;
    public bool StartedOpen = StartedOpen;
}

internal class MapHazardInfo(string Kind, Vector3 Position, string Code)
{
    public string Kind = Kind;
    public double[] Position = DungeonMapUtil.ToArray(Position);
    public string Code = Code;
}

internal class Stats
{
    public int Seed;
    public int Version;

    public MoonInfo MoonInfo;
    public DungeonInfo? DungeonInfo;
    public HazardInfo? HazardInfo;

    public PerformanceInfo PerformanceInfo;

    public SpecialItemInfo BeeInfo;
    public SpecialItemInfo EggInfo;
    public SpecialItemInfo KnifeInfo;
    public SpecialItemInfo ShotgunInfo;

    public QuotaInfo QuotaInfo;

    public EventInfo EventInfo;

    public Dictionary<ulong, PlayerStats> Players;

    public List<SpawnInfo> IndoorSpawns;
    public List<SpawnInfo> DayTimeSpawns;
    public List<SpawnInfo> NightTimeSpawns;

    public Dictionary<string, int> ShopSales;
    public Dictionary<string, FurnitureInfo> FurnitureInfo;

    public List<GiftBoxInfo> GiftBoxesOpened;
    public List<SpawnedItemInfo> SpawnedItems;
    public List<MissingItemInfo> MissedItems;

    public Stats(int version, string moonName, string weather, GameNetcodeStuff.PlayerControllerB[] allPlayers)
    {
        Seed = 0;
        Version = version;
        MoonInfo = new(moonName, weather);
        PerformanceInfo = new();
        BeeInfo = new();
        EggInfo = new();
        KnifeInfo = new();
        ShotgunInfo = new();
        QuotaInfo = new();
        EventInfo = new();
        Players = [];
        IndoorSpawns = [];
        DayTimeSpawns = [];
        NightTimeSpawns = [];
        GiftBoxesOpened = [];
        SpawnedItems = [];
        MissedItems = [];
        ShopSales = [];
        FurnitureInfo = [];

        foreach (GameNetcodeStuff.PlayerControllerB player in allPlayers)
            Players[player.playerSteamId] = new(player.playerUsername);
    }
}
