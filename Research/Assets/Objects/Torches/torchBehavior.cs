using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class torchBehavior : MonoBehaviour {
	//this var represents the color of the children
	//not the torch itself
	Color color = Color.red;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//fade in
		Color c = GetComponent<SpriteRenderer> ().color;
		if (c.a < 1f) {
			GetComponent<SpriteRenderer> ().color = new Color (c.r, c.g, c.b, c.a + .1f);
		}
	}

	public Color getColor(){
		return color;
	}
}
