using System;
using System.Collections.Generic;
using UnityEngine;

namespace StatsTracker.Util;

internal class MoonInfo(string Name, string Weather)
{
  public string Name = Name;
  public string Weather = Weather;
}

internal class DungeonInfo(int ItemCount, string Interior)
{
  public int ItemCount = ItemCount;
  public string Interior = Interior;
}

internal class HazardInfo(int TurretCount, int LandmineCount, int SpiketrapCount)
{
  public int TurretCount = TurretCount;
  public int LandmineCount = LandmineCount;
  public int SpiketrapCount = SpiketrapCount;
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

internal class SpawnInfo(EnemyType EnemyType, string Time)
{
  public string Enemy = EnemyType.enemyName;
  public string SpawnTime = Time;
  public string TimeOfDeath = "";
}

internal class FurnitureInfo(UnlockableItem Furniture, Terminal Terminal)
{
  public bool InStock = Furniture.alwaysInStock || Terminal.ShipDecorSelection.Contains(Furniture.shopSelectionNode);
  public bool Owned = Furniture.alreadyUnlocked || Furniture.hasBeenUnlockedByPlayer;
  public int ApparentPrice = Furniture.shopSelectionNode.itemCost;
  public int RealPrice = Furniture.shopSelectionNode.terminalOptions[0].result.itemCost;
  public float Luck = Furniture.luckValue;
}

internal class GiftBoxInfo(int newScrapValue, int GiftScrapValue, int GiftBoxAge)
{
  public int NewScrapValue = newScrapValue;
  public int GiftScrapValue = GiftScrapValue;
  public int GiftBoxAge = GiftBoxAge;
  public bool Collected = false;
}

internal class MissingItemInfo(string Name, int Value, Vector3 SpawnPosition, Vector3 DespawnPosition, bool CollectedOnPreviousDay, int ScrapInsideGiftValue = 0)
{
  public int Value = Value;
  public string ItemType = Name;
  public double[] SpawnPosition = [Math.Round(SpawnPosition.x, 1), Math.Round(SpawnPosition.y, 1), Math.Round(SpawnPosition.z, 1)];
  public double[] DespawnPosition = [Math.Round(DespawnPosition.x, 1), Math.Round(DespawnPosition.y, 1), Math.Round(DespawnPosition.z, 1)];
  public bool CollectedOnPreviousDay = CollectedOnPreviousDay;
  public int ScrapInsideGiftValue = ScrapInsideGiftValue;
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
    MissedItems = [];
    ShopSales = [];
    FurnitureInfo = [];

    foreach (GameNetcodeStuff.PlayerControllerB player in allPlayers)
      Players[player.playerSteamId] = new(player.playerUsername);
  }
}
