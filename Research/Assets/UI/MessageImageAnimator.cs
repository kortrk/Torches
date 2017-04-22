using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageImageAnimator : MonoBehaviour {
	public Sprite green_up_sprite;
	public Sprite red_up_sprite;
	public Sprite red_down_sprite;
	public Sprite OR_sprite;

	Dictionary<string, Sprite[]> sprite_sequences;
	Sprite[] green_up;
	Sprite[] red_down;
	Sprite[] green_or_red;
	Sprite[] green_and_red_flip;
	Sprite[] current;

	int flip_wait_frames = 15; //how many frames before we show next sprite
	int flip_countdown;
	int pos_in_current = 0;

	// Use this for initialization
	void Start () {
		current = new Sprite[0];

		green_up = new Sprite[]{green_up_sprite};
		red_down = new Sprite[]{red_down_sprite};
		green_or_red = new Sprite[]{green_up_sprite, OR_sprite, red_up_sprite, OR_sprite};
		green_and_red_flip = new Sprite[]{green_up_sprite, red_up_sprite};

		sprite_sequences = new Dictionary<string, Sprite[]> ();
		sprite_sequences.Add ("green up", green_up);
		sprite_sequences.Add ("paddles down", red_down);
		sprite_sequences.Add ("green or red", green_or_red);
		sprite_sequences.Add ("flip", green_and_red_flip);

		flip_countdown = flip_wait_frames;

		//send up a signal that we can be set inactive
		transform.parent.gameObject.GetComponent<MessageBehavior>().set_up = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (flip_countdown > 0) {
			flip_countdown--;
		} else {
			flip_countdown = flip_wait_frames;
			pos_in_current++;
			if (current.Length != 0)
				GetComponent<Image> ().sprite = current [pos_in_current % current.Length];
		}
	}

	public void useSequence(string sequence_name){
		current = 
			sprite_sequences [sequence_name];
		pos_in_current = 0;
		GetComponent<Image> ().sprite = current [pos_in_current % current.Length];
	}
}
