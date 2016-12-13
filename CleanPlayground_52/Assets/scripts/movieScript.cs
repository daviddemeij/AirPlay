using UnityEngine;
using System.Collections;
[RequireComponent (typeof(AudioSource))]
public class movieScript : MonoBehaviour {
    public MovieTexture movie;
	// Use this for initialization
	void Start () {
       GetComponent<Renderer>().material.mainTexture = movie as MovieTexture;

	}
    public void playInstruction()
    {
        movie.Play();
        GetComponent<AudioSource>().clip = movie.audioClip;
        GetComponent<AudioSource>().Play();

    }
    public void stopInstruction()
    {
        movie.Stop();
        GetComponent<AudioSource>().Stop();
    }

}
