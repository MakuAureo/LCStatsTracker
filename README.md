# StatTracker

StatTracker is a mod that allows you to see (almost) everything that happened during your day, it tracks everything automatically as you play and pushes all the info as a JSON at the end of the day to a local server.
This is meant to allow people to treat this data however they want.

# Local server

The local server is hosted on port 2145, it uses SSE and can be queried at any time but will only release the data after the current day ends.
The data can only be queried once per day, once it is sent the server will wait for the next day to finish.

Just HTTP request it and wait until the day is over to get your stats.

# JSON Schema
```
{
  "title": "Stats",
  "type": "object",
  "required": [
    "Seed",
    "Version",
    "MoonInfo",
    "PerformanceInfo",
    "BeeInfo",
    "EggInfo",
    "KnifeInfo",
    "ShotgunInfo",
    "QuotaInfo",
    "EventInfo",
    "Players",
    "IndoorSpawns",
    "DayTimeSpawns",
    "NightTimeSpawns",
    "ShopSales",
    "FurnitureInfo",
    "GiftBoxesOpened",
    "MissedItems"
  ],
  "properties": {
    "Seed":    { "type": "integer" },
    "Version": { "type": "integer" },

    "MoonInfo": {
      "type": "object",
      "required": ["Name", "Weather"],
      "properties": {
        "Name":    { "type": "string" },
        "Weather": { "type": "string" }
      }
    },

    "DungeonInfo": {
      "type": ["object", "null"],
      "required": ["ItemCount", "Interior"],
      "properties": {
        "ItemCount": { "type": "integer" },
        "Interior":  { "type": "string" }
      }
    },

    "HazardInfo": {
      "type": ["object", "null"],
      "required": ["TurretCount", "LandmineCount", "SpiketrapCount"],
      "properties": {
        "TurretCount":    { "type": "integer" },
        "LandmineCount":  { "type": "integer" },
        "SpiketrapCount": { "type": "integer" }
      }
    },

    "PerformanceInfo": {
      "type": "object",
      "required": [
        "CollectedNoExtra",
        "CollectedTotal",
        "InitialAvailableValue",
        "TotalAvailableValue",
        "ExtraFromOldGift"
      ],
      "properties": {
        "CollectedNoExtra":      { "type": "integer" },
        "CollectedTotal":        { "type": "integer" },
        "InitialAvailableValue": { "type": "integer" },
        "TotalAvailableValue":   { "type": "integer" },
        "ExtraFromOldGift":      { "type": "integer" }
      }
    },

    "BeeInfo":     { "$ref": "#/$defs/SpecialItemInfo" },
    "EggInfo":     { "$ref": "#/$defs/SpecialItemInfo" },
    "KnifeInfo":   { "$ref": "#/$defs/SpecialItemInfo" },
    "ShotgunInfo": { "$ref": "#/$defs/SpecialItemInfo" },

    "QuotaInfo": {
      "type": "object",
      "required": ["ValueSold", "NewQuota"],
      "properties": {
        "ValueSold": { "type": "integer" },
        "NewQuota":  { "type": "integer" }
      }
    },

    "EventInfo": {
      "type": "object",
      "required": [
        "AppSpawned",
        "IndoorFog",
        "TakeOffTime",
        "SIDType",
        "InfestationType",
        "MeteorShowerTime"
      ],
      "properties": {
        "AppSpawned":      { "type": "boolean" },
        "IndoorFog":       { "type": "boolean" },
        "TakeOffTime":     { "type": "string" },
        "SIDType":         { "type": "string" },
        "InfestationType": { "type": "string" },
        "MeteorShowerTime":{ "type": "string" }
      }
    },

    "Players": {
      "type": "object",
      "additionalProperties": { "$ref": "#/$defs/PlayerStats" }
    },

    "IndoorSpawns":    { "type": "array", "items": { "$ref": "#/$defs/SpawnInfo" } },
    "DayTimeSpawns":   { "type": "array", "items": { "$ref": "#/$defs/SpawnInfo" } },
    "NightTimeSpawns": { "type": "array", "items": { "$ref": "#/$defs/SpawnInfo" } },

    "ShopSales": {
      "type": "object",
      "additionalProperties": { "type": "integer" }
    },

    "FurnitureInfo": {
      "type": "object",
      "additionalProperties": { "$ref": "#/$defs/FurnitureInfo" }
    },

    "GiftBoxesOpened": { "type": "array", "items": { "$ref": "#/$defs/GiftBoxInfo" } },
    "MissedItems":     { "type": "array", "items": { "$ref": "#/$defs/MissingItemInfo" } }
  },

  "$defs": {
    "SpecialItemInfo": {
      "type": "object",
      "required": ["Available", "Collected"],
      "properties": {
        "Available": { "type": "array", "items": { "type": "integer" } },
        "Collected": { "type": "array", "items": { "type": "integer" } }
      }
    },

    "PlayerStats": {
      "type": "object",
      "required": ["Name", "Alive", "Disconnected", "TimeOfDeath", "CauseOfDeath"],
      "properties": {
        "Name":        { "type": "string" },
        "Alive":       { "type": "boolean" },
        "Disconnected":{ "type": "boolean" },
        "TimeOfDeath": { "type": "string" },
        "CauseOfDeath":{ "type": "string" }
      }
    },

    "SpawnInfo": {
      "type": "object",
      "required": ["Enemy", "SpawnTime", "TimeOfDeath"],
      "properties": {
        "Enemy":       { "type": "string" },
        "SpawnTime":   { "type": "string" },
        "TimeOfDeath": { "type": "string" }
      }
    },

    "FurnitureInfo": {
      "type": "object",
      "required": ["InStock", "Owned", "Stored", "ApparentPrice", "RealPrice", "Luck"],
      "properties": {
        "InStock":       { "type": "boolean" },
        "Owned":         { "type": "boolean" },
        "Stored":        { "type": "boolean" },
        "ApparentPrice": { "type": "integer" },
        "RealPrice":     { "type": "integer" },
        "Luck":          { "type": "number" }
      }
    },

    "GiftBoxInfo": {
      "type": "object",
      "required": ["NewScrapValue", "GiftScrapValue", "GiftBoxAge", "Collected"],
      "properties": {
        "NewScrapValue":  { "type": "integer" },
        "GiftScrapValue": { "type": "integer" },
        "GiftBoxAge":     { "type": "integer" },
        "Collected":      { "type": "boolean" }
      }
    },

    "MissingItemInfo": {
      "type": "object",
      "required": ["Value", "ItemType", "SpawnPosition", "DespawnPosition", "CollectedOnPreviousDay", "ScrapInsideGiftValue"],
      "properties": {
        "Value":                  { "type": "integer" },
        "ItemType":               { "type": "string" },
        "SpawnPosition":          { "type": ["array", "null"], "items": { "type": "number" }, "minItems": 3, "maxItems": 3 },
        "DespawnPosition":        { "type": "array", "items": { "type": "number" }, "minItems": 3, "maxItems": 3 },
        "CollectedOnPreviousDay": { "type": "boolean" },
        "ScrapInsideGiftValue":   { "type": "integer" }
      }
    }
  }
}
```
