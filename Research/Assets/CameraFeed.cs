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
	public int id;

	public Blob(int x_, int y_, int i){
		min_x = x_;
		max_x = x_;
		min_y = y_;
		max_y = y_;
		center = new Vector2 ((float)x_, (float)y_);
		pixels = new HashSet<int> ();
		pixels.Add (i);
		id = -1;
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

	public float getWidth(){
		return max_x - min_x;
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
	public int WIDTH = 1280;
	public int HEIGHT = 720;
	public float SCREEN_SCALEDOWN = 10f;//Unity struggles with large coordinates
	int TOP_PADDLE_Y = 570; //where on screen is the top paddle
	int BOTTOM_PADDLE_Y = 240; //        "       bottom paddle
	int TOP_PADDLE_H = 20;    //we use height to predict diminishing..
	int BOTTOM_PADDLE_H = 100; //paddle size with row in lecture hall


	//public variables
	public GameObject color_indicator;
	public GameObject single_pixel;

	//Blob
	public List<Blob> green_blobs;
	WebCamTexture webcam;
	HeightInfo[] expected_dimensions;

	//Blob Persistance
	Dictionary<Vector2, int> locations_and_ids;

	//Show Control
	ShowControl sc;

	//TEST
	List<Vector3> dist_points;
	int num_green_found = 0;

	// Use this for initialization
	void Start () {
		print ("Reminder: TOP_PADDLE_Y and BOTTOM_\" need to be set");
		//place screen
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

		locations_and_ids = new Dictionary<Vector2, int> ();

		sc = GetComponent<ShowControl> ();

		//TEST
		dist_points = new List<Vector3>();
		//print(isGreen (94 / 255f, 12 / 255f, 88 / 255f));

	}
	
	// Update is called once per frame
	void Update () {
		getPaddleBlobs ();

		//TEST
		//drawCentersToScreen ();
		if (Input.GetKeyDown (KeyCode.B) || Input.GetMouseButton(2)) {
			//drawBlobsToScreen ();
			print ("Number of green blobs: "+green_blobs.Count);
			string all_ids = "";
			foreach (Blob b in green_blobs)
				all_ids += b.id + ", ";
			print ("IDs present: " + all_ids);
			//print ("Number of green pixels found: " + num_green_found);
		}
		//mouseColorTest();
		//drawDistance();

		if ((Input.GetKeyDown (KeyCode.R) || Input.GetMouseButtonDown(1)) && !sc.phase_input.isFocused) {
			assignIDs ();
			sc.RecordCenters(locations_and_ids);
			print ("IDs assigned for " + locations_and_ids.Count+" paddle centers");
		}

		sc.ShowBusiness (green_blobs);

		if (Input.GetMouseButtonDown (0)) {
			//print (Input.mousePosition.y);
		}
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
		//print ("Mouse: "+mp.x+","+mp.y);
		color_indicator.SetActive (true);
		color_indicator.GetComponent<SpriteRenderer> ().color = hue;

		if (Input.GetMouseButtonDown (0)) {
			float h, s, v;
			Color.RGBToHSV (hue, out h, out s, out v);
			print (hue+" "+(h*360).ToString("N")+","+(s*100).ToString("N")+","+(v*100).ToString("N")+" expected width at y="+mp.y+": "+expected_dimensions[(int)mp.y].threshold*2);
		}
	}
	
	void getPaddleBlobs(){
		//first we need to clear out green blobs at the start of the frame
		green_blobs.Clear ();
		num_green_found = 0;
		//now we get the pixel array
		Color32[] pixels = webcam.GetPixels32();
		for (int i = 0; i < pixels.Length; i+=2) {
			if (isGreen (pixels [i].r, pixels [i].g, pixels [i].b)) {
				num_green_found++;
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
			if (x.pixels.Count > expected_dimensions [(int)x.getCenter ().y].min_pixels) {
				//check to see if this is an existing paddle
				Vector2 x_c = x.getCenter();
				foreach (Vector2 v2 in locations_and_ids.Keys) {
					if (Vector2.Distance (x_c, v2) < expected_dimensions [(int)x_c.y].threshold) {
						//this must be the same paddle
						x.id = locations_and_ids[v2];
						break;
					}
				}
				if (x.id == -1 && sc.phase!="stars"/*or any other phase where having no id is ok*/) {
					//we use this check to eliminate paddles that
					//weren't in the original id assignment
					continue;
					//x.id = locations_and_ids.Count;
				}
				
				keepers.Add (x);
			}
		}
		green_blobs = keepers;
	}

	bool isGreen(float r, float g, float b){
		//using the builtin is faster than doing this by hand, somehow
		float h, s, v;
		Color.RGBToHSV (new Color (r, g, b), out h, out s, out v);
		h *= 360;
		//print("RGB Given: "+r+","+g+","+b+" HSV Output:"+h+","+s+","+v);
		return 90f < h && h < 150f && s > 20/255f && v > 30/255f;
	}

	bool isGreenRGB(float r, float g, float b){
		return g >  (r + b)*.5f && r < (g * 1.1f) && b < g;
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
			if (Input.GetMouseButtonDown (1))
				print ("Making color for id " + green_blobs [x].id + " with hue " + green_blobs [x].id % 48 * 7.5f);
			Color c = Color.HSVToRGB((green_blobs[x].id%48 * 50f)/360f, 1f, 1f);
			Vector2 p = green_blobs [x].getCenter ();
			GameObject pixel = (GameObject) Instantiate (single_pixel, new Vector3 (p.x/SCREEN_SCALEDOWN, p.y/SCREEN_SCALEDOWN, 0), Quaternion.identity);
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
				dist_points.Add (new Vector3 (m_pos.x*SCREEN_SCALEDOWN, m_pos.y*SCREEN_SCALEDOWN, 0));
			} else {
				//second click
				Vector2 m_pos = mouseInWorld ();
				dist_points.Add (new Vector3 (m_pos.x*SCREEN_SCALEDOWN, m_pos.y*SCREEN_SCALEDOWN, 0));
				GetComponent<LineRenderer> ().SetPositions (dist_points.ToArray ());
				print ("at y (first click) = "+dist_points[0].y+", distance: "+Vector3.Distance (dist_points [0], dist_points [1]));
			}
		}
	}

	HeightInfo[] ExpectedDimensions(){
		//the expected paddle width (in pixels) reduces as a
		//paddle gets higher in the lecture hall. Here we make
		//an array of expected THRESHOLDs and min_pixels

		//first, we'll make up a line equation from one known 
		//y location and width to the other

		//width = mx + b
		float m = ((float)TOP_PADDLE_H - BOTTOM_PADDLE_H) / ((float)TOP_PADDLE_Y - BOTTOM_PADDLE_Y);
		float b = TOP_PADDLE_H - m * TOP_PADDLE_Y;

		//Now we use the line equation to calculate min_pixels and thresholds
		HeightInfo[] result  = new HeightInfo[721];
		for (int i = 0; i <= 720; i++) { //each i is an input y location
			result [i] = new HeightInfo ();
			float width_here = m * i + b;
			result [i].threshold = width_here; //a little bigger for leeway
			result [i].min_pixels = width_here;// * width_here * .8f; 
			//we use 80% of the pixels we're expecting in a paddle
			//as our threshold so we know that we're giving some leeway
			//if the paddle is being tilted around

			//print("expected width at y="+i+": "+width_here);
		}
		return result;
	}

	void assignIDs(){
		locations_and_ids.Clear ();
		int id = 0;
		foreach (Blob b in green_blobs) {
			locations_and_ids.Add (b.getCenter (), id);
			print (id + ": " + b.getCenter());
			id++;
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

	/*
	3/7 Five Hour Rally Notes:
	Red would not work - too much red in the lights. 

	For persistant paddle tracking. Start off with 
	everyone's paddles revealed. Assign an id to every 
	center via Dictionary. False paddles will be
	eliminated by checking if they are one of the IDed 
	paddles. 
	
	Most importantly, we only want the audience to
	show their paddles all at once a single time.
	We should save the ids and centers outside 
	the run so we have them should anything go 
	wrong. We only rewrite that stash of ids and 
	centers if we are at the "calibration" scene again.

	*/

	/*
		3/8 Use lights setting 4 in DCC 318.
		We need a hoisting mechanism to lift camera
		55.25 inches off the top of the table at the 
		front of the room. 7 feet in height total.
	*/

		/*
		4/13 Darken shapes background. I can't see my shape fly
		into place over the camera feed.
		--done RPI logo should stick around for a while..--
		Make heart effect repeatable? Ask the playtesters about this.
		DONE?? Kill those lingering stars!!
		Take glow's perlin noise and mimic it in the illumination
		*/

}
