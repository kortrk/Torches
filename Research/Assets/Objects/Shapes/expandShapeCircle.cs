using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class expandShapeCircle : MonoBehaviour {
	float goal_radius = 100f;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (GetComponent<SpriteRenderer>().bounds.size.x < goal_radius){
			transform.localScale = transform.localScale * 1.2f;
		}
	}

	public void setGoalRadius(float width, float height){
		float a_squared_plus_b_squared = width * width + height * height;
		goal_radius = Mathf.Sqrt (a_squared_plus_b_squared);
	}
}
