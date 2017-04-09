using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class torchFireColor : MonoBehaviour {
	Slider slider;

	// Use this for initialization
	void Start () {
		slider = GameObject.Find ("Slider").GetComponent<Slider>();
	}
	
	// Update is called once per frame
	void Update () {
		ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
		settings.startColor = new ParticleSystem.MinMaxGradient (Color.HSVToRGB(slider.value, 1f, 1f));
	}
}
