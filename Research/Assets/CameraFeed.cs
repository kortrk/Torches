using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pixel{
	Color color;
	int x;
	int y;

	public Pixel(int x_, int y_){
		x = x_;
		y = y_;
	}
}

public class CameraFeed : MonoBehaviour {
	//each paddle is a set of pixels
	List<HashSet<Pixel>> green_paddles;
	List<HashSet<Pixel>> red_paddles;

	WebCamTexture webcam;

	// Use this for initialization
	void Start () {
		//set up the paddle lists
		green_paddles = new List<HashSet<Pixel>> ();
		red_paddles = new List<HashSet<Pixel>> ();

		foreach (WebCamDevice w in WebCamTexture.devices) {
			print (w.name);
		}
		webcam = new WebCamTexture (WebCamTexture.devices [1].name);
		if (GetComponent<MeshRenderer> () != null)
			GetComponent<MeshRenderer> ().material.mainTexture = webcam;
		else if (GetComponent<SpriteRenderer> () != null)
			GetComponent<SpriteRenderer> ().material.mainTexture = webcam;
		webcam.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		//getPaddles ();

		Vector2 mp = mouseInWorld ();
		print (mp.x + "," + mp.y);

		//TEST
		//if (Input.GetMouseButtonDown (0)) {
			//Vector2 m_pos = mouseInWorld ();
			Color hue = webcam.GetPixel (10, 10);
			//print ("Hue: " + hue);
			GameObject.Find ("Selected Color Indicator").GetComponent<SpriteRenderer> ().color = hue;
		//}
	}
		
	Vector2 mouseInWorld(){
		Vector3 v3 = Input.mousePosition;
		v3.z = 10.0f;
		v3 = Camera.main.ScreenToWorldPoint(v3);
		return new Vector2((int) v3.x, (int) v3.y);
	}
	
	void getPaddles(){

	}

	/*
	 * 1/21 Notes:
	 * Brighten projection?
	 * System for recording pixel color on click
	 * Ignore regions
	 * Walking in would be good with Option 7 (Cloud)
	 * 	from the lighting panel
	 */
}
