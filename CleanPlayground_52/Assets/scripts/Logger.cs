using UnityEngine;
using System.Net;
using System.Collections;
using System.IO;
using System.Text;
using System;

//Logger will execute before default time of execution for scripts
public class Logger : MonoBehaviour {
	
	public bool logToFile = false;
	
	private bool lastLogToFile = false;
	public string logFolderPath = "C:/taglog/";
	public string logFileNameBase = "DIPP";
	private string dateFileName = "";
	
	//for keeping track of players and their positions.
	public bool logPositionsToFile = true;
	private string logFileNamePositions = "";
	private TextWriter m_tw_Positions;
	private bool logFilePositionsGenerated = false;

	//EXAMPLE of two other logfiles have been left in throughout the code in order to be able to build one easilty yourself
	//for keeping count of the peculiar updates
//	private bool logUpdateToFile = true;
//	private string logFileNameUpdate = "";
//	private TextWriter m_tw_Update;
//	private bool logFileUpdateGenerated = false;
//	
//	//for keeping count of the broken paddles in Dual_Wield gameplay
//	public bool logPaddlesToFile = true;
//	private string logFileNamePaddles = "";
//	private TextWriter m_tw_Paddles;
//	private bool logFilePaddlesGenerated = false;
	
	//StartTime of the Logger script
	DateTime startTime;
	
	
	// Use this for initialization
	void Start () {
		//WATCH OUT the logfile script should be executed before the kinectrigclient, Edit--> project settings --> Script Execution Order ; drag the script in set it to e.g. -10 to run before default time.
		Debug.Log("start logger, first in execution order");
		
		startTime = DateTime.Now;
		
		lastLogToFile = logToFile;
		
		//always create a log for the peculiar updates (too long between)
		DateTime currentDateLogFile = DateTime.Now;
		dateFileName = currentDateLogFile.Month + "_" + currentDateLogFile.Day + "_" + currentDateLogFile.Hour + "_" + currentDateLogFile.Minute + "_" + currentDateLogFile.Second;
				
		//logfile is always generated, logging will only be stopped, not the creation of the logfiles themselves...
		if(logToFile)
		{
			InitiateLogFile();
		}
	}
	
	//?never called?
	void LogLine() {
		if(!logFilePositionsGenerated)
		{
			InitiateLogFile();
		}
	}
	
	//creates a unique logfile name by increasing int
	void InitiateLogFile()
	{
		//TODO change folder in the unity project
		
		if (!Directory.Exists (logFolderPath)) 
		{         
			Directory.CreateDirectory (logFolderPath);    
		}
		
		// POSITIONS LOGGING: check and create stuff for logging 
		if(logPositionsToFile && !logFilePositionsGenerated) {
			logFileNamePositions = logFolderPath+logFileNameBase+"_positions_"+dateFileName+".txt";
			
			//create actual file by putting a header in it, doesnt work well with the streamreader method I believe!
			//File.AppendAllText(logFileNamePositions, Time.timeSinceLevelLoad.ToString() + "------NEW POSITIONS FILE----" + "\n");
			
			//create streamreader
			m_tw_Positions = new StreamWriter(logFileNamePositions);
			
			logFilePositionsGenerated = true;
		}

		//EXAMPLE of another logger
//		// WRONGUPDATES LOGGING: check and create stuff for logging 
//		if(logUpdateToFile && !logFileUpdateGenerated) {
//			logFileNameUpdate = logFolderPath+logFileNameBase+"_wrongUpdate_"+dateFileName+".txt";
//			
//			//create actual file by putting a header in it
//			File.AppendAllText(logFileNameUpdate, Time.timeSinceLevelLoad.ToString() + "------NEW WRONGUPDATE FILE----" + "\n");
//
//			//EXAMPLE of another logger
//			//create streamreader
//			//m_tw_Update = new StreamWriter(logFileNameUpdate);
//			
//			logFileUpdateGenerated = true;
//		}
//		
//		// PADDLES LOGGING: check and create stuff for logging 
//		if(logPaddlesToFile && !logFilePaddlesGenerated) {
//			logFileNamePaddles = logFolderPath+logFileNameBase+"_Paddles_"+dateFileName+".txt";
//			
//			//create actual file by putting a header in it
//			File.AppendAllText(logFileNamePaddles, Time.timeSinceLevelLoad.ToString() + "------NEW PADDLES FILE----" + "\n");
//			
//			//create streamreader
//			m_tw_Paddles = new StreamWriter(logFileNamePaddles);
//			
//			logFilePaddlesGenerated = true;
//		}

	}
	
	void Update () {
		
		//checke if logtofile has changed during the game
		if (lastLogToFile != logToFile)
		{
			lastLogToFile = logToFile;
			
			//Check if all logfiles are created
			if (logToFile && logPositionsToFile && !logFilePositionsGenerated){
				InitiateLogFile();
			}

			//EXAMPLE of another logger
//			if (logToFile && logUpdateToFile && !logFileUpdateGenerated){
//				InitiateLogFile();
//			}
//			
//			if (logToFile && logPaddlesToFile && !logFilePaddlesGenerated){
//				InitiateLogFile();
//			}
		}
		
	}
	
	void OnApplicationQuit() {
		Debug.Log("application quit in Logger");
		
		try {
			m_tw_Positions.Close();
		} catch (Exception e) {
			Debug.Log ("probably no serverupdate streamreader started" + e);
		}

		//EXAMPLE OF ANOTHER LOGGER
//		try {
//			m_tw_Update.Close();
//		} catch (Exception e) {
//			Debug.Log ("probably no peculiar update logger started" + e);
//		}

		//EXAMPLE OF ANOTHER LOGGER
//		try {
//			m_tw_Paddles.Close();
//		} catch (Exception e) {
//			Debug.Log ("probably no peculiar paddle logger started" + e);
//		}

	}


	//Write text to the Positions log file
	public void LogLineRigClientUpdate(string text) {
		if (logToFile && logPositionsToFile && logFilePositionsGenerated) {
			//this uses ticks instead of game time in order to be able to run it from outside the main Unity thread
			//10000 ticks per millisecond.
			long elapsedTicks = DateTime.Now.Ticks - startTime.Ticks;
			float ticksToMilliSeconds = elapsedTicks/10000;
			m_tw_Positions.WriteLine(ticksToMilliSeconds.ToString()+"," + text);
		}
	}

	//EXAMPLES OF OTHER LOGGERS:

	//Write text to the Update log file
//	public void LogLineMissingUpdate(int idOfPlayer, bool updateDeath) {
//		if(logToFile && logUpdateToFile && logFileUpdateGenerated){
//			//notify the texfile if an object hasn't been updated the last frame (assumption +- 1/20s) or when it is updated after it wasn't for a long time
//			//if player's track has died 1 if it has a rebirth this will be 0
//			int i = updateDeath ? 1 : 0;
//			string textLine = idOfPlayer.ToString() + "," + i;
//			if(logToFile && logFilePositionsGenerated) {
//				DateTime currentDate = DateTime.Now;
//				
//				//TODO check
//				m_tw_Update.WriteLine(currentDate.Month.ToString() + "," + currentDate.Day.ToString() + "," + currentDate.Hour.ToString() + "," + currentDate.Minute.ToString() + "," + currentDate.Second.ToString() + "," + currentDate.Millisecond.ToString() + "," + textLine);
//			}
//		}
//	}
//	
//	//Write text to the Paddles log file
//	public void LogLinePaddleUpdate(string text) {
//		if (logToFile && logPaddlesToFile && logFilePaddlesGenerated) {
//			//10000 ticks per millisecond.
//			long elapsedTicks = DateTime.Now.Ticks - startTime.Ticks;
//			float ticksToMilliSeconds = elapsedTicks/10000;
//			m_tw_Paddles.WriteLine(ticksToMilliSeconds.ToString()+"," + text);
//		}
//	}

}

