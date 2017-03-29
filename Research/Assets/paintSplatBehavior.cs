using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class paintSplatBehavior : MonoBehaviour {
	Vector3 original_scale;
	float time_to_full_size = .1f;//seconds 
	float start_time;
	bool done = false;

	// Use this for initialization
	void Start () {
		original_scale = transform.localScale;
		transform.localScale = Vector3.one;
		start_time = Time.time;
		transform.rotation = Quaternion.Euler( new Vector3(0, 0, Random.Range(0, 360)) );
	}
	
	// Update is called once per frame
	void Update () {
		if (done) return;
		float percentage = (Time.time - start_time) / time_to_full_size;
		transform.localScale = original_scale * percentage;
		if (percentage > 1) { 
			transform.localScale = original_scale;
			done = true;
		}

	}
}
