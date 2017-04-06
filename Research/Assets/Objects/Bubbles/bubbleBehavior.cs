using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Note: in the cleanup of this phase, we must search
//out any remaining bubbles and pop them.

public class bubbleBehavior : MonoBehaviour {
	Vector3 goal;
	float travel_time = 2f; //milliseconds
	float start_time = 0f;
	float screen_width;
	bool waiting_to_pop = false;
	public GameObject pop_particles;
	CameraFeed cf;

	// Use this for initialization
	void Start () {
		travel_time = Random.Range (180f, 230f);
		start_time = Time.time;
		transform.localScale = transform.localScale * Random.Range (.8f, 1.2f);
		cf = GameObject.FindObjectOfType<CameraFeed> ();
	}
	
	// Update is called once per frame
	void Update () {
		if (transform.position != goal) {
			float time_covered = Time.time - start_time;
			transform.position = Vector2.Lerp (transform.position, goal, time_covered / travel_time);
			if (Vector2.Distance (transform.position, goal) < screen_width * .01f) 
				transform.position = goal; //if we're close enough, we snap to goal
		} else {
			if (!waiting_to_pop) {
				waiting_to_pop = true;
				StartCoroutine (WaitAndPop ());
			}
		}

		foreach (Blob p in cf.green_blobs)
			if (Vector2.Distance (p.getCenter ()/cf.SCREEN_SCALEDOWN, transform.position) < GetComponent<SpriteRenderer> ().bounds.size.x*.5f)
				Pop ();

		if (Vector2.Distance (transform.position, Input.mousePosition/cf.SCREEN_SCALEDOWN) < GetComponent<SpriteRenderer> ().bounds.size.x*.5f)
			Pop ();
	}

	public void setStartAndGoal(Vector2 quad_center, float width, float height, bool start_at_center){
		screen_width = width;
		Vector3 pos_near_edge;
		//are we going near the top/bottom or near a side?
		bool by_side = (Random.Range(0,1) == 0);
		//will we be near the left/top or the right/bottom?
		int by_lesser;
		if (Random.value < .5f)
			by_lesser = 1;
		else
			by_lesser = -1;

		pos_near_edge = Vector3.zero;
		if (by_side) {
			//then x is the one that needs to be specific - 
			//y can be anything
			pos_near_edge.y = Random.Range (0, height);
			//put x somewhere by the edge
			pos_near_edge.x = quad_center.x + by_lesser * ((width/2 * .85f) + Random.Range(0, width/2*.15f));
		} else {
			//x can be anything
			pos_near_edge.x = Random.Range(0, width);
			//pick a y near the top or bottom edge
			pos_near_edge.y = quad_center.y + by_lesser * ((height/2 * .85f) + Random.Range(0, height/2*.15f));
		}

		//print ("pos near edge: " + pos_near_edge.x + ", " + pos_near_edge.y);

		if (start_at_center) {
			transform.position = quad_center + Random.insideUnitCircle * (width*.2f);
			goal = pos_near_edge;
		} else {
			transform.position = pos_near_edge;
			goal = quad_center + Random.insideUnitCircle * (width*.2f);
		}
	}

	IEnumerator WaitAndPop(){
		yield return new WaitForSeconds (1f);
		Pop ();
	}

	public void Pop(){
		Instantiate (pop_particles, transform.position, Quaternion.identity);
		Destroy (gameObject);
	}

}
