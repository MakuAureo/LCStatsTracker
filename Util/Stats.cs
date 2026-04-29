using System.Collections.Generic;
using UnityEngine;

namespace StatsTracker.Util;

internal class PlayerStats
{
  public bool Alive;
  public string? TimeOfDeath;
  public string? CauseOfDeath;

  public PlayerStats()
  {
    Alive = true;
  }

  public void Died(string TimeOfDeath, string CauseOfDeath)
  {
    this.Alive = false;
    this.TimeOfDeath = TimeOfDeath;
    this.CauseOfDeath = CauseOfDeath;
  }
}

internal class KiwiBirdInfo
{
  public bool Spawned;
  public int[]? Eggs;

  public KiwiBirdInfo(bool spawned)
  {
    this.Spawned = spawned;
  }

  public void AddEggValue(int[] eggs) {
    this.Eggs = eggs;
  }
}

internal class BeeInfo
{
  public int BeeCount;
  public int[]? Bees;

  public BeeInfo(int BeeCount)
  {
    this.BeeCount = BeeCount;
  }

  public void AddBeeValue(int[] Bees)
  {
    this.Bees = Bees;
  }
}

internal class ItemInfo
{
  public int Value;
  public string ItemType;
  public Vector3 DespawnPosition;

  public ItemInfo(string Name, int Value, Vector3 DespawnPosition)
  {
    this.ItemType = Name;
    this.Value = Value;
    this.DespawnPosition = DespawnPosition;
  }
}

internal class HazardInfo
{
  public int TurretCount;
  public int LandmineCount;
  public int SpiketrapCount;

  public HazardInfo(int TurretCount, int LandmineCount, int SpiketrapCount)
  {
    this.TurretCount = TurretCount;
    this.LandmineCount = LandmineCount;
    this.SpiketrapCount = SpiketrapCount;
  }
}

internal class MoonInfo
{
  public string Name;
  public string Weather;

  public MoonInfo(string Name, string Weather) 
  {
    this.Name = Name;
    this.Weather = Weather;
  }
}

internal class DungeonInfo
{
  public int ItemCount;
  public string Interior;

  public DungeonInfo(int ItemCount, string Interior)
  {
    this.ItemCount = ItemCount;
    this.Interior = Interior;
  }
}

internal class Stats
{
  public MoonInfo MoonInfo;
  public DungeonInfo? DungeonInfo;

  public BeeInfo? BeeInfo;
  public KiwiBabyItem? BirdInfo;
  
  public List<ItemInfo> MissedItems;
  
  public Dictionary<EnemyType, string> IndoorSpawns;
  public Dictionary<EnemyType, string> DayTimeSpawns;
  public Dictionary<EnemyType, string> NightTimeSpawns;
  
  public Dictionary<ulong, PlayerStats> Players;

  public int Seed;

  public int ShotgunsCollected;
  public int KnivesCollected;
  
  public int TopLine;
  public int BottomLine;
  public int BottomLineTrue;
  
  public bool AppSpawned;
  public bool IndoorFog;
  public string? SIDType;
  public string? InfestationType;
  public string? MeteorShowerTime;

  public Stats(int seed, string moonName, string weather, ulong[] allPlayerIDs)
  {
    MoonInfo = new(moonName, weather);
    MissedItems = new();
    IndoorSpawns = new();
    DayTimeSpawns = new();
    NightTimeSpawns = new();
    Players = new();
    Seed = seed;
    ShotgunsCollected = 0;
    KnivesCollected = 0;
    TopLine = 0;
    BottomLine = 0;
    BottomLineTrue = 0;

    foreach (ulong playerID in allPlayerIDs)
      Players[playerID] = new();
  }
}
