using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageBehavior : MonoBehaviour {
	Text text;
	MessageImageAnimator image;
	public bool set_up = false;
	bool display_called = false;

	// Use this for initialization
	void Start () {
		text = GetComponentInChildren<Text> ();
		image = GetComponentInChildren<MessageImageAnimator> ();
		StartCoroutine (waitToBeSetup ());
	}

	public void display(string text_, string image_sequence, float duration){
		print ("DISPLAY");
		display_called = true;
		if (!set_up) {
			StartCoroutine (waitToDisplay (text_, image_sequence, duration));
			return;
		}
		image.useSequence (image_sequence);
		text.text = text_; //I am ashamed of this line
							//but not enough to change it
		StartCoroutine(waitAndDeactivate(duration));
		display_called = false;
	}

	IEnumerator waitAndDeactivate(float wait_seconds){
		yield return new WaitForSeconds (wait_seconds);
		gameObject.SetActive (false);
	}

	IEnumerator waitToBeSetup(){
		yield return new WaitForSeconds (.005f);
		if (display_called) yield break; //we no longer want to deactivate, 
											//which this is waiting to do
		if (!set_up)
			StartCoroutine (waitToBeSetup ());
		else
			gameObject.SetActive (false);
	}

	IEnumerator waitToDisplay(string text_, string image_sequence, float duration){
		yield return new WaitForSeconds (.005f);
		if (!set_up)
			StartCoroutine (waitToDisplay (text_, image_sequence, duration));
		else
			display(text_, image_sequence, duration);
	}

}
