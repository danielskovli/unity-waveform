using UnityEngine;
using System.Collections;

public class createSphericalWaveform : MonoBehaviour {

	private GameObject sphere;
	private Vector3[] basePos;
	
	// Use this for initialization
	void Start () {
		sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
		sphere.transform.localScale = Vector3.one * 10f;
	}
	
	// Update is called once per frame
	void Update () {
		
		//Debug.Log(Mathf.Sin(Time.time * Time.deltaTime));
		//return;
		
		Mesh mesh = sphere.GetComponent<MeshFilter>().mesh;
		
		if (basePos == null) {
			basePos = mesh.vertices;
		}
		
		Vector3[] vertices = new Vector3[basePos.Length];
		
		for (int i=0; i<vertices.Length; i++) {
			Vector3 vertex = basePos[i];
			vertex.y += Mathf.Sin((Time.time + (basePos[i].x + basePos[i].y + basePos[i].z)) * basePos[i].y / basePos[i].z ) * 2;
			vertices[i] = vertex;
		}
		
		mesh.vertices = vertices;
		mesh.RecalculateNormals();
	}
}
