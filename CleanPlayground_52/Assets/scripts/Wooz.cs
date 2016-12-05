using UnityEngine;
using System.Collections;

public class Wooz : MonoBehaviour {

	//players:
	//http://forum.unity3d.com/threads/46032-Array-of-GameObjects-solved
	//will be set from kinectrigclient
	[HideInInspector] public GameObject[] playersWooz; //= new GameObject[10];
	[HideInInspector] public bool loadedGameObjects = false;

	public bool woozGame = true;
	private bool firstrun = true;
	private bool previousWoozGame;
	private bool multiplySwitch = false;
	private float deltaMultiplierSlow = 5.0f; 
	private float deltaMultiplierQuick = 50.0f;

	private GameObject currentplayer;
	public float deltatimemultiplier = 100.0f;

	KinectRigClient thisClient;
	Player playerScript;
	CalibrationOfPlayground calibrationOfPlaygroundScript;

	// Use this for initialization
	void Start () {
		thisClient = this.GetComponent("KinectRigClient") as KinectRigClient;
		calibrationOfPlaygroundScript = this.GetComponent("CalibrationOfPlayground") as CalibrationOfPlayground;

		previousWoozGame = woozGame;
		thisClient.setOverwriteWithWooz(woozGame);
		deltatimemultiplier = deltaMultiplierSlow;
		firstrun = true;
		multiplySwitch = false;
		try{
			currentplayer = playersWooz[0];
		}
		catch(System.Exception e)
		{
			Debug.Log("didnot initiate");
			Debug.Log(e.ToString());
			Application.Quit();
		}
	}
	
	// Update is called once per frame
	void Update () {

		if (woozGame)
		{
			//??this is also done in kinectRigClienct!
			keySelectPlayerInputHandler();
			keyMovePlayerInputHandler();
		}

		if (firstrun || (woozGame!=previousWoozGame)){
			thisClient.setOverwriteWithWooz(woozGame);
			previousWoozGame = woozGame;

			//be sure that the order etc are the same as in the kinectrig class
			//made this set from kinectrigclient at startup, 
			//we assume no player prefabs are generated at gametime, we simply use the number in the scene instead and let those dissapear etc.
			//playersWooz=thisClient.tagPlayerGameObjects;

			//the tagplayerscript needs to know whether it should listen to the server, for instance to prevent it from dieing
			//this partly dealt with in the client as well, as it won't read the server if it is in wooz mode
			//TODO make it only dependent on this script, dont allow movement in tagplayer with moveTo if it is in WOOZ mode
			for(int i=0;i<playersWooz.Length;i++)
			{
				playerScript = playersWooz[i].GetComponent("Player") as Player;
				playerScript.woozIsOn = woozGame;
			}
			firstrun = false;
		}
	}

	//TODO move to keyHandling?
	void keyMovePlayerInputHandler(){
		//bloxBall.transform.localPosition = Vector3.Lerp(bloxBall.transform.localPosition, targetPos, deltatimemultiplier*Time.deltaTime);
		Vector3 moveVector;
		moveVector = Vector3.zero;
		if (Input.GetKeyUp(KeyCode.D))
		{
			multiplySwitch = !multiplySwitch;
			if (multiplySwitch)
			{
				deltatimemultiplier = deltaMultiplierQuick;
				calibrationOfPlaygroundScript.calibrationTextStateInfo = "set to highspeed Wooz";
			}
			else
			{
				deltatimemultiplier = deltaMultiplierSlow;
				calibrationOfPlaygroundScript.calibrationTextStateInfo = "set to normal speed Wooz";
			}
		}

		if(!Input.GetKey(KeyCode.X) && !Input.GetKey(KeyCode.Y) && !Input.GetKey(KeyCode.Q) && !Input.GetKey(KeyCode.S))
		{
			if (Input.GetKey(KeyCode.UpArrow))
			{
				moveVector.z = 0.1f;
			}

			if (Input.GetKey(KeyCode.DownArrow))
			{
				moveVector.z = -0.1f;
			}

			if (Input.GetKey(KeyCode.RightArrow))
			{
				moveVector.x = 0.1f;
			}

			if (Input.GetKey(KeyCode.LeftArrow))
			{
				moveVector.x = -0.1f;
			}

			if (moveVector != Vector3.zero)
			{
				playerScript = currentplayer.GetComponent("Player") as Player;
				playerScript.updateLastDetectionTime();

				currentplayer.transform.localPosition = Vector3.Lerp(currentplayer.transform.localPosition, currentplayer.transform.localPosition+moveVector,  deltatimemultiplier*Time.deltaTime);
			
				//Notify the calibrationOfPlayground of this position of calibration
				calibrationOfPlaygroundScript.setLastIncomingWoozPosition(currentplayer.transform.localPosition);
			}
		}
	}

	//even we are not in the calibrationmode the wooz player should be the same as the calibration player if the user would switch to the right player and then enters calibration mode.
	//TODO move to keyhandler? this is keyhandling
	void keySelectPlayerInputHandler(){

		if (Input.GetKeyUp(KeyCode.Alpha0))
		{
			print("0 key was released");
			currentplayer = playersWooz[0];
			calibrationOfPlaygroundScript.calibrationID = 0;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 0";
		}

		if (Input.GetKeyUp(KeyCode.Alpha1))
		{
			print("1 key was released");
			currentplayer = playersWooz[1];
			calibrationOfPlaygroundScript.calibrationID = 1;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 1";
		}

		if (Input.GetKeyUp(KeyCode.Alpha2))
		{
			print("2 key was released");
			currentplayer = playersWooz[2];
			calibrationOfPlaygroundScript.calibrationID = 2;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 2";
		}

		if (Input.GetKeyUp(KeyCode.Alpha3))
		{
			print("3 key was released");
			currentplayer = playersWooz[3];
			calibrationOfPlaygroundScript.calibrationID = 3;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 3";
		}

		if (Input.GetKeyUp(KeyCode.Alpha4))
		{
			print("4 key was released");
			currentplayer = playersWooz[4];
			calibrationOfPlaygroundScript.calibrationID = 4;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 4";
		}

		if (Input.GetKeyUp(KeyCode.Alpha5))
		{
			print("5 key was released");
			currentplayer = playersWooz[5];
			calibrationOfPlaygroundScript.calibrationID = 5;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 5";
		}

		if (Input.GetKeyUp(KeyCode.Alpha6))
		{
			print("6 key was released");
			currentplayer = playersWooz[6];
			calibrationOfPlaygroundScript.calibrationID = 6;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 6";
		}

		if (Input.GetKeyUp(KeyCode.Alpha7))
		{
			print("7 key was released");
			currentplayer = playersWooz[7];
			calibrationOfPlaygroundScript.calibrationID = 7;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 7";
		}

		if (Input.GetKeyUp(KeyCode.Alpha8))
		{
			print("8 key was released");
			currentplayer = playersWooz[8];
			calibrationOfPlaygroundScript.calibrationID = 8;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 8";
		}

		if (Input.GetKeyUp(KeyCode.Alpha9))
		{
			print("9 key was released");
			currentplayer = playersWooz[9];
			calibrationOfPlaygroundScript.calibrationID = 9;
			calibrationOfPlaygroundScript.calibrationTextStateInfo = "selected in Wooz 9";
		}
	}


	public void AssignPlayerGameObjects(GameObject[] players)
	{
		if(!loadedGameObjects && players != null)
		{
			//we currently assume the number of players does not change during the GAME, 
			//the players that "dissapear" are simply set to another location or as set to invisible
			//we need to keep track of tehse scripts nonetheless.
			playersWooz = players;
			int i=0;
			for(i=0;i<playersWooz.Length;i++)
			{
				playerScript = playersWooz[i].GetComponent("Player") as Player;
				playerScript.woozIsOn = woozGame;
			}
			loadedGameObjects = true;
		}
		else if (players == null)
		{
			Debug.Log("players are empty in wooz!");
			Application.Quit();		
		}
	}

}
