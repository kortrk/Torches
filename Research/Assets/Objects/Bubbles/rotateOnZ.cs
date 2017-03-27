using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotateOnZ : MonoBehaviour {
	float rotation_speed = 1.5f;

	// Use this for initialization
	void Start () {
		rotation_speed = Random.Range (-3f, 3f);
	}
	
	// Update is called once per frame
	void Update () {
		transform.eulerAngles = new Vector3 (transform.eulerAngles.x, 
			transform.eulerAngles.y, transform.eulerAngles.z - rotation_speed);
		
	}
}
