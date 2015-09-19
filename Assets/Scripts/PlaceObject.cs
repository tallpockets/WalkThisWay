using UnityEngine;
using System.Collections;

public class PlaceObject : MonoBehaviour {

	public GameObject mObjectToPlace;
	public Vector3 mTerrainGridSize;

	// Use this for initialization

	void Start () {

		if(mTerrainGridSize.magnitude == 0)
		{
			Debug.Log("no terrain size specified, setting to 1");
			mTerrainGridSize = new Vector3(1f,1f,1f);
		}
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
					float lengthGrid = (br.size.x/mTerrainGridSize.x);
					float heightGrid = (br.size.y/mTerrainGridSize.y);
					float depthGrid = (br.size.z/mTerrainGridSize.z);
					float length = Mathf.Floor(p.x/lengthGrid);
					float height = Mathf.Floor(p.y/heightGrid);
					float depth = Mathf.Floor(p.z/depthGrid);
					Vector3 finalPos = new Vector3(length * lengthGrid,height * heightGrid, depth * depthGrid);
					Vector3 offset = new Vector3(lengthGrid/2f,heightGrid/2f,depthGrid/2f);
					finalPos = finalPos + offset;

					if(mObjectToPlace != null)
						Instantiate(mObjectToPlace, finalPos, new Quaternion(0,0,0,0));

				} else if(c.gameObject.tag == "PlacedObject")
				{
					GameObject.Destroy(c.gameObject);
				}
			}

		}
	}


}
