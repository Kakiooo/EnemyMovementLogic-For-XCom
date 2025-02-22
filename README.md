# EnemyMovementLogic-For-XCom
## Set Up
1.Setting barricades in the scene
<br/>2.setting Player Game object as the target for individual enemies
<br/>3.Put All enemy under EnemyManager GameObject. 
<br/>4.Add EnemyManagerSc to Manager, and enemyMovement to Individuals. 
## Logic of Code
<br/>-First Enemy will get a list that contain all the Transform info of the Barricades which is also called Covers.
<br/>-Then enemy will first search for random closest cover to go
<br/>-After all enemy got cover, enemy will start to search for next cover to go. During this time, the sequence for enemy to move on will be shuffled by enemy manager. Then Enemy will first determine if player are in their detectable range * if player are in the range, enemies will search for closest cover to go or retreat when player get closer to them. * if player is outside the range, enemies will go after player by go to the covers that are closer to players. 
