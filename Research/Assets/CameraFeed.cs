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
	public GameObject color_indicator;

	//each paddle is a set of pixels
	List<HashSet<Pixel>> green_paddles;
	List<HashSet<Pixel>> red_paddles;

	WebCamTexture webcam;

	int g_paddles_found = 0;

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
		webcam.requestedWidth = 1280;
		webcam.requestedHeight = 720;
		webcam.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		getPaddles ();


		//TEST
		if (Input.GetKeyDown (KeyCode.P))
			print (g_paddles_found);
		mouseColorTest();
	}
		
	Vector2 mouseInWorld(){
		Vector3 v3 = Input.mousePosition;
		v3.z = 10.0f;
		v3 = Camera.main.ScreenToWorldPoint(v3);
		return new Vector2((int) v3.x, (int) v3.y);
	}

	void mouseColorTest(){
		Vector2 mp = mouseInWorld (); 
		Color hue = webcam.GetPixels() [(int)(mp.x + 1280*mp.y)];
		print ("Mouse: "+mp.x+","+mp.y);
		color_indicator.SetActive (true);
		color_indicator.GetComponent<SpriteRenderer> ().color = hue;

		if (Input.GetMouseButtonDown (0)) {
			float h, s, v;
			Color.RGBToHSV (hue, out h, out s, out v);
			print (hue+" "+h*360+","+s*100+","+v*100);
		}
	}
	
	void getPaddles(){
		//Using Google Color Selector:
		//google.com/#q=color+selector
		//Green	hue is between 102 and 140
		//		sat > 55
		//		val > 33
		//Red 	hue is < 15 or > 355
		//		sat > 55
		//		val > 33
		Color[] pixels = webcam.GetPixels();
		float h = 0;
		float s = 0;
		float v = 0;
		for (int i = 0; i < pixels.Length; i++) {
			Color.RGBToHSV (pixels [i], out h, out s, out v);
			if (160 < h*360 && h*360 < 170 && s*100 > 50 && v*100 > 33) {
				//^ the range of valid greens 
				g_paddles_found++;
			}
		}
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
