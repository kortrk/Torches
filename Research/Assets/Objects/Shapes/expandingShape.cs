using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class expandingShape : MonoBehaviour {
	Vector2 quad_center;
	float total_move_distance;
	float total_rotation_distance = 0f;
	bool get_moving = false;
	Vector3 full_size;
	bool turn = false;

	// Use this for initialization
	void Start () {

	}
	
	// Update is called once per frame
	void Update () {
		if (!get_moving) return;
		transform.position = Vector3.MoveTowards (transform.position, quad_center, 6f);
		//print (Vector2.Distance (transform.position, quad_center)+"/"+total_move_distance);
		float percentage = 1f - (Vector2.Distance (transform.position, quad_center) / total_move_distance);
		transform.localScale = full_size * percentage;
		if (turn) transform.rotation = Quaternion.Euler(0f, 0f, total_rotation_distance*(Vector2.Distance (transform.position, quad_center) / total_move_distance));
		if (percentage >= 1f) get_moving = false; //stop moving if we're there
	}

	public void StartMoveAndTurn(Vector2 center, bool do_turn){
		quad_center = center;
		full_size = transform.localScale;
		transform.localScale = Vector3.one;
		total_move_distance = Vector2.Distance (transform.position, quad_center);
		total_rotation_distance = total_move_distance*3;
		get_moving = true;
		turn = do_turn;
		if (turn) transform.rotation = Quaternion.Euler(0f, 0f, total_rotation_distance);
	}
}
