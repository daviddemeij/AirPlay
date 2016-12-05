using UnityEngine;
using System.Collections;

public class gameStateChecker : MonoBehaviour {
    [HideInInspector]public bool IsCountdown = true;
    [HideInInspector]public bool isAnnounceWinner = false;
    public Texture[] countdownTexture = new Texture[4];
    public AudioClip countdownAudio;
    public Texture gameBackground;
    public Texture blueWinsTexture;
    public Texture redWinsTexture;
    private GameObject backgroundImageObject;
    private int countdownCounter;
    private float timeSinceCountdown;
    private float textTimer;
    private GameSettings gameSettingsScript;
    // Use this for initialization
    void Start () {
        backgroundImageObject = GameObject.FindGameObjectWithTag("BackgroundImage");
        countdownCounter = -1;
        gameSettingsScript = this.GetComponent<GameSettings>();
    }

    // Update is called once per frame
    void Update()
    {

        if (IsCountdown) { checkCountdown(); }
        if (isAnnounceWinner)
        {
            announceWinner();
        }
            
    }
    void announceWinner()
    {
        if (textTimer > 0)
        {
            textTimer -= Time.deltaTime;
        }
        else
        {
            gameSettingsScript.nextRound = true;
            isAnnounceWinner = false;
            backgroundImageObject.GetComponent<GUITexture>().texture = gameBackground;
        }
    }

    public void blueWins()
    {
        backgroundImageObject.GetComponent<GUITexture>().texture = blueWinsTexture;
        textTimer = 5f;
        isAnnounceWinner = true;
    }
    public void redWins()
    {
        backgroundImageObject.GetComponent<GUITexture>().texture = redWinsTexture;
        textTimer = 5f;
        isAnnounceWinner = true;
    }
    private void checkCountdown()
    {
        if (countdownCounter == -1)
        {

            print("play countdown");
            GetComponent<AudioSource>().clip = countdownAudio;
            GetComponent<AudioSource>().Play();
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
}
