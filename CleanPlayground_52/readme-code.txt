Readme Interactive Tag Playground (ITP) Unity code
created 7-12-2014
last changes 7-12-2014

General description:
The ITP is a digital version of the traditional game of tag. A tagger has an orange circle and has to let his circle touch that of a runner (with a blue circle) in order to let the runner become the tagger. Upon this touch the circles will change color

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
	
Setup:
on the tracker PC there is a tracker running (start with shortcur ObjectTracker on the desktop). This sends info over udp to a javascript that forwards this information to a game PC. For testing you might want to make a copy of the javascript and set it to your own ip adress. 
On the game PC there is a javascript based server (gameSideServer requires node, so install node-v0.10.28-x64 both the script and install for windows are included in the zip in the folder ServerStuff)
The information send from the server is a comma seperated list:
in which xpos and ypos can be different than used in your game world coordinates. Also they represent 1/10000 meters 

ID, xpos, ypos, zpos
for instance
0, 21000, -23000, 34000
1, 25000, -34000, 12000

With 0,0 is the center of the first Kinect to the left, when entering from the main entrance of the Gallery

Code
Everything is written in C# for Unity, remember that values of public variables are overwritten with the unity scene, some are also overwritten with the settings.cfg file which should be placed under 
!!! c:/taglog/settings.cfg !!!

The mainscript getting the information from the gamesideserver, loading the players and setting their position is 
KinectRigClient.cs , which can be found in Directional light/ main camera
It also deals with setting the wooz player selection and calibration from worldcoordinates to gamecoordinates, see background for orientation of the game (tag you are it is directed towards the main entrance)

TagPlayer.cs (in each player in the scene/ in the player prefab)
The players have a script that sets their circle size (pulsating) and keeps track of how long someone has been a tagger etc.
It also has the moveTo method which eventually in the update loop is used to set the position of this player. If you want to use this please add the linear interpolation as we think it will give better collision detection and allows for influencing the lag of tracking players. 

CollisionDetection.cs (in each player/Collider in the scene)
Detects if a player is tagged, or whether he/she has gathered a power-up, effect of shield power up was set here but currently not working.

backgroundimagechanger (in BackgroundImage
Changes the background each tag, one can set the images to be used

loadrecordedsession (in main camera)
Should be usable to load a previous session of tag

powerupgeneration (in main camera)
generates power ups at predefined positions at a predefined interval

chainarrow (in arrows)
creates the number of arrows in the chain based on the distance), 

orbitaround (minions)
moves the minions circling around the player

shieldscript, textscript, 
?, not that important
