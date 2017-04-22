using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class makeSpaceDark : MonoBehaviour {
	float radius = 4f;
	float x = 0;
	float y = 0;
	Vector2 quad_bounds;
	public GameObject darkness_prefab;
	int total_darkness = 0;

	// Use this for initialization
	void Start () {
		radius = darkness_prefab.transform.localScale.x;
		Bounds bounds = GameObject.FindObjectOfType<CameraFeed> ().gameObject.GetComponent<MeshRenderer> ().bounds;
		quad_bounds = new Vector2 (bounds.size.x, bounds.size.y);
		//print ("X quad bounds: "+quad_bounds.x+", Y quad bounds: "+quad_bounds.y);
		while (y < quad_bounds.y) {
			while (x < quad_bounds.x) {
				Instantiate (darkness_prefab, new Vector3 (x, y, 0), Quaternion.identity).transform.parent = transform; 
				Instantiate (darkness_prefab, new Vector3 (x, y, 0), Quaternion.identity).transform.parent = transform; 
				x += radius;
				//print ("new x: " + x);
				total_darkness++;
			}
			x = 0;
			y += radius;
			//print ("new y: " + y);
		}
		print ("total darkness: " + total_darkness);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
