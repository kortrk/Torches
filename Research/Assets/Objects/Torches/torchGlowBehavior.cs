using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class torchGlowBehavior : MonoBehaviour {
	float currentY = 0;
	float currentX = 0;
	float x_change_rate = .2f; //rate at which x changes. also, a pun.

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		//we move across the perlin noise "square" going 
		//left to right and picking a new y when needed
		currentX += x_change_rate;
		if (currentX >= 1f) {
			currentX = 0;
			currentY = Random.value;
		}
		GetComponent<SpriteRenderer> ().color = transform.parent.GetComponent<torchBehavior> ().getColor ();
		Color current_color = GetComponent<SpriteRenderer> ().color;
		GetComponent<SpriteRenderer>().color = new Color(current_color.r, current_color.g, current_color.b, 
			1f - Mathf.PerlinNoise(currentX, currentY)/2f);
	}
}
