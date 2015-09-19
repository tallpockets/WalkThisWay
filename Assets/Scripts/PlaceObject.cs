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
					MeshRenderer mr = c.gameObject.GetComponent<MeshRenderer>();
					Bounds br = mr.bounds;
					Vector3 p = rayHit.point;
					float length = Mathf.Floor(p.x/(br.size.x/mGridSize.x));
					float height = Mathf.Floor(p.y/(br.size.y/mGridSize.y));
					float depth = Mathf.Floor(p.z/(br.size.z/mGridSize.z));
					Vector3 finalPos = new Vector3(length * (br.size.x/mGridSize.x),height* (br.size.y/mGridSize.y),depth * (br.size.z/mGridSize.z));
					Vector3 offset = new Vector3((br.size.x/mGridSize.x)/2f,(br.size.y/mGridSize.y)/2f,(br.size.z/mGridSize.z)/2f);
					finalPos = finalPos + offset;

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
