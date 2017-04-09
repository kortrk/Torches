using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class torchGlowBehavior : MonoBehaviour {
	float currentY = 0;
	float currentX = 0;
	float x_change_rate = .2f; //rate at which x changes. also, a pun.
	Slider slider;

	// Use this for initialization
	void Start () {
		slider = GameObject.Find ("Slider").GetComponent<Slider> ();
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
		GetComponent<SpriteRenderer> ().color = Color.HSVToRGB (slider.value, 1f, 1f);
		Color current_color = GetComponent<SpriteRenderer> ().color;
		GetComponent<SpriteRenderer>().color = new Color(current_color.r, current_color.g, current_color.b, 
			1f - Mathf.PerlinNoise(currentX, currentY)/2f);
	}
}
