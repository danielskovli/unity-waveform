using UnityEngine;
using System.Collections;

public class rotateScene : MonoBehaviour {
	
	private float rotation = 0.0f;
	
	// Use this for initialization
	void Start () {
		Application.runInBackground = true;
	}
	
	// Update is called once per frame
	void Update () {
		this.transform.localEulerAngles = new Vector3(0, rotation, 0);
		float temp = Mathf.Abs(rotation);
		temp = Mathf.Abs(90-temp)+1f; // approaches 0 when the offset towards screen is close to 90 degrees
		this.transform.position = new Vector3(-(30-(temp/3)), this.transform.position.y, this.transform.position.z);
	}
	
	void OnGUI() {
		
		if (GUI.RepeatButton(new Rect(Screen.width-245, 5, 80, 25), "Rotate -")) {
			rotation -= (rotation > -180f) ? 1f : 0;;
		}
		
		if (GUI.RepeatButton(new Rect(Screen.width-160, 5, 70, 25), "Reset")) {
			rotation = 0;
		}
		
		if (GUI.RepeatButton(new Rect(Screen.width-85, 5, 80, 25), "Rotate +")) {
			rotation += (rotation < 180) ? 1f : 0;
		}

	}
}
