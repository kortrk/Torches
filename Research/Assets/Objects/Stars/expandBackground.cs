using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class expandBackground : MonoBehaviour {
	float goal_width;
	float goal_height;

	float start_time;
	float change_time;
	Color old_color;
	Color new_color;
	float original_alpha;
	float goal_alpha;
	bool changing = false;
	public bool grow = false;


	// Use this for initialization
	void Start () {
		GameObject quad = GameObject.Find ("Quad");
		goal_width = quad.GetComponent<MeshRenderer> ().bounds.size.x;
		goal_height = quad.GetComponent<MeshRenderer> ().bounds.size.y;
		transform.position = quad.transform.position;

		if (!grow) {
			float current_w = GetComponent<SpriteRenderer>().bounds.size.x;
			float current_h = GetComponent<SpriteRenderer>().bounds.size.y;
			transform.localScale = 
				new Vector2 (transform.localScale.x * (goal_width / current_w),
					transform.localScale.y * (goal_height / current_h));
		} else {
			transform.localScale = Vector3.zero;
		}

	}
	
	// Update is called once per frame
	void Update () {
		if (grow) {
			Vector3 bounds_size = GetComponent<SpriteRenderer> ().bounds.size;
			if (bounds_size.x < goal_width) {
				transform.localScale = new Vector3 (transform.localScale.x + 1000, transform.localScale.y, 1);
			}
			if (bounds_size.y < goal_height) {
				transform.localScale = new Vector3 (transform.localScale.x, transform.localScale.y + 700, 1);
			}
		}
		if (changing) changeColor ();
	}

	public void startSwitchColor(Color goal_color, float transparency, float time){
		start_time = Time.time;
		change_time = time;
		old_color = GetComponent<SpriteRenderer> ().color;
		new_color = goal_color;
		original_alpha = old_color.a;
		goal_alpha = transparency;
		changing = true;
	}

	void changeColor(){
		float percentage = (Time.time - start_time) / change_time;
		SpriteRenderer sr = GetComponent<SpriteRenderer>();
		sr.color = Color.Lerp (old_color, new_color, percentage);
		float new_alpha = Mathf.Lerp (original_alpha, goal_alpha, percentage);
		sr.color = new Color (sr.color.r, sr.color.g, sr.color.b, new_alpha);
		if (percentage >= 1f) changing = false;
	}
}
