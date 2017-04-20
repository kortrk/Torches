using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class globeQuadBehavior : MonoBehaviour {
	bool fade_in = true;
	bool fade_down = false;
	float fade_down_goal;
	float rate = .03f;

	// Use this for initialization
	void Start () {
		

	}
	
	// Update is called once per frame
	void Update () {
		if (fade_in) {
			Color c = GetComponent<MeshRenderer> ().material.GetColor ("_TintColor");
			GetComponent<MeshRenderer> ().material.SetColor ("_TintColor", new Color(c.r, c.g, c.b, c.a+rate));
			if (c.a >= 1f) fade_in = false;
		}
		if (fade_down) {
			print ("fading");
			Color c = GetComponent<MeshRenderer> ().material.GetColor ("_TintColor");
			GetComponent<MeshRenderer> ().material.SetColor ("_TintColor", new Color(c.r, c.g, c.b, c.a-rate));
			if (c.a <= fade_down_goal) fade_down = false;
		}
	}

	public void FadeDown(float goal){
		print ("fading down");
		fade_in = false;
		fade_down = true;
		fade_down_goal = goal;
	}

	public void FadeDown(float goal, float rate_){
		print ("fading down");
		fade_in = false;
		fade_down = true;
		fade_down_goal = goal;
		rate = rate_;
	}
}
