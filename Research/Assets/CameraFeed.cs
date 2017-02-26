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

public class Blob{
	public HashSet<int> pixels;
	Vector2 center;
	int min_x;
	int min_y;
	int max_x;
	int max_y;

	public Blob(int x_, int y_, int i){
		min_x = x_;
		max_x = x_;
		min_y = y_;
		max_y = y_;
		center = new Vector2 ((float)x_, (float)y_);
		pixels = new HashSet<int> ();
		pixels.Add (i);
	}

	public void add(int x_, int y_, int i){
		min_x = Mathf.Min(min_x, x_);
		max_x = Mathf.Max(max_x, x_);
		min_y = Mathf.Min(min_y, y_);
		max_y = Mathf.Max(max_y, y_);
		center = findCenter ();
		pixels.Add (i);
	}

	Vector2 findCenter(){
		return new Vector2 ((min_x + max_x) / 2, (min_y + max_y) / 2);
	}

	public Vector2 getCenter(){
		return center;
	}

	public string toString(){
		return "(" + center.x + "," + center.y + ") " + " pixels size: " + pixels.Count;
	}
}

public class CameraFeed : MonoBehaviour {
	//constants
	int WIDTH = 1280;
	int HEIGHT = 720;
	int THRESHOLD = 400;

	//public variables
	public GameObject color_indicator;
	public GameObject single_pixel;

	//BFS each paddle is a set of pixels
	List<HashSet<int>> green_paddles;
	List<HashSet<int>> red_paddles;
	HashSet<int> already_seen_green;
	HashSet<int> already_seen_red;
	int pixels_BFSed = 0;

	//Blob
	List<Blob> green_blobs;

	WebCamTexture webcam;

	//TEST 
	bool freeze = false;

	// Use this for initialization
	void Start () {
		//set up the paddle lists
		green_paddles = new List<HashSet<int>> ();
		red_paddles = new List<HashSet<int>> ();
		already_seen_green = new HashSet<int> ();
		already_seen_red = new HashSet<int> ();

		green_blobs = new List<Blob> ();

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
		getPaddleBlobs ();


		//TEST
		if (Input.GetKeyDown (KeyCode.P)) {
			print (green_paddles.Count);
			drawPaddlesToScreen ();
		}
		if (Input.GetKeyDown (KeyCode.B)) {
			print (green_blobs.Count);
			if (green_blobs.Count < 50) {
				for (int b = 0; b < green_blobs.Count; b++) {
					string p = green_blobs [b].toString();
					print (p);
				}
			}
			drawBlobsToScreen ();
			//freeze = true;
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
		pixels_BFSed = 0;
		//clear out the paddles already in there
		green_paddles.Clear();
		red_paddles.Clear ();
		already_seen_green.Clear ();
		already_seen_red.Clear ();
		Color32[] pixels = webcam.GetPixels32();
		for (int i = 0; i < pixels.Length; i++) {
			if (already_seen_green.Contains (i))
				continue;
			if (isGreen(pixels[i].r, pixels[i].g, pixels[i].b)) {
				if (already_seen_green.Contains (i)) continue;
				HashSet<int> paddle = BFS (i, pixels, "green");
				//we may actually want a paddle object: {pxl_set, center}
				if (paddle.Count > 500) { 
					green_paddles.Add (paddle);
					//already_seen_green.UnionWith (paddle);
				}
			}
		}
		if (Input.GetKeyDown(KeyCode.O))
			print(pixels_BFSed);
	}

	void getPaddleBlobs(){
		//first we need to clear out green blobs at the start of the frame
		green_blobs.Clear ();
		//now we get the pixel array
		Color32[] pixels = webcam.GetPixels32();
		for (int i = 0; i < pixels.Length; i++) {
			if (isGreen (pixels [i].r, pixels [i].g, pixels [i].b)) {
				//is there a blob nearby that this belongs to?
				Vector2 p = get_Pixel_xy(i, WIDTH*HEIGHT);
				bool found = false;
				for (int j = 0; j < green_blobs.Count; j++) {
					if (Vector2.Distance(p, green_blobs[j].getCenter())<THRESHOLD){
						found = true;
						green_blobs[j].add((int)p.x, (int)p.y, i);
						break;
					}
				}
				if (!found)
					green_blobs.Add (new Blob ((int)p.x, (int)p.y, i));
			}
		}
	}

	HashSet<int> BFS (int i, Color32[] pixels, string color){
		//add this to a paddle hashset
		HashSet<int> paddle = new HashSet<int>();
		HashSet<int> visited = new HashSet<int>();	//all the pixels we've seen, including non-red/non-green
		Queue<int> q = new Queue<int>();
		visited.Add (i);
		q.Enqueue (i);

		while (q.Count != 0) {
			int x = q.Dequeue ();
			paddle.Add (x);
			pixels_BFSed++;
			//there are potentially eight neighbors for this pixel
			List<int> neighbors = getPixelNeighbors(x, pixels.Length);
			//check each neighbor - if it's the right color, it goes in q
			for (int y = 0; y < neighbors.Count; y++) {
				if (color == "green") already_seen_green.Add (y);
				else if (color == "red") already_seen_red.Add (y);
				if (!visited.Contains (neighbors[y])) 
					visited.Add (neighbors[y]);
				else 
					continue;
				if (color == "green" && isGreen (pixels[neighbors[y]].r, pixels[neighbors[y]].g, pixels[neighbors[y]].b) /*|| (color=="red"&& isRed())*/) {
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

	bool isGreenOLD(float h, float s, float v){
		//the range of valid green pixels:
		return 150 < h * 360 && h * 360 < 180 && s * 100 > 50 /*&& v * 100 > 33*/;
	}

	bool isGreen(float r, float g, float b){
		return g > (r + b);
	}

	Pixel get_Pixel_xyOLD(int i, int pixels_len){
		//makes a pixel given its 
		//index in pixels array
		int y_val = i / WIDTH;
		int x_val = i - WIDTH*y_val;
		return new Pixel (x_val, y_val);
	}

	Vector2 get_Pixel_xy(int i, int pixels_len){
		int y_val = i / WIDTH;
		int x_val = i - WIDTH*y_val;
		return new Vector2 (x_val, y_val);
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
				Vector2 p = get_Pixel_xy (i, WIDTH * HEIGHT);
				GameObject pixel = (GameObject) Instantiate (single_pixel, new Vector3 (p.x/10f, p.y/10f, 0), Quaternion.identity);
				pixel.GetComponent<SpriteRenderer> ().color = c;
				pixel.transform.parent = container.transform;
			}
		}
	}

	void drawBlobsToScreen(){
		GameObject container;
		container = GameObject.Find ("Blobs");
		if (container) {
			Destroy (container);
		}
		container = new GameObject ();
		container.name = "Blobs";
		for (int x = 0; x < green_blobs.Count; x++) {
			Color c = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
			foreach (int i  in green_blobs[x].pixels) {
				Vector2 p = get_Pixel_xy (i, WIDTH * HEIGHT);
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
