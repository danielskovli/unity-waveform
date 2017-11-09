using UnityEngine;
using System.Collections;

public class createLinearWaveform : MonoBehaviour {
	
	// Init
	//public GameObject parent;
	public Material colourMat;
	public Material wireframeMat;
	
	public Color baseColor = new Color(0f, 0.75f, 1f, 1f);
	public int width = 40;
	public int length = 200;
	
	private int widthVerts = 40;
	private int lengthVerts = 256;
	private float amp = 200f;
	private float[] freqData = new float[64];
	
	//private float[,] rows;
	private Vector3[] vertices;
	private Vector2[] uvs;
	private int[] triangles;
	private float widthScale;
	private float lengthScale;
	private int numVerts;
	private GameObject plane;
	private float[] dampingVelocities;
	
	private float sineScale1;
	private float sineScale2;
	
	// Build
	void Start () {
		
		// Build wireframe texture
		Color color = Color.white;
		Texture2D texture = new Texture2D(32, 32);
		texture.anisoLevel = 9;
		texture.filterMode = FilterMode.Trilinear;
		
		// Loop through the width rows
		for (int x=0; x<texture.width; x++) {
			
			// Set all pixels to black
			for (int y=0; y<texture.height; y++) {
				texture.SetPixel(x, y, Color.black);
			}
			
			// Conditionally set the borders to white (creating a white square)
			if (x==0 || x==(texture.width-1)) {
				for (int y=0; y<texture.height; y++) {
					texture.SetPixel(x, y, color);
				}
			} else {
				texture.SetPixel(x, 0, color);
				texture.SetPixel(x, texture.height-1, color);
			}
		}
		
		// Apply and update the texture
		texture.Apply(true);
		wireframeMat.mainTexture = texture;
		wireframeMat.mainTextureScale = new Vector2(lengthVerts/4, widthVerts/2);
		
		// Calculations
		int widthSegments = widthVerts-1;
		int lengthSegments = lengthVerts-1;
		numVerts = widthVerts * lengthVerts;
		widthScale = (float)width / (float)widthSegments;
		lengthScale = (float)length / (float)lengthSegments;
		
		vertices = new Vector3[numVerts];
		uvs = new Vector2[numVerts];
		triangles = new int[widthSegments * lengthSegments * 6];
		
		float uvFactorX = 1f/widthSegments;
		float uvFactorY = 1f/lengthSegments;
		
		dampingVelocities = new float[numVerts];
		//rows = new float[lengthVerts, widthVerts];
		
		// Create the grid of vertices
		int index = 0;
		for (float x=0.0f; x<lengthVerts; x++) {
			for (float z=0.0f; z<widthVerts; z++) {
				vertices[index] = new Vector3(x * lengthScale, 0, z * widthScale);
				uvs[index] = new Vector2(x * uvFactorY, z * uvFactorX);
				index++;
			}
		}
		
		// Dive up in segments (triangles)
		index = 0;
		for (int x=0; x<lengthSegments; x++) {
			for (int z=0; z<widthSegments; z++) {
				triangles[index] =   (x     * widthVerts) + z;
				triangles[index+1] = ((x+1) * widthVerts) + z;
				triangles[index+2] = (x     * widthVerts) + z + 1;
				
				triangles[index+3] = ((x+1) * widthVerts) + z;
				triangles[index+4] = ((x+1) * widthVerts) + z + 1;
				triangles[index+5] = (x     * widthVerts) + z + 1;
				index += 6;
			}
		}
		
		// Check that we have a color
		//if (baseColor == null) {baseColor = new Color32(0f, 0.75f, 1f, 1f);};
		
		// Assign it
		Color[] colors = new Color[vertices.Length];
		int a = 0;
		while (a < vertices.Length) {
			//colors[a] = new Color(1f, 1f, ((float)a+1f)/(float)vertices.Length, 1f);
			colors[a] = baseColor;
			a++;
		}
		
		// Create the mesh object
		Mesh mesh = new Mesh();
		mesh.name = "PolyPlane";
		mesh.vertices = vertices;
		mesh.uv = uvs;
		mesh.colors = colors;
		mesh.triangles = triangles;
		mesh.RecalculateNormals();
		
		// Create the transform/gameobject and add the mesh to it
		plane = new GameObject("Plane");
		MeshFilter meshFilter = (MeshFilter)plane.AddComponent("MeshFilter");
        plane.AddComponent("MeshRenderer");
		plane.GetComponent<MeshFilter>().mesh = mesh;
        meshFilter.sharedMesh = mesh;
        mesh.RecalculateBounds();
		
		// parent, apply material, etc
		plane.transform.localScale = new Vector3(1,-1,1);
		plane.transform.parent = this.transform;
		plane.transform.localEulerAngles = new Vector3(0,-90,0);
		plane.transform.localPosition = new Vector3(width/2, 0, -length/2);
		plane.renderer.material = colourMat;
	}
	
	// Update
	void Update () {
		
		// Get updated frequency samples
		AudioListener.GetSpectrumData(freqData, 0, FFTWindow.BlackmanHarris);
		
		// Apply translation to each vertex based on the spectrum data
		Mesh mesh = plane.GetComponent<MeshFilter>().mesh;
		Color[] colors = mesh.colors;
		int index = 0;
		int iterations = 0;
		float decrementMaster = 5.5f;
		float factorMaster = 1.25f;
		float smoothTimeMaster = 0.2f;
		
		for (int x=lengthVerts/2; x<lengthVerts; x++) {
			float decrement = decrementMaster;
			float factor = factorMaster;
			float smoothTime = smoothTimeMaster;
			
			for (int a=widthVerts/2; a<widthVerts; a++) {
				setNewVertexPos(ref vertices, ref colors, ref dampingVelocities, ref decrement, ref index, iterations, factor, amp, smoothTime, x, a);
			}
			decrement = decrementMaster;
			for (int a=widthVerts/2-1; a>=0; a--) {
				setNewVertexPos(ref vertices, ref colors, ref dampingVelocities, ref decrement, ref index, iterations, factor, amp, smoothTime, x, a);
			}
			iterations++;
		}
		
		iterations = 0;
		for (int x=lengthVerts/2-1; x>=0; x--) {
			float decrement = decrementMaster;
			float factor = factorMaster;
			float smoothTime = smoothTimeMaster;
			for (int a=widthVerts/2; a<widthVerts; a++) {
				setNewVertexPos(ref vertices, ref colors, ref dampingVelocities, ref decrement, ref index, iterations, factor, amp, smoothTime, x, a);
			}
			decrement = decrementMaster;
			for (int a=widthVerts/2-1; a>=0; a--) {
				setNewVertexPos(ref vertices, ref colors, ref dampingVelocities, ref decrement, ref index, iterations, factor, amp, smoothTime, x, a);
			}
			iterations++;
		}
		mesh.vertices = vertices;
		mesh.colors = colors;
		mesh.RecalculateNormals();
	}
	
	
	// GUI loop
	void OnGUI() {
		
		Rect buttonCoords = new Rect(Screen.width-245, 35, 240, 25);
		if (plane.renderer.material.name.StartsWith(colourMat.name)) {
			if (GUI.Button(buttonCoords, "Switch to Wireframe render")) {
				plane.renderer.material = wireframeMat;
			}
		} else {
			if (GUI.Button(buttonCoords, "Switch to Shaded render")) {
				plane.renderer.material = colourMat;
			}
		}
		
		/*
		if (GUI.Button(new Rect(50, 100, 150, 50), "Wireframe")) {
			plane.renderer.material = wireframeMat;
			Debug.Log();
		}
		if (GUI.Button(new Rect(50, 160, 150, 50), "Shaded/Colours")) {
			plane.renderer.material = colourMat;
			Debug.Log(plane.renderer.material.name + " - " + colourMat.name);
		}
		*/
		
	}
	
	
	void setNewVertexPos(ref Vector3[] vertices, ref Color[] colors, ref float[] dampingVelocities, ref float decrement, ref int index, int iterations, float factor, float amp, float smoothTime, int x, int a) {
		
		// Bypass when audio is paused
		if (!this.audio.isPlaying) {
			return;
		}
		
		// Figure out which frequency to grab (each frequency gets grabbed by 4 rows of vertices)
		int freqIndex = Mathf.FloorToInt((iterations+1f)/4f);
		
		// Figure out for which time we're grabbing this frequency
		float remains = ((float)iterations+1f) % 4f;
		//float remains1 = ((float)(iterations+1f) % 4f) * 0.25f;	// equals 0, 0.25, 0.5 or 0.75 - according to how close to the correct index we are
		//float remains2 = 1f-remains1;							// equals 1, 0.75, 0,5 or 0.25 - according to how close to the correct index we are
		
		// Calculate this/target, previous and next - in order to smooth the curve out
		float target = freqData[freqIndex];
		float previous = ((freqIndex-1) > -1) ? freqData[freqIndex-1] : freqData[freqIndex];
		float next = ((freqIndex+1) < freqData.Length) ? freqData[freqIndex+1] : freqData[freqIndex];
		
		// Some inverting and adjustment to distrubute the amplitude properly
		float multiplier = Mathf.Clamp((freqIndex+1f)/8f, 0.2f, 100f);
		
		sineScale1 = sineScale(0.2f, 0, 3f, 5f);
		//sineScale2 = sineScale(1f, 0, 4f, 6f); // 4+6
		//sineScale2 = sineScale(1f, 0, 1f, 8f);
		sineScale2 = sineScale(0.2f, 0, sineScale(0.1f, 0, 0.8f, 6f), 13f);
		
		//target = -Mathf.Clamp(target * (amp-decrement) * multiplier, 0, 999f);
		target = target * ((amp/sineScale1) * (Mathf.Sin((a+1)/sineScale2) * ((a+1)/3))) * multiplier;
		target = -Mathf.Clamp(target, 0, 999f);
		//previous = -Mathf.Clamp(previous * (amp-decrement) * multiplier, 0, 999f);
		//next = -Mathf.Clamp(next * (amp-decrement) * multiplier, 0, 999f);
		
		if (x == 0 || x == (lengthVerts-1)) {
			vertices[widthVerts * x + a].y = 0f; // cap ends
		} else {
			vertices[widthVerts * x + a].y = Mathf.SmoothDamp(vertices[widthVerts * x + a].y, target, ref dampingVelocities[index], smoothTime);
		}
		
		//Random.seed = x;
		//colors[widthVerts * x + a].r = Mathf.Abs(vertices[widthVerts * x + a].y)/Random.Range(10f, 40f);
		//colors[widthVerts * x + a].g = Mathf.Abs(vertices[widthVerts * x + a].y)/Random.Range(5f, 10f);
		//colors[widthVerts * x + a].g = 0.3f;
		
		colors[widthVerts * x + a].r = Mathf.Abs(vertices[widthVerts * x + a].y) / sineScale(0.4f, 4, 5f, 12f); //8
		colors[widthVerts * x + a].g = Mathf.Abs(vertices[widthVerts * x + a].y) / sineScale(0.1f, -2, 9f, 15f); //10
		colors[widthVerts * x + a].b = Mathf.Abs(vertices[widthVerts * x + a].y) / sineScale(0.05f, 0, 2f, 8f); //2

		index++;
		decrement *= factor;
		
		//colors[widthVerts * (x-1) + a].r = freqData[x];
	}
	
	float sineScale(float speed, float offset, float min, float max) {
		if (min > max) {float tmp=min; min=max; max=tmp;}
		
		float dist = Mathf.Abs(max - min);
		float mid = dist / 2;
		
		return (Mathf.Sin((Time.time + offset) * speed) * mid) + (max - mid);
	}
}
