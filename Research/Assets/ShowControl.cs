using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShowControl : MonoBehaviour {
	//CONSTANTS
	int MAX_PADDLES = 48;
	int WIDTH = 128;
	int HEIGHT = 72;
	float SCREEN_SCALEDOWN = 1f;

	CameraFeed cf;
	List<Blob> paddles;
	AudioSource music;
	//phase dictates what group of behaviors is active.
	public string phase = "bubbles"; //for versatility, I use strings rather than #s

	//MULTI-PHASE VARIABLES
	public GameObject paddle_center_prefab;
	GameObject background;

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

	//"bubbles"
	public GameObject bubble_prefab;

	//"heart"
	public GameObject heart_prefab;
	bool shape_color_bursts_active = false;
	bool[] heart_ids_used;
	public GameObject heart_particles_prefab;
	public GameObject shape_circle_prefab;

	//"expanding star"
	public GameObject expanding_star_prefab;
	bool[] star_ids_used;
	public GameObject star_particles_prefab;

	//"RPI"
	public GameObject rpi_logo_prefab;
	public GameObject paint_splat;
	HashSet<Blob> seen_in_last_frame;

	//"fireworks"
	public GameObject firework_ball_prefab; //travels and then bursts
	public GameObject basic_burst_prefab;
	public GameObject firework_sparkle_prefab;
	bool store = true;
	Dictionary<int, Vector2> stored_bursts;
	Vector3 firework_launch_loc;
	Color[] firework_colors;
	int num_stored_to_launch = 0;

	void Start () {
		print (GetComponent<MeshRenderer> ().bounds.size.x);
		cf = GetComponent<CameraFeed> ();
		HEIGHT = cf.HEIGHT;
		WIDTH = cf.WIDTH;
		SCREEN_SCALEDOWN = cf.SCREEN_SCALEDOWN;

		paddles = cf.green_blobs;
		music = GameObject.Find ("Music").GetComponent<AudioSource>();

		star_array = new GameObject[MAX_PADDLES+2];
		star_assignments = new Vector3[MAX_PADDLES+2];

		heart_ids_used = new bool[MAX_PADDLES + 2];

		star_ids_used = new bool[MAX_PADDLES + 2];

		seen_in_last_frame = new HashSet<Blob> ();

		stored_bursts = new Dictionary<int, Vector2> ();
		firework_launch_loc = new Vector3 (transform.position.x, transform.position.y - GetComponent<MeshRenderer>().bounds.size.y/2);
		firework_colors = new Color[MAX_PADDLES + 2];
		for (int ci = 0; ci < firework_colors.Length; ci++)
			firework_colors [ci] = Color.HSVToRGB (Random.Range (0f, 1f), Random.Range (.75f, 1f), 1f);

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
			#region
			if (Random.value > .9) {
				GameObject b = (GameObject)Instantiate (bubble_prefab);
				Bounds quad_bounds = GetComponent<MeshRenderer> ().bounds;
				b.GetComponent<bubbleBehavior> ().setStartAndGoal (transform.position,
					quad_bounds.size.x, quad_bounds.size.y, (Random.value > .5f));
			}
			break;
			#endregion

		case "heart":
			#region
			foreach (Blob p in paddles) {
				if (!heart_ids_used [p.id] && shape_color_bursts_active) {
					Instantiate (heart_particles_prefab, p.getCenter () / SCREEN_SCALEDOWN, Quaternion.identity);
					heart_ids_used [p.id] = true;
					GameObject heart = (GameObject)Instantiate (heart_prefab, p.getCenter () / SCREEN_SCALEDOWN, Quaternion.identity);
					Bounds quad_bounds = GetComponent<MeshRenderer> ().bounds;
					heart.GetComponent<expandingShape> ().StartMoveAndTurn (transform.position, false);
					//circle.GetComponent<expandShapeCircle> ().setGoalRadius (quad_bounds.size.x, quad_bounds.size.y);
				}
			}
			//TEST
			if (Input.GetMouseButtonDown(0) && shape_color_bursts_active) {
				Instantiate (heart_particles_prefab, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
				GameObject heart = (GameObject)Instantiate (heart_prefab, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
				heart.GetComponent<expandingShape> ().StartMoveAndTurn (transform.position, false);
			}
			break;
			#endregion

		case "expanding star":
			#region
			foreach (Blob p in paddles) {
				if (!star_ids_used [p.id]) {
					Instantiate (star_particles_prefab, p.getCenter () / SCREEN_SCALEDOWN, Quaternion.identity);
					star_ids_used [p.id] = true;
					GameObject star = (GameObject)Instantiate (expanding_star_prefab, p.getCenter () / SCREEN_SCALEDOWN, Quaternion.identity);
					Bounds quad_bounds = GetComponent<MeshRenderer> ().bounds;
					star.GetComponent<expandingShape> ().StartMoveAndTurn (transform.position, true);
					//circle.GetComponent<expandShapeCircle> ().setGoalRadius (quad_bounds.size.x, quad_bounds.size.y);
				}
			}
			//TEST
			if (Input.GetMouseButtonDown(0)) {
				Instantiate (star_particles_prefab, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
				GameObject star = (GameObject)Instantiate (expanding_star_prefab, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
				star.GetComponent<expandingShape> ().StartMoveAndTurn (transform.position, true);
			}
			break;
			#endregion

		case "RPI":
			HashSet<Blob> next_hash = new HashSet<Blob> ();
			foreach (Blob b in paddles) {
				if (!seen_in_last_frame.Contains (b)) {
					//this paddle appeared this frame - it
					//gets to sling some paint! 
					Instantiate (paint_splat, b.getCenter (), Quaternion.identity);
					next_hash.Add (b);
				}
				seen_in_last_frame = next_hash;
			}

			//TEST 
			if (Input.GetMouseButtonDown (0)) {
				Instantiate (paint_splat, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
			}
			break;

		case "fireworks":
			foreach (Blob p in paddles) {
				if (store) {
					stored_bursts.Add (p.id, p.getCenter());
				} else {
					GameObject f = (GameObject)Instantiate (firework_ball_prefab);
					f.GetComponent<fireworkBallBehavior> ().Launch (firework_launch_loc, p.getCenter () / SCREEN_SCALEDOWN, firework_colors [p.id], basic_burst_prefab);
				}
			}

			//TEST 
			if (Input.GetMouseButtonDown (0)) {
				if (store) {
					stored_bursts.Add (Random.Range(0, MAX_PADDLES), Input.mousePosition / SCREEN_SCALEDOWN);
				} else {
					GameObject f = (GameObject)Instantiate (firework_ball_prefab);
					f.GetComponent<fireworkBallBehavior> ().Launch (firework_launch_loc, Input.mousePosition / SCREEN_SCALEDOWN, firework_colors [Random.Range(0, MAX_PADDLES)], basic_burst_prefab);
				}
			}
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
			StartCoroutine(EndTimer (19.309f - music.time, "intro"));
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
			background = Instantiate (star_back_prefab, center_screen, Quaternion.identity);
			background.GetComponent<expandBackground>().grow = true;
			background.GetComponent<expandBackground>().startSwitchColor(Color.black, .3f, 0f);

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
			if (music.time < 19.5f || music.time > 71.97f) {
				//bubbles should start at 71.971 seconds
				music.time = 56.89f;
			}

			//set when the next phase starts
			StartCoroutine(EndTimer(71.971f - music.time, "stars twinkling"));
			break;
			#endregion
		
		case "bubbles":
			#region
			music.time = 71.971f;
			//set up to end this phase
			StartCoroutine(EndTimer(129.756f - music.time, "bubbles")); 
			//heart starts at 2 min 9 sec 756 millis
			break;
			#endregion

		case "heart":
			#region
			music.time = 129.756f;
			if (!background)
				background = Instantiate (star_back_prefab,transform.position, Quaternion.identity);
			background.GetComponent<expandBackground>().startSwitchColor(Color.white, .25f, 2f);
			background.GetComponent<expandBackground>().grow = false;
			//we allow the heart to start being revealed when the violin comes in
			//in the soundtrack, at 2 min 23 sec 713 millis
			StartCoroutine (waitAndActivateHeart (143.713f - music.time));
			//set up when this ends
			StartCoroutine (EndTimer(172.546f - music.time, "heart"));
			break;
			#endregion

		case "expanding star":
			#region
			music.time = 172.546f;
			if (!background)
				background = Instantiate (star_back_prefab, transform.position, Quaternion.identity);
			background.GetComponent<expandBackground> ().
			  startSwitchColor(new Color(0f,0f,75f), .25f, 1.5f);
			//set up when this ends
			StartCoroutine(EndTimer(204.651f - music.time, "expanding star"));
			break;
			#endregion

		case "RPI":
			#region
			music.time = 204.651f;
			if (!background)
				background = Instantiate (star_back_prefab, transform.position, Quaternion.identity);
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, 1f, 1.5f);
			Instantiate (rpi_logo_prefab, transform.position, Quaternion.identity);
			StartCoroutine (BackgroundToRed (259.913f - music.time, background));
			//set up when this ends
			StartCoroutine(EndTimer(273.269f - music.time, "RPI"));
			break;
			#endregion

		case "second intro":
			music.time = 273.269f;
			if (!background)
				background = Instantiate (star_back_prefab, transform.position, Quaternion.identity);
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .25f, 16f);
			break;

		case "fireworks":
			music.time = 400.154f;

			if (!background)
				background = Instantiate (star_back_prefab, transform.position, Quaternion.identity);
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .5f, 0f);
			seen_in_last_frame.Clear ();
			foreach (Blob p in paddles) {
				seen_in_last_frame.Add (p);
			}

			//initial burst
			StartCoroutine (fireworksDoStore (406.2f - music.time, false));
			StartCoroutine (launchStore (406.2f - music.time, basic_burst_prefab));
			//timed bursts later on in the phase
			StartCoroutine (fireworksDoStore (431.507f - music.time, true));
			StartCoroutine (setStorePercentage (435.440f - music.time, .25f));
			StartCoroutine (launchStorePercentage (435.440f - music.time, basic_burst_prefab));
			StartCoroutine (launchStorePercentage (436.276f - music.time, basic_burst_prefab));
			StartCoroutine (launchStorePercentage (439.010f - music.time, basic_burst_prefab));
			StartCoroutine (launchStorePercentage (439.993f - music.time, basic_burst_prefab));
			StartCoroutine (clearStoredBursts (441.000f - music.time));
			StartCoroutine (fireworkSparkle (443.567f - music.time, 3.35f));
			StartCoroutine (EndTimer (447.702f - music.time, "fireworks"));
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
			StartPhase ("heart");
			break;

		case "heart":
			expandingShape[] hearts = GameObject.FindObjectsOfType<expandingShape> ();
			foreach (expandingShape e in hearts)
				Destroy (e.gameObject);
			StartPhase ("expanding star");
			break;

		case "expanding star":
			expandingShape[] stars = GameObject.FindObjectsOfType<expandingShape> ();
			foreach (expandingShape e in stars)
				Destroy (e.gameObject);
			StartPhase ("RPI");
			break;

		case "RPI":
			paintSplatBehavior[] splats = GameObject.FindObjectsOfType<paintSplatBehavior> ();
			foreach (paintSplatBehavior p in splats)
				Destroy (p.gameObject);
			Destroy (GameObject.FindObjectOfType<rpiLogoBehavior> ().gameObject);
			StartPhase ("second intro");
			break;

		case "fireworks": 
			stored_bursts.Clear ();
			StartPhase ("fireworks shapes");
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

	IEnumerator waitAndActivateHeart(float wait_seconds){
		yield return new WaitForSeconds (wait_seconds);
		//Instantiate (heart_prefab, transform.position, Quaternion.identity);
		print("hearts active");
		shape_color_bursts_active = true;
	}

	IEnumerator BackgroundToRed(float wait_seconds, GameObject background){
		yield return new WaitForSeconds (wait_seconds);
		background.GetComponent<expandBackground> ().startSwitchColor (Color.red, 1f, 3f);
	}

	IEnumerator fireworksDoStore(float wait_seconds, bool store_val){
		yield return new WaitForSeconds (wait_seconds);
		store = store_val;
	}

	IEnumerator launchStore(float wait_seconds, GameObject chosen_burst){
		yield return new WaitForSeconds (wait_seconds);
		foreach (int key in stored_bursts.Keys) {
			GameObject new_firework = (GameObject) Instantiate (chosen_burst, stored_bursts[key], Quaternion.identity);
			ParticleSystem.MainModule settings = new_firework.GetComponent<ParticleSystem>().main;
			settings.startColor = new ParticleSystem.MinMaxGradient (firework_colors[key]);
		}
		stored_bursts.Clear ();
	}

	/// <summary>
	/// Sets what percentage of fireworks you want to launch when 
	/// launchStorePercentage is called.
	/// </summary>
	IEnumerator setStorePercentage(float wait_seconds, float percentage){
		yield return new WaitForSeconds (wait_seconds);
		num_stored_to_launch = (int) (stored_bursts.Keys.Count * percentage);
		print ("from percentage " + percentage + " of " + stored_bursts.Keys.Count +
		" chose launch number " + num_stored_to_launch);
	}

	/// <summary>
	/// Launches and removes num_stored_to_launch fireworks, the number 
	/// of fireworks specified when setStorePercentage was called.
	/// </summary>
	/// <returns>The store percentage.</returns>
	IEnumerator launchStorePercentage(float wait_seconds, GameObject chosen_burst){
		yield return new WaitForSeconds (wait_seconds);
		bool use_all = false;
		int how_many = num_stored_to_launch;
		List<int> to_remove = new List<int> ();
		if (how_many < 1) {
			//if there are too few to split into this percentage, we use
			//all the fireworks each burst and don't clear them out
			use_all = true;
		}
		foreach (int key in stored_bursts.Keys) {
			GameObject new_firework = (GameObject) Instantiate (chosen_burst, stored_bursts[key], Quaternion.identity);
			ParticleSystem.MainModule settings = new_firework.GetComponent<ParticleSystem>().main;
			settings.startColor = new ParticleSystem.MinMaxGradient (firework_colors[key]);
			if (!use_all) {
				how_many--;
				to_remove.Add (key);
			}
			if (how_many <= 0) break;
		}
		foreach (int k in to_remove) stored_bursts.Remove (k);
		//stored_bursts.Clear ();
	}

	IEnumerator clearStoredBursts(float wait_seconds){
		yield return new WaitForSeconds (wait_seconds);
		stored_bursts.Clear ();
	}

	IEnumerator fireworkSparkle(float wait_seconds, float duration){
		yield return new WaitForSeconds (wait_seconds);
		GameObject sparkle = (GameObject) Instantiate (firework_sparkle_prefab, transform.position, Quaternion.identity);
		yield return new WaitForSeconds (duration);
		Destroy (sparkle);
	}

	/*Schedule:
	 * Narration from 0s to 18.699s
	 * Black Background explodes out and Stars appear at 1945s
	 * */
}
