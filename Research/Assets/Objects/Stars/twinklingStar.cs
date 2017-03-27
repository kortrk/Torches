using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class twinklingStar : MonoBehaviour {
	int position_last_set = 0;
	public int twinkle_frame_interval = 40;
	public int twinkle_frame_countdown = 40;
	public Sprite sprite1;
	public Sprite sprite2;

	// Use this for initialization
	void Start () {
		int which_sprite = Random.Range (0, 2);
		if (which_sprite == 0)
			GetComponent<SpriteRenderer> ().sprite = sprite1;
		else
			GetComponent<SpriteRenderer> ().sprite = sprite2;
	}
	
	// Update is called once per frame
	void Update () {
		position_last_set++;
		//if it's been too long since our 
		//paddle was seen, disappear
		if (position_last_set > 5) {
			GetComponent<SpriteRenderer> ().sprite = null;
			return; //saves us a test to see if the sprite's null
		}

		if (GetComponent<SpriteRenderer>().sprite != null)
			twinkle (); //no point twinkling a star that isn't shown
	}

	public void setLocation(Vector2 v2){
		transform.position = v2;
		position_last_set = 0;
		if (GetComponent<SpriteRenderer> ().sprite == null)
			GetComponent<SpriteRenderer> ().sprite = sprite1;
	}

	void twinkle(){
		twinkle_frame_countdown--;
		if (twinkle_frame_countdown <= 0) {
			twinkle_frame_countdown = twinkle_frame_interval;
			if (GetComponent<SpriteRenderer> ().sprite == sprite1)
				GetComponent<SpriteRenderer> ().sprite = sprite2;
			else if (GetComponent<SpriteRenderer> ().sprite == sprite2) {
				GetComponent<SpriteRenderer> ().sprite = sprite1;
			}
		}
	}
}
