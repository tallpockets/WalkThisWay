using UnityEngine;
using System.Collections;

public class PlaceObject : MonoBehaviour {

	public GameObject mObjectToPlace;
	public Vector3 mGridSize;
	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0))
		{
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
			RaycastHit rayHit;

			if (Physics.Raycast(ray, out rayHit, 100f))
			{
				Collider c = rayHit.collider;
				if(c.gameObject.tag == "Terrain")
				{
					MeshFilter mf = c.gameObject.GetComponent<MeshFilter>();
					MeshRenderer mr = c.gameObject.GetComponent<MeshRenderer>();
					//Bounds b = mf.mesh.bounds;
					Bounds br = mr.bounds;
					//Debug.Log("Filter bounds : " + b);
					//Debug.Log("Renderder bounds.size : " + br.size);
					//Debug.Log("HitPoint is : " + rayHit.point);
					Vector3 p = rayHit.point;
					//length, height, depth
					//??, 20,20
					//float length = p.x / (br.size.x/mGridSize.x);
					//float height = p.y / (br.size.y/mGridSize.y);
					//float depth = p.z / (br.size.z/mGridSize.z);

					float length = Mathf.Floor(p.x/(br.size.x/mGridSize.x));
					float height = Mathf.Floor(p.y/(br.size.x/mGridSize.x));
					float depth = Mathf.Floor(p.z/(br.size.x/mGridSize.x));
					Vector3 finalPos = new Vector3(length/2,height/2,depth/2);

					Vector3 offset = new Vector3(0.25f,0.25f,0.25f);
					finalPos = finalPos + offset;

					//Debug.Log("rayhit: " + p + "grid : " + finalPos);
					Instantiate(mObjectToPlace, finalPos, new Quaternion(0,0,0,0));
				} else if(c.gameObject.tag == "PlacedObject")
				{
					GameObject.Destroy(c.gameObject);
				}
			}

		}
	}

	void CreateCube()
	{
	
	}



}
