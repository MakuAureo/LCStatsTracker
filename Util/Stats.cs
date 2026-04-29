using System.Collections.Generic;
using UnityEngine;

namespace StatsTracker.Util;

internal class PlayerStats
{
  public bool alive;
  public string? time_of_death;
  public string? death_cause;

  public PlayerStats()
  {
    alive = true;
  }

  public void Died(string time_of_death, string death_cause)
  {
    this.alive = false;
    this.time_of_death = time_of_death;
    this.death_cause = death_cause;
  }
}

internal class KiwiBirdInfo
{
  public bool spawned;
  public int[]? eggs;

  public KiwiBirdInfo(bool spawned)
  {
    this.spawned = spawned;
  }

  public void AddEggValue(int[] eggs) {
    this.eggs = eggs;
  }
}

internal class BeeInfo
{
  public int bee_count;
  public int[]? bees;

  public BeeInfo(int bee_count)
  {
    this.bee_count = bee_count;
  }

  public void AddBeeValue(int[] bees)
  {
    this.bees = bees;
  }
}

internal class ItemInfo
{
  public int value;
  public string name;
  public Vector3 starting_position;

  public ItemInfo(string name, int value, Vector3 starting_position)
  {
    this.name = name;
    this.value = value;
    this.starting_position = starting_position;
  }
}

internal class HazardInfo
{
  public int turret_count;
  public int landmine_count;
  public int spiketrap_count;

  public HazardInfo(int turretCount, int landmineCount, int spiketrapCount)
  {
    this.turret_count = turretCount;
    this.landmine_count = landmineCount;
    this.spiketrap_count = spiketrapCount;
  }
}

internal class MoonInfo
{
  public string name;
  public string weather;
  public string? interior;
  public int item_count;

  public MoonInfo(string name, string weather, int item_count, string? interior = null) 
  {
    this.name = name;
    this.weather = weather;
    this.item_count = item_count;
    this.interior = interior;
  }
}

internal class Stats
{
  public MoonInfo moon_info;

  public BeeInfo? bee_info;
  public KiwiBabyItem? bird_info;
  
  public List<ItemInfo> missed_items;
  
  public Dictionary<EnemyType, string> indoor_spawns;
  public Dictionary<EnemyType, string> outdoor_spawns;
  
  public Dictionary<ulong, PlayerStats> player_stats;

  public int seed;

  public int shotguns_collected;
  public int knifes_collected;
  
  public int topline;
  public int botline;
  public int botline_true;
  
  public bool has_app;
  public bool indoor_fog;
  public string? SID;
  public string? infestation_type;
  public string? meteor_shower_time;

  public Stats(int seed, string moon_name, string weather, int item_count, bool indoor_fog, string? interior = null, string? SID = null, string? infestation_type = null)
  {
    moon_info = new(moon_name, weather, item_count, interior);
    missed_items = new();
    indoor_spawns = new();
    outdoor_spawns = new();
    player_stats = new();
    this.seed = seed;
    shotguns_collected = 0;
    knifes_collected = 0;
    topline = 0;
    botline = 0;
    botline_true = 0;
    this.indoor_fog = indoor_fog;
    this.SID = SID;
    this.infestation_type = infestation_type;
  }
}
