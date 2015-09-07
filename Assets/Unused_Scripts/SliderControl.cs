using UnityEngine;
using System.Collections;
using UnityEngine.UI; 

public class SliderControl : MonoBehaviour {

	public Slider slid;
	public int kHeightOffset = 4;
	public PlaceCubes pc;
	[Tooltip("bigger number = slower camrea response to slider")]
	public float InterpolationDivider = 5.0f; 
	private Vector3 FinalPos;

	// Use this for initialization
	void Start () {

		FinalPos = Camera.main.transform.position;
		FinalPos.z = slid.value;
		FinalPos.y = pc.mCubeHeigts[0,(int)slid.value] + kHeightOffset;

		slid.maxValue = pc.kTerrainLength-1;
	}


	// Update is called once per frame
	void Update () {
		Vector3 pos = Camera.main.transform.position;
		pos.z = slid.value;
		//int y = pc.mCubeHeigts[(int)slid.value] + kHeightOffset;
		pos.y = pc.mCubeHeigts[0,(int)slid.value] + kHeightOffset;

		Vector3 dir = FinalPos - pos;
		float mag = Vector3.Magnitude(dir);

		if(Mathf.Abs(mag) > 0.01f){
			FinalPos -= dir/InterpolationDivider;
		}
	
		Camera.main.transform.position = FinalPos;

	}

}
