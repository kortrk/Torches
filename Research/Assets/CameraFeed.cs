using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFeed : MonoBehaviour {

	// Use this for initialization
	void Start () {
		foreach (WebCamDevice w in WebCamTexture.devices) {
			print (w.name);
		}
		WebCamTexture webcam = new WebCamTexture (WebCamTexture.devices [0].name);
		GetComponent<MeshRenderer> ().material.mainTexture = webcam;
		webcam.Play ();
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
