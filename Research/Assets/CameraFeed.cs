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
	//constants
	int WIDTH = 1280;
	int HEIGHT = 720;

	//public variables
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
		webcam.requestedWidth = WIDTH;
		webcam.requestedHeight = HEIGHT;
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
		Color hue = webcam.GetPixels() [(int)(mp.x + WIDTH*mp.y)];
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
		HashSet<int> already_in_paddles;
		float h = 0;
		float s = 0;
		float v = 0;
		for (int i = 0; i < pixels.Length; i++) {
			if (already_in_paddles.Contains (i)) continue;
			Color.RGBToHSV (pixels [i], out h, out s, out v);
			if (isGreen(h,s,v)) {
				HashSet<Pixel> paddle = BFS (i, pixels, already_in_paddles, "green");
				//we may actually want a paddle object: {pxl_set, center}
				green_paddles.Add (paddle);
			}
		}
	}

	HashSet<int> BFS (int i, Color[] pixels, HashSet<Pixel> already_in_pixels, string color){
		//add this to a paddle hashset
		HashSet<int> paddle = new HashSet<int>();
		HashSet<int> visited = new HashSet<int>();	//all the pixels we've seen, including non-red/non-green
		Queue<int> q = new Queue<int>();
		visited.Add (i);
		q.Enqueue (i);

		while (q.Count != 0) {
			int x = q.Dequeue ();
			//there are potentially eight neighbors for this pixel
			List<int> neighbors = getPixelNeighbors(i, pixels.Length);
			//check each neighbor - if it's the right color, it goes in q
			Vector3 i_color = hsvAt(i, pixels); //pun
			if (color=="green" && isGreen(i_color.x,i_color.y,i_color.z) /*|| (color=="red"&& isRed())*/){

			}
		}

	}

	Vector3 hsvAt(int i, Color[] pixels){
		//TODO: get the hsv of an index in pixels
		return new Vector3 ();
	}

	List<int> getPixelNeighbors(int i, int length){
		List<int> result = new List<int> ();
		if (i + 1 < length) //right
			result.Add (i + 1);
		if (i - 1 > 0) //left
			result.Add (i - 1);
		if (i - WIDTH > 0)//above
			result.Add(i - WIDTH);
		if (i + WIDTH < length)//below
			result.Add(i + WIDTH);
		if (i - WIDTH + 1 > 0)//diag r above
			result.Add(i - WIDTH + 1);
		if (i - WIDTH - 1 > 0)//diag l above
			result.Add(i - WIDTH - 1);
		if (i + WIDTH + 1 < length)//diag r below
			result.Add(i + WIDTH + 1);
		if (i + WIDTH - 1 < length)//diag l below
			result.Add(i - WIDTH + 1);
		return result;
	}

	bool isGreen(float h, float s, float v){
		//the range of valid green pixels:
		return 160 < h * 360 && h * 360 < 170 && s * 100 > 50 && v * 100 > 33;
	}

	Pixel xyPixel(int i, int pixels_len){
		//makes a pixel given its 
		//index in pixels array
		int y_val = i / HEIGHT;
		int x_val = pixels_len - y_val;
		return new Pixel (x_val, y_val);
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
