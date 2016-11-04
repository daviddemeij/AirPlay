using UnityEngine;
using System.Collections;


//TODO make a seperate scene in which we deal with setting the calibration and use loading on start in an actual game

//for doing calibration simply stand on a corner of the playingfield, once being tracked properly select the track of the player standing in this corner and [press HOME].
//switch to the WOOZ mode [press W], and move the circle to the same corner and underneath the player. [Press HOME]
//Move to the other corner and switch back to the tracker mode [press W] and then [press PageDown] ( be sure you are being tracked and have selected the appropriate player [0-9]
//switch to the WOOZ mode [press W], and move the circle to the same corner as where the  and underneath the player. [Press PAGEDOWN]

//calculate whether to mirror the factor and offsets of the playground etc. [press M]
//if it works export these setting [press E]
////else if it doesnt work probably  the shapes move horizontally when you walk vertically, indicate the X and Y coordinates are switched [press F] 
////now you need to recalculte the paramaters [press M]
////if it works now export the settings [press E]
public class KeyHandling : MonoBehaviour {

	//needed for calibration
	//the ID use to calibrate the playground in the calibration mode, accesed by presseing c during game time
	public int calibrationID = 0;	
	//indicates whether the game is calibration mode or not
	private bool calibration = false;

	CalibrationOfPlayground calibrationScript;
	KinectRigClient kinectRigClientScript;
	Wooz woozScript;

	//allows a Wizard of Oz player to move quicker
	bool speedAdapt = false;

	//this is problematic as it hast to be loaded from the kinectrig where it is loaded during start as well;
	[HideInInspector] public GameObject[] playerGameObjects;

	// Use this for initialization
	void Start () {
		calibrationScript = this.GetComponent("CalibrationOfPlayground") as CalibrationOfPlayground;
		woozScript = this.GetComponent("Wooz") as Wooz;

		//We ORDER when this script will be run/done 
		//This script has to be done after the main kinectrig
		kinectRigClientScript = this.GetComponent("KinectRigClient") as KinectRigClient;

		//TODO perhaps it is better to have a gamestatechecker with the objects as public object to also return the gameobjects from one place
		playerGameObjects = kinectRigClientScript.playerGameObjects;

		speedAdapt = false;
	}
	
	// Update is called once per frame
	void Update () {
		handleKeySettings();
	}

	//TODO in Wooz script there are also some keyhandling settings left
	void handleKeySettings(){
				
		if (Input.GetKeyUp(KeyCode.C))
		{
			calibration = !calibration;
			calibrationScript.calibration = calibration; 
		}
		
		if (Input.GetKeyUp(KeyCode.W))
		{
			woozScript.woozGame = !woozScript.woozGame;  
		}
		
		//reset the connection during the game 
		if (Input.GetKeyUp(KeyCode.Z))
		{
			kinectRigClientScript.resetClientBool = !kinectRigClientScript.resetClientBool;
		}
		
		//if holding g one can select the ghost player by pressiong a number
		if (Input.GetKey(KeyCode.G))
		{
			//print("G key is pressed");
			keySelectPlayerInputHandler(true);
		}
		else if (calibration && !kinectRigClientScript.overWriteWithWooz)
		{
			keySelectPlayerInputHandler(false);
		}

		//setting mirroring should be set automatically by comparing the game coordinates with the trackercoordinates
		//setting this manually is only needed if somethings goes wrong there, although it will be overwritten once a file is loaded!
		if(calibration && Input.GetKey(KeyCode.X) && Input.GetKeyUp(KeyCode.M))
		{
			calibrationScript.mirrorXPosition = !calibrationScript.mirrorXPosition;
		}
		
		if(calibration && Input.GetKey(KeyCode.Y) && Input.GetKeyUp(KeyCode.M))
		{
			calibrationScript.mirrorYPosition = !calibrationScript.mirrorYPosition;
		}
		
		if(calibration && !Input.GetKey(KeyCode.Y) && !Input.GetKey(KeyCode.X) && Input.GetKeyUp(KeyCode.M))
		{
			calibrationScript.setPlaygroundMapping();
		}

		//deal with setting stuff during an allready compiled game
		//use q for topleft, s for bottom right
		//use x and y for setting either the x or y value
		//use up for upping the gamecoordinate and right for upping the realworld value
		//use down for reducing the gamecoordinate and left for reducing the realworld value
		float settingDelta = 0.001f;
		
		//set the topleft and bottomriught with a press of a button
		if(calibration && kinectRigClientScript.overWriteWithWooz && Input.GetKey(KeyCode.Home) )
		{
			calibrationScript.topLeftGame.x = calibrationScript.lastIncomingPosition.x;
			calibrationScript.topLeftGame.y = calibrationScript.lastIncomingPosition.y;
			calibrationScript.calibrationTextStateInfo = "topleft game set to x " + calibrationScript.topLeftGame.x + "and y" + calibrationScript.topLeftGame.y;
			
			//topLeft.y = 
		}
		
		if(calibration && kinectRigClientScript.overWriteWithWooz && Input.GetKey(KeyCode.PageDown) )
		{
			calibrationScript.bottomRightGame.x = calibrationScript.lastIncomingPosition.x;
			calibrationScript.bottomRightGame.y = calibrationScript.lastIncomingPosition.y;
			calibrationScript.calibrationTextStateInfo = "bottomright game set to x " + calibrationScript.bottomRightGame.x + "and y" + calibrationScript.bottomRightGame.y;
			//topLeft.y = 
		}
		
		//set the topleft and bottomriught in real world with a press of a button
		if(calibration && !kinectRigClientScript.overWriteWithWooz && Input.GetKey(KeyCode.Home) )
		{
			//playerScrip = gameObjects[calibrationID].GetComponent("Player") as Player;
			//topLeftGame = playerScript.transform.position;
			
			calibrationScript.topLeft.x  = calibrationScript.lastIncomingPosition.x;
			calibrationScript.topLeft.y = calibrationScript.lastIncomingPosition.y;
			calibrationScript.calibrationTextStateInfo = "topleft world set to x " + calibrationScript.lastIncomingPosition.x + "and y" + calibrationScript.lastIncomingPosition.y;
		}
		
		if(calibration && !kinectRigClientScript.overWriteWithWooz && Input.GetKey(KeyCode.PageDown) )
		{
			//playerScript = gameObjects[calibrationID].GetComponent("Player") as Player;
			//bottomRight = playerScript.transform.position;
			//topLeft.y = 
			calibrationScript.bottomRight.x = calibrationScript.lastIncomingPosition.x;
			calibrationScript.bottomRight.y = calibrationScript.lastIncomingPosition.y;
			calibrationScript.calibrationTextStateInfo = "bottomRight world set to x " + calibrationScript.lastIncomingPosition.x + "and y" + calibrationScript.lastIncomingPosition.y;
		}
		
		if (calibration && Input.GetKey(KeyCode.Q))
		{
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.UpArrow))
			{
				calibrationScript.topLeftGame.x = calibrationScript.topLeftGame.x +settingDelta;
				calibrationScript.calibrationTextStateInfo = "topleftGame : " + calibrationScript.topLeftGame.x;
			}
			
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.DownArrow))
			{
				calibrationScript.topLeftGame.x = calibrationScript.topLeftGame.x -settingDelta;
				calibrationScript.calibrationTextStateInfo = "topleftGame : " + calibrationScript.topLeftGame.y;
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.UpArrow))
			{
				calibrationScript.topLeftGame.y = calibrationScript.topLeftGame.y +settingDelta;
				calibrationScript.calibrationTextStateInfo = "topleftGame : " + calibrationScript.topLeftGame.y;
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.DownArrow))
			{
				calibrationScript.topLeftGame.y = calibrationScript.topLeftGame.y -settingDelta; 
				calibrationScript.calibrationTextStateInfo =  "topleftGame : " + calibrationScript.topLeftGame.y;
			}
			
			//incoming World coordinates using left right
			
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.RightArrow))
			{
				calibrationScript.topLeft.x = calibrationScript.topLeft.x +settingDelta;
				calibrationScript.calibrationTextStateInfo = "topleftRWx : " + calibrationScript.topLeft.x;
			}
			
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.LeftArrow))
			{
				calibrationScript.topLeft.x = calibrationScript.topLeft.x -settingDelta;
				calibrationScript.calibrationTextStateInfo = "topleftx: " + calibrationScript.topLeft.x;
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.RightArrow))
			{
				calibrationScript.topLeft.y = calibrationScript.topLeft.y +settingDelta;
				calibrationScript.calibrationTextStateInfo = "topleftRW y : " + calibrationScript.topLeft.y;
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.LeftArrow))
			{
				calibrationScript.topLeft.y = calibrationScript.topLeft.y -settingDelta; 
				calibrationScript.calibrationTextStateInfo ="topleftRW y: " + calibrationScript.topLeft.y;
			}
		}
		
		
		if (calibration && Input.GetKey(KeyCode.S))
		{
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.UpArrow))
			{
				calibrationScript.bottomRightGame.x = calibrationScript.bottomRightGame.x +settingDelta;
				calibrationScript.calibrationTextStateInfo ="topleftGame : " + calibrationScript.bottomRightGame.x;
			}
			
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.DownArrow))
			{
				calibrationScript.bottomRightGame.x = calibrationScript.bottomRightGame.x -settingDelta;
				calibrationScript.calibrationTextStateInfo ="topleftGame : " + calibrationScript.bottomRightGame.x;
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.UpArrow))
			{
				calibrationScript.bottomRightGame.y = calibrationScript.bottomRightGame.y +settingDelta;
				calibrationScript.calibrationTextStateInfo ="topleftGame : " + calibrationScript.bottomRightGame.y;
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.DownArrow))
			{
				calibrationScript.bottomRightGame.y = calibrationScript.bottomRightGame.y -settingDelta; 
				calibrationScript.calibrationTextStateInfo ="topleftGame : " + calibrationScript.bottomRightGame.y;
			}
			
			//incoming World coordinates using left right
			
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.RightArrow))
			{
				calibrationScript.bottomRight.x = calibrationScript.bottomRight.x +settingDelta;
				calibrationScript.calibrationTextStateInfo ="topleftx : " + calibrationScript.bottomRight.x;
			}
			
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.LeftArrow))
			{
				calibrationScript.bottomRight.x = calibrationScript.bottomRight.x -settingDelta;
				calibrationScript.calibrationTextStateInfo ="topleftx: " + calibrationScript.bottomRight.x;
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.RightArrow))
			{
				calibrationScript.bottomRight.y = calibrationScript.bottomRight.y +settingDelta;
				calibrationScript.calibrationTextStateInfo ="topleftGame y : " + calibrationScript.bottomRight.y;
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.LeftArrow))
			{
				calibrationScript.bottomRight.y = calibrationScript.bottomRight.y -settingDelta; 
				calibrationScript.calibrationTextStateInfo ="topleftGame y: " + calibrationScript.bottomRight.y;
			}
		}
		
		//flip incoming x and y if beamer or setup is rotated this might happen
		if (calibration && Input.GetKeyUp(KeyCode.F))
		{
			calibrationScript.xIsY = !calibrationScript.xIsY;
			calibrationScript.calibrationTextStateInfo = "flip is set to" + calibrationScript.xIsY;
		}

		//turn on and off logging by pressing L
		if (calibration && Input.GetKeyUp(KeyCode.L))
		{
			kinectRigClientScript.loggerScript.logToFile = !kinectRigClientScript.loggerScript.logToFile;
			calibrationScript.calibrationTextStateInfo = "logToFile is set to" + kinectRigClientScript.loggerScript.logToFile;
		}

		//open and use the last saved calibrationfile
		if (calibration && Input.GetKeyUp(KeyCode.O))
		{
			calibrationScript.loadCalibrationFromFile();
		}

		//save the current calibration settings to the settings.cfg file
		if (calibration && Input.GetKeyUp(KeyCode.E))
			calibrationScript.saveCalibrationToFile();

		//overwrite the lineair calibration with a different approach.
		//this basically is a deprecated legacy function, only use when you understand what you are doing
		if (Input.GetKeyUp(KeyCode.U))
		{
			calibrationScript.overwriteFactorShit = !calibrationScript.overwriteFactorShit;
			calibrationScript.lastCalibrationText = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xFactor + " yF:" + calibrationScript.yFactor+ "\n";
		}

		//toggle between changing values quick or accuratly by pressing backspace
		//this will also be used by the wooz mode
		if (Input.GetKeyUp(KeyCode.Backspace))
		{
			speedAdapt = !speedAdapt;
		}

		//when in calibration mode and not setting one of the two corners, allow a combination of a letter and arrow to alter settings directly
		if (calibration && !Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.S))
		{
			//handle the toggle between moving quick or not by pressing backspace
			float speed = 0.0001f;
			if (speedAdapt)
				speed = 0.1f;

			//allow for manual changes in yfactor, xfactor, etc. this is bassically not needed!
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.UpArrow))
			{
				calibrationScript.yFactor= calibrationScript.yFactor+ speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xFactor + " yF:" + calibrationScript.yFactor+ "\n";
			}
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.DownArrow))
			{
				calibrationScript.yFactor= calibrationScript.yFactor- speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xFactor + " yF:" + calibrationScript.yFactor+ "\n";
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.LeftArrow))
			{
				calibrationScript.yDifferenceRW= calibrationScript.yDifferenceRW - speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xDifferenceGW + " yF:" + calibrationScript.yDifferenceGW+ "\n";
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.RightArrow))
			{
				calibrationScript.yDifferenceRW= calibrationScript.yDifferenceRW + speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xDifferenceGW + " yF:" + calibrationScript.yDifferenceGW+ "\n";
			}
			
			//////x 
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.UpArrow))
			{
				calibrationScript.xFactor = calibrationScript.xFactor+ speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + "\n" + "xF:" + calibrationScript.xFactor + " yF:" + calibrationScript.yFactor+ "\n";
			}
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.DownArrow))
			{
				calibrationScript.xFactor= calibrationScript.xFactor - speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xFactor + " yF:" + calibrationScript.yFactor+ "\n";
			}
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.LeftArrow))
			{
				calibrationScript.xDifferenceRW = calibrationScript.xDifferenceRW- speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xDifferenceGW + " yF:" + calibrationScript.yDifferenceGW+ "\n";
			}
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.RightArrow))
			{
				calibrationScript.xDifferenceRW= calibrationScript.xDifferenceRW+ speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xDifferenceGW + " yF:" + calibrationScript.yDifferenceGW+ "\n";
			}
			
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.Period))
			{
				calibrationScript.xDifferenceGW= calibrationScript.xDifferenceGW+ speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xDifferenceGW + " yF:" + calibrationScript.yDifferenceGW+ "\n";
			}
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.Comma))
			{
				calibrationScript.xDifferenceGW= calibrationScript.xDifferenceGW-speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.xDifferenceGW + " yF:" + calibrationScript.yDifferenceGW+ "\n";
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.Period))
			{
				calibrationScript.yDifferenceGW= calibrationScript.yDifferenceGW+ speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.yDifferenceGW + " yF:" + calibrationScript.yDifferenceGW+ "\n";
			}
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.Comma))
			{
				calibrationScript.yDifferenceGW= calibrationScript.yDifferenceGW- speed;
				
				calibrationScript.calibrationTextStateInfo = "overw xy f" + calibrationScript.overwriteFactorShit + "\n" + "xF:" + calibrationScript.yDifferenceGW + " yF:" + calibrationScript.yDifferenceGW+ "\n";
			}
			
			//for mirror x y, we might need to adjust the distance
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.L))
			{
				calibrationScript.distanceXGame= calibrationScript.distanceXGame + speed;
				
				calibrationScript.calibrationTextStateInfo = "xy f" + calibrationScript.overwriteFactorShit + "\n" + "distXGame" + calibrationScript.distanceXGame + "\n";
			}
			if (Input.GetKey(KeyCode.X) && Input.GetKey(KeyCode.K))
			{
				calibrationScript.distanceXGame= calibrationScript.distanceXGame - speed;
				
				calibrationScript.calibrationTextStateInfo = "xy f" + calibrationScript.overwriteFactorShit + "\n" + "distXGame" + calibrationScript.distanceXGame + "\n";
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.K))
			{
				calibrationScript.distanceYGame= calibrationScript.distanceYGame + speed;
				
				calibrationScript.calibrationTextStateInfo = "xy f" + calibrationScript.overwriteFactorShit + "\n" + "distXGame" + calibrationScript.distanceYGame + "\n";
			}
			
			if (Input.GetKey(KeyCode.Y) && Input.GetKey(KeyCode.L))
			{
				calibrationScript.distanceYGame= calibrationScript.distanceYGame - speed;
				
				calibrationScript.calibrationTextStateInfo = "xy f" + calibrationScript.overwriteFactorShit + "\n" + "distYGame" + calibrationScript.distanceYGame + "\n";
			}
			
			
		}
	}
	//end of handlekeys


	//TODO same method as used in Player, call via that class would make sense?
	void keySelectPlayerInputHandler(bool forGhost)
	{
		
		if (Input.GetKeyUp(KeyCode.Alpha0))
		{
			print("0 key was released");
			if (forGhost)
				SelectPlayer(0);
			calibrationID = 0;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 0";
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha1))
		{
			print("1 key was released");
			if (forGhost)
				SelectPlayer(1);
			calibrationID = 1;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 1";
			
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha2))
		{
			print("2 key was released");
			if (forGhost)
				SelectPlayer(2);
			calibrationID = 2;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 2";
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha3))
		{
			print("3 key was released");
			if (forGhost)
				SelectPlayer(3);
			calibrationID = 3;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 3";
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha4))
		{
			print("4 key was released");
			if (forGhost)
				SelectPlayer(4);
			calibrationID = 4;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 4";
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha5))
		{
			print("5 key was released");
			if (forGhost)
				SelectPlayer(5);
			calibrationID = 5;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 5";
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha6))
		{
			print("6 key was released");
			if (forGhost)
				SelectPlayer(6);
			calibrationID = 6;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 6";
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha7))
		{
			print("7 key was released");
			if (forGhost)
				SelectPlayer(7);
			calibrationID = 7;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 7";
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha8))
		{
			print("8 key was released");
			if (forGhost)
				SelectPlayer(8);
			calibrationID = 8;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 8";
		}
		
		if (Input.GetKeyUp(KeyCode.Alpha9)) {
			
			print("9 key was released");
			if (forGhost)
				SelectPlayer(9);
			calibrationID = 9;
			calibrationScript.calibrationID = calibrationID;
			calibrationScript.calibrationTextStateInfo = "selected 9";
		}
	}
	
	void SelectPlayer(int id)
	{
		for (int i=0;(i< playerGameObjects.Length);i++) {
			//GameObject playertomove = gameObjects[id];
			Player playerScriptSingleWooz = playerGameObjects[i].GetComponent("Player") as Player;
			if (i==id)
				playerScriptSingleWooz.singleWoozPlayer = true;
			else
				playerScriptSingleWooz.singleWoozPlayer = false;
			
		}
		
		if (id >= playerGameObjects.Length) {
			//GameObject playertomove = gameObjects[id];
			
			//removed for designlab:
			//playerScriptSingleWooz = gameObjects[id].GetComponent("Player") as Player;
			//}
			//else {
			print("not valid choice for ghost player" + id.ToString() + ",only " + playerGameObjects.Length.ToString()+ "players found in scene");	
		}
	}
}
