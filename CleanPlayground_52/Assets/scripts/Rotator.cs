using UnityEngine;
using System.Collections;

public class Rotator : MonoBehaviour {
    private float lastUpdate;
    public float moveTimer;
    private GameSettings gamesettingsScript;
    private GameObject mainCameraObject;
    private bool singlePlayer;
    private bool coinBattle;
    // Use this for initialization
    void Start () {
        mainCameraObject = GameObject.FindGameObjectWithTag("MainCamera");
        gamesettingsScript = mainCameraObject.GetComponent<GameSettings>();
        singlePlayer = gamesettingsScript.singlePlayer;
        coinBattle = gamesettingsScript.coinBattle;
        print(singlePlayer);
        if (singlePlayer && gamesettingsScript.nrOfPlayers<2)
        {
            lastUpdate = Time.time;
        }
	}
	
	// Update is called once per frame
	void Update () {
        transform.Rotate(new Vector3(50, 0, 0) * Time.deltaTime);
        if((singlePlayer && gamesettingsScript.nrOfPlayers < 2)||coinBattle) {
            if (Time.time> lastUpdate+moveTimer)
            {
                transform.position = new Vector3(Random.Range(0.65f, 2.95f), 0, Random.Range(-3.1f, -0.2f));
                lastUpdate = Time.time;
            }
        }
	}
}
