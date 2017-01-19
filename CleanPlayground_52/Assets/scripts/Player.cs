using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    public bool isTagger;
    private bool bothTrail;
    public int powerUpCounter = 0;
    public Texture runnerTexture;
    public Texture[] taggerTexture;
    public Material trailMaterial;
    public Material trailMaterialTagger;
    [HideInInspector]
    public bool currentMatTagger = false;

    //[HideInInspector]public GameObject[] lines;
    [HideInInspector]
    public GameObject collisionMeshObject;
    public float positionx;
    public float positiony;
    public int id = -1;
    public int trackerid = -1;
    public int gameid = -1;


    //this is automatically set at the start of the game and upon a change of number of alive players
    Vector3 targetPos = new Vector3(0, 0, 0);
    Vector3 startPosition = new Vector3(0, 0, 0);
    private float lastUpdateTime;
    public float lastDurationUpdateTime;
    public float dieThresholdTime = 3.0f;
    public bool death = false;
    bool previousDeath = false;

    public bool woozIsOn = false;
    public bool singleWoozPlayer;
    public float singleWoozDeltatimeMultiplier = 100.0f; //changes the speed with which the wooz player is moved
    private GameObject flame;
    private GameObject smoke;
    private GameObject innerFire;
    private GameObject outerFire;
    private GameObject innerIce;
    private GameObject outerIce;

    //TODO re add moveVectorK it was used for the wooz
    Vector3 moveVectorK;

    //EXAMPLE of an old logger, indicating if the update took to long, saving those moments in which it did
    //private bool updateDeath = false;
    //bool previousUpdateDeath = false;
    //public float updateDieThresholdTime = 0.08f;

    //keep count of "speed" 
    public Vector2[] positionsForSpeed;
    public int sizeOfSpeedVector = 15;
    //!!!! assign this in the scene per player it should be the actual arrow group in the scene not a prefab, thus it is assigned in the scene not in the prefab!
    private GameObject mainCameraObject;


    private GameObject backgroundImageObject;
    private trailScript trailscript;
    private GameSettings gameSettingsScript;
    private gameStateChecker gameStateCheckerScript;
    private texture_animation textureAnimationScript;

    // Use this for initialization
    void Start()
    {
        flame = this.transform.GetChild(1).gameObject;
        innerFire = flame.transform.GetChild(0).gameObject;
        outerFire = flame.transform.GetChild(1).gameObject;
        smoke = flame.transform.GetChild(2).gameObject;
        innerIce = flame.transform.GetChild(3).gameObject;
        outerIce = flame.transform.GetChild(4).gameObject;
        trailscript = this.GetComponent<trailScript>();
        mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        gameStateCheckerScript = mainCameraObject.GetComponent<gameStateChecker>();
        gameSettingsScript = mainCameraObject.GetComponent<GameSettings>();
        bothTrail = gameSettingsScript.BothTrail;
        textureAnimationScript = this.GetComponent<texture_animation>();

        updateTaggerMaterial();

        //buildMesh missileCopy = Instantiate<buildMesh>(missile);


        //average speed over 5 frames
        initiatePositionVector();

        startPosition = this.gameObject.transform.position;


        targetPos = startPosition;

        //keep track of last update time, be able to show and hide players based on this
        lastUpdateTime = Time.realtimeSinceStartup;
        lastDurationUpdateTime = 1.0f / 25.0f; //approximate framerate
    }

    // Update is called once per frame
    void Update()
    {
        // Set right material based on who is the tagger
        if (isTagger && !currentMatTagger)
        {
            updateTaggerMaterial();
            checkGameEnd();
        }
        else if (!isTagger && currentMatTagger) // if the player is not the tagger, but the used material is of the tagger -> change material
        {
            updateTaggerMaterial();
            checkGameEnd();
        }

        //WoozPlayer are wooz of oz players, we switch between normal server controlled players and wooz with pressing W in the game
        if (singleWoozPlayer)
        {
            singleWoozKeyMovePlayerInputHandler();
            lastUpdateTime = Time.realtimeSinceStartup;
        }

        if (woozIsOn)
        {
            lastUpdateTime = Time.realtimeSinceStartup;
        }

        //if a player hasnt been update in the previous seconds set it to a dead player and for instance later on hide it etc.
        if ((Time.realtimeSinceStartup - lastUpdateTime) > dieThresholdTime)
        {
            death = true;
            //also reset the time he/she has been a tagger we only use this in expo mode after all and we do not use adaptive circles here
        }
        else
        {
            death = false;
        }

        if (death != previousDeath)
        {
            if (death)
            {
                //hide the player and turn of its collider child.
                this.transform.GetComponent<Renderer>().enabled = false;
                Transform colliderChild = this.transform.FindChild("Collider");
                colliderChild.GetComponent<Collider>().enabled = false;

                //move towards start position 
                this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, startPosition, Time.deltaTime);

                //you might want to reset the material
                //this.renderer.material = runnerMat;

            }
            else
            {
                //make player visible again
                this.transform.GetComponent<Renderer>().enabled = true;
                Transform colliderChild = this.transform.FindChild("Collider");
                colliderChild.GetComponent<Collider>().enabled = true;
            }

            previousDeath = death;
        }
        //Collisions etc are calculated after fixedupdate, therefore moving rigidbodies should be dealt with in the fixedupdate
        //thus we move this to fixed update http://unity3d.com/learn/tutorials/modules/beginner/scripting/update-and-fixedupdate
        //similar to death this is called only once and never changed
        else if (death)
        {
            //death = true;
            //previousDeath = true;
            //this.transform.renderer.enabled = false;
            //Transform colliderChild = this.transform.FindChild("Collider");
            //colliderChild.collider.enabled = false;

            //move towards start position 
            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, startPosition, Time.deltaTime);
        }

        //EXAMPLE of another logger
        //this code was able to indicate moments in which the player hasnt been updated for a short amount of time, some frames have been dropped kind of thing:
        //if ((Time.realtimeSinceStartup-lastUpdateTime)>updateDieThresholdTime)
        //{
        //	updateDeath = true;
        //}
        //else
        //{
        //	updateDeath = false;
        //}

        //if (updateDeath != previousUpdateDeath)
        //{
        //	loggerScript.LogLineMissingUpdate(id, updateDeath);
        //	previousUpdateDeath = updateDeath;
        //}


    }

    void FixedUpdate()
    {
        if (!woozIsOn && !singleWoozPlayer)
        {
            //the updaterate from the moveTo is taken from the kinectrig and is approximmetly 30fps the lastUpdateTime and the lastDuration are also set there
            //the update of update is supposed to be quicker
            float fracComplete = (Time.realtimeSinceStartup - lastUpdateTime) / lastDurationUpdateTime;

            //Collisions etc are calculated after fixedupdate, therefor rigidbodies should be done in fixedupdate
            //thus we move this to fixed update http://unity3d.com/learn/tutorials/modules/beginner/scripting/update-and-fixedupdate
            //changed back to lerp to provide better collision detection and smoother movement etc.....
            //TODO make an extrapolation


            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, targetPos, fracComplete);//slowDownMovementMultiplier*fracComplete);
                                                                                                               //this.transform.localPosition = targetPos;
        }
        else
        {

            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.transform.localPosition + moveVectorK, singleWoozDeltatimeMultiplier * Time.deltaTime);
            //TODO check if this works
            //the player should stop moving when the button is not pressed
            moveVectorK = Vector3.zero;
        }

    }

    private void initiatePositionVector()
    {

        positionsForSpeed = new Vector2[sizeOfSpeedVector];
        positionsForSpeed[0].x = gameObject.transform.localPosition.x;
        //we will never update y
        //positionsForSpeed[0].y = gameObject.transform.localPosition.y;
        positionsForSpeed[0].y = gameObject.transform.localPosition.z;
        for (int i = 0; i < positionsForSpeed.Length; i++)
        {
            positionsForSpeed[i] = positionsForSpeed[0];
        }
    }
    public void resetPlayer()
    {
        if (bothTrail && isTagger)
        {
            trailscript.resetTrail(trailMaterialTagger);
        }
        else
        {
            trailscript.resetTrail(trailMaterial);
        }
        
        powerUpCounter = 0;
        updateTaggerMaterial();
        print("reset player is called");
    }
    
    public void updateTaggerMaterial()
    {
        if (isTagger)
        {
            if (gameSettingsScript.noTagging) // always enable fire if tagging is disabled 
            {
                innerFire.SetActive(true);
                outerFire.SetActive(true);
                innerIce.SetActive(false);
                outerIce.SetActive(false);
                currentMatTagger = true;
                this.GetComponent<Renderer>().material.mainTexture = taggerTexture[taggerTexture.Length - 1];
            }
            else
            {
                innerIce.SetActive(false);
                outerIce.SetActive(false);
                currentMatTagger = true;
                if (powerUpCounter >= taggerTexture.Length - 1)
                {
                    this.GetComponent<Renderer>().material.mainTexture = taggerTexture[taggerTexture.Length - 1];
                    innerFire.SetActive(true);
                    outerFire.SetActive(true);

                    textureAnimationScript.setColumns(1);

                }

                else
                {
                    innerFire.SetActive(false);
                    outerFire.SetActive(false);


                    this.GetComponent<Renderer>().material.mainTexture = taggerTexture[powerUpCounter];
                    if (powerUpCounter >= taggerTexture.Length - 2)
                    {
                        smoke.SetActive(true);
                        textureAnimationScript.setColumns(4);
                    }
                    else
                    {
                        textureAnimationScript.setColumns(6);
                        smoke.SetActive(false);
                    }
                }
            }
                if (!bothTrail)
                {
                    trailscript.enabled = false;
                }
                else
                {
                    trailscript.enabled = true;
                    trailscript.trail.GetComponent<Renderer>().material = trailMaterialTagger;
                    trailscript.trailMaterial = trailMaterialTagger;
                }
            }
            else
            {
                currentMatTagger = false;
                this.GetComponent<Renderer>().material.mainTexture = runnerTexture;
                trailscript.enabled = true;
                trailscript.trail.GetComponent<Renderer>().material = trailMaterial;
                trailscript.trailMaterial = trailMaterial;
                innerFire.SetActive(false);
                outerFire.SetActive(false);
                smoke.SetActive(false);
                innerIce.SetActive(true);
                outerIce.SetActive(true);
                textureAnimationScript.setColumns(1);
            }
    }
    public void checkGameEnd()
    {
        bool remainingPlayers = isTagger;
        bool gameEnd = true;
        foreach (var player in FindObjectsOfType(typeof(Player)) as Player[])
        {
            if (remainingPlayers != player.isTagger)
            { gameEnd = false; }
        }
        if (gameEnd)
        {
            if (isTagger)
            {
                print("red wins!");
                gameStateCheckerScript.redWins();
            }
            else
            {
                print("blue wins!");
                gameStateCheckerScript.blueWins();
            }
        }

    }

    //THERE IS SOMETHING STRANGE HAPPENNING with these values
    public Vector2 get2DSpeedVector()
    {
        //if I divide by correct number of frames here it goes wrong if I do it later on it is done correct..
        float xdiff = (positionsForSpeed[sizeOfSpeedVector - 1].x - positionsForSpeed[0].x);
        float ydiff = (positionsForSpeed[sizeOfSpeedVector - 1].y - positionsForSpeed[0].y);
        Vector2 returnVector = new Vector2(xdiff, ydiff);
        return returnVector;
    }

    private void updatePositionVector(Vector3 latestPosition)
    {
        for (int i = 0; i < positionsForSpeed.Length - 1; i++)
        {
            positionsForSpeed[i] = positionsForSpeed[i + 1];
        }
        positionsForSpeed[positionsForSpeed.Length - 1].x = latestPosition.x;
        positionsForSpeed[positionsForSpeed.Length - 1].y = latestPosition.z;

    }

    public void updateLastDetectionTime()
    {
        float actualDuration = Time.realtimeSinceStartup - lastUpdateTime;
        if (actualDuration > 1.0f)
        {
            lastDurationUpdateTime = 1.0f / 30.0f;
        }
        else
        {
            //TODO perhaps use an average instead of the last value
            lastDurationUpdateTime = Time.realtimeSinceStartup - lastUpdateTime;
        }
        lastUpdateTime = Time.realtimeSinceStartup;
    }

    //only accesed via the kinectrigclient
    public void moveTo(float x, float y, float z)
    {
        updateLastDetectionTime();
        //lastUpdateTime = Time.realtimeSinceStartup;
        if (!singleWoozPlayer)
        {
            //this.transform.position = new Vector3 (x, z, y);
            targetPos.x = x;
            targetPos.y = z;
            targetPos.z = y;
        }

        //the actual moving happends inside the fixed update in order to keep a kind of steady movement, less dependent on the tracker and more reliable physics calculations
    }

    //TODO ideally for collision detection this is done in FixedUpdate
    private void singleWoozKeyMovePlayerInputHandler()
    {

        moveVectorK = Vector3.zero;
        if (Input.GetKey(KeyCode.UpArrow))
        {
            moveVectorK.z = 0.2f;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            moveVectorK.z = -0.2f;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            moveVectorK.x = 0.2f;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            moveVectorK.x = -0.2f;
        }
        //this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, this.transform.localPosition+moveVectorK, singleWoozDeltatimeMultiplier*Time.deltaTime);
    }
}
