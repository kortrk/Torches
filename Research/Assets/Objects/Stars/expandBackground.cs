using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class expandBackground : MonoBehaviour {
	float original_width;
	float original_height;


	// Use this for initialization
	void Start () {
		original_width = GetComponent<SpriteRenderer> ().bounds.size.x;
		original_height = GetComponent<SpriteRenderer> ().bounds.size.y;

		transform.localScale = Vector3.zero;

	}
	
	// Update is called once per frame
	void Update () {
		Vector3 bounds_size = GetComponent<SpriteRenderer> ().bounds.size;
		if (bounds_size.x < original_width) {
			transform.localScale = new Vector3(transform.localScale.x+5000, transform.localScale.y, 1);
		}
		if (bounds_size.y < original_height) {
			transform.localScale = new Vector3(transform.localScale.x, transform.localScale.y+2500, 1);
		}
	}
}
