using UnityEngine;
using System.Collections;

public class CollisionDetector : MonoBehaviour {
	Player playerCollider;
	Player thisPlayer;
    public Transform coinPrefab;
    private GameObject coin;
    public Transform iceCoinPrefab;
    private GameObject iceCoin;



	//select some simple audio files to play upon collision
	public AudioClip[] a_powerUpSounds; 
	public AudioClip[] a_tagSounds; //burn, a_coin, a_coinsDown, a_coinsUp, a_drunk, a_growing, a_shield, a_shrink;


	private GameObject mainCameraObject;
    private float coinCountdown = 1.0f;
    private gameStateChecker gameState; 
	// Use this for initialization
	void Start () {
		mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        gameState = mainCameraObject.GetComponent("gameStateChecker") as gameStateChecker;
        thisPlayer = this.transform.parent.GetComponent("Player") as Player;

	}
	
	// Update is called once per frame
	void Update () {
        coinCountdown -= Time.deltaTime;
    }
    void OnTriggerExit(Collider myTrigger)
    {
        string triggertag = myTrigger.gameObject.transform.tag;
        if (triggertag == "SafeHouse")
        {
            thisPlayer.inSafeHouse = false;
            thisPlayer.updateTaggerMaterial();
        }
    }
	void OnTriggerEnter(Collider myTrigger)
	{
        if (gameState != null && myTrigger != null )
        {
            if (!gameState.IsCountdown && !gameState.isAnnounceWinner && !thisPlayer.inSafeHouse)
            {

                //indeed player:
                //print("tagcollision name: " + myTrigger.gameObject.transform.parent.name + " & "+ thisPlayer.name);

                //check on null to deal with no parent

                if (myTrigger.gameObject.transform.parent != null && myTrigger.gameObject.transform.parent.tag == "Player")
                {
                    GameObject playerCollision = myTrigger.transform.parent.gameObject;
                    playSound();

                    if (!playerCollision.GetComponent<Player>().inSafeHouse && !thisPlayer.noTagging && (thisPlayer.isTagger || (thisPlayer.bothTrail || thisPlayer.noTrails)) && (thisPlayer.isTagger != playerCollision.GetComponent<Player>().isTagger))
                    {
                        if (thisPlayer.powerUpCounter >= thisPlayer.coinsRequired)
                        {
                            playerCollision.GetComponent<Player>().resetPlayer();
                            thisPlayer.resetPlayer();
                            playerCollision.GetComponent<Player>().isTagger = thisPlayer.isTagger;
                            playerCollision.GetComponent<Player>().updateTaggerMaterial();
                        }
                    }
                }

                else
                {
                    string triggertag = myTrigger.gameObject.transform.tag;
                    if (triggertag == "Coin" && thisPlayer.isTagger && coinCountdown <= 0)
                    {
                        playPowerUpSound(0);
                        thisPlayer.powerUpCounter++;
                        thisPlayer.updateTaggerMaterial();
                         myTrigger.transform.position = new Vector3(Random.Range(0.65f,2.95f), 0, Random.Range(-3.1f, -0.2f));
                    }
                    else if (triggertag == "IceCoin" && !thisPlayer.isTagger && coinCountdown <= 0)
                    {
                        playPowerUpSound(0);
                        thisPlayer.powerUpCounter++;
                        thisPlayer.updateTaggerMaterial();
                        myTrigger.transform.position = new Vector3(Random.Range(0.65f, 2.95f), 0, Random.Range(-3.1f, -0.2f));
                    }
                    else if(triggertag == "SafeHouse")
                    {
                        thisPlayer.inSafeHouse = true;
                        thisPlayer.updateTaggerMaterial();
                    }

                }
            }
        }
	}



	//example to play a sound on some collision trigger
	void playSound()
	{
		GetComponent<AudioSource>().clip = a_tagSounds[0];
		GetComponent<AudioSource>().Play();
	}

	//another example to play a sound on some collision trigger
	void playPowerUpSound(int type)
	{
		GetComponent<AudioSource>().clip = a_powerUpSounds[type];
		GetComponent<AudioSource>().Play();
	}

}
