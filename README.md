# StatTracker

StatTracker is a mod that allows you to see (almost) everything that happened during your day, it tracks everything automatically as you play and pushes all the info as a JSON at the end of the day to a local server.
This is meant to allow people to treat this data however they want.

# Local server

The local server is hosted on port 2145, it uses SSE and can be queried at any time but will only release the data after the current day ends.
The data can only be queried once per day, once it is sent the server will wait for the next day to finish.

Just HTTP request it and wait until the day is over to get your stats.

# Currently Tracked Stats
```
{
   "Seed":19367748,
   "Version":81,
   "MoonInfo":{
      "Name":"68 Artifice",
      "Weather":"Eclipsed"
   },
   "DungeonInfo":{
      "ItemCount":32,
      "Interior":"Mineshaft"
   },
   "HazardInfo":{
      "TurretCount":0,
      "LandmineCount":7,
      "SpiketrapCount":0
   },
   "PerformanceInfo":{
      "CollectedNoExtra":1852,
      "CollectedTotal":1988,
      "InitialAvailableValue":1881,
      "TotalAvailableValue":2077,
      "ExtraFromOldGift":0
   },
   "BeeInfo":{
      "Available":[
         
      ],
      "Collected":[
         
      ]
   },
   "EggInfo":{
      "Available":[
         
      ],
      "Collected":[
         
      ]
   },
   "KnifeInfo":{
      "Available":[
         35
      ],
      "Collected":[
         35
      ]
   },
   "ShotgunInfo":{
      "Available":[
         60,
         60
      ],
      "Collected":[
         60
      ]
   },
   "QuotaInfo":{
      "ValueSold":0,
      "NewQuota":0
   },
   "EventInfo":{
      "AppSpawned":false,
      "IndoorFog":false,
      "TakeOffTime":"12:09 PM",
      "SIDType":"",
      "InfestationType":"",
      "MeteorShowerTime":""
   },
   "Players":{
      "76561198980273231":{
         "Name":"AureoHatsune",
         "Alive":true,
         "Disconnected":false,
         "TimeOfDeath":"",
         "CauseOfDeath":""
      }
   },
   "IndoorSpawns":[
      {
         "Enemy":"Puffer",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":""
      },
      {
         "Enemy":"Stingray",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":""
      },
      {
         "Enemy":"Spring",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":""
      },
      {
         "Enemy":"Nutcracker",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":"10:33 AM"
      },
      {
         "Enemy":"Butler",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":"11:22 AM"
      },
      {
         "Enemy":"Butler Bees",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":""
      },
      {
         "Enemy":"Nutcracker",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":""
      }
   ],
   "DayTimeSpawns":[
      
   ],
   "NightTimeSpawns":[
      {
         "Enemy":"RadMech",
         "SpawnTime":"7:39 AM",
         "TimeOfDeath":""
      },
      {
         "Enemy":"Earth Leviathan",
         "SpawnTime":"7:39 AM",
         "TimeOfDeath":""
      },
      {
         "Enemy":"ForestGiant",
         "SpawnTime":"7:39 AM",
         "TimeOfDeath":"9:58 AM"
      },
      {
         "Enemy":"RadMech",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":""
      },
      {
         "Enemy":"MouthDog",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":"10:27 AM"
      },
      {
         "Enemy":"ForestGiant",
         "SpawnTime":"10:13 AM",
         "TimeOfDeath":""
      }
   ],
   "ShopSales":{
      "Walkie-talkie":10,
      "Flashlight":0,
      "Shovel":50,
      "Lockpicker":0,
      "Pro-flashlight":0,
      "Stun grenade":0,
      "Boombox":0,
      "TZP-Inhalant":0,
      "Zap gun":0,
      "Jetpack":0,
      "Extension ladder":0,
      "Radar-booster":0,
      "Spray paint":0,
      "Weed killer":0,
      "Belt bag":70,
      "Cruiser":0
   },
   "FurnitureInfo":{
      "Green suit":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":60,
         "RealPrice":60,
         "Luck":0.0
      },
      "Hazard suit":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":90,
         "RealPrice":90,
         "Luck":0.0
      },
      "Pajama suit":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":900,
         "RealPrice":900,
         "Luck":0.0
      },
      "Cozy lights":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":140,
         "RealPrice":140,
         "Luck":0.005
      },
      "Television":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":130,
         "RealPrice":130,
         "Luck":0.02
      },
      "Toilet":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":150,
         "RealPrice":150,
         "Luck":0.01
      },
      "Shower":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":180,
         "RealPrice":180,
         "Luck":0.015
      },
      "Record player":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":120,
         "RealPrice":120,
         "Luck":0.005
      },
      "Table":{
         "InStock":true,
         "Owned":false,
         "ApparentPrice":70,
         "RealPrice":70,
         "Luck":0.004
      },
      "Romantic table":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":120,
         "RealPrice":120,
         "Luck":0.005
      },
      "Signal translator":{
         "InStock":true,
         "Owned":false,
         "ApparentPrice":255,
         "RealPrice":255,
         "Luck":-0.012
      },
      "JackOLantern":{
         "InStock":true,
         "Owned":true,
         "ApparentPrice":50,
         "RealPrice":50,
         "Luck":0.012
      },
      "Welcome mat":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":40,
         "RealPrice":40,
         "Luck":0.003
      },
      "Goldfish":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":50,
         "RealPrice":50,
         "Luck":0.006
      },
      "Plushie pajama man":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":100,
         "RealPrice":100,
         "Luck":0.003
      },
      "Purple Suit":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":70,
         "RealPrice":70,
         "Luck":0.0
      },
      "Bee Suit":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":110,
         "RealPrice":110,
         "Luck":0.0
      },
      "Bunny Suit":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":200,
         "RealPrice":200,
         "Luck":0.0
      },
      "Disco Ball":{
         "InStock":true,
         "Owned":false,
         "ApparentPrice":150,
         "RealPrice":150,
         "Luck":0.06
      },
      "Microwave":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":80,
         "RealPrice":80,
         "Luck":0.01
      },
      "Sofa chair":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":150,
         "RealPrice":150,
         "Luck":0.008
      },
      "Fridge":{
         "InStock":true,
         "Owned":false,
         "ApparentPrice":225,
         "RealPrice":150,
         "Luck":0.01
      },
      "Classic painting":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":400,
         "RealPrice":400,
         "Luck":0.006
      },
      "Electric chair":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":140,
         "RealPrice":140,
         "Luck":0.012
      },
      "Dog house":{
         "InStock":false,
         "Owned":false,
         "ApparentPrice":80,
         "RealPrice":80,
         "Luck":0.007
      }
   },
   "GiftBoxesOpened":[
      {
         "NewScrapValue":58,
         "GiftScrapValue":17,
         "GiftBoxAge":0,
         "Collected":true
      }
   ],
   "MissedItems":[
      {
         "Value":60,
         "ItemType":"Double-barrel",
         "SpawnPosition":[
            -25.7,
            -219.1,
            -8.0
         ],
         "DespawnPosition":[
            -25.7,
            -219.5,
            -8.0
         ],
         "CollectedOnPreviousDay":false,
         "ScrapInsideGiftValue":0
      },
      {
         "Value":29,
         "ItemType":"Whoopie cushion",
         "SpawnPosition":[
            -12.6,
            -219.7,
            -23.1
         ],
         "DespawnPosition":[
            -33.6,
            -226.5,
            -7.0
         ],
         "CollectedOnPreviousDay":false,
         "ScrapInsideGiftValue":0
      }
   ]
}
```
