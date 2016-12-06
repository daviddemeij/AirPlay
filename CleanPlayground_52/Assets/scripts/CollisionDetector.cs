using UnityEngine;
using System.Collections;

public class CollisionDetector : MonoBehaviour {
	Player playerCollider;
	Player thisPlayer;
    public Transform coinPrefab;
    private GameObject coin;
    //TODO perhaps generate in shields;


	//select some simple audio files to play upon collision
	public AudioClip[] a_powerUpSounds; 
	public AudioClip[] a_tagSounds; //burn, a_coin, a_coinsDown, a_coinsUp, a_drunk, a_growing, a_shield, a_shrink;
	

	private GameObject mainCameraObject;
    private float coinCountdown = 1.0f;
    private gameStateChecker gameState;
    private GameSettings gameSettingsScript; 
	// Use this for initialization
	void Start () {
		mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");


        gameSettingsScript = mainCameraObject.GetComponent("GameSettings") as GameSettings;
        gameState = mainCameraObject.GetComponent("gameStateChecker") as gameStateChecker;
        thisPlayer = this.transform.parent.GetComponent("Player") as Player;
	}
	
	// Update is called once per frame
	void Update () {
        coinCountdown -= Time.deltaTime;
    }

	void OnTriggerEnter(Collider myTrigger)
	{
        if (gameState != null && myTrigger != null)
        {
            if (!gameState.IsCountdown && !gameState.isAnnounceWinner)
            {

                //indeed player:
                //print("tagcollision name: " + myTrigger.gameObject.transform.parent.name + " & "+ thisPlayer.name);

                //check on null to deal with no parent

                if (myTrigger.gameObject.transform.parent != null && myTrigger.gameObject.transform.parent.tag == "Player")
                {
                    GameObject playerCollision = myTrigger.transform.parent.gameObject;
                    string triggername = playerCollision.name;
                    playSound();

                    if (thisPlayer.isTagger && !playerCollision.GetComponent<Player>().isTagger)
                    {
                        if (thisPlayer.powerUpCounter >= thisPlayer.taggerTexture.Length - 1)
                        {
                            playerCollision.GetComponent<Player>().resetPlayer();
                            thisPlayer.resetPlayer();
                            playerCollision.GetComponent<Player>().isTagger = true;
                            
                        }
                    }


                }
                //powerUpOrbit , powerUpScaleDown, powerUpScaleUp, powerUpShield
                else
                {
                    string triggertag = myTrigger.gameObject.transform.tag;
                    if (triggertag == "Coin" && thisPlayer.isTagger && coinCountdown <= 0)
                    {
                        playPowerUpSound(0);
                        thisPlayer.powerUpCounter++;
                        thisPlayer.updateTaggerMaterial();
                        //coin = Instantiate(coinPrefab).transform.gameObject;

                        //GameObject otherCoin;
                        //if(myTrigger.name == "Coin 1")
                        //{
                          //  otherCoin = GameObject.Find("Coin 2");
                        //}
                        //else
                        //{
                          ///  otherCoin = GameObject.Find("Coin 1");
                        //}

                        

                        //Vector2 positionOtherCoin;
                        //positionOtherCoin = new Vector2(otherCoin.transform.position.x, otherCoin.transform.position.z);


                        //Vector2 positionPlayer;
                        //positionPlayer = new Vector2(thisPlayer.transform.position.x, thisPlayer.transform.position.z);
                        //Vector2 randomNumbers;
                        //randomNumbers = new Vector2(otherCoin.transform.position.x, otherCoin.transform.position.z);
                        //print(positionOtherCoin.x);


                        //while ((positionOtherCoin - randomNumbers).sqrMagnitude < 7f && (positionPlayer - randomNumbers).sqrMagnitude < 7f)
                        //{
                            //print((positionOtherCoin - randomNumbers).sqrMagnitude);
                            //randomNumbers.x = Random.Range(0.5f, gameSettingsScript.stageDimensions.x - 0.5f);
                          //  randomNumbers.y = Random.Range(gameSettingsScript.stageDimensions.z + 0.5f, -0.5f);
                        //}
                            myTrigger.transform.position = new Vector3(Random.Range(0.65f,2.95f), 0, Random.Range(-3.1f, -0.2f));
                        //coinCountdown = 1.0f;
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
