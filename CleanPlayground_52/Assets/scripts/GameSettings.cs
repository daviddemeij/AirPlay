using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSettings : MonoBehaviour {
    public bool nextRound = false;
	public int nrOfPlayers = 0;
    public List<int> taggers;
    public int redWins = 0;
    public float textTimer = 0;
	//we need to use a woozscript;
	Wooz woozScript;

	//WATCH OUT the game settings  script should be execured before the kinectrigclient, Edit--> project settings --> Script Execution Order ; drag the script in set it to e.g. -5 to run before default time.
	[HideInInspector]public GameObject[] playerGameObjects; 
	// a single instance of the player script
	Player playerScript;
	KinectRigClient thisClient;
    public bool gameOff = false;
	//WATCH OUT drag the playerprefab from the project folder into the player in creation line!
	public Transform playerPrefab;
	Vector3 startPositionOfFirstPlayer = new Vector3(0.8f,0.0f,-0.41f);

	// Use this for initialization
	void Start () {
        //loadplayers based on the number of player objects that are in the scene
        LoadPlayers();
		Debug.Log ("start gamesettings before the other scripts" );
		thisClient = this.GetComponent("KinectRigClient") as KinectRigClient;
		thisClient.playerGameObjects = playerGameObjects;

		Debug.Log ("get wooz" );
		woozScript = this.GetComponent("Wooz") as Wooz;
		woozScript.loadedGameObjects = false;
		//assigns the gameobjects and later on assigns the scripts
		woozScript.AssignPlayerGameObjects(playerGameObjects);
	}
	
	// Update is called once per frame
	void Update () {
        if (textTimer > 0)
        {
            textTimer -= Time.deltaTime;
            if (gameOff == false)
            {
                foreach (var player in FindObjectsOfType(typeof(Player)) as Player[])
                {
                    player.enabled = false;
                }
                gameOff = true;


            }
        }
        else if(gameOff)
        {
            gameOff = false;
            nextRound = false;
            foreach (var player in FindObjectsOfType(typeof(Player)) as Player[])
            {
                player.enabled = true;
            }
            GameObject red = GameObject.FindGameObjectWithTag("redwins");
            red.transform.position = new Vector3(0, 4f, 0);

            GameObject blue = GameObject.FindGameObjectWithTag("bluewins");
            blue.transform.position = new Vector3(0, 4f, 0);

        }
        if (nextRound == true)
        {
            bool first = true;
            foreach (var gameObj in FindObjectsOfType(typeof(Player)) as Player[])
            {
                gameObj.GetComponent<Player>().powerUpCounter = 0;
                if (first) { gameObj.GetComponent<Player>().isTagger = true; first = false; }
                else { gameObj.GetComponent<Player>().isTagger = false; }
            }
            nextRound = false;

        }
	
	}

	//we loadplayers based on the scene
	void LoadPlayers() {

		//remove all leftovers of players in the scene
		foreach (var playerScriptObj in FindObjectsOfType(typeof(Player)) as Player[])
		{
			Destroy(playerScriptObj.transform.gameObject,0.01f);
		}

		//place the players next to each other at some location with startPositionOfPlayer


		Transform lastCreatedPlayer;
		int i = 0; 
		for ( i = 0; i<nrOfPlayers; i++)
		{
			startPositionOfFirstPlayer.x = startPositionOfFirstPlayer.x + 0.4f;
			lastCreatedPlayer = Instantiate(playerPrefab, startPositionOfFirstPlayer, Quaternion.LookRotation(Vector3.back)) as Transform;
			lastCreatedPlayer.name = "player " +i;
         
		}

		//i++ so number of i " is now 6, although it was 5 in last run for loop

		//now we simply recreate the player variable with the right size
		//i ==6 after for loop
		playerGameObjects = new GameObject[i];
		i = 0;
		
		//now assign the objects
		foreach (var gameObj in FindObjectsOfType(typeof(Player)) as Player[])
		{
			if(gameObj.transform.name.StartsWith("player"))
			{
				//this is extra precaution that shouldn't be needed
				if(i<playerGameObjects.Length)
				{
					//assign the script to vector of players which we will use in the rest of the game and scripts
					playerGameObjects[i] = gameObj.transform.gameObject;

					//set the id of the players once
					playerScript = playerGameObjects[i].GetComponent("Player") as Player;
					playerScript.id =i;
					playerGameObjects[i].name = "player " +i;
                    if (taggers.Contains(i))
                    {
                        playerScript.isTagger = true;
                    }
                }
			}
			i = i+1;
		}
	}
}
