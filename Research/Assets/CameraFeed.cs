using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pixel{
	Color color;
	public int x;
	public int y;

	public Pixel(int x_, int y_){
		x = x_;
		y = y_;
	}

	public string toString(){
		return x + ", " + y;
	}
}

public class CameraFeed : MonoBehaviour {
	//constants
	int WIDTH = 1280;
	int HEIGHT = 720;

	//public variables
	public GameObject color_indicator;
	public GameObject single_pixel;

	//each paddle is a set of pixels
	List<HashSet<int>> green_paddles;
	List<HashSet<int>> red_paddles;

	WebCamTexture webcam;

	// Use this for initialization
	void Start () {
		//set up the paddle lists
		green_paddles = new List<HashSet<int>> ();
		red_paddles = new List<HashSet<int>> ();

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
		webcam.requestedFPS = 60;
		webcam.Play ();

	}
	
	// Update is called once per frame
	void Update () {
		getPaddles ();


		//TEST
		if (Input.GetKeyDown (KeyCode.P)) {
			print (green_paddles.Count);
			drawPaddlesToScreen ();
		}
		//mouseColorTest();
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
		//clear out the paddles already in there
		green_paddles.Clear();
		red_paddles.Clear ();
		//Using Google Color Selector:
		//google.com/#q=color+selector
		//Green	hue is between 102 and 140
		//		sat > 55
		//		val > 33
		//Red 	hue is < 15 or > 355
		//		sat > 55
		//		val > 33
		Color[] pixels = webcam.GetPixels();
		HashSet<int> already_in_paddles = new HashSet<int>();
		float h = 0;
		float s = 0;
		float v = 0;
		for (int i = 0; i < pixels.Length; i++) {
			if (already_in_paddles.Contains (i)) continue;
			Color.RGBToHSV (pixels [i], out h, out s, out v);
			if (isGreen(h,s,v)) {
				HashSet<int> paddle = BFS (i, pixels, already_in_paddles, "green");
				//we may actually want a paddle object: {pxl_set, center}
				green_paddles.Add (paddle);
				already_in_paddles.UnionWith (paddle);
			}
		}
	}

	HashSet<int> BFS (int i, Color[] pixels, HashSet<int> already_in_paddles, string color){
		//add this to a paddle hashset
		HashSet<int> paddle = new HashSet<int>();
		HashSet<int> visited = new HashSet<int>();	//all the pixels we've seen, including non-red/non-green
		Queue<int> q = new Queue<int>();
		visited.Add (i);
		q.Enqueue (i);

		while (q.Count != 0) {
			int x = q.Dequeue ();
			paddle.Add (x);
			//there are potentially eight neighbors for this pixel
			List<int> neighbors = getPixelNeighbors(x, pixels.Length);
			//check each neighbor - if it's the right color, it goes in q
			for (int y = 0; y < neighbors.Count; y++) {
				if (!visited.Contains (neighbors[y])) 
					visited.Add (neighbors[y]);
				else 
					continue;
				Vector3 y_color = hsvAt (neighbors[y], pixels);
				if (color == "green" && isGreen (y_color.x, y_color.y, y_color.z) /*|| (color=="red"&& isRed())*/) {
					q.Enqueue (neighbors[y]);
				}
			}
		}
			
		return paddle;

	}

	Vector3 hsvAt(int i, Color[] pixels){
		//get the hsv of an index in pixels
		float h,s,v;
		Color.RGBToHSV (pixels [i], out h, out s, out v);
		return new Vector3 (h, s, v);
	}

	List<int> getPixelNeighbors(int i, int length){
		List<int> result = new List<int> ();
		if (i + 1 < length) //right
			result.Add (i + 1);
		if (i - 1 >= 0) //left
			result.Add (i - 1);
		if (i - WIDTH >= 0)//above
			result.Add(i - WIDTH);
		if (i + WIDTH < length)//below
			result.Add(i + WIDTH);
		if (i - WIDTH + 1 >= 0)//diag r above
			result.Add(i - WIDTH + 1);
		if (i - WIDTH - 1 >= 0)//diag l above
			result.Add(i - WIDTH - 1);
		if (i + WIDTH + 1 < length)//diag r below
			result.Add(i + WIDTH + 1);
		if (i + WIDTH - 1 < length)//diag l below
			result.Add(i + WIDTH - 1);
		return result;
	}

	bool isGreen(float h, float s, float v){
		//the range of valid green pixels:
		return 160 < h * 360 && h * 360 < 170 && s * 100 > 50 /*&& v * 100 > 33*/;
	}

	Pixel get_Pixel_xy(int i, int pixels_len){
		//makes a pixel given its 
		//index in pixels array
		int y_val = i / WIDTH;
		int x_val = i - WIDTH*y_val;
		return new Pixel (x_val, y_val);
	}

	void drawPaddlesToScreen(){
		GameObject container;
		container = GameObject.Find ("Pixels");
		if (container) {
			Destroy (container);
		}
		container = new GameObject ();
		container.name = "Pixels";
		for (int x = 0; x < green_paddles.Count; x++) {
			Color c = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
			foreach (int i  in green_paddles[x]) {
				Pixel p = get_Pixel_xy (i, WIDTH * HEIGHT);
				GameObject pixel = (GameObject) Instantiate (single_pixel, new Vector3 (p.x/10f, p.y/10f, 0), Quaternion.identity);
				pixel.GetComponent<SpriteRenderer> ().color = c;
				pixel.transform.parent = container.transform;
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
