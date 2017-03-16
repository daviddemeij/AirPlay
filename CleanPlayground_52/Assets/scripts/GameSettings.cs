﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameSettings : MonoBehaviour
{
    public bool nextRound = true;
    public bool coinBattle = false;
    public int nrOfPlayers = 0;
    public int nrOftaggers = 0;
    private List<int> taggers;
    private float textTimer = 0;
    public Material[] trailMaterial;
    public Material[] trailMaterialTagger;
    public bool BothTrail; // gives both teams a trail to capture other players
    public bool noTagging; // Disables tagging
    public bool noTrails; // Disables trails
    public bool singlePlayer = false;
    public GameObject coinsText;
    //we need to use a woozscript;
    Wooz woozScript;

    //WATCH OUT the game settings  script should be execured before the kinectrigclient, Edit--> project settings --> Script Execution Order ; drag the script in set it to e.g. -5 to run before default time.
    [HideInInspector]
    public GameObject[] playerGameObjects;
    // a single instance of the player script
    Player playerScript;
    KinectRigClient thisClient;
    [HideInInspector]public bool gameOff = false;
    //WATCH OUT drag the playerprefab from the project folder into the player in creation line!
    public Transform playerPrefab;
    Vector3 startPositionOfFirstPlayer = new Vector3(0.8f, 0.0f, -0.41f);
    [HideInInspector]public Vector3 stageDimensions;
    private GameObject coin1;
    private GameObject coin2;
    private GameObject iceCoin1;
    private GameObject iceCoin2;
    private GameObject backgroundImageObject;
    private GameObject safeHouse;
    public float safeHouseTime; // time until safehouse spawns
    private float safeHouseTimer; // time since last safehouse or start of the game;
    public bool isPause = false;
    public int currentLevel;
    public Vector3[] safeHousePositions;
    int safeHousePosition;
    // Use this for initialization
    void Start()
    {
        safeHousePositions = new Vector3[4];
        safeHousePositions[0] = new Vector3(0.84f,-5f,-0.34f);
        safeHousePositions[1] = new Vector3(0.84f, -5f, -2.89f);
        safeHousePositions[2] = new Vector3(2.76f, -5f, -0.34f);
        safeHousePositions[3] = new Vector3(2.76f, -5f, -2.89f);
        safeHouseTimer = Time.time;

        nrOfPlayers = PlayerPrefs.GetInt("NrPlayers");
        nrOftaggers = PlayerPrefs.GetInt("NrTaggers");
        currentLevel = PlayerPrefs.GetInt("Level");
        print("current level" + currentLevel);
        if (currentLevel == 0) { singlePlayer = true; }
        else { singlePlayer = false; coinsText.SetActive(false); }
        if (currentLevel == 1) { noTrails = true; noTagging = true; BothTrail = false; coinBattle = true; coinsText.SetActive(true);  }
        if (currentLevel == 2) { noTrails = false; noTagging = true; BothTrail = true;}
        if (currentLevel == 3) { noTagging = false; BothTrail = false; noTrails = false; }


        if (nrOfPlayers == 1)
        {
            singlePlayer = true;
        }
        stageDimensions = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, 0, Screen.height));
        print(stageDimensions.x + " x " + stageDimensions.y + " y " + stageDimensions.z + " z ");
        //loadplayers based on the number of player objects that are in the scene
        LoadPlayers();
        Debug.Log("start gamesettings before the other scripts");
        thisClient = this.GetComponent("KinectRigClient") as KinectRigClient;
        thisClient.playerGameObjects = playerGameObjects;

        Debug.Log("get wooz");
        woozScript = this.GetComponent("Wooz") as Wooz;
        woozScript.loadedGameObjects = false;
        //assigns the gameobjects and later on assigns the scripts
        woozScript.AssignPlayerGameObjects(playerGameObjects);
        safeHouse = GameObject.Find("SafeHouse");
        coin1 = GameObject.Find("Coin 1");
        coin2 = GameObject.Find("Coin 2");
        iceCoin1 = GameObject.Find("IceCoin 1");
        iceCoin2 = GameObject.Find("IceCoin 2");
        coin1.transform.position = new Vector3(Random.Range(0.65f, 2.95f), 0, Random.Range(-3.1f, -0.2f));
        coin2.transform.position = new Vector3(Random.Range(0.65f, 2.95f), 0, Random.Range(-3.1f, -0.2f));
        iceCoin1.transform.position = new Vector3(Random.Range(0.65f, 2.95f), 0, Random.Range(-3.1f, -0.2f));
        iceCoin2.transform.position = new Vector3(Random.Range(0.65f, 2.95f), 0, Random.Range(-3.1f, -0.2f));

        if (singlePlayer)
        {
            safeHouse.SetActive(false);
            coin1.SetActive(false);
            coin2.SetActive(false);
            iceCoin1.SetActive(true);
            iceCoin2.SetActive(true);
        }
        if (coinBattle)
        {
            safeHouse.SetActive(false);
            coin1.SetActive(true);
            coin2.SetActive(true);
            iceCoin1.SetActive(true);
            iceCoin2.SetActive(true);
        }
        else if (noTagging)
        {
            
            coin1.SetActive(false);
            coin2.SetActive(false);
            iceCoin1.SetActive(false);
            iceCoin2.SetActive(false);
        }
        else if (BothTrail || noTrails)
        {
            iceCoin1.SetActive(true);
            iceCoin2.SetActive(true);
        }
        else
        {
            iceCoin1.SetActive(false);
            iceCoin2.SetActive(false);
        }
    }

    // Update is called once per frame
    
    void Update()
    {
        if (singlePlayer)
        {
            foreach (var gameObj in FindObjectsOfType(typeof(Player)) as Player[])
            {
                coinsText.GetComponent<GUIText>().text = ("Muntjes verzameld: " + gameObj.powerUpCounter);
            }
        }
        if (coinBattle)
        {
            int taggerCoins = 0;
            int runnerCoins = 0;
            foreach (var gameObj in FindObjectsOfType(typeof(Player)) as Player[])
            {
                if (gameObj.isTagger)
                {
                    taggerCoins += gameObj.powerUpCounter;
                }
                else
                {
                    runnerCoins += gameObj.powerUpCounter;
                }
                coinsText.GetComponent<GUIText>().text = ("Rood: " + taggerCoins + " - Blauw: "+runnerCoins );
            }
        }
        else if(Time.time>safeHouseTimer + safeHouseTime)
        {
            safeHouseTimer = Time.time;
            int pos = (int)Random.Range(0, 3);
            print("Safehouse pos " + pos);
            safeHouse.transform.position = safeHousePositions[pos];
        }
        if (nextRound == true)
        {
            safeHouseTimer = Time.time;
            safeHouse.transform.position = new Vector3(0f, -5f, 0f);
            int nrTaggersRemaining = nrOftaggers;
            taggers = new List<int>();
            int randomTagger;
            while (nrTaggersRemaining > 0)
            {
                print("taggers remaining" + nrTaggersRemaining);
                randomTagger = Mathf.RoundToInt(Random.Range(0, nrOfPlayers - 1));
                if (!taggers.Contains(randomTagger))
                {
                    taggers.Add(randomTagger);
                    nrTaggersRemaining--;
                    print("tagger:"+ randomTagger);
                }
            }
            int i = 0;

            foreach (var gameObj in FindObjectsOfType(typeof(Player)) as Player[])
            {

                gameObj.GetComponent<Player>().resetPlayer();

                if (taggers.Contains(i)) {
                    gameObj.GetComponent<Player>().isTagger = true;
                }
                else { gameObj.GetComponent<Player>().isTagger = false; }
                gameObj.GetComponent<Player>().trailMaterial = trailMaterial[i % trailMaterial.Length];
                if (BothTrail)
                {
                    gameObj.GetComponent<Player>().trailMaterialTagger = trailMaterialTagger[i % trailMaterialTagger.Length];
                }
                i++;
            }
            nextRound = false;

        }
        

    }

  
    //we loadplayers based on the scene
    void LoadPlayers()
    {

        //remove all leftovers of players in the scene
        foreach (var playerScriptObj in FindObjectsOfType(typeof(Player)) as Player[])
        {
            Destroy(playerScriptObj.transform.gameObject, 0.01f);
        }

        //place the players next to each other at some location with startPositionOfPlayer

        int nrTaggersRemaining = nrOftaggers;
        taggers = new List<int>();
        int randomTagger;
        while (nrTaggersRemaining > 0)
        {
            randomTagger = Mathf.RoundToInt(Random.Range(0, nrOfPlayers - 1));
            if (!taggers.Contains(randomTagger))
            {
                taggers.Add(randomTagger);
                nrTaggersRemaining--;
            }
        }
        Transform lastCreatedPlayer;
        int i = 0;
        for (i = 0; i < nrOfPlayers; i++)
        {
            startPositionOfFirstPlayer.x = startPositionOfFirstPlayer.x + 0.4f;
            lastCreatedPlayer = Instantiate(playerPrefab, startPositionOfFirstPlayer, Quaternion.LookRotation(Vector3.back)) as Transform;
            lastCreatedPlayer.name = "player " + i;

        }

        //i++ so number of i " is now 6, although it was 5 in last run for loop

        //now we simply recreate the player variable with the right size
        //i ==6 after for loop
        playerGameObjects = new GameObject[i];
        i = 0;

        //now assign the objects
        foreach (var gameObj in FindObjectsOfType(typeof(Player)) as Player[])
        {
            if (gameObj.transform.name.StartsWith("player"))
            {
                //this is extra precaution that shouldn't be needed
                if (i < playerGameObjects.Length)
                {
                    //   if (i > 0)
                    // { playerGameObjects[i].GetComponent<trailScript>().trail.GetComponent<Renderer>().material = trailMaterial[i%trailMaterial.Length]; }

                    //assign the script to vector of players which we will use in the rest of the game and scripts
                    
                    playerGameObjects[i] = gameObj.transform.gameObject;
                    playerGameObjects[i].GetComponent<Player>().trailMaterial = trailMaterial[i % trailMaterial.Length];
                    if (BothTrail)
                    {
                        playerGameObjects[i].GetComponent<Player>().trailMaterialTagger = trailMaterialTagger[i % trailMaterialTagger.Length];
                    }
                    //set the id of the players once
                    playerScript = playerGameObjects[i].GetComponent("Player") as Player;
                    playerScript.id = i;
                    playerGameObjects[i].name = "player " + i;
                    if (taggers.Contains(i))
                    {
                        playerScript.isTagger = true;
                    }
                }
            }
            i = i + 1;
        }
    }
}
