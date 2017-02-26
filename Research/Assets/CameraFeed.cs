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

public class CameraFeed : MonoBehaviour {
	//constants
	int WIDTH = 1280;
	int HEIGHT = 720;
	int THRESHOLD = 400;

	//public variables
	public GameObject color_indicator;
	public GameObject single_pixel;

	//Blob
	List<Blob> green_blobs;

	WebCamTexture webcam;

	// Use this for initialization
	void Start () {
		//set up the paddle lists
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
		drawCentersToScreen ();
		if (Input.GetKeyDown (KeyCode.B)) {
			drawBlobsToScreen ();
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
		//remove any blob that isn't big enough to 
		//be a real paddle
		List<Blob> keepers = new List<Blob> ();
		foreach (Blob x in green_blobs) {
			if (x.pixels.Count > 200)
				keepers.Add (x);
		}
		green_blobs = keepers;
	}

	bool isGreen(float r, float g, float b){
		return g > (r + b);
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
				GameObject pixel = (GameObject) Instantiate (single_pixel, new Vector3 (p.x/10f, p.y/10f, 0), Quaternion.identity);
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
}
