using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class MiniMap : MonoBehaviour {

	public GameObject mTerrain;
	// Use this for initialization
	void Start () {
		Mesh m = mTerrain.GetComponentInChildren<MeshFilter>().mesh;
		Bounds bounds = m.bounds;
		Vector3 bSize = bounds.size;
		Debug.Log("terrain width is: " + bSize.x);


		GameObject miniMap = Instantiate(mTerrain);
		miniMap.transform.SetParent(this.gameObject.transform);
		miniMap.AddComponent<RectTransform>();
		//miniMap.AddComponent<MeshRenderer>();
		//miniMap.AddComponent<MeshFilter>();

		//mTerrain.AddComponent<RectTransform>();

		RectTransform rt = this.gameObject.GetComponent<RectTransform>();
		Debug.Log ("canvas width is: " + rt.rect.width);
		float scaleX = rt.rect.width/bSize.x;
		float scaleY = rt.rect.height/bSize.y;
		Vector3 myScale = new Vector3(scaleX, scaleY, 1);
		miniMap.GetComponent<RectTransform>().localScale = myScale;
		Vector3 anchorPos = new Vector3(0,0,0);
		Vector2 minMaxPos = new Vector2(0,0);
		miniMap.GetComponent<RectTransform>().anchorMin = minMaxPos;
		miniMap.GetComponent<RectTransform>().anchorMax = minMaxPos;
		miniMap.GetComponent<RectTransform>().anchoredPosition3D = anchorPos;
		miniMap.layer = (int)5;
	
		//Debug.Log ("Scale in x should be: " + scaleX);
		//Debug.Log ("Scale in y should be: " + scaleY);

	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
