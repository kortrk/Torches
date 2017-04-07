using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class boxFireworksBehavior : MonoBehaviour {
	Vector3 original_size; 
	float total_time_to_grow = .5f;
	float start_time;


	// Use this for initialization
	void Start () {
		if (name.Contains ("circle")) total_time_to_grow = .3f;
		start_time = Time.time;
		original_size = transform.localScale;
		transform.localScale = transform.localScale * .1f;
	}
	
	// Update is called once per frame
	void Update () {
		transform.localScale = Vector3.Lerp (transform.localScale, original_size, 
			(Time.time - start_time) / total_time_to_grow);
	}
}
