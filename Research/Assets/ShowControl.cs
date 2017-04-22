using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowControl : MonoBehaviour {
	//CONSTANTS
	int MAX_PADDLES = 48;
	int WIDTH = 128;
	int HEIGHT = 72;
	float SCREEN_SCALEDOWN = 1f;

	CameraFeed cf;
	List<Blob> paddles;
	AudioSource music;
	NarrationBehavior narration;
	//phase dictates what group of behaviors is active.
	public string phase = "bubbles"; //for versatility, I use strings rather than #s
	public GameObject message_ui;

	//MULTI-PHASE VARIABLES
	public GameObject paddle_center_prefab;
	GameObject background;
	Vector2[] centers;
	public InputField phase_input;

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
	HashSet<int> seen_in_last_frame;

	//"fireworks"
	public GameObject firework_ball_prefab; //travels and then bursts
	public GameObject basic_burst_prefab;
	public GameObject firework_sparkle_prefab;
	bool store = true;
	Dictionary<int, Vector2> stored_bursts;
	Vector3 firework_launch_loc;
	Color[] firework_colors;
	int num_stored_to_launch = 0;

	//"fireworks shapes"
	public GameObject double_burst_prefab;
	public GameObject box_firework_prefab;
	public GameObject circle_firework_prefab;

	//"great torch"
	public GameObject great_torch_prefab;
	public GameObject slider;
	public GameObject torch_UI;
	GameObject great_torch;

	//"torches"
	GameObject[] torches;
	public GameObject indiv_torch_prefab;
	int[] frames_since_seen;
	int absence_tolerance = 15; //how long we'll keep a torch around if its paddle disappears
	bool mouse_held_last_frame = false;
	GameObject mouse_torch;
	int frames_since_mouse_seen = 0;
	bool disrupt_normal_torches = false;
	 //illuminating torches
	bool illuminate = false;
	public GameObject dark_parent_prefab;
	GameObject darkness_parent; 

	//"network"
	GameObject[] dots;
	public GameObject dot_prefab;
	public GameObject dot_particle_prefab;
	bool placing_dots = true;
	bool connecting = false;
	List<List<Edge> > edge_lists;
	float connection_radius = 30f;
	float connection_start_time = 0f;
	float connection_time = 574.787f; //when is the network completed?
	public GameObject connecting_particles;
	public GameObject twinkling_line_prefab;
	public GameObject dot_sparkle_prefab;

	//"globes"
	public GameObject sphere_prefab;
	public Camera network_camera;
	public Camera globe_camera;
	public GameObject globe_quad_prefab;
	public GameObject blocking_quad_prefab;
	GameObject globe_quad;
	GameObject[] finale_stars;
	bool finale_stars_active = false;


	void Start () {
		print (GetComponent<MeshRenderer> ().bounds.size.x);
		cf = GetComponent<CameraFeed> ();
		HEIGHT = cf.HEIGHT;
		WIDTH = cf.WIDTH;
		SCREEN_SCALEDOWN = cf.SCREEN_SCALEDOWN;

		paddles = cf.green_blobs;
		music = GameObject.Find ("Music").GetComponent<AudioSource>();
		narration = GameObject.Find ("Narration").GetComponent<NarrationBehavior>();
		background = GameObject.Find("star background");
		centers = new Vector2[0];

		star_array = new GameObject[MAX_PADDLES+2];
		star_assignments = new Vector3[MAX_PADDLES+2];

		heart_ids_used = new bool[MAX_PADDLES + 2];

		star_ids_used = new bool[MAX_PADDLES + 2];

		seen_in_last_frame = new HashSet<int> ();

		stored_bursts = new Dictionary<int, Vector2> ();
		firework_launch_loc = new Vector3 (transform.position.x, transform.position.y - GetComponent<MeshRenderer>().bounds.size.y/2);
		firework_colors = new Color[MAX_PADDLES + 2];
		for (int ci = 0; ci < firework_colors.Length; ci++)
			firework_colors [ci] = Color.HSVToRGB (Random.Range (0f, 1f), Random.Range (.75f, 1f), 1f);

		torches = new GameObject[MAX_PADDLES + 1];
		frames_since_seen = new int[MAX_PADDLES + 1];

		dots = new GameObject[MAX_PADDLES + 1];
		edge_lists = new List<List<Edge>> ();
		for (int x = 0; x < MAX_PADDLES + 1; x++) {
			edge_lists.Add (new List<Edge> ());
		}

		finale_stars = new GameObject[MAX_PADDLES + 1];

		StartPhase (phase);

	}

	void Update(){
		if (Input.GetKeyDown (KeyCode.Alpha1)) {
			phase_input.gameObject.SetActive (!phase_input.gameObject.activeSelf);
		}
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
			if (Random.value > .85f) {
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
					star.GetComponent<expandingShape> ().StartMoveAndTurn (transform.position, true, 3f);
					//circle.GetComponent<expandShapeCircle> ().setGoalRadius (quad_bounds.size.x, quad_bounds.size.y);
				}
			}
			//TEST
			if (Input.GetMouseButtonDown(0)) {
				Instantiate (star_particles_prefab, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
				GameObject star = (GameObject)Instantiate (expanding_star_prefab, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
				star.GetComponent<expandingShape> ().StartMoveAndTurn (transform.position, true, 3f);
			}
			break;
			#endregion

		case "RPI":
			#region
			HashSet<int> next_hash = new HashSet<int> ();
			foreach (Blob b in paddles) {
				if (!seen_in_last_frame.Contains (b.id)) {
					//this paddle appeared this frame - it
					//gets to sling some paint! 
					Instantiate (paint_splat, b.getCenter ()/SCREEN_SCALEDOWN, Quaternion.identity);
				}
				next_hash.Add (b.id);
			}
			seen_in_last_frame = next_hash;

			//TEST 
			if (Input.GetMouseButtonDown (0)) {
				Instantiate (paint_splat, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
			}
			break;
			#endregion

		case "fireworks":
			#region
			HashSet<int> next_hash_f = new HashSet<int> ();
			foreach (Blob p in paddles) {
				if (!seen_in_last_frame.Contains (p.id)) {
					if (store) {
						if (!stored_bursts.ContainsKey(p.id)) stored_bursts.Add (p.id, p.getCenter ());
					} else {
						GameObject f = (GameObject)Instantiate (firework_ball_prefab);
						f.GetComponent<fireworkBallBehavior> ().Launch (firework_launch_loc, p.getCenter () / SCREEN_SCALEDOWN, firework_colors [p.id], basic_burst_prefab);
					}
				}
				next_hash_f.Add (p.id);
			}
			seen_in_last_frame = next_hash_f;

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
			#endregion

		case "fireworks shapes":
			#region
			HashSet<int> next_hash_fs = new HashSet<int> ();
			foreach (Blob p in paddles) {
				if (!seen_in_last_frame.Contains (p.id)) {
					GameObject f = (GameObject)Instantiate (firework_ball_prefab);
					f.GetComponent<fireworkBallBehavior> ().Launch (firework_launch_loc, p.getCenter () / SCREEN_SCALEDOWN, firework_colors [p.id], chooseSpecialFirework ());
				}
				next_hash_fs.Add (p.id);
			}
			seen_in_last_frame = next_hash_fs;

			//TEST
			if (Input.GetMouseButtonDown (0)) {
				GameObject f = (GameObject)Instantiate (firework_ball_prefab);
				f.GetComponent<fireworkBallBehavior> ().Launch (firework_launch_loc, Input.mousePosition / SCREEN_SCALEDOWN, firework_colors [Random.Range (0, MAX_PADDLES)], chooseSpecialFirework ());
			}
			break;
			#endregion

		case "great torch":
			if (centers.Length != 0) {
				slider.GetComponent<Slider> ().value = (float) paddles.Count / centers.Length;
			}
			
			break;

		case "torches":
			#region
			if (!disrupt_normal_torches) {

				if (illuminate) {
					//turn all the spots dark - we'll check if they are illuminated paddle by paddle
					foreach (darknessBehavior dark_spot in GameObject.FindObjectsOfType<darknessBehavior>()) {
						dark_spot.checkAround (torches);
						//TEST
						dark_spot.checkAround (new GameObject[1]{ mouse_torch });
					}
				}

				HashSet<int> seen_this_frame = new HashSet<int> ();
				foreach (Blob p in paddles) {
					if (!seen_in_last_frame.Contains (p.id)) {
						torches [p.id].GetComponent<ParticleSystem> ().Play ();
						torches [p.id].GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = true;
					}
					torches [p.id].transform.position = p.getCenter () / SCREEN_SCALEDOWN;
					seen_this_frame.Add (p.id);
					frames_since_seen [p.id] = 0;
				}
				seen_in_last_frame = seen_this_frame;
				for (int fs = 0; fs < centers.Length; fs++) {
					if (frames_since_seen [fs] > absence_tolerance) {
						torches [fs].GetComponent<ParticleSystem> ().Stop ();
						torches [fs].GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = false;

					}
					frames_since_seen [fs]++;
				}
			}

			//TEST
			if (!disrupt_normal_torches) {

				if (Input.GetMouseButton (0)) {
					if (!mouse_held_last_frame) {
						mouse_torch.GetComponent<ParticleSystem> ().Play ();
						mouse_torch.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = true;
					}
					mouse_torch.transform.position = Input.mousePosition / SCREEN_SCALEDOWN;
					mouse_held_last_frame = true;
					frames_since_mouse_seen = 0;
				} else {
					mouse_held_last_frame = false;
				}
				if (frames_since_mouse_seen > absence_tolerance) {
					mouse_torch.GetComponent<ParticleSystem> ().Stop ();
					mouse_torch.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = false;
				}
				frames_since_mouse_seen++;
			} 
			break;
			#endregion

		case "network":
			#region
			foreach (Blob p in paddles) {
				if (placing_dots) {
					if (dots [p.id] == null) {
						dots [p.id] = (GameObject)Instantiate (dot_prefab, p.getCenter () / SCREEN_SCALEDOWN, Quaternion.identity);
						dots[p.id].GetComponent<dotBehavior>().startUp(.5f,firework_colors [p.id]);
						GameObject particles = (GameObject)Instantiate (dot_particle_prefab, p.getCenter () / SCREEN_SCALEDOWN, Quaternion.identity);
						ParticleSystem.MainModule settings = particles.GetComponent<ParticleSystem> ().main;
						settings.startColor = new ParticleSystem.MinMaxGradient (firework_colors [p.id]);
					}
				}
			}

			if (connecting){
				float connection_percentage = (music.time - connection_start_time) / (connection_time - connection_start_time);
				print(connection_start_time+", "+connection_time);
				print(connection_percentage+"%");
				foreach (List<Edge> edges in edge_lists){
					foreach (Edge edge in edges){
						//pairs consist of a starter, a dest, and a particle system
						edge.ps.transform.position = Vector3.Lerp(edge.start.transform.position, edge.end.transform.position, connection_percentage);
					}
				}
				if (connection_percentage >= 1f) connecting = false;
			}

			//TEST
			if (Input.GetMouseButtonDown(0)){
				int random_id = Random.Range(0, MAX_PADDLES);
				dots[random_id] = (GameObject) Instantiate (dot_prefab, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
				dots[random_id].GetComponent<dotBehavior>().startUp(.5f, Color.cyan);//firework_colors [random_id]);
				GameObject particles = (GameObject)Instantiate (dot_particle_prefab, Input.mousePosition / SCREEN_SCALEDOWN, Quaternion.identity);
				ParticleSystem.MainModule settings = particles.GetComponent<ParticleSystem> ().main;
				settings.startColor = new ParticleSystem.MinMaxGradient (firework_colors [random_id]);
			}

			break;
			#endregion

		case "globe":
			if (finale_stars_active) {
				foreach (Blob p in paddles) {
					finale_stars [p.id].GetComponent<SpriteRenderer> ().enabled = true;
					finale_stars [p.id].transform.position = p.getCenter () / SCREEN_SCALEDOWN;
					frames_since_seen [p.id] = 0;
				}
				for (int ff = 0; ff < centers.Length; ff++) {
					if (frames_since_seen [ff] > absence_tolerance) {
						finale_stars [ff].GetComponent<SpriteRenderer> ().enabled = false;
					}
					frames_since_seen [ff]++;
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
			StartCoroutine(EndTimer (19.309f - music.time, "intro", "stars"));
			background.GetComponent<expandBackground>().startSwitchColor(Color.black, 0f, 0f);
			narration.PlayClip("intro", 5f);
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
			background.transform.localScale = Vector3.zero;
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

			//activate the ui
			message_ui.SetActive(true);
			message_ui.GetComponent<MessageBehavior>().display("Raise your paddle. For best results, tilt it slightly backward.", "green up", 5f);

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
			narration.PlayClip("welcome", (56.698f + 2f) - music.time);
			StartCoroutine(EndTimer(71.971f - music.time, "stars twinkling", "bubbles"));
			break;
			#endregion
		
		case "bubbles":
			#region
			music.time = 71.971f;
			//set up to end this phase
			background.GetComponent<expandBackground>().startSwitchColor(Color.black, .3f, .5f);
			StartCoroutine(EndTimer(129.756f - music.time, "bubbles", "heart")); 
			//heart starts at 2 min 9 sec 756 millis

			//activate the ui
			message_ui.SetActive(true);
			message_ui.GetComponent<MessageBehavior>().display("Raise your paddle.", "green up", 4f);

			break;
			#endregion

		case "heart":
			#region
			music.time = 129.756f;
			background.GetComponent<expandBackground>().startSwitchColor(Color.white, .5f, 2f);
			background.GetComponent<expandBackground>().grow = false;
			narration.PlayClip("heart", 1f);
			//we allow the heart to start being revealed when the violin comes in
			//in the soundtrack, at 2 min 23 sec 713 millis
			StartCoroutine (waitAndActivateHeart (143.713f - music.time));
			//set up when this ends
			StartCoroutine (EndTimer(172.546f - music.time, "heart", "expanding star"));

			//activate the ui
			message_ui.SetActive(true);
			message_ui.GetComponent<MessageBehavior>().display("Paddles down.", "paddles down", 3f);
			StartCoroutine(waitThenMessage(151f - music.time, "Raise your paddle.", "green up", 4f));
			StartCoroutine(waitThenMessage(165f - music.time, "Paddles down.", "paddles down", 3f));

			break;
			#endregion

		case "expanding star":
			#region
			music.time = 172.546f;
			background.GetComponent<expandBackground> ().
			  startSwitchColor(new Color(0f,0f,75f), .5f, 1.5f);

			StartCoroutine(waitThenMessage(178f - music.time, "Paddles up.", "paddles down", 3f));

			//set up when this ends
			StartCoroutine(EndTimer(204.651f - music.time, "expanding star", "RPI"));
			break;
			#endregion

		case "RPI":
			#region
			music.time = 204.651f;
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, 1f, 1.5f);
			Instantiate (rpi_logo_prefab, transform.position, Quaternion.identity);
			StartCoroutine (DelayBackgroundChange (259.913f - music.time, Color.red, 1f, 3f));
			StartCoroutine(waitThenMessage(208f - music.time, "Flip your paddle from red to green to throw some paint.", "flip", 8f));
			narration.PlayClip("rpi", .5f);
			//set up when this ends
			StartCoroutine(EndTimer(273.269f - music.time, "RPI", "second intro"));
			break;
			#endregion

		case "second intro":
			music.time = 273.269f;
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .25f, 16f);
			narration.PlayClip ("torches", 5f);
			float old_music_volume = music.volume;
			music.volume = 1f;
			StartCoroutine(FadeMusicIntoFireworks(320f - music.time, 4f, music.time + (320f - music.time) + 4f, old_music_volume));
			break;

		case "fireworks":
			#region
			music.time = 400.154f;

			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .5f, 0f);
			seen_in_last_frame.Clear ();
			foreach (Blob p in paddles) {
				seen_in_last_frame.Add (p.id);
			}

			StartCoroutine(waitThenMessage(1f, "PADDLES UP!", "green up", 3f));

			//initial burst
			StartCoroutine (fireworksDoStore (406.2f - music.time, false));
			StartCoroutine (launchStore (406.2f - music.time, basic_burst_prefab));

			StartCoroutine(waitThenMessage(410f - music.time, "Flip your paddle from red to green.", "flip", 4f));

			//timed bursts later on in the phase
			StartCoroutine (fireworksDoStore (431.507f - music.time, true));
			StartCoroutine (setStorePercentage (435.440f - music.time, .25f));
			StartCoroutine (launchStorePercentage (435.440f - music.time, basic_burst_prefab));
			StartCoroutine (launchStorePercentage (436.276f - music.time, basic_burst_prefab));
			StartCoroutine (launchStorePercentage (439.010f - music.time, basic_burst_prefab));
			StartCoroutine (launchStorePercentage (439.993f - music.time, basic_burst_prefab));
			StartCoroutine (clearStoredBursts (441.000f - music.time));
			StartCoroutine (fireworkSparkle (443.567f - music.time, 3.35f));
			StartCoroutine (EndTimer (447.702f - music.time, "fireworks", "fireworks shapes"));
			break;
			#endregion

		case "fireworks shapes":
			#region
			music.time = 447.702f;
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .75f, 0f);
			StartCoroutine(EndTimer(472.672f - music.time, "fireworks shapes", "great torch"));
			break;
			#endregion

		case "great torch":
			#region
			music.time = 472.672f;
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .5f, .25f);
			great_torch = (GameObject) Instantiate (great_torch_prefab);
			torch_UI.SetActive (true);
			StartCoroutine(EndTimer(503.498f - music.time, "great torch", "torches"));
			StartCoroutine(waitThenMessage(1f, "Put up red or green.", "green or red", 8f));
			break;
			#endregion

		case "torches":
			#region
			music.time = 503.498f;
			StartCoroutine(EndTimer(545.200f - music.time, "torches", "network"));
			seen_in_last_frame.Clear ();
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .65f, 0f);
			//The torches array holds a torch object for every paddle.
			//Whenever the paddle is present, ShowBusiness plays the Particle
			//System and the torch is kept locked to the location of the paddle.
			for (int tt = 0; tt < torches.Length; tt++) {
				if (centers.Length != 0) { //if we're using paddles + ids
					if (centers.Length > tt) //if there's a paddle for this
						torches [tt] = (GameObject)Instantiate (indiv_torch_prefab, centers [tt] / SCREEN_SCALEDOWN, Quaternion.identity);
				} else
					torches [tt] = (GameObject)Instantiate (indiv_torch_prefab, Random.insideUnitCircle * 40f + (Vector2)transform.position, Quaternion.identity);
			}
			//TEST
			mouse_torch = (GameObject)Instantiate (indiv_torch_prefab);
			mouse_torch.name = "mouse torch";

			StartCoroutine (torchesBackground ());
			StartCoroutine (FlickerTorches ());
			break;
			#endregion

		case "network":
			music.time = 545.200f;
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .65f, 0f);
			StartCoroutine (StartNodeConnection (555.419f - music.time));
			StartCoroutine (TwinkleNetwork (562.449f - music.time, 3.590f));
			StartCoroutine (DelayBackgroundChange (574.665f - music.time, Color.black, 1f, 2f));
			StartCoroutine (EndTimer (580.833f - music.time, "network", "globe"));
			StartCoroutine (waitThenMessage (1f, "Paddles up!", "green up", 4f));
			narration.PlayClip ("tapestry", 1f);
			network_camera.gameObject.SetActive (true);
			globe_camera.gameObject.SetActive (true);
			Instantiate (blocking_quad_prefab);
			break;

		case "globe":
			music.time = 580.833f;
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .65f, 0f);
			globe_quad = (GameObject)Instantiate (globe_quad_prefab, transform.position, Quaternion.identity);
			StartCoroutine (GlobeDisplay (597.761f - music.time));
			for (int fx = 0; fx < finale_stars.Length; fx++) {
				finale_stars [fx] = (GameObject)Instantiate (single_star_prefab);
				finale_stars [fx].GetComponent<SpriteRenderer> ().sprite = star_sprites [Random.Range (0, star_sprites.Length - 1)];
				if (fx < centers.Length)
					finale_stars [fx].transform.position = centers [fx];
				//TEST
				if (centers.Length == 0)
					finale_stars [fx].transform.position = new Vector2 (Random.Range (0, 128), Random.Range (0, 72)); 

				finale_stars [fx].SetActive (false);
				finale_stars [fx].GetComponent<SpriteRenderer> ().enabled = false;
			}
			for (int fss = 0; fss < frames_since_seen.Length; fss++) {
				frames_since_seen [fss] = absence_tolerance + 5;
			}
			StartCoroutine (BlinkFinaleStars ());
			narration.PlayClip ("thank you", 623.204f - music.time);
			break;
		}
			
	}

	//cleans up phase objects and starts the next
	void EndPhase(string old_phase, string next){
		message_ui.SetActive (false);

		switch (old_phase) {

		case "intro":
			StartPhase (next);//stars
			break;

		case "stars":
			if (GameObject.Find ("Stars")) {
				destroy_stars_container = true;
				Destroy (GameObject.Find ("Stars"));
			}
			foreach (GameObject star in GameObject.FindGameObjectsWithTag("star")) {
				Destroy (star);
			}

			StartPhase (next);//stars twinkling
			break;

		case "stars twinkling":
			star_array = new GameObject[0];
			foreach (GameObject star in GameObject.FindGameObjectsWithTag("star")) {
				Destroy (star);
			}
			StartPhase (next);//bubbles
			break;
		
		case "bubbles":
			bubbleBehavior[] bubbles_left = GameObject.FindObjectsOfType<bubbleBehavior> ();
			foreach (bubbleBehavior b in bubbles_left)
				b.Pop ();
			StartPhase (next);//heart
			break;

		case "heart":
			expandingShape[] hearts = GameObject.FindObjectsOfType<expandingShape> ();
			foreach (expandingShape e in hearts)
				Destroy (e.gameObject);
			StartPhase (next);//expanding star
			break;

		case "expanding star":
			expandingShape[] stars = GameObject.FindObjectsOfType<expandingShape> ();
			foreach (expandingShape e in stars)
				Destroy (e.gameObject);
			StartPhase (next);//RPI
			break;

		case "RPI":
			paintSplatBehavior[] splats = GameObject.FindObjectsOfType<paintSplatBehavior> ();
			foreach (paintSplatBehavior p in splats)
				Destroy (p.gameObject);
			GameObject.FindObjectOfType<rpiLogoBehavior> ().fadeOut();
			StartPhase (next);//second intro
			break;

		case "second intro":
			StartPhase (next);
			break;

		case "fireworks": 
			stored_bursts.Clear ();
			StartPhase (next);//"fireworks shapes"
			break;

		case "fireworks shapes":
			StartPhase (next);//"great torch"
			break;

		case "great torch":
			torch_UI.SetActive (false);
			Destroy (great_torch);
			StartPhase (next);//"torches"
			break;

		case "torches":
			Destroy (GameObject.FindObjectOfType<makeSpaceDark> ().gameObject);
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, 1f, 0f);
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .5f, 2.5f);
			foreach (GameObject t in torches)
				Destroy (t);
			Destroy (mouse_torch);
			StartPhase (next); //"network"
			break;

		case "network":
			//take screenshot
			background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .5f, 4f);
			StartCoroutine (FreezeCam ());
			Instantiate (sphere_prefab);
			//wait, then destroy the other objects
			StartCoroutine (waitThenDestroyNetwork ());
			StartPhase (next);
			break;

		case "globe":
			foreach (GameObject fi in finale_stars) {
				Destroy (fi);
			}
			network_camera.gameObject.SetActive (false);
			globe_camera.gameObject.SetActive (false);
			Destroy (GameObject.FindObjectOfType<globeQuadBehavior> ().gameObject);
			Destroy (GameObject.Find ("Blocking Quad(Clone)"));
			break;
		}
			
	}

	IEnumerator StarTimer(float first_stop, float second_stop, float third_stop, float last_stop, float music_restart_time){
		bool phase_changed = false;
		//there are a couple of places where we might start the twinkling phase
		yield return new WaitForSeconds(first_stop);
		print ("stop 1");
		if (positions_recorded && !phase_changed) {
			EndPhase ("stars", "stars twinkling");
			phase_changed = true;
		}
		yield return new WaitForSeconds (second_stop - first_stop);
		print ("stop 2");
		if (positions_recorded && !phase_changed) {
			EndPhase ("stars", "stars twinkling");
			phase_changed = true;
		}
		yield return new WaitForSeconds (third_stop - (second_stop + first_stop));
		print ("stop 3");
		if (positions_recorded && !phase_changed) {
			EndPhase ("stars", "stars twinkling");
			phase_changed = true;
		}
		yield return new WaitForSeconds (last_stop - (third_stop + second_stop + first_stop));
		print ("last stop");
		if (positions_recorded && !phase_changed) {
			EndPhase ("stars", "stars twinkling");
			phase_changed = true;
		}
		else if (!positions_recorded){
			//if we got here without being told we can end,
			//we need to loop back in the music and try again
			music.time = music_restart_time;
			StartCoroutine(StarTimer(0f, 0f, 0f, last_stop - third_stop, music_restart_time));
		}
	}

	IEnumerator EndTimer(float seconds_to_wait, string phase_to_end, string next_phase){
		yield return new WaitForSeconds (seconds_to_wait);
		EndPhase (phase_to_end, next_phase);
	}

	IEnumerator waitAndActivateHeart(float wait_seconds){
		yield return new WaitForSeconds (wait_seconds);
		//Instantiate (heart_prefab, transform.position, Quaternion.identity);
		print("hearts active");
		shape_color_bursts_active = true;
	}

	IEnumerator DelayBackgroundChange(float wait_seconds, Color new_color, float transparency, float change_time){
		yield return new WaitForSeconds (wait_seconds);
		background.GetComponent<expandBackground> ().startSwitchColor (new_color, transparency/*1f*/, change_time/*3f*/);
	}

	IEnumerator FadeMusicIntoFireworks(float wait_seconds, float fade_duration, float fade_time_goal, float old_music_volume){
		if (wait_seconds > 0) {
			yield return new WaitForSeconds (wait_seconds);
			print ("fading!");
		} else {
			yield return new WaitForSeconds (.05f);
		}
		//what percentage of the time have we covered?
		print("fade time goal("+fade_time_goal+") - music.time("+music.time+") / fade_duration("+fade_duration);
		music.volume = (fade_time_goal - music.time) / (float)fade_duration; 
		print (music.volume);
		if (music.volume <= 0) {
			music.volume = old_music_volume;
			EndPhase ("second intro", "fireworks");
		} 
		else {
			//loop and wait around for more music fading
			StartCoroutine (FadeMusicIntoFireworks(0f, fade_duration, fade_time_goal, old_music_volume));
		}
	}



	IEnumerator fireworksDoStore(float wait_seconds, bool store_val){
		yield return new WaitForSeconds (wait_seconds);
		store = store_val;
		if (store_val) {
			message_ui.SetActive (true);
			message_ui.GetComponent<MessageBehavior> ().display ("Paddles up!", "green up", 3f);
		}
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

	GameObject chooseSpecialFirework(){
		float choice = Random.value;
		if (choice < .33f) {
			return double_burst_prefab;
		} 
		else if (choice >= .33f && choice <= .66f) {
			return box_firework_prefab;
		} 
		else
			return circle_firework_prefab;
	}

	public void RecordCenters(Dictionary<Vector2, int> locations_and_ids){
		centers = new Vector2[locations_and_ids.Count];
		foreach (Vector2 v in locations_and_ids.Keys) {
			centers [locations_and_ids [v]] = v;
		}
	}

	IEnumerator FlickerTorches(){
		List<GameObject> torches_on = new List<GameObject> ();
		//left half
		yield return new WaitForSeconds(527.983f - music.time);
		disrupt_normal_torches = true;
		foreach (GameObject t in torches) {
			if (t != null) {
				t.GetComponent<ParticleSystem> ().Stop ();
				t.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = false;
			}
		}
		mouse_torch.GetComponent<ParticleSystem> ().Stop ();//TEST
		foreach (GameObject t in torches) {
			if (t == null) continue;
			if (t.transform.position.x < transform.position.x) {
				t.GetComponent<ParticleSystem> ().Play ();
				t.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = true;
				StartCoroutine (TorchOffTimer (528.500f - music.time, t));
			} else {
				t.GetComponent<ParticleSystem> ().Stop ();
				t.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = false;
			}
		}
		//right half
		yield return new WaitForSeconds(528.908f - music.time);
		foreach (GameObject t in torches) {
			if (t == null) continue;
			if (t.transform.position.x > transform.position.x) {
				t.GetComponent<ParticleSystem> ().Play ();
				t.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = true;
				StartCoroutine(TorchOffTimer(529.373f - music.time, t));
			}
		}
		//center
		yield return new WaitForSeconds(529.778f - music.time);
		foreach (GameObject t in torches) {
			if (t == null) continue;
			if (Vector2.Distance(t.transform.position, transform.position) < 20f) {
				t.GetComponent<ParticleSystem> ().Play ();
				t.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = true;
				StartCoroutine(TorchOffTimer(530.241f - music.time, t));
			}
		}
		//random hits
		yield return new WaitForSeconds (530.611f - music.time); //8 min 50 sec .611 
		burstRandomTorch ();
		yield return new WaitForSeconds (530.891f - music.time); //8 min 50 sec .891
		burstRandomTorch ();
		yield return new WaitForSeconds (531.201f - music.time); //8 min 51 sec .201
		burstRandomTorch();
		yield return new WaitForSeconds (531.461f - music.time); //8 min 51 sec .461
		burstRandomTorch();
		yield return new WaitForSeconds (531.631f - music.time); //8 min 51 sec .631
		burstRandomTorch();
		yield return new WaitForSeconds (531.781f - music.time); //8 min 51 sec .781
		burstRandomTorch();
		yield return new WaitForSeconds (531.901f - music.time); //8 min 51 sec .901
		burstRandomTorch();
		yield return new WaitForSeconds (532f - music.time);     //8 min 52 sec .000
		burstRandomTorch();
		yield return new WaitForSeconds (532.041f - music.time); //8 min 52 sec .041
		burstRandomTorch();
		yield return new WaitForSeconds (532.301f - music.time); //8 min 52 sec .301
		burstRandomTorch();

		//set back to free roam mode
		yield return new WaitForSeconds(532.421f - music.time);
		disrupt_normal_torches = false;
	}

	IEnumerator TorchOffTimer(float wait_seconds, GameObject t){
		yield return new WaitForSeconds (wait_seconds);
		t.GetComponent<ParticleSystem> ().Stop ();
		t.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = false;
	}

	IEnumerator TorchOffTimer(GameObject t){
		yield return new WaitForSeconds (.3f);
		t.GetComponent<ParticleSystem> ().Stop ();
		t.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = false;
	}

	void burstRandomTorch(){
		GameObject random_torch = torches[Random.Range(0, centers.Length)]; 
		random_torch.GetComponent<ParticleSystem> ().Play ();
		random_torch.GetComponentInChildren<torchGlowBehavior> ().gameObject.GetComponent<SpriteRenderer> ().enabled = true;
		StartCoroutine(TorchOffTimer(random_torch));
	}

	IEnumerator torchesBackground(){
		yield return new WaitForSeconds (514.615f - music.time);
		background.GetComponent<expandBackground> ().startSwitchColor (Color.black, 1f, 5f); 
		yield return new WaitForSeconds (520.894f - music.time);
		background.GetComponent<expandBackground> ().startSwitchColor (Color.black, .2f, 1f); 
		Instantiate (dark_parent_prefab);
		illuminate = true;
	}

	public void PhaseJump(){
		StopAllCoroutines ();
		narration.gameObject.GetComponent<AudioSource> ().clip = null;
		EndPhase (phase, phase_input.text);
	}

	IEnumerator StartNodeConnection(float wait_seconds){
		yield return new WaitForSeconds (wait_seconds);

		//place all the dots that haven't been placed
		if (centers.Length != 0) {//non test case
			for (int center_index = 0; center_index < centers.Length; center_index++) {
				if (dots [center_index] == null) {
					dots [center_index] = (GameObject)Instantiate (dot_prefab, centers [center_index] / SCREEN_SCALEDOWN, Quaternion.identity);
					dots [center_index].GetComponent<dotBehavior> ().startUp (.5f, Color.cyan);//firework_colors [center_index]);
					GameObject particles = (GameObject)Instantiate (dot_particle_prefab, centers [center_index] / SCREEN_SCALEDOWN, Quaternion.identity);
					ParticleSystem.MainModule settings = particles.GetComponent<ParticleSystem> ().main;
					settings.startColor = new ParticleSystem.MinMaxGradient (firework_colors [center_index]);
				}
			}
		}/*
		else { //TEST
			for (int c = 0; c < dots.Length; c++) {
				if (dots [c] == null) {
					Vector2 random_loc = new Vector2 (Random.Range (0, 128), Random.Range (0, 72));
					dots [c] = (GameObject)Instantiate (dot_prefab, random_loc, Quaternion.identity);
					dots [c].GetComponent<dotBehavior> ().startUp (.5f, Color.cyan);//firework_colors [c]);
					GameObject particles = (GameObject)Instantiate (dot_particle_prefab, random_loc, Quaternion.identity);
					ParticleSystem.MainModule settings = particles.GetComponent<ParticleSystem> ().main;
					settings.startColor = new ParticleSystem.MinMaxGradient (firework_colors [c]);
				}
			}
		}*/

		//now choose which connections will happen
		int centers_length = centers.Length;
		if (centers.Length == 0)
			centers_length = MAX_PADDLES;
		for (int i = 0; i < dots.Length; i++) {
			for (int j = 0; j < dots.Length; j++) {
				//we only make a connection FROM lower id TO higher id when the dots are close enough
				if (i < j && i < centers_length && j < centers_length && dots[i] != null && dots[j] != null &&
					Vector2.Distance (dots [i].transform.position, dots [j].transform.position) < connection_radius) {
					//edge_lists is a List of Lists of edge object, one list of edges for each point
					GameObject ps = (GameObject) Instantiate(connecting_particles, dots[i].transform.position, Quaternion.identity);
					Edge edge = new Edge (dots[i], dots[j], ps);
					edge_lists [i].Add (edge);
				}
			}
		}
		placing_dots = false;
		connecting = true;
		connection_start_time = music.time;
	}

	class Edge{
		public GameObject start;
		public GameObject end;
		public GameObject ps;

		public Edge (GameObject s, GameObject e, GameObject p){
			start = s;
			end = e;
			ps = p;
		}
	}

	IEnumerator TwinkleNetwork(float wait_seconds, float duration){
		List<GameObject> twinkling_lines = new List<GameObject>();

		yield return new WaitForSeconds (wait_seconds);
		GameObject[] lines = GameObject.FindGameObjectsWithTag ("line particles");
		foreach (GameObject line in lines) {
			line.GetComponent<ParticleSystemRenderer>().enabled = false;
		}
		foreach (List<Edge> edges in edge_lists) {
			foreach (Edge e in edges) {
				Vector2 position = new Vector2 ((e.start.transform.position.x + e.end.transform.position.x) * .5f, 
					                   (e.start.transform.position.y + e.end.transform.position.y) * .5f);
				float length = Vector2.Distance (e.start.transform.position, e.end.transform.position);
				float rotation = Vector2.Angle (e.start.transform.position, e.end.transform.position);
				GameObject particles = (GameObject)Instantiate (twinkling_line_prefab, position, Quaternion.identity);
				particles.transform.localRotation = Quaternion.Euler (0f, 0f, Random.Range(0,360));
				ParticleSystem.ShapeModule settings = particles.GetComponent<ParticleSystem> ().shape;
				settings.radius = length;
				twinkling_lines.Add (particles);
			}
		}
		for (int d = 0; d< dots.Length; d++) {
			if (dots[d] != null) {
				dots[d].SetActive (false);
				GameObject ps = Instantiate (dot_sparkle_prefab, dots[d].transform.position, Quaternion.identity);
				ParticleSystem.MainModule settings = ps.GetComponent<ParticleSystem> ().main;
				settings.duration = duration;
				settings.startColor = firework_colors [d];
				ps.GetComponent<ParticleSystem> ().Play ();
			}
		}

		yield return new WaitForSeconds (duration);

		foreach (GameObject line in twinkling_lines) {
			Destroy (line);
		}
		foreach (GameObject line in lines) {
			line.GetComponent<ParticleSystemRenderer>().enabled = true;
		}
		foreach (GameObject dot in dots) {
			if (dot != null)
				dot.SetActive (true);
		}

		network_camera.enabled = true;
	}

	IEnumerator FreezeCam()
	{
		network_camera.clearFlags = CameraClearFlags.Nothing;
		yield return null;
		network_camera.cullingMask = 0;
	}

	IEnumerator waitThenDestroyNetwork(){
		yield return new WaitForSeconds (2f);
		foreach (ParticleSystem p in GameObject.FindObjectsOfType<ParticleSystem>())
			Destroy (p.gameObject);
		foreach (dotBehavior dot in GameObject.FindObjectsOfType<dotBehavior>())
			Destroy (dot.gameObject);
	}

	IEnumerator GlobeDisplay(float wait_seconds){
		//fade globe quad
		yield return new WaitForSeconds(wait_seconds);
		globe_quad.GetComponent<globeQuadBehavior> ().FadeDown (.3f);
		foreach (GameObject g in finale_stars) g.SetActive (true);
		finale_stars_active = true;
		yield return new WaitForSeconds (611.183f - music.time);
		globe_quad.GetComponent<globeQuadBehavior> ().FadeDown (0, .005f);
	}

	IEnumerator BlinkFinaleStars(){
		GameObject chosen_star;
		yield return new WaitForSeconds (624.044f - music.time);
		//set up the blinking
		foreach (GameObject star in finale_stars)
			star.GetComponent<SpriteRenderer> ().enabled = false;
		finale_stars_active = false;

		finale_stars [Random.Range (0, finale_stars.Length - 1)].
			GetComponent<blinkBriefly>().blink();

		float[] blinks = 
		   {624.074f, 624.324f, 624.414f, 624.614f, 625.004f, 625.154f, 625.264f, 625.444f,
			627.509f, 627.709f, 627.809f, 627.949f, 627.999f, 628.199f, 628.589f, 628.689f, 628.827f,
			631.084f, 631.184f, 631.384f, 631.484f, 631.784f, 632.154f, 632.204f, 632.224f, 632.414f,
			634.609f, 634.709f, 634.909f, 635.109f, 635.269f, 635.709f, 635.809f, 635.909f,
			640f, 640.2f, 640.3f, 640.5f, 640.7f, 641.1f, 641.4f};

		int range = centers.Length - 1;
		if (centers.Length == 0) range = finale_stars.Length - 1;
		foreach (float f in blinks) {
			yield return new WaitForSeconds (f - music.time);
			finale_stars [Random.Range (0, range)].
			GetComponent<blinkBriefly>().blink();
		}
	}

	IEnumerator waitThenMessage(float wait_seconds, string message, string sequence, float duration){
		print ("getting ready to activate ui");
		yield return new WaitForSeconds (wait_seconds);
		print ("activating ui");
		message_ui.SetActive (true);
		message_ui.GetComponent<MessageBehavior> ().display (message, sequence, duration);
	}

	/*Schedule:
	 * Narration from 0s to 18.699s
	 * Black Background explodes out and Stars appear at 1945s
	 * */
}
