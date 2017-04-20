using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class blinkBriefly : MonoBehaviour {
	bool blink_on = false;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void blink(){
		//cancel out any old coroutines
		if (blink_on) StopCoroutine (blink_coroutine()); //there can be only one! 
		StartCoroutine (blink_coroutine());
	}

	IEnumerator blink_coroutine(){
		GetComponent<SpriteRenderer> ().enabled = true;
		blink_on = true;
		yield return new WaitForSeconds (.3f);
		GetComponent<SpriteRenderer> ().enabled = false;
		blink_on = false;
	}
}
