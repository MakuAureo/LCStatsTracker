# StatTracker

StatTracker is a mod that allows you to see (almost) everything that happened during your day, it tracks everything automatically as you play and pushes all the info as a JSON at the end of the day to a local server.
This is meant to allow people to treat this data however they want.

# Local server

The local server is hosted on port 2145, it uses SSE and can be queried at any time but will only release the data after the current day ends.

Just HTTP request it and wait until the day is over to get your stats.

# Currently Tracked Stats

- Seed Number
- Item Info:
  - Collected
  - Available
  - True Available (Available + items that don't add to it)
- Moon Info:
    - Name
    - Weather
    - Interior type
- Events:
    - Single Item Day type
    - Infestation type
    - Indoor Fog
    - Meteor Shower
- Missed Items:
    - Type
    - Value
    - Position
- Spawns:
    - Type
    - Time
- Player:
    - Alive?
    - Time of death
    - Cause of death
- Special Items:
    - Bee Hives
    - Shotguns
    - Knifes
    - Eggs
