using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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


public class HeightInfo{
	public float threshold;
	public float min_pixels;
}

public class CameraFeed : MonoBehaviour {
	//constants
	int WIDTH = 1280;
	int HEIGHT = 720;
	int THRESHOLD = 14; 
	float SCREEN_SCALEDOWN = 1f;//Unity struggles with such large coordinates
	int TOP_PADDLE_Y = 620; //where on screen is the top paddle
	int BOTTOM_PADDLE_Y = 170; //        "       bottom paddle
	int TOP_PADDLE_W = 14; //we use height to predict diminishing
	int BOTTOM_PADDLE_W = 78;//paddle size with row in lecture hall


	//public variables
	public GameObject color_indicator;
	public GameObject single_pixel;

	//Blob
	List<Blob> green_blobs;

	WebCamTexture webcam;

	HeightInfo[] expected_dimensions;

	//TEST
	List<Vector3> dist_points;

	// Use this for initialization
	void Start () {
		print ("Reminder: TOP_PADDLE_Y and BOTTOM_\" need to be set");
		//scale screen
		transform.localScale = new Vector2 (transform.localScale.x/SCREEN_SCALEDOWN, 
			transform.localScale.y/SCREEN_SCALEDOWN);
		transform.position = new Vector2 (transform.localScale.x / 2, transform.localScale.y / 2);

		//set up the paddle lists
		green_blobs = new List<Blob> ();

		//set up camera feed - error if webcam not present
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

		//paddle pixel counts and thresholds narrowed by distance
		expected_dimensions = ExpectedDimensions();

		//TEST
		dist_points = new List<Vector3>();

	}
	
	// Update is called once per frame
	void Update () {
		getPaddleBlobs ();

		//TEST
		drawCentersToScreen ();
		if (Input.GetKeyDown (KeyCode.B)) {
			drawBlobsToScreen ();
		}
		//mouseColorTest();
		//drawDistance();
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
					if (Vector2.Distance(p, green_blobs[j].getCenter())
						< expected_dimensions[(int)p.y].threshold){
						found = true;
						green_blobs[j].add((int)p.x, (int)p.y, i);
						break;
					}
				}
				if (!found)
					green_blobs.Add (new Blob ((int)p.x, (int)p.y, i));
			}
		}
		//remove any blob that isn't big enough to 
		//be a real paddle
		List<Blob> keepers = new List<Blob> ();
		foreach (Blob x in green_blobs) {
			if (x.pixels.Count > expected_dimensions[(int)x.getCenter().y].min_pixels)
				keepers.Add (x);
		}
		green_blobs = keepers;
	}

	bool isGreen(float r, float g, float b){
		return g >  (r + b)/*.5f*/;
	}

	Vector2 get_Pixel_xy(int i, int pixels_len){
		int y_val = i / WIDTH;
		int x_val = i - WIDTH*y_val;
		return new Vector2 (x_val, y_val);
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
				GameObject pixel = (GameObject) Instantiate (single_pixel, new Vector3 (p.x/SCREEN_SCALEDOWN , p.y/SCREEN_SCALEDOWN, 0), Quaternion.identity);
				pixel.GetComponent<SpriteRenderer> ().color = c;
				pixel.transform.parent = container.transform;
			}
		}
	}

	void drawCentersToScreen(){
		GameObject container;
		container = GameObject.Find ("Centers");
		if (container) {
			Destroy (container);
		}
		container = new GameObject ();
		container.name = "Centers";
		for (int x = 0; x < green_blobs.Count; x++) {
			Color c = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
			Vector2 p = green_blobs [x].getCenter ();
			GameObject pixel = (GameObject) Instantiate (single_pixel, new Vector3 (p.x/10f, p.y/10f, 0), Quaternion.identity);
			pixel.transform.localScale = new Vector3 (100, 100, 1);
			pixel.GetComponent<SpriteRenderer> ().color = c;
			pixel.transform.parent = container.transform;
		}
	}

	void drawDistance(){
		if (Input.GetMouseButtonDown (0)) {
			if (dist_points.Count != 1) {
				//first click
				dist_points.Clear ();
				Vector2 m_pos = mouseInWorld ();
				dist_points.Add (new Vector3 (m_pos.x, m_pos.y, 0));
			} else {
				//second click
				Vector2 m_pos = mouseInWorld ();
				dist_points.Add (new Vector3 (m_pos.x, m_pos.y, 0));
				GetComponent<LineRenderer> ().SetPositions (dist_points.ToArray ());
				print (Vector3.Distance (dist_points [0], dist_points [1]));
			}
		}
	}

	HeightInfo[] ExpectedDimensions(){
		//the expected paddle width (in pixels) reduces as a
		//paddle gets higher in the lecture hall. Here we make
		//an array of expected widths for reference when 
		//calculating THRESHOLD and min_pixels
		HeightInfo[] result  = new HeightInfo[721];
		for (int i = BOTTOM_PADDLE_Y; i <= TOP_PADDLE_Y; i++) {
			result [i] = new HeightInfo ();
			float width_here = Mathf.Lerp (BOTTOM_PADDLE_W, TOP_PADDLE_W, 
				  (i - BOTTOM_PADDLE_Y) / (TOP_PADDLE_Y - BOTTOM_PADDLE_Y));
			result [i].threshold = width_here * .55f; //a little bigger for leeway
			result [i].min_pixels = (width_here * width_here) * .8f;
			//we use 80% of the pixels we're expecting in a paddle
			//as our threshold so we know that we're giving some leeway
			//if the paddle is being tilted around
		}
		//now we'll fill the heights above and below the paddles we
		//took measurements for with the same values from those paddles
		for (int j = 0; j < BOTTOM_PADDLE_Y; j++) {
			result [j] = new HeightInfo ();
			result [j].threshold = result [BOTTOM_PADDLE_Y].threshold;
			result [j].min_pixels = result [BOTTOM_PADDLE_Y].min_pixels;
		}
		for (int k = TOP_PADDLE_Y; k <= 720; k++) {
			result [k] = new HeightInfo ();
			result [k].threshold = result [TOP_PADDLE_Y].threshold;
			result [k].min_pixels = result [TOP_PADDLE_Y].min_pixels;
		}
		return result;
	}

	/*
	 * 1/21 Notes:
	 * Brighten projection?
	 * System for recording pixel color on click
	 * Ignore regions
	 * Walking in would be good with Option 7 (Cloud)
	 * 	from the lighting panel
	 */

	/*
		2/25 Notes:
		Paddle detection is better on lower part of
		camera feed than top. 
		Need a better definition for
		green.
		Also, the paddle blob threshold must be adjusted.
		When the blob threshold is smaller, we can bring
		down the smallest possible valid blob without 
		worrying that false positives will be getting in.
	*/

	/*
		Professor Lawson Meeting 2/27
		Try green larger than average of blue and red
		Try green larger than each of the other two
		After thoughts: THRESHOLD should be paddle's 
		width and paddles should be 2 paddle widths 
		apart? Next DCC trip, we'll get the right isGreen
		definition and then measure paddle widths at 
		back and front of room.
	*/

	/*
		2/28 Distance and Color Testing in the DCC
		Front and Center of the Room Paddle Size - 78.1
		Back and Center Paddle Width - 14
	*/
}
