using UnityEngine;
using System.Collections;

public class CollisionDetector : MonoBehaviour {

	Player playerCollider;
	Player thisPlayer;

	//TODO perhaps generate in shields;
	public bool inShield;
	public bool lastInShield;

	//select some simple audio files to play upon collision
	public AudioClip[] a_powerUpSounds; 
	public AudioClip[] a_tagSounds; //burn, a_coin, a_coinsDown, a_coinsUp, a_drunk, a_growing, a_shield, a_shrink;
	
	private Logger loggerScript;
	private GameObject mainCameraObject;

	// Use this for initialization
	void Start () {
		mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
		loggerScript = mainCameraObject.GetComponent("Logger") as Logger;
		thisPlayer = this.transform.parent.GetComponent("Player") as Player;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider myTrigger)
	{
		//indeed player:
		print("tagcollision name: " + myTrigger.gameObject.transform.parent.name + " & "+ thisPlayer.name);

        //check on null to deal with no parent

        if (myTrigger.gameObject.transform.parent != null && myTrigger.gameObject.transform.parent.tag == "Player")
		{ 
			GameObject playercollision = myTrigger.transform.parent.gameObject;
			string triggername = myTrigger.gameObject.transform.name;
			playSound();
			print ("debug line " + triggername);
            if (thisPlayer.isTagger) { myTrigger.gameObject.transform.parent.GetComponent<Player>().isTagger = true; }

            
        }
		//powerUpOrbit , powerUpScaleDown, powerUpScaleUp, powerUpShield
		else 
		{
			string triggername = myTrigger.gameObject.transform.name;
			GameObject powerUpObject = myTrigger.gameObject;
			GameObject parentOfPU = powerUpObject.transform.parent.gameObject;
			//only if you are a runner you can collect the particles
			//and only if you havent just been tagged, either when you were protected (swagged) or when you just tagged someone
			//the upper prevents people from chasing the runner all the time and become invincible
			if (triggername == "particleCollider")
			{
				if(!GetComponent<AudioSource>().isPlaying) 
				{ 
					playPowerUpSound(0);
				}
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
