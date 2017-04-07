using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class fireworkBallBehavior : MonoBehaviour {
	float travel_rate = 25f;
	Vector3 destination;
	bool move = true;
	GameObject burst;
	Color color;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (move) {
			if (Vector3.Distance (transform.position, destination) < travel_rate) {
				transform.position = destination;
				move = false;
			} else
				transform.position = Vector3.MoveTowards (transform.position, destination, travel_rate);
		} 
		else {
			GameObject new_firework = (GameObject) Instantiate (burst, transform.position, Quaternion.identity);
			ParticleSystem.MainModule settings = new_firework.GetComponent<ParticleSystem>().main;
			settings.startColor = new ParticleSystem.MinMaxGradient (color);
			//ParticleSystem.EmissionModule emission = GetComponent<ParticleSystem>().emission;
			//emission.rateOverTime = new ParticleSystem.MinMaxCurve (0f);
			Destroy (gameObject);
		}
	}

	public void Launch(Vector3 start_loc, Vector3 goal_loc, Color c, GameObject burst_prefab){
		transform.position = start_loc;
		destination = goal_loc;
		//GetComponent<ParticleSystem> ().startColor = c;
		ParticleSystem.MainModule settings = GetComponent<ParticleSystem>().main;
		settings.startColor = new ParticleSystem.MinMaxGradient (c);
		color = c;
		burst = burst_prefab;
		move = true;
	}
}
