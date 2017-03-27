using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowControl : MonoBehaviour {
	//CONSTANTS
	int MAX_PADDLES = 48;
	int WIDTH = 1280;
	int HEIGHT = 720;
	float SCREEN_SCALEDOWN = 1f;

	CameraFeed cf;
	List<Blob> paddles;
	AudioSource music;
	//phase dictates what group of behaviors is active.
	public string phase = "stars"; //for versatility, I use strings rather than #s

	//MULTI-PHASE VARIABLES


	//SPECIFIC PHASE VARIABLES

	//"stars"
	public GameObject single_star_prefab;
	public GameObject star_back_prefab;
	bool positions_recorded = true;
	bool destroy_stars_container = false;

	//"stars twinkling"
	public GameObject twinkling_star_prefab;
	public Sprite[] star_sprites; //8 star sprites
	GameObject[] star_array;
	Vector3[] star_assignments; //star num, frame interval, pos in interval

	void Start () {
		cf = GetComponent<CameraFeed> ();
		HEIGHT = cf.HEIGHT;
		WIDTH = cf.WIDTH;
		SCREEN_SCALEDOWN = cf.SCREEN_SCALEDOWN;

		paddles = cf.green_blobs;
		music = GameObject.Find ("Music").GetComponent<AudioSource>();

		star_array = new GameObject[MAX_PADDLES+2];
		star_assignments = new Vector3[MAX_PADDLES+2];

		StartPhase (phase);

	}

	//ShowBusiness is the "Update" function of 
	//ShowControl, called every frame by CameraFeed
	//once the paddles have been found.
	public void ShowBusiness(List<Blob> all_blobs){
		paddles = all_blobs;
		switch (phase) {

		//Every switch case looks the same
		case "intro":
			#region
			//regular functionality

			break;
			#endregion

		case "stars":
			#region
			//regular functionality
			GameObject container;
			container = GameObject.Find ("Stars");
			if (container) {
				//delete all the old stars
				Destroy (container);
			}
			if (destroy_stars_container) break;
			container = new GameObject ();
			container.name = "Stars";
			foreach (Blob b in paddles) {
				GameObject star = Instantiate (single_star_prefab, b.getCenter ()/SCREEN_SCALEDOWN, Quaternion.identity);
				star.transform.parent = container.transform;
			}

			if (Input.GetKeyDown (KeyCode.R)) {
				//pressing R activates the function assignIDs in CameraFeed
				positions_recorded = true;
			}

			break;
			#endregion
		
		case "stars twinkling":
			#region
			//regular functionality
			foreach (Blob b in paddles) {
				if (star_array [b.id] == null) {
					//set up a new twinkling star
					star_array[b.id] = (GameObject) Instantiate (twinkling_star_prefab, b.getCenter (), Quaternion.identity);
					twinklingStar t = star_array[b.id].GetComponent<twinklingStar> ();
					int star_choice = Random.Range (0, 4)*2;
					t.sprite1 = star_sprites [star_choice];
					t.sprite2 = star_sprites [star_choice + 1];
					t.twinkle_frame_interval = Random.Range (1, 3);
					t.twinkle_frame_countdown = Random.Range (0, t.twinkle_frame_interval);
				}
				star_array [b.id].GetComponent<twinklingStar> ().setLocation (b.getCenter ()/SCREEN_SCALEDOWN);
			}
			break;
			#endregion
		
		case "bubbles":
			break;
		}
	}

	//StartPhase does one-time set up work for each
	//different phase in the show. 
	void StartPhase(string new_phase){
		print (new_phase);
		phase = new_phase;

		switch (new_phase) {

		case "intro":
			#region
			music.time = 0;
			StartCoroutine(EndTimer (19.309f, "intro"));
			break;
			#endregion

		case "stars":
			#region
			//set variables
			positions_recorded = false;

			//change music
			music.time = 19.309f;

			//set when the next phase starts
			StartCoroutine(StarTimer (26.534f - music.time, 38.884f - music.time, 49.694f - music.time, 56.4f - music.time, 49.694f));

			//make expanding black background - it handles its own expansion
			Vector3 center_screen = new Vector3 (WIDTH / SCREEN_SCALEDOWN * .5f, HEIGHT / SCREEN_SCALEDOWN * .5f, 0);
			Instantiate (star_back_prefab, center_screen, Quaternion.identity);

			//Assign a star to every id
			for (int x = 0; x <= MAX_PADDLES; x++) {
				int interval = Random.Range (30, 50);
				Vector3 v3 = new Vector3 (Random.Range (0, 8), interval, Random.Range (0, interval));
				//assign one of the 8 stars to this id, and in preparation for twinklig
				//give it some frame interval for twinkle, and start it at a random place
				//in that interval
				star_assignments[x] = v3;
			}
			break;
			//Notes: For this I'll need to make 1 star background
			//of dimensions 1280 x 720 (or a ratio thereof)
			//and 8 star variants (100 x 100 px would be fine)
			#endregion
		
		case "stars twinkling":
			#region
			//set variables

			//don't change music unless we're way far away
			if (music.time < 19.5f || music.time > 82.579f) {
				//bubbles should start at 82.579 seconds
				music.time = 56.89f;
			}

			//set when the next phase starts
			StartCoroutine(EndTimer(79.971f - music.time, "stars twinkling"));
			break;
			#endregion
		
		case "bubbles":
			music.time = 79.971f;
			//set up to end this phase
			break;
		}
			
	}

	//cleans up phase objects and starts the next
	void EndPhase(string old_phase){
		switch (old_phase) {

		case "intro":
			StartPhase ("stars");
			break;

		case "stars":
			if (GameObject.Find ("Stars")) {
				destroy_stars_container = true;
				Destroy (GameObject.Find ("Stars"));
			}
			StartPhase ("stars twinkling");
			break;

		case "stars twinkling":
			star_array = new GameObject[0];
			StartPhase ("bubbles");
			break;
		
		case "bubbles":
			bubbleBehavior[] bubbles_left = GameObject.FindObjectsOfType<bubbleBehavior> ();
			foreach (bubbleBehavior b in bubbles_left)
				b.Pop ();
			break;
		}
			
	}

	IEnumerator StarTimer(float first_stop, float second_stop, float third_stop, float last_stop, float music_restart_time){
		//there are a couple of places where we might start the twinkling phase
		yield return new WaitForSeconds(first_stop);
		print ("stop 1");
		if (positions_recorded)
			EndPhase ("stars");
		yield return new WaitForSeconds (second_stop - first_stop);
		print ("stop 2");
		if (positions_recorded)
			EndPhase ("stars");
		yield return new WaitForSeconds (third_stop - (second_stop + first_stop));
		print ("stop 3");
		if (positions_recorded)
			EndPhase ("stars");
		yield return new WaitForSeconds (last_stop - (third_stop + second_stop + first_stop));
		print ("last stop");
		if (positions_recorded)
			EndPhase ("stars");
		else {
			//if we got here without being told we can end,
			//we need to loop back in the music and try again
			music.time = music_restart_time;
			StartCoroutine(StarTimer(0f, 0f, 0f, last_stop - third_stop, music_restart_time));
		}
	}

	IEnumerator EndTimer(float seconds_to_wait, string phase_to_end){
		yield return new WaitForSeconds (seconds_to_wait);
		EndPhase (phase_to_end);
	}

	//To do:
	//	Fix locations with the SCREEN_SCALEDOWN

	/*Schedule:
	 * Narration from 0s to 18.699s
	 * Black Background explodes out and Stars appear at 1945s
	 * */
}
