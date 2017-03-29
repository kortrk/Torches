using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class expandBackground : MonoBehaviour {
	float original_width;
	float original_height;
	bool get_white = false;
	bool get_blue = false;
	bool get_black = false;
	bool get_red = false;
	public Sprite white_background;
	float progress_to_blue = 0f;
	float progress_to_red = 0f;
	Color chosen_blue;

	// Use this for initialization
	void Start () {
		original_width = GetComponent<SpriteRenderer> ().bounds.size.x;
		original_height = GetComponent<SpriteRenderer> ().bounds.size.y;
		chosen_blue = new Color (0f, 0f, 75f);

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
		Color sr_color = GetComponent<SpriteRenderer> ().color;
		if (get_white && sr_color.a < 1f) {
			GetComponent<SpriteRenderer> ().color = new Color (1f, 1f, 1f, sr_color.a + .01f); 
			if (sr_color.a >= 1f) get_white = false; //make sure this doesn't restart later in another part of the show
		}
		if (get_blue) {
			GetComponent<SpriteRenderer> ().color = Color.Lerp (Color.white, chosen_blue, progress_to_blue);
			progress_to_blue += .03f;
			if (sr_color == chosen_blue)
				get_blue = false;
		}
		if (get_black) {
			float fade_red = Mathf.Ceil(sr_color.r)*.1f; //these will be non-zero
			float fade_green = Mathf.Ceil(sr_color.g)*.1f;//only if r,g,b respectively
			float fade_blue = Mathf.Ceil(sr_color.b)*.1f;//are non-zero
			GetComponent<SpriteRenderer> ().color = new Color(sr_color.r - fade_red, sr_color.g - fade_green, sr_color.b - fade_blue);
			if (sr_color == Color.black)
				get_black = false;
		}
		if (get_red) {
			GetComponent<SpriteRenderer> ().color = Color.Lerp (Color.black, Color.red, progress_to_red);
			progress_to_red += .01f;
		}
	}

	public IEnumerator whiten(){
		yield return new WaitForSeconds (1f);
		GetComponent<SpriteRenderer> ().sprite = white_background;
		get_white = true;
	}

	public void make_it_blue(){
		GetComponent<SpriteRenderer> ().sprite = white_background;
		get_blue = true;
	}

	public void blacken(){
		GetComponent<SpriteRenderer> ().sprite = white_background;
		get_black = true;
	}

	public void redden(){
		GetComponent<SpriteRenderer> ().sprite = white_background;
		get_red = true;
	}
}
