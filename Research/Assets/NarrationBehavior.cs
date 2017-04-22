using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NarrationBehavior : MonoBehaviour {
	public AudioClip intro;
	public AudioClip welcome_to_torches;
	public AudioClip heart_intro;
	public AudioClip rpi;
	public AudioClip our_own_torches;
	public AudioClip tapestry_of_light;
	public AudioClip thank_you;

	private AudioSource audiio;

	// Use this for initialization
	void Start () {
		audiio = GetComponent<AudioSource> ();
	}

	void Update(){

	}

	public void PlayClip(string s, float delay = 0){
		switch (s) {
		case "intro":
			audiio.clip = intro;
			audiio.PlayDelayed(delay); //calling play each case is redundant, but safer
			break;

		case "welcome":
			audiio.clip = welcome_to_torches;
			audiio.PlayDelayed (delay); //calling play each case is redundant, but safer
			break;

		case "heart":
			audiio.clip = heart_intro;
			audiio.PlayDelayed (delay); //calling play each case is redundant, but safer
			break;

		case "rpi":
			audiio.clip = rpi;
			audiio.PlayDelayed (delay); //calling play each case is redundant, but safer
			break;

		case "torches":
			audiio.clip = our_own_torches;
			audiio.PlayDelayed (delay); //calling play each case is redundant, but safer
			break;

		case "tapestry":
			audiio.clip = tapestry_of_light;
			audiio.PlayDelayed (delay); //calling play each case is redundant, but safer
			break;

		case "thank you":
			audiio.clip = thank_you;
			audiio.PlayDelayed (delay); //calling play each case is redundant, but safer
			break;
		}
	}
}
