# EnemyMovementLogic-For-XCom
## Set Up
1.Setting barricades with children as side Transform in the scene
<br/>2.setting Player Game object as the target for individual enemies
<br/>3.Put All enemy under EnemyManager GameObject. 
<br/>4.Add EnemyManagerSc to Manager, and enemyMovement to Individuals. 
## Logic of Code
<br/>-First Enemy will get a list that contain all the Transform info of the Barricades which is also called Covers.
<br/>-Then enemy will first search for random closest cover to go
<br/>-After all enemy got cover, enemy will start to search for next cover to go. Enemy will first determine if player are in their detectable range 
- if player are in the range, enemies will search for closest cover to go or retreat when player get closer to them. 
- if player is outside the range, enemies will go after player by go to the covers that are closer to players.
- At last, enemy will choose to go to the sides of cover that is furthest from player.
  
<br/>-Enemy will search for covers on their turns, but the sequence for enemy to move on will be shuffled by enemy manager. So that enemy will not always move in the same order. 
<br/>-FYI: Enemy will only run cover search once time instead update the cover info all the time in UPDATE() function. //Bool like IsMyTurn is to make searching only run once.
## More Info
-The Script uses unity package called AI Navigation to make enemy has the pathfinding ability, may need to install the package before using the Script.// _Nav_Enemy.destination = _sidePos; is to set the target position for enemy.
## Info for Example Project File
### Control
-Left Mouse click for player to move (QTE shows up for player to have faster move).
