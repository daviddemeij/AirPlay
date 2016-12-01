using UnityEngine;
using System.Collections;

public class CollisionDetector : MonoBehaviour {
	Player playerCollider;
	Player thisPlayer;
    public Transform coinPrefab;
    private GameObject coin;
    //TODO perhaps generate in shields;
    public bool inShield;
	public bool lastInShield;

	//select some simple audio files to play upon collision
	public AudioClip[] a_powerUpSounds; 
	public AudioClip[] a_tagSounds; //burn, a_coin, a_coinsDown, a_coinsUp, a_drunk, a_growing, a_shield, a_shrink;
	
	private Logger loggerScript;
	private GameObject mainCameraObject;
    private float coinCountdown = 1.0f;
	// Use this for initialization
	void Start () {
		mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
		loggerScript = mainCameraObject.GetComponent("Logger") as Logger;
		thisPlayer = this.transform.parent.GetComponent("Player") as Player;
	}
	
	// Update is called once per frame
	void Update () {
        coinCountdown -= Time.deltaTime;
    }

	void OnTriggerEnter(Collider myTrigger)
	{
		//indeed player:
		//print("tagcollision name: " + myTrigger.gameObject.transform.parent.name + " & "+ thisPlayer.name);

        //check on null to deal with no parent

        if (myTrigger.gameObject.transform.parent != null && myTrigger.gameObject.transform.parent.tag == "Player")
		{ 
			GameObject playercollision = myTrigger.transform.parent.gameObject;
			string triggername = myTrigger.gameObject.transform.parent.name;
			playSound();
			print ("debug line " + triggername);
            if (thisPlayer.isTagger)
            {
                if (thisPlayer.powerUpCounter >= thisPlayer.taggerMat.Length-1)
                {
                    myTrigger.gameObject.transform.parent.GetComponent<Player>().isTagger = true;
                    thisPlayer.powerUpCounter = 0;
                    thisPlayer.updateTaggerMaterial();
                }
            }

            
        }
		//powerUpOrbit , powerUpScaleDown, powerUpScaleUp, powerUpShield
		else 
		{
			string triggertag = myTrigger.gameObject.transform.tag;
            print("Trigger: " + triggertag);
			//GameObject powerUpObject = myTrigger.gameObject;
			//GameObject parentOfPU = powerUpObject.transform.parent.gameObject;
			//only if you are a runner you can collect the particles
			//and only if you havent just been tagged, either when you were protected (swagged) or when you just tagged someone
			//the upper prevents people from chasing the runner all the time and become invincible
            //if (triggername == "loopObject" || triggername == "loopObjectReversed")
            //{
            ///    if (thisPlayer.isTagger) { thisPlayer.isTagger = false; }
            //}
            if(triggertag == "Coin" && thisPlayer.isTagger && coinCountdown<=0)
            {
                print("pickup coin by " + thisPlayer.name);
                //GameObject.Destroy(myTrigger.gameObject);
                playPowerUpSound(0);
                thisPlayer.powerUpCounter++;
                thisPlayer.updateTaggerMaterial();
                //coin = Instantiate(coinPrefab).transform.gameObject;
                myTrigger.transform.position = new Vector3(Random.Range(1.0f, 2f), 0, Random.Range(-1.3f, -1.8f));
                //coinCountdown = 1.0f;
            }
		
		}
	}

	void OnTriggerStay(Collider myShieldTrigger)
	{
		//for instance use when you have something interacting whule being inside a shield
		//if(myShieldTrigger.gameObject.transform.parent != null && myShieldTrigger.gameObject.transform.parent.name == "Shield" && myShieldTrigger.gameObject.transform.parent.transform.parent != null && myShieldTrigger.gameObject.transform.parent.transform.parent!= this.transform.parent)
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
