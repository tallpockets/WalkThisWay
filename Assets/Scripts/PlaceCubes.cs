using UnityEngine;
using System.Collections;

public class PlaceCubes : MonoBehaviour {

	public GameObject cube;
	public GameObject cubeWater;

	public GameObject MinimapCanvas;
	public GameObject MinimapCube;

	public int kTerrainLength = 500;
	public int kTerrainDepth = 6;

	public int kStepChance = 40;

	public int kMaxHeight = 30;
	public int kMinHeight = 0;
	public int kStepHeightMin = -4;
	public int kStepHeightMax = 4;

	public int kStepHeightChangeFrequencyMin = 5;
	public int kStepHeightChangeFrequencyMax = 15;
	private int CurrentStepHeightFrequency;

	public int[,] mCubeHeigts;

	private int interp;
	private int cubeCount;
	private int stepHeight;
	private GameObject[,,] theCubes;


	// Use this for initialization
	void Awake () {
		
		mCubeHeigts = new int[kTerrainDepth, kTerrainLength];
	}


	void Start () {

		CurrentStepHeightFrequency = Random.Range(kStepHeightChangeFrequencyMin, kStepHeightChangeFrequencyMax);
		if(cube == null)
			cube = (GameObject)Resources.Load("Cube");

		if(cubeWater == null)
			cubeWater = (GameObject)Resources.Load ("Cube_Water");

		if(MinimapCube == null)
			MinimapCube = (GameObject)Resources.Load("Cube_Minimap");

		theCubes = new GameObject[kTerrainDepth, kTerrainLength, kMaxHeight];


		GenerateCubes();

		Debug.Log("Width is: " + Screen.width);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void GenerateCubes()
	{
		for (int i = 0; i < kTerrainLength; i++)
		{
			if(interp != stepHeight)
			{
				int dif = stepHeight-interp;

				if(dif > 0)
					interp++;
				
				if(dif < 0)
					interp--;

				if(interp <= 0)
				{
					interp++;
					stepHeight = -stepHeight;
				}
				if(interp > kMaxHeight)
				{
					stepHeight = -stepHeight;
					interp--;
				}
				stepHeight = Mathf.Clamp(stepHeight, kMinHeight, kMaxHeight);
			}


			interp = Mathf.Clamp(interp, kMinHeight, kMaxHeight-1);

			mCubeHeigts[0,i] = interp;

			if(interp <= 1)
			{
				cubeWater.transform.position = new Vector3 (0, interp, i);
				theCubes[0, i, interp] = Instantiate(cubeWater);
			} else {
				cube.transform.position = new Vector3 (0,interp,i);
				theCubes[0, i, interp] = Instantiate(cube);
			}

			//theCubes[0, i, interp] = Instantiate(cube);


			Vector3 p = new Vector3 (0,interp,i);
			p.z = p.z/43;
			p.y = p.y/43 + 4;
			p.z = p.z-5.7f;
			GameObject myCube = Instantiate(MinimapCube, p, Quaternion.identity) as GameObject;
			myCube.transform.parent = MinimapCanvas.transform;

			cubeCount++;
			
			int k = i%CurrentStepHeightFrequency;
			if(k == 0)
			{
				stepHeight += SetNewStepHeight();
				CurrentStepHeightFrequency = Random.Range(kStepHeightChangeFrequencyMin, kStepHeightChangeFrequencyMax);
			}
		}

		GenerateVariant(kTerrainDepth);
		GenerateFill(kTerrainDepth);
	}


	void Regenerate()
	{
		stepHeight = 0;
		interp = 0;
		cubeCount = 0;

		for(int j=0; j < kTerrainDepth; j++)
		{
			for(int i=0; i < kTerrainLength; i++)
			{
				for(int height=0; height<kMaxHeight; height++)
				{
					if(theCubes[j,i,height] != null)
						Destroy(theCubes[j,i,height]);
				}
			}
		}
		GenerateCubes();
	}



	void GenerateVariant(int depth)
	{
		float lastHeight = theCubes[0,0,mCubeHeigts[0,0]].transform.position.y;

		for(int currentDepth=1; currentDepth < depth; currentDepth++)
		{
			for(int i=0; i < kTerrainLength; i++)
			{
				Vector3 v = theCubes[0,i,mCubeHeigts[0,i]].transform.position;
				v.x = -currentDepth;
				if(v.y != lastHeight)
				{
					v.y = v.y + Random.Range(-1, 1);
					v.y = Mathf.CeilToInt(v.y);
					v.y = Mathf.Clamp(v.y, 0, kMaxHeight);
				}
				cube.transform.position = v;
				cubeWater.transform.position = v;

				mCubeHeigts[currentDepth,i] = (int)v.y;

				if(v.y <= 1)
				{
					theCubes[currentDepth, i,(int)v.y] = Instantiate(cubeWater);
				} else {
					theCubes[currentDepth, i,(int)v.y] = Instantiate(cube);
				}

				//lastHeight = mCubeHeigts[0,i];
				//unintended version below, but created interesting gullies...
				lastHeight = theCubes[currentDepth,i,(int)v.y].transform.position.y;

			}
		}
	}


	void GenerateFill(int depth)
	{
		for(int currentDepth=0; currentDepth < depth; currentDepth++)
		{
			for(int i=0; i < kTerrainLength; i++)
			{
				Vector3 v = theCubes[currentDepth,i,mCubeHeigts[currentDepth,i] ].transform.position;

				if(v.y > 0)
				{
					for(int height=0; height<v.y; height++)
					{
						Vector3 p = v;
						p.y = height;
						cube.transform.position = p;
						cubeWater.transform.position = p;
						if(height <= 1)
						{
							theCubes[currentDepth, i, height] = Instantiate(cubeWater);
						} else {
							theCubes[currentDepth, i, height] = Instantiate(cube);
						}
					}

				}

			}
		}
	}

	int SetNewStepHeight(){

		int pick = Random.Range(0,101);
		if(pick > kStepChance)
		{
			pick = 0;
			return pick;
		} else {
			pick = Random.Range(kStepHeightMin,kStepHeightMax);
			return pick;
		}
	}

}
 