using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dotBehavior : MonoBehaviour {
	float start_time;
	float total_time;
	bool fade_in = false;
	SpriteRenderer sr;
	float original_scale;

	// Use this for initialization
	void Start () {
		sr = GetComponent<SpriteRenderer> ();
		original_scale = transform.localScale.x;
	}
	
	// Update is called once per frame
	void Update () {
		if (fade_in) {
			float percentage = (Time.time - start_time) / total_time;
			sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, percentage);
			transform.localScale = new Vector3 (original_scale * percentage, original_scale * percentage, original_scale * percentage);
			if (percentage >= 1f) {
				fade_in = false;
				transform.localScale = new Vector3 (original_scale, original_scale, original_scale);
			}
		}

	}

	public void startUp(float wait_seconds, Color c){
		start_time = Time.time;
		total_time = wait_seconds;
		fade_in = true;
		GetComponent<SpriteRenderer> ().color = new Color (c.r, c.g, c.b, 0f);
	}
}
