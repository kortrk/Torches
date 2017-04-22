using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class darknessBehavior : MonoBehaviour {

	float glow_radius = 15f;
	SpriteRenderer sr;


	// Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer> ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void checkAround(GameObject[] torches){
		sr.enabled = true;
		sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, 1f);
		float shortest_distance = 999999999f;
		foreach (GameObject torch in torches) {
			if (torch != null && torch.GetComponent<ParticleSystem> ().isEmitting) {
				float distance = Vector3.Distance (torch.transform.position, transform.position);
				if (distance < glow_radius) {
					//I am illuminated
					//sr.enabled = false;
					if (distance < shortest_distance) {
						shortest_distance = distance;
					}
				}
			}
		}
		if (shortest_distance != 999999999f) {
			//print ("setting transparency to " + shortest_distance / glow_radius);
			sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, (shortest_distance / glow_radius)*(shortest_distance / glow_radius)/**.5f*/);
		}
	}
}
