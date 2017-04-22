using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBehavior : MonoBehaviour {
	Text text;
	MessageImageAnimator image;
	public bool set_up = false;

	// Use this for initialization
	void Start () {
		text = GetComponentInChildren<Text> ();
		image = GetComponentInChildren<MessageImageAnimator> ();
		StartCoroutine (waitToBeSetup ());
	}

	public void display(string text_, string image_sequence, float duration){
		image.useSequence (image_sequence);
		text.text = text_; //I am ashamed of this line
							//but not enough to change it
		StartCoroutine(waitAndDeactivate(duration));
	}

	IEnumerator waitAndDeactivate(float wait_seconds){
		yield return new WaitForSeconds (wait_seconds);
		gameObject.SetActive (false);
	}

	IEnumerator waitToBeSetup(){
		yield return new WaitForSeconds (.005f);
		if (!set_up)
			StartCoroutine (waitToBeSetup ());
		else
			gameObject.SetActive (false);
	}

}
