Readme Interactive Tag Playground (ITP) Unity code
created 7-12-2014
last changes 7-12-2014

========= General description ===================
The ITP is a digital version of the traditional game of tag. A tagger has an orange circle and has to let his circle touch that of a runner (with a blue circle) in order to let the runner become the tagger. Upon this touch the circles will change color

========= HOTKEYS ===============
Most important keys for settings in the game:
w- toggle wizard of oz mode, control a player by selecting him (press 0-5) and then use the arrow keys
v- toggle adaptive circle sizes, circle of the tagger will become bigger based on the time he has been the tagger and vice versa for the runners
p- toggle power ups, 4 different power ups are generated, minions,shield,grow/shrink, runners can pick the blue ones and taggers the red ones
a- toggle arrows, assigns an arrow to a random person
c - calibration mode PLEASE DONT USE THIS ON THE GAME PC WITHOUT ASKING ALEJANDRO, ROBBY or DENNIS FIRST
	select the tracked player (0-5)
	home - setup topleft (both in wooz mode and normal mode)
	page down- setup bottomright part (both in wooz mode and tracker mode)
	e - export the settings to c:/taglog/settings.cfg
	
========= SETUP THE GAME ===============
There are two PCs running the system. 1 for the tracker (trackerpc) and 1 for the visualization (gamePC with Unity in this case).
Login on the tracker PC: 
user: students
password: pl@yground (watch out for wrong (e.g. German or Dutch) keyboard settings!)

1. On the tracker PC start a tracker (start with shortcut ObjectTracker on the desktop). This sends info over udp to a javascript that forwards this information to a game PC. 
2. Start a node javascript that will forward this information to the games. For testing you might want to make a copy of the javascript and set it to your own ip adress. 
UPDATE: Soon there will be a multicast version so you can run the game on your laptop and on the gamepc at the same time. 

3. On the game PC there is a javascript based server (gameSideServer requires node, so install node-v0.10.28-x64 both the script and install for windows are included in the zip in the folder ServerStuff)
The information send from the server is a comma seperated list:
in which xpos and ypos can be different than used in your game world coordinates. Also they represent 1/10000 meters 

ID, xpos, ypos, zpos
for instance
0, 21000, -23000, 34000
1, 25000, -34000, 12000

With 0,0 is the center of the first Kinect to the left, when entering from the main entrance of the Gallery and in the Smart XP (probably) the one closest to Alfred's office.

Code
Everything is written in C# for Unity, remember that values of public variables are overwritten within the unity scene, some are also overwritten with the settings.cfg file which should be placed under 
!!! c:/taglog/settings.cfg !!!

- KinectRigClient.cs - 
The mainscript getting the information from the gamesideserver, loading the players and setting their position is 
KinectRigClient.cs , which can be found in Directional light/ main camera
It also deals with setting the wooz player selection and calibration from worldcoordinates to gamecoordinates, see background for orientation of the game (tag you are it is directed towards the main entrance)

- TagPlayer.cs - (in each player in the scene/ in the player prefab)
The players have a script that sets their circle size (pulsating) and keeps track of how long someone has been a tagger etc.
It also has the moveTo method which eventually in the update loop is used to set the position of this player. If you want to use this please add the linear interpolation as we think it will give better collision detection and allows for influencing the lag of tracking players. 

- CollisionDetection.cs - (in each player/Collider in the scene)
Detects if a player is tagged, or whether he/she has gathered a power-up, effect of shield power up was set here but currently not working.

- BackGroundChanger (in BackgroundImage) - 
Changes the background each tag, one can set 4 images to be used in the scene under BackgroundImage - Design Lab Mat... Commit Mat.

- PowerUpGeneration (in main camera)
Generates power ups at predefined positions at a predefined interval

- Chainarrow.cs (in arrows)
Creates a number of arrows in the chain based on the distance 

- Orbitaround (Assets-> prefabs --> player > (each) Minion)
Moves the minions circling around the player

- LoadRecordedSession.cs (in main camera)
(Not working yet) should be usable to load a previous session of tag

shieldscript, textscript, 
?, not that important
