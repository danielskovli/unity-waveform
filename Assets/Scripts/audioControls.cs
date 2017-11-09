using UnityEngine;
using System.Collections;

public class audioControls : MonoBehaviour {
	
	public AudioClip[] tracks;
	private int currentTrack;
	private bool paused = false;
	
	// Use this for initialization
	void Start () {
		this.audio.clip = tracks[0];
		currentTrack = 0;
    	this.audio.Play();
	}
	
	// Update is called once per frame
	void Update () {
		if (!this.audio.isPlaying && !paused) {
			nextTrack();
		}
	}
	
	void nextTrack() {
		this.audio.Pause();
		int newTrack = currentTrack+1;
		
		if (newTrack >= tracks.Length) {
			newTrack = 0;
		}
		
		this.audio.clip = tracks[newTrack];
    	currentTrack = newTrack;
		this.audio.Play();
	}
	
	void prevTrack() {
		this.audio.Pause();
		int newTrack = currentTrack-1;
		
		if (newTrack < 0) {
			newTrack = tracks.Length-1;
		}
		
		this.audio.clip = tracks[newTrack];
    	currentTrack = newTrack;
		this.audio.Play();
	}
	
	void playPause() {
		if (this.audio.isPlaying) {
			this.audio.Pause();
			paused = true;
		} else {
			this.audio.Play();
			paused = false;
		}
	}
	
	void OnGUI() {
		
		GUIStyle text = new GUIStyle();
		text.alignment = TextAnchor.UpperLeft;
		text.normal.textColor = new Color(1f, 1f, 1f);
		
		
		if (GUI.Button(new Rect(5, 5, 60, 25), "Prev")) {
			prevTrack();
		}
		
		if (GUI.Button(new Rect(70, 5, 100, 25), "Play/Pause")) {
			playPause();
		}
		
		if (GUI.Button(new Rect(175, 5, 60, 25), "Next")) {
			nextTrack();
		}
		
		GUI.Label(new Rect(7, 37, 350, 25), "Currently playing ("+ (currentTrack+1) +"/"+ tracks.Length +"): "+ this.audio.clip.name, text);
	}
}
