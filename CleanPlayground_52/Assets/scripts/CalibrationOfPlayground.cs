using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;


public class CalibrationOfPlayground : MonoBehaviour {

	KinectRigClient kinectRigClientScript;


	//Watch out the values set here or in the GUI will all be overwritten/loaded from a config file open the start of the game!
	public String calibrationSettingsFile  = "C:/taglog/settings.cfg";
	[HideInInspector]public string serverIP = "127.0.0.1";
	[HideInInspector]public int serverPort  = 9009;

	//values needed for mapping the game with the incoming tracker data
	//we assume a lineair projection and incoming trackerdata, therefor these four point are enough for calibration. 
	//in calibration mode "home" and "end" are used
	public Vector2 topLeft;
	public Vector2 bottomRight;
	public Vector2 topLeftGame;
	public Vector2 bottomRightGame;

	//a boolean that decides whether factorshit can be calculated automaticall (yes), or that it should listen to handcalculated possible incorrect settings
	public bool overwriteFactorShit = false;

	//the other variables that are saved in the settings file are thus not really needed for calculating the transformation, but can be used to fidel around with settings seperatly,  you would need to set overwrite with manual in the settings file
	//xfactor, yfactor, xdifferencegw (zeropoint gameworld), ydifferencegw, xdifferenceRW (zeropoint realworld for tracking), ydifferenceRW
	//to convert from meters to world units based on topleft and bottomright positions, without doing the calibration calculation each frame
	[HideInInspector]public  float xFactor,yFactor,zfactor;
	[HideInInspector]public  float xDifferenceRW,yDifferenceRW = 0.0f;
	[HideInInspector]public  float xDifferenceGW,yDifferenceGW = 0.0f;
	[HideInInspector]public  float zDifference = 0.0f;
	[HideInInspector]public  float distanceXGame = 1.0f;
	[HideInInspector]public  float distanceYGame = 1.0f;
	public bool xIsY = false;
	public bool mirrorXPosition;
	public bool mirrorYPosition;
	public bool drawOnlyWithinBounds = false;


	//the ID use to calibrate the playground in the calibration mode, accesed by presseing c during game time
	public int calibrationID = 0;
	public bool calibration = false;
	System.Object thisSettingsFileLock = new System.Object();

	//for feedback of calibration
	//constantly updated values of the incoming player positions
	[HideInInspector]public Vector3 lastIncomingPosition;

	public GameObject calibrationtext;
	//public is ugly access from wooz as well:
	public string calibrationTextStateInfo; 
	public string lastCalibrationText;


	//TODO when should this bbe done?  before the main kinectrigclient right?
	// Use this for initialization
	void Start () {
		//do we need to have that for initmapping?
		kinectRigClientScript = this.GetComponent("KinectRigClient") as KinectRigClient;
		initMapping();

		//init to zero
		lastIncomingPosition = Vector3.zero;
		//assume calibration without touching 0-9
		calibrationID = 0;
		//assume no calibration mode at start
		calibration = false; 
	}

	public void initMapping() {
		
		//no calibrationtext
		calibrationTextStateInfo = "";
		//standard select the first player aka id-0
		calibrationID = 0;
		//the status line of the calibration text is set, but only added in calibration mode
		lastCalibrationText = "no calibration text \n";
		
		//before starting the server load the calibration:
		//loads the settings file if it exist otherwise the settings from unity will be used
		loadCalibrationFromFile();
		
		//at least once set the mapping, following changes have to be done by pressing M in calibration mode (without pressing x or y as these will set other values)
		setPlaygroundMapping();

	}

	
	// Update is called once per frame
	//We need to run this after we run the kinectrigclient
	//except at startup when we need to load the calibration file...
	void Update () {
		//TODO we used to check if incoming message was correct and set these to either not parsed or a value
		//we parse each line, starting with the older and then the newer etc,
		//we need to do this for every incoming line as each player will be placed on a new line
		//suggestion for future work: use a check for the timestamp e.g. a framenumber and  anticipate or simply log if something goes wrong, especially now we switched from the tcp to an udp type of connection
		//if (!wooz)
		//	bool nolineparsed = true;
		//if (msg.Length > 3)
		//	nolineparsed = false; 
		// afterwards:
		//string isLinesEmpty = "";
		//if (nolineparsed)
		//	isLinesEmpty = "no line parsed";

		
		//in calibration mode, switched by pressing "c" enter calibration show calibration information otherwise let the text for calibration dissapear. 
		if (calibration)// && id == calibrationID)
		{

			//we no longer need to check if the players is allowed/is actually there
			//if (tagPlayerGameObjects.Length>calibrationID && checkWooz)
			if (kinectRigClientScript.overWriteWithWooz)
			{
				//TODO WOOZ doesnt send this information to calibration
				lastCalibrationText =
						"TL.x" + floatRoundFunction(topLeftGame.x,4) + ", TL.y" + floatRoundFunction(topLeftGame.y,4) + "\n" 
						+ "BR.x" + floatRoundFunction(bottomRightGame.x,4) + ", BR.y" + floatRoundFunction(bottomRightGame.y,4) + "\n"  
						+ "TLW.x" + floatRoundFunction(topLeft.x,4) + ", TLW.y" + floatRoundFunction(topLeft.y,4) + "\n" 
						+ "BRW.x" + floatRoundFunction(bottomRight.x,4) + ", BRW.y" + floatRoundFunction(bottomRight.y,4) + "\n"
						+ "X:" + floatRoundFunction(lastIncomingPosition.x,4).ToString()  + "Y:" + floatRoundFunction(lastIncomingPosition.y,4).ToString() + "Z:" + floatRoundFunction(lastIncomingPosition.z,4).ToString() + "\n"
						+ calibrationTextStateInfo;
			}
			else 
			{
				lastCalibrationText = 
					  "X:" + lastIncomingPosition.x.ToString() + "\n" 
					+ "Y:" + lastIncomingPosition.y.ToString() + "\n"
					+ "xf" + xFactor + "\n"
					+ "yf" + yFactor + "\n"
					+ "xdR" + xDifferenceRW + "\n"
					+ "ydR" + yDifferenceRW + "\n"
					+ "xdG" + xDifferenceGW + "\n"
					+ "ydG" + yDifferenceGW + "\n"
					+ calibrationTextStateInfo;
			}

			calibrationtext.GetComponent<GUIText>().text = lastCalibrationText;
		}
		else 
		{
			calibrationtext.GetComponent<GUIText>().text = "";
		}
	}


	public void loadCalibrationFromFile()
	{
		//the pareameters are used in the setPlaygroundMapping that is called in the updat in calibrate modus. the only way to call this method is in the calibrate mode
		FileInfo theSourceFile = null;
		//StringReader reader2 = null; 
		StreamReader reader2 = null; 
		
		//theSourceFile = new FileInfo (Application.dataPath + "/puzzles.txt");
		
		//no proper handling of file open in the fileinfo line etc.
		lock(thisSettingsFileLock) {
			
			theSourceFile = new FileInfo (calibrationSettingsFile);
			
			if (!overwriteFactorShit)
			{
				
				if ( theSourceFile != null && theSourceFile.Exists )
					reader2 = theSourceFile.OpenText();
				
				if ( reader2 == null )
				{
					Debug.Log("config file not found or not readable");
				}
				else
				{
					String textLine = null; 
					// Read each line from the file
					//while( (textLine = reader2.ReadLine()) != null )
					
					Debug.Log("open calibration file without overwrite first");
					for(int i = 0; i<4;i++)
					{
						if ((textLine = reader2.ReadLine()) != null)
						{
							string[] msg = textLine.Split('\t');
							if (msg.Length == 4 && i==0) {
								topLeft.x = int.Parse (msg [0])/ 10000.0f;
								topLeft.y= int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==1) {
								bottomRight.x = int.Parse (msg [0])/ 10000.0f;
								bottomRight.y= int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==2) {
								topLeftGame.x = int.Parse (msg [0])/ 10000.0f;
								topLeftGame.y= int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==3) {
								bottomRightGame.x = int.Parse (msg [0])/ 10000.0f;
								bottomRightGame.y= int.Parse (msg [1])/ 10000.0f;
							}
						}
						else
						{
							Debug.Log("no 4 lines found");
						}
					}
					//Debug.Log("-->" + textLine);
					calibrationTextStateInfo = "opened file";
					
					for(int i = 4; i<10;i++)
					{
						if ((textLine = reader2.ReadLine()) != null)
						{
							string[] msg = textLine.Split('\t');
							
							//we need to check if overwrite is true here and also we need the one manual setting whether x and y have been flipped
							//so even if we only use four values we still need the ten lines of information
							if (msg.Length == 4 && i==8) {
								print("read calibration file xdiff");
								Debug.Log("open calibration file, iterate rest");
								int tempxisy = int.Parse (msg [0]);
								if (tempxisy == 1)
									xIsY = true;
								else
									xIsY = false;
								
								int tempOverwriteFactor = int.Parse (msg [1]);
								if (tempOverwriteFactor == 1)
								{
									overwriteFactorShit = true;
									Debug.Log("read line and will overwritefactorshit");
								}
								else
								{
									overwriteFactorShit = false;
									Debug.Log("read line and will skip overwritefactorshit");
								}
							}
							else if (msg.Length == 4 && i==9) {
								Debug.Log("read calibration server load port and ip");
								//TODO load this seperatly in kinectrigclient so we are not dependent on calibraition to start the kinectrig?
								serverPort = int.Parse (msg [0]);
								serverIP = msg[1];
							}
						}
						else
							Debug.Log("no 10 lines found");
					}
				}
				//to check, what happens if the file is not there?
				reader2.Close();
			}
			
			//even if overwritefactorshit was off at the start of this function (before checking the settings file), we will overwrite if it is set to true in the settings file
			//so we need to run again if overwrite was true;
			if(overwriteFactorShit)
			{
				if ( theSourceFile != null && theSourceFile.Exists )
					reader2 = theSourceFile.OpenText();
				
				if ( reader2 == null )
				{
					Debug.Log("config file not found or not readable");
				}
				else
				{
					String textLine = null; 
					for(int i = 0; i<10;i++)
					{
						if ((textLine = reader2.ReadLine()) != null)
						{
							string[] msg = textLine.Split('\t');
							if (msg.Length == 4 && i==0) {
								print("line topleft start");
								topLeft.x = int.Parse (msg [0])/ 10000.0f;
								topLeft.y= int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==1) {
								bottomRight.x = int.Parse (msg [0])/ 10000.0f;
								bottomRight.y= int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==2) {
								topLeftGame.x = int.Parse (msg [0])/ 10000.0f;
								topLeftGame.y= int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==3) {
								print("line bottomrightgame");
								bottomRightGame.x = int.Parse (msg [0])/ 10000.0f;
								bottomRightGame.y= int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==4) {
								print("line xfact");
								xFactor = int.Parse (msg [0])/ 10000.0f;
								yFactor = int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==5) {
								print("line xdiff");
								xDifferenceGW = int.Parse (msg [0])/ 10000.0f;
								yDifferenceGW = int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==6) {
								print("line xdiff");
								xDifferenceRW = int.Parse (msg [0])/ 10000.0f;
								yDifferenceRW = int.Parse (msg [1])/ 10000.0f;
							}
							else if (msg.Length == 4 && i==7) {
								print("line mirror");
								int tempx = int.Parse (msg [0]);
								if (tempx == 1)
									mirrorXPosition = true;
								else
									mirrorXPosition = false;
								int tempy = int.Parse (msg [1]);
								if (tempy == 1)
									mirrorYPosition = true;
								else
									mirrorYPosition = false;
							}
							else if (msg.Length == 4 && i==8) {
								print("line xdiff");
								int tempxisy = int.Parse (msg [0]);
								if (tempxisy == 1)
									xIsY = true;
								else
									xIsY = false;
								int tempOverwriteFactor = int.Parse (msg [1]);
								if (tempOverwriteFactor == 1)
									overwriteFactorShit = true;
								else
									overwriteFactorShit = false;
							}
							else if (msg.Length == 4 && i==9) {
								print("line gamedistance");
								serverPort = int.Parse (msg [0]);
								serverIP = msg[1];
							}
						}
						else
						{
							Debug.Log("no 10 lines found");
						}
					}
				}
				//Debug.Log("-->" + textLine);
				calibrationTextStateInfo = "opened file";
				Debug.Log("opened file succesfully");
				reader2.Close();
				Debug.Log("r");
			}
		}
	}
	
	
	public void saveCalibrationToFile()
	{
		// create a writer and open the file
		TextWriter tw = new StreamWriter(calibrationSettingsFile);
		// write a line of text to the file, now always save all information,, sio it can be loaded in any case. whettehr it is about overwrite factor shitr or not
		//		if(!overwriteFactorShit)
		//		{
		//			String lineToPass = topLeft.x*10000.0f + "\t" + topLeft.y*10000.0f + "\t" + "topleft.x*10e4" + "\t" + "topleft.y*10e4" + "\n" + 
		//				bottomRight.x*10000.0f + "\t" +bottomRight.y*10000.0f + "\t" + "bottomRight.x*10e4" + "\t" + "bottomRight.y*10e4"  + "\n" +
		//				topLeftGame.x*10000.0f + "\t" +topLeftGame.y*10000.0f  + "\t" + "topLeftGame.x*10e4" + "\t" + "topLeftGame.y*10e4" + "\n" + 	
		//				bottomRightGame.x*10000.0f + "\t" +bottomRightGame.y*10000.0f + "\t" + "bottomRightGame.x*10e4" + "\t" + "bottomRightGame.y*10e4";
		//			tw.WriteLine(lineToPass);
		//		}
		//		else
		//		{
		String lineToPass2 = floatRoundFunction(topLeft.x,4)*10000.0f + "\t" + floatRoundFunction(topLeft.y,4)*10000.0f + "\t" + "topleft.x*10e4" + "\t" + "topleft.y*10e4" + "\n" + 
			floatRoundFunction(bottomRight.x,4)*10000.0f + "\t" +floatRoundFunction(bottomRight.y,4)*10000.0f + "\t" + "bottomRight.x*10e4" + "\t" + "bottomRight.y*10e4"  + "\n" +
				floatRoundFunction(topLeftGame.x,4)*10000.0f + "\t" +floatRoundFunction(topLeftGame.y,4)*10000.0f  + "\t" + "topLeftGame.x*10e4" + "\t" + "topLeftGame.y*10e4" + "\n" + 	
				floatRoundFunction(bottomRightGame.x,4)*10000.0f + "\t" +floatRoundFunction(bottomRightGame.y,4)*10000.0f + "\t" + "bottomRightGame.x*10e4" + "\t" + "bottomRightGame.y*10e4"  + "\n" +
				floatRoundFunction(xFactor,4)*10000.0f + "\t" + floatRoundFunction(yFactor,4)*10000.0f + "\t" + "xfactor*10e4" + "\t" + "yfactor.y*10e4"  +  "\n" +
				floatRoundFunction(xDifferenceGW,4)*10000.0f + "\t" + floatRoundFunction(yDifferenceGW,4)*10000.0f + "\t" + "xDifferencGW*10e4" + "\t" + "yDifferencGW*10e4"  + "\n" +
				floatRoundFunction(xDifferenceRW,4)*10000.0f + "\t" + floatRoundFunction(yDifferenceRW,4)*10000.0f + "\t" + "xDifferencRW*10e4" + "\t" + "yDifferencRW*10e4"  + "\n" +
				booleanToFloat(mirrorXPosition) + "\t" + booleanToFloat(mirrorYPosition) + "\t" + "<float> mirrorx" + "\t" + "<float> mirrorx"  + "\n" +
				booleanToFloat(xIsY) + "\t" + booleanToFloat(overwriteFactorShit) + "\t" + "<float> switch/flip xandy" + "\t" + "<float> overwrite with manual set factors instead off cornerbased"  + "\n" +
				serverPort + "\t" + serverIP + "\t" + "serverPort" + "\t" + "serverIP"  + "\n";
		// switch/flip xandy
		// overwritefactorshit
		tw.WriteLine(lineToPass2);
		//		}
		
		tw.Close();
		calibrationTextStateInfo = "saved log file";
	}



	//set parameters to map the distance in meters to game world coordinates
	//called from the keyhandler in the callibration mode by pressing M
	public void setPlaygroundMapping() 
	{
		
		if(!overwriteFactorShit)
		{
			//TODO/WIP smart implementation
			
			
			bool mirrorXPositionInMethod = false;
			bool mirrorYPositionInMethod = false;
			
			xDifferenceGW = topLeftGame.x;
			if (topLeftGame.x>bottomRightGame.x)
			{
				mirrorXPositionInMethod = true;
				distanceXGame = topLeftGame.x - bottomRightGame.x; 
			}
			else
			{
				mirrorXPositionInMethod = false;
				distanceXGame = bottomRightGame.x - topLeftGame.x;
			}
			
			yDifferenceGW = topLeftGame.y;
			if (topLeftGame.y > bottomRightGame.y)
			{
				mirrorYPositionInMethod = true;
				distanceYGame = topLeftGame.y - bottomRightGame.y;
			}
			else
			{
				mirrorYPositionInMethod = false;
				distanceYGame =  bottomRightGame.y - topLeftGame.y;
			}
			
			//check for zero distances to prevent dividing by zero
			if (distanceXGame == 0)
			{
				distanceXGame = 1.0f;
				print("distance X in game not set properly!");
			}
			if (distanceYGame == 0)
			{
				distanceYGame = 1.0f;
				print("distance Y in game not set properly!");
			}
			
			xDifferenceRW = topLeft.x;
			if (topLeft.x > bottomRight.x)
			{
				xFactor = distanceXGame/(topLeft.x - bottomRight.x); 
				mirrorXPosition = !mirrorXPositionInMethod;
			}
			else
			{
				xFactor = distanceXGame/(bottomRight.x - topLeft.x); 
				mirrorXPosition = mirrorXPositionInMethod;
			}
			
			yDifferenceRW = topLeft.y;
			if(topLeft.y > bottomRight.y)
			{
				yFactor = distanceYGame/(topLeft.y - bottomRight.y); 
				mirrorYPosition = !mirrorYPositionInMethod;
			}
			else
			{
				yFactor = distanceYGame/(bottomRight.y - topLeft.y); 
				mirrorYPosition = mirrorYPositionInMethod;
			}
			
			//probably use realword y from the settings as x value 
			//and same for realword x
		}
		else
		{
			//set the factor and distances once by using calibration and turning off overwritefactorshit [U].
			//then change manually no reason to know what mapping should be done here
		}

		//save to the KinectRigClient
		sendToKinectRigClient();
	}

	public void setLastIncomingWoozPosition(Vector3 posIn) {
		lastIncomingPosition.x = posIn.x;
		//WATCH OUT the values y and z are switched with respect to the tracker
		lastIncomingPosition.y = posIn.z;
		lastIncomingPosition.z = posIn.y;
	}


	//run from the client every frame it also sets the last incoming values needed for the calibration
	////getGameWorldPositionsFromTrackingValues(realWorldCoordinateX,realWorldCoordinateY,realWorldCoordinateZ,id,drawWithinBounds);
	public Vector3 getGameWorldPositionsFromTrackingValues(float xin, float yin, float zin, int id) {
		//we need some positions
		float realWorldCoordinateX, realWorldCoordinateY, realWorldCoordinateZ, calibratedRealWorldCoordinateX, calibratedRealWorldCoordinateY, calibratedRealWorldCoordinateZ;

		if (xIsY)
		{
			realWorldCoordinateX = yin;
			realWorldCoordinateY = xin;
			realWorldCoordinateZ = zin;
		}
		else
		{
			realWorldCoordinateX = xin;
			realWorldCoordinateY = yin;
			realWorldCoordinateZ = zin;
		}

		calibratedRealWorldCoordinateX = realWorldCoordinateX  - xDifferenceRW;
		calibratedRealWorldCoordinateY = realWorldCoordinateY  - yDifferenceRW;
		calibratedRealWorldCoordinateZ = realWorldCoordinateZ;

		//take into account the translation form meters to pixels/worldunits
		//the actual coordinates used in the game are set here:
		Vector3 returnPosition;
		//for some uses you might want to measure distance from floor instead from the Kinect
		//this is not done here 
		//float z = (realWorldCoordinateZ / 10000.0f);
		returnPosition.z = realWorldCoordinateZ;

		if (mirrorXPosition) 
			returnPosition.x = xDifferenceGW - calibratedRealWorldCoordinateX *xFactor;
		else
			returnPosition.x = xDifferenceGW + calibratedRealWorldCoordinateX *xFactor;
		
	
		if (mirrorYPosition) 
			returnPosition.y = yDifferenceGW - calibratedRealWorldCoordinateY *yFactor;
		else
			returnPosition.y = yDifferenceGW + calibratedRealWorldCoordinateY *yFactor;

		if (id == calibrationID)
		{
			lastIncomingPosition.x = realWorldCoordinateX;
			lastIncomingPosition.y = realWorldCoordinateY;
			lastIncomingPosition.z = realWorldCoordinateZ;
		}

		//TODO add a little extra in the draw within bounds e.g. 0.5M which is harder to do in pixels...
		//we need to know the actual min and max values to compare to, this depends on the gameworld setup
		if (drawOnlyWithinBounds)
		{
			float minX, maxX,minY,maxY;
			if (bottomRightGame.x < topLeftGame.x)
			{
				minX = bottomRightGame.x;
				maxX = topLeftGame.x;
			}
			else
			{
				minX = topLeftGame.x;
				maxX = bottomRightGame.x;
			}
			
			if (bottomRightGame.y < topLeftGame.y)
			{
				minY = bottomRightGame.y;
				maxY = topLeftGame.y;
			}
			else
			{
				minY = topLeftGame.y;
				maxY = bottomRightGame.y;	
			}
			
			//check if it is within limits and move when it is
			//in the game world the "y" direction is denoted as z-positions, however this is allready taken care if in the moveTo method in the tagplayerscript
			if (returnPosition.x<maxX && returnPosition.y<maxY && returnPosition.x>minX && returnPosition.y>minY)
			{
				//TRACKER BUG REPAIR
				if(xin==0.0f&& yin ==0.0f&&zin<0)
				{
					returnPosition.x = 0.0f;
					returnPosition.y = 0.0f;
					returnPosition.z = 0.0f;
					return returnPosition;
				}
				else{
					return returnPosition;
				}
			}
			else
			{
				//in the calling method later don't draw "empty positions" 
				returnPosition.x = 0.0f;
				returnPosition.y = 0.0f;
				returnPosition.z = 0.0f;
				return returnPosition;
			}
		}

		return returnPosition;
	}

	//TODO probably not needed
	private void sendToKinectRigClient() {
		kinectRigClientScript.serverPort = serverPort;
		kinectRigClientScript.serverIP = serverIP;

	}

	public static float floatRoundFunction(float value, int digits)
	{
		float mult = Mathf.Pow(10.0f, (float)digits);
		return Mathf.Round(value * mult) / mult;
	}
	
	public static float booleanToFloat(bool b)
	{
		if (b)
			return 1.0f;
		else
			return 0.0f;
	}

}
