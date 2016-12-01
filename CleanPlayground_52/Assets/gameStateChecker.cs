using UnityEngine;
using System.Collections;

public class gameStateChecker : MonoBehaviour {
    [HideInInspector]public bool IsCountdown = true;
    public Texture[] countdownTexture = new Texture[4];
    public AudioClip countdownAudio;
    public Texture gameBackground;

    private GameObject[] playerGameObjects;
    private GameObject backgroundImageObject;
    private GameSettings gameSettingsScript;
    private int countdownCounter;
    private float timeSinceCountdown;
    // Use this for initialization
    void Start () {
        backgroundImageObject = GameObject.FindGameObjectWithTag("BackgroundImage");
        gameSettingsScript = this.GetComponent<GameSettings>();
        print(gameSettingsScript.nrOfPlayers);
        playerGameObjects = new GameObject[gameSettingsScript.nrOfPlayers];
        loadPlayers();
        countdownCounter = -1;
        playCountdownAudio();
        checkCountdown();
    }
	
	// Update is called once per frame
	void Update () {

        if (IsCountdown){ checkCountdown();}
	
	}
    public void freezePlayers()
    {
        for (int i = 0; i < playerGameObjects.Length; i++)
        {
            Player playerScript = playerGameObjects[i].GetComponent("Player") as Player;
            playerScript.freezed = true;
        }
        print("freezeplayers");
    }

    private void checkCountdown()
    {
        if (countdownCounter == -1)
        {
            timeSinceCountdown = Time.time;
            backgroundImageObject.GetComponent<GUITexture>().texture = countdownTexture[0];
            countdownCounter++;        
        }
        else
        {
            if (Time.time - timeSinceCountdown > 0.80f)
            {
                if (countdownCounter < 4)
                {
                    timeSinceCountdown = Time.time;
                    print(backgroundImageObject.gameObject.name);
                    backgroundImageObject.GetComponent<GUITexture>().texture = countdownTexture[countdownCounter];
                    countdownCounter++;

                }
                else
                {
                    IsCountdown = false;
                    backgroundImageObject.GetComponent<GUITexture>().texture = gameBackground;

                }
            }
        }
    }
    private void loadPlayers()
    {
        int i = 0;
        foreach (var player in FindObjectsOfType(typeof(Player)) as Player[])
        {
            playerGameObjects[i] = player.gameObject;
            i++;
        }
    }
    private void playCountdownAudio()
    {
        print("play countdown");
        GetComponent<AudioSource>().clip = countdownAudio;
        GetComponent<AudioSource>().Play();
    }
}
