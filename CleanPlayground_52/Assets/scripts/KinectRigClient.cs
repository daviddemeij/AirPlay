using UnityEngine;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Text;
using System.Threading;
using System;

//for lists
using System.Collections.Generic;

public class KinectRigClient : MonoBehaviour {

	//we read incoming data based on setting file which is called from another class: CalibrationOfPlayground
	//settings will be overwritten by a settings.cfg file at the start. By default the settings.cfg should be located at c:\taglog/settings.cfg
	//in this file, also to be found in the root folder, there are 20 variables
	//the one remaining we need here is the server that should be local as we use the tcp protocol in c#, but need UDP between computers to prevent lag
	[HideInInspector]public string serverIP = "127.0.0.1";
	[HideInInspector]public int serverPort  = 9009;

	//the tagplayers, no longer called just "gameObjects" 

	////the size of this array is set/dependent on the number of Player scripts found at the start of this script, this is done in LoadPlayers function
	//this is set from the GameSettings file, 
	//WATCH OUT the game settings  script should be execured before the kinectrigclient, Edit--> project settings --> Script Execution Order ; drag the script in set it to e.g. -5 to run before default time.
	[HideInInspector]public GameObject[] playerGameObjects; 

	//INCOMING TRACKER VALUES ARE TRANSFORMED AND CALIBRATION IS SET IN CALIBRATION OF PLAYGROUND
	//to be sure we run calibration before the server
	CalibrationOfPlayground calibrationScript;

	//you might want to only keep alive and draw the players that are within bounds of the game
	//public bool drawWithinBounds = true;

	//variables for the local TCP/IP connection
	//instead create this allocation in the thread itself
	//	TcpClient tcpClient;
	//	//StreamReader
	//	byte[] buffer = new byte[1024];
	NetworkStream theStream;
	Thread mThread;

	//used for sharing the data between the two threads, the network thread and the update loop
	//basically stores the raw position information 
	string rdata;

	//used to exit the loops in the network thread, to re-establish connection etc.
	bool mRunning;
	bool mRunningClientOuterLoop;

	[HideInInspector]public bool overWriteWithWooz = false;

	//towards threadsafety we can use locks, however, its behavior is not 100% guaranteed and its use will slow down the process
	System.Object thisLock = new System.Object();
	System.Object thisDataLock = new System.Object();

//	Wooz woozScript;

	//the generic tagplayer script
	Player playerScript, playerScriptSingleWooz;

	//LOG STUFF in seperate class
	//this also requires the appropriate order of running and compiling this project
	//script execution order Logger first: -100, rest on default time
	//Edit -> project settings -> script execution order -> Logger 100, other scripts on default time.
	[HideInInspector]public Logger loggerScript;

	//press Z to reset the connection 
	public bool resetClientBool = false;
	private int lastNumberOfPlayers = 0;
	
	void Start () {
		Debug.Log("start rigclient");
		Application.runInBackground = true;

		//we don't break the networkconnection (once its build) at the start
		resetClientBool = false;

		//no incoming data yet
		rdata = "";

		//calibration removed here
		calibrationScript = this.GetComponent("CalibrationOfPlayground") as CalibrationOfPlayground;
		//calibrationScript.initMapping();


		lastNumberOfPlayers = playerGameObjects.Length;

		//load logger
		loggerScript = this.gameObject.GetComponent("Logger") as Logger;

		//create a client listening to the tracker in a second thread
		ThreadStart ts = new ThreadStart (ReceiveData);
		mThread = new Thread(ts);
		mRunning = true;
		mRunningClientOuterLoop = true;
		mThread.Start();
	}


	//The main loop
	//
	public void Update() {
		bool checkWooz = false;
	
		//used to set a lock but this might slow it down, while the wooz is only read in another thread fairly irregularly
		//lock(thisLock) {
		if (overWriteWithWooz)
			checkWooz = true;

		//if not in wooz mode, get the latest server info and deal with it
		if (!checkWooz )	
		{
			//this gets the latest received data
			string localRData = changeOrGetData(null, false);
			//every line is a seperate player (ID)
			string[] lines = localRData.Split('\n');

			if (localRData.Length != 0)
			{
				foreach (string line in lines) 
				{

					//changed to comma seperated lines to be compatible with some programs that cant use floats but need (long) int
					string[] msg = line.Split(',');

					//if you want to check if the data is send
					//Debug.Log(msg[0]);

					//assume the message protocol is 4 numbers, otherwise it might be a not yet completly parsed last line
					//rather skip it and wait for the next line than trying to use it with missing info
					//changed from ==4 to >3, on 4-12-14, before uploading (or more for another protocol)
					if (msg.Length > 3) {
						//TODO optimization check if all values should be floats here especially if we are going to test on 0.0f
						float realWorldCoordinateX, realWorldCoordinateY, realWorldCoordinateZ;
						Vector3 gameWorldCoordinates;

						realWorldCoordinateX = int.Parse (msg [1])/ 10000.0f;
						realWorldCoordinateY = int.Parse (msg [2])/ 10000.0f;
						realWorldCoordinateZ = int.Parse (msg [3])/ 10000.0f;
						int id = int.Parse (msg [0]);

						//TODO call function of CalibrationOfPlayground that does this transformation
						//tenths of mm to gameworldcoordinates
						//in the calibrationsofplayground script one can set whether a null value will be given if a location is outside the bounds of the playground
						gameWorldCoordinates = calibrationScript.getGameWorldPositionsFromTrackingValues(realWorldCoordinateX,realWorldCoordinateY,realWorldCoordinateZ,id);

						//make a suggestion to move the player instead of an actual movement. In that way we can use overshoot/a wooz etc.
						if (id < playerGameObjects.Length) {
							//GameObject playertomove = gameObjects[id];
							//at start we assigned these in the same order so 0 has id 0 etc.
							playerScript = playerGameObjects[id].GetComponent("Player") as Player;
							//only actually sets location if single wooz is not selected in tagplayers

							//TODO add padding to these bounds,
							//we shouldn't double the error at the corners of 1 the tracker not working 100% at these edges and 
							//2 the game kicking out players that are on these edges of recognition if they are drawn only within bounds

							//instead of checking for appropriate values incoming, we check for appropriate world coordinates
							//if(realWorldCoordinateX!= 0.0f || realWorldCoordinateY!=0.0f)
														
							//set the target position, in the update of the player it will move towards this target, 
							//WATCH OUT : in this game world, the "y" direction is denoted as z-positions, however this is taken care off in the moveTo method in the tagplayerscript
							if(gameWorldCoordinates.x!= 0.0f || gameWorldCoordinates.y!=0.0f)
								playerScript.moveTo(gameWorldCoordinates.x,gameWorldCoordinates.y,0.0f);

							int fakeinformation = 0;
							//logline protocol: id | X | Y | Z | some information (a boolean stating whether a player was a tagger for the ITP)
							//by calling the logging method a timestamp will be added there resulting in e.g. 
							//340,2,-0.6432,-3.5215,3.493,0
							//if you want to make use of the fake tracker or matlab scripts we advise to keep to the same protocol (or change the node and matlab scripts accordingly)

							//ADDED a check if they are dissappeared in the tracker and have been set to 0.0f , we don't need to save those. 
							if (realWorldCoordinateX != 0.0f || realWorldCoordinateY!=0.0f) {
								loggerScript.LogLineRigClientUpdate(id.ToString()  + "," + realWorldCoordinateX.ToString() + "," + realWorldCoordinateY.ToString() + "," + realWorldCoordinateZ.ToString() + ","  + fakeinformation.ToString());
							}
						}
						else //the id is too big to handle
						{
							//TODO LOGGING IS TO DEPENDENT ON THE GAME TO PUT HERE
							//logline protocol: id | X | Y | Z | tagger == 0!!!!
							//indicate with a 1000+ id that the tagger is not recognised properly as its id is out of bounds
							id =id+1000;

							//send tagbool int== 0;
							//TODO Alejandro check if you agree:
							loggerScript.LogLineRigClientUpdate(id.ToString()  + "," + realWorldCoordinateX.ToString() + "," + realWorldCoordinateY.ToString() + "," + realWorldCoordinateZ.ToString()  + "," + 0);
						}
					}
					else
					{
						//you might want to know that these lines are not complete. Sometimes this also happends after each player block, so after all players have been updated there is an added empty line
						//Debug.Log("message length!=4");
					}
				}
			} //end if localrdata ==0
		
		}
					
		//don't delete half lines of information
		changeOrGetData(null, true);
	}

	/// <summary>
	/// NETWORK THREAD FUNCTIONS
	/// </summary>

	//connect to the server, receive data and sent it to the program
	void ReceiveData() {
		
		TcpClient tcpClient;
		
		//needs to be closed seperatly so global 
		//NetworkStream theStream;
		//need to be global
		//UdpClient udpClient;
		
		
		while(mRunningClientOuterLoop) {
			bool checkWooz = false;
			
			//checkwooz is read from the other thread as well, 
			//but this threadlock is removed in an attempt to speed things up, as it is used here only as long as there is no server, using lock every loop in the main thread is a little bit overcautious
			//lock(thisLock) {
			if (overWriteWithWooz)
				checkWooz = true;
			//}
			
			if (!checkWooz)	{
				
				//first run mrunning is true, if false it allready tried to connect and failed
				if(!mRunning) {
					mRunning = true;
					Debug.Log("turn mRunning backon");
				}
				
				//"Ping" (using a non-blocking method) the client to see if it is ready to connect, as creating a TCPClient will block 
				//and "new TcpClient" does not automaticaally rewake if it is able to create a client
				
				if (tryAPingFirst(serverIP, serverPort, 50))
				{
					try {
						Debug.Log("connect to the tcpclient");
						//this  next line new client will block if no server is there
						tcpClient = new TcpClient (serverIP, serverPort);
						tcpClient.NoDelay = true;
						tcpClient.ReceiveTimeout = 1;
						tcpClient.SendTimeout = 1;
						
						theStream = tcpClient.GetStream ();
						StreamReader sr = new StreamReader(theStream);
						//Removed 4-12-14, before uploading, might help a little in speeding up
						//theStream.ReadTimeout = 3;
						
						string textlineServer = "";
						Debug.Log ("Successfully created TCP client and open the NetworkStream.");
						
						while (mRunning) {
							//Ignbore the following comment for now, if you bump into problems with reconnection have a look at it:
							////bugfix don't check whether it canread if it can't readline it will simply throw an exception and we can proceed, 
							////canread on the other hand might have been responsible for the blocking behavior after disconnection in the previous implementation,
							////now in combination with the added ping, it does automatically reconnect to the server after an error
							
							//read when data is available, try to prevent blocking for longer time
							try {
								if (theStream.CanRead && theStream.DataAvailable) {
									//this was working but equally slow as the readline command, it seems to have to same problem on .Read as it has on .Readline
									//numberOfBytesRead = theStream.Read (buffer, 0, buffer.Length);
									
									//THE former TCP IP PROBLEM
									//it reads until it has a line that is not yet ready
									//THEN the apparently "under the water" used winsock implementation seems to block for a set timeout of 200ms
									//Thread.Sleep(sleepBeforeRead);
									textlineServer = sr.ReadLine() + "\n";
									
									//send the info to a variable, an array of string lines, that is used in the main loop
									changeOrGetData(textlineServer,false);
									//changeOrGetData(returnData,false);
									
								}
								else {
									//keeps coming here even the client doesnt exist anymore
									
									//Removed 4-12-14, before uploading, might help a little in speeding up
									//Thread.Sleep(sleepWhenNoData);
									
									//idealy check on connected but Unity doesn't allow any of that
									if (resetClientBool)
									{
										mRunning = false;
										CloseConnection();
										resetClientBool = false;
									}
								}
								
							} catch (Exception e) {
								Debug.Log ("Error in checking data available: " + e);
								//kickout the connection and don't keep trying to read a errororenous connection
								mRunning = false;
								CloseConnection();
							}
							
						}
					} catch (ThreadAbortException) {
						Debug.Log("Thread Abort Exception");
						mRunning = false;
						CloseConnection();
					} finally {
						mRunning = false;
						CloseConnection();
					}
				}
				else
				{
					//m_tw2.WriteLine("no_server" + i);
				}
			}
		}
		Debug.Log("exit outerloop");
	}

	private bool tryAPingFirst(string strIpAddress, int intPort, int nTimeoutMsec)
	{
		Socket socket = null;
		try
		{
			socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.DontLinger, false);
			
			
			IAsyncResult result = socket.BeginConnect(strIpAddress, intPort, null, null);
			bool success = result.AsyncWaitHandle.WaitOne(nTimeoutMsec, true);
			
			return socket.Connected;
		}
		catch
		{
			Debug.Log("ping failed");
			return false;
		}
		finally
		{
			if (null != socket)
				socket.Close();
		}
		
	}



	/// <summary>
	/// CALLED FROM BOTH THREADS, ALLOWS FOR LOCKING /// </summary>
	/// <returns> the string data from the tracker id,x,y,z if called with nu data from the main loop or returns empty "" if the data is set </returns>
	/// <param name="data"> input data from the tracker or null if called from the main </param>
	/// <param name="throwaway">If set to true the completed incoming trackerinformation (upto the last complete line) will be deleted  <c>true</c> otherwise the tracker information will be added to existing information.</param>

	private string changeOrGetData(string data, bool throwaway)
	{
		
		//will it speed up?
		//lock(thisDataLock)
		///{
		//probably to delete the parsed lines;
		//however...... we are not 100 % sure that a line is not set in the meantime :(
		int rest = rdata.LastIndexOf("\n");
		
		if(throwaway)
		{
			rdata = rdata.Substring(rest+1);
		}
		else if(data!=null)
		{
			rdata +=data;
		}
		else
		{
			return rdata;
		}
		//}
		//if 
		return "";
	}

	
	//will lead to an issue if no stream was started in the first place so we simply catch this situation
	//called when quiting the program and before a new connection attempt is made
	public void CloseConnection() {
		try
		{
			theStream.Close();
		}
		catch(NullReferenceException)
		{
			Debug.Log("null reference exception due to no server started in the first place");
		}
		
	}
	/// <summary>
	/// OTHER MAIN THREAD FUNCTIONS
	/// </summary>



	void StopListening() {
		mRunning = false;
		mRunningClientOuterLoop = false;
	}
	
	void OnApplicationQuit() {
		Debug.Log("application quit in kinectrigclient");
		StopListening ();
		CloseConnection();
		mThread.Join(500);
	}


	//guess it will be called from another thread as Wooz
	public void setOverwriteWithWooz(bool b) {
		lock(thisLock) {
			overWriteWithWooz = b;
		}
	}

}

