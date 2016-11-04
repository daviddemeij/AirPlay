using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;



public class BackGroundChanger : MonoBehaviour {

	//public Texture designLabMat, tagYouReItMat, s2d4sMat, commitMat; 
	public Texture[] carpet;
	//public GameObject backgroundImage;
	private int totalNumberOfTags = 0;
	public float lastTagTime;


	// Use this for initialization
	void Start () {
		//this.guiTexture.texture = s2d4sMat;
		if (carpet.Length>0 && carpet[0]!=null)
			this.GetComponent<GUITexture>().texture = carpet[0];
		totalNumberOfTags = 0;
		lastTagTime = 0;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Tagged (bool tag) {
		if (tag)
		{
			totalNumberOfTags = totalNumberOfTags+1;
			if (totalNumberOfTags >15)
				totalNumberOfTags = 0;

			Debug.Log ("repeated change");
			if (totalNumberOfTags <2 && carpet[0]!=null)
				this.GetComponent<GUITexture>().texture = carpet[0];
			else if (totalNumberOfTags<4&& carpet[1]!=null )
				this.GetComponent<GUITexture>().texture = carpet[1];
			else if (totalNumberOfTags<6)
				this.GetComponent<GUITexture>().texture = carpet[2];
			else if (totalNumberOfTags<8)
				this.GetComponent<GUITexture>().texture = carpet[3];
			else if (totalNumberOfTags<10)
				this.GetComponent<GUITexture>().texture = carpet[4];
			else if (totalNumberOfTags<12)
				this.GetComponent<GUITexture>().texture = carpet[5];
			else if (totalNumberOfTags<14)
				this.GetComponent<GUITexture>().texture = carpet[6];
			else if (totalNumberOfTags<16)
				this.GetComponent<GUITexture>().texture = carpet[7];
		}

	}

}
