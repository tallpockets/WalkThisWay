#if false
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class SL_TilePicker : MonoBehaviour {

	public float FlipTransparency = 0;
	private float angle = 0.0f;
	//the update loop
	
	public bool HexViewEnabled = false;
	public Color MainHexColor;

	public Color SelectedHexColor;

	public Color MainHexSpecColor;
	public float Amplitude = 0.8f;

	public bool WantWaves = false;
	public Color WaveColor;
	public int WaveTime = 100;
	public int WaveUpdateRate = 1;

	public bool WantLife = false;
	private int LifeHex;
	public Color LifeColor;
	
	public GameObject Atmos; 
	
	public bool tilesEnabled = false;
	
	public AudioSource warpOn;
	public AudioSource warpOff;
	
	public int lastPickedHex;
	public bool SetTileColors = false;
	
	
	public int Time = 0;
	public int WaveCenter = -1;
	public int wavenum = 0;

	private bool clicked = false;
	public GameObject BioMetricA;
	public GameObject BioMetricB;
	public GameObject BioMetricC;
	public GameObject BioMetricD;
	public GameObject BioMetricE;
	public GameObject Cam;

	private GameObject[] theSelection = new GameObject[2];
	private int	selectionCount = 0;

	private spin2 	theSpin2; 
	private int		updateCounter;

	private int	touchType, lastTouchType;


	// Use this for initialization
	void Start () {
		GameObject gameObject=(GameObject.Find ("MainGameObject"));
		theSpin2=(spin2)(gameObject.GetComponent("spin2"));
		updateCounter = 0;
		touchType = 0;
		lastTouchType = 0;
	}

	public void TouchTypeSelect()
	{
		touchType = 0;
	}
	public void TouchTypeWaves()
	{
		touchType = 1;
	}
	public void TouchTypeLife()
	{
		touchType = 2;
	}

	public void updateTilesVisibility()
	{
		bool change = false;
		bool popChange = false;
		for(int i = 0; i < SL_TileReader.Tiles.Count; i++)
		{
			if(SL_TileReader.Population[i] != SL_TileReader.LastPopulation[i])
			{
				popChange = true;

				int[] sub_triangles = SL_TileReader.theMesh.GetTriangles(i);	
				for(int k=0; k < sub_triangles.Length; k++)
				{
					Color c = LifeColor;
					float p = (float)SL_TileReader.Population[i]/1000.0f;
					c.a = p;
					c.a = c.a * 0.3f;
					SL_TileReader.SecondMeshColors[sub_triangles[k]] = c;
				}

				SL_TileReader.LastPopulation[i] = SL_TileReader.Population[i];
			}


			if((Color)SL_TileReader.HexColors[i] != (Color)SL_TileReader.NextHexColors[i])
			{

				change = true;
				int[] sub_triangles = SL_TileReader.theMesh.GetTriangles(i);

				for(int k=0; k < sub_triangles.Length; k++)
				{
					SL_TileReader.MeshColors[sub_triangles[k]] = SL_TileReader.NextHexColors[i];
				}
				SL_TileReader.HexColors[i] = SL_TileReader.NextHexColors[i];
			} 

			//draw selected hex
			if(SL_TileReader.Selected[i] != SL_TileReader.LastSelected[i] && SL_TileReader.Selected[i] )
			{
				Vector3[] vertices = SL_TileReader.theDisplayMesh.vertices;
				Vector2[] uvs = SL_TileReader.theDisplayMesh.uv;
				int[] sub_triangles = SL_TileReader.theMesh.GetTriangles(i);

				List<int> vertNums = new List<int>();

				for(int j=0; j < sub_triangles.Length; j++)
				{
					vertNums.Add(sub_triangles[j]);
				}

				List<int> noDupes = vertNums.Distinct().ToList();

				Object.Destroy(theSelection[selectionCount], 0.0f);

				selectionCount += 1;
				selectionCount = selectionCount%2;

				//code to make a mesh shaped particle
				theSelection[selectionCount] = (GameObject)Instantiate(Resources.Load("SelectFade"));
				ParticleSystemRenderer pr = theSelection[selectionCount].GetComponent<ParticleSystemRenderer>();
				theSelection[selectionCount].transform.position = vertices[noDupes[0]];
				
				Mesh theMesh = new Mesh();
				Vector3[] theMeshVerts 	= new Vector3[noDupes.Count];
				Vector2[] theMeshUVs 	= new Vector2[noDupes.Count];
				Color[] theMeshColors	= new Color[noDupes.Count];

				for(int k=0; k < noDupes.Count; k++)
				{
					var vNorm_CONV = new Vector3(SL_TileReader.TileNormals[i].x,SL_TileReader.TileNormals[i].y,SL_TileReader.TileNormals[i].z);
					var ext = vNorm_CONV * 1.04f;

					theMeshVerts[k] = vertices[noDupes[k]] - ext;
					theMeshUVs[k]	= uvs[noDupes[k]];
					theMeshColors[k] = Color.white;

				}
				theMesh.vertices = theMeshVerts;
				theMesh.uv = theMeshUVs;

				if(noDupes.Count == 7)
				{
					theMesh.triangles = SL_TileReader.HexIndices;
				} else {
					theMesh.triangles = SL_TileReader.PentIndices;
				}

				theMesh.colors = theMeshColors;
				theMesh.RecalculateNormals();                               
				theMesh.RecalculateBounds();
				theMesh.Optimize();

				pr.mesh = theMesh;

				SL_TileReader.LastSelected[i] = SL_TileReader.Selected[i]; 
			} 
		}


		if(change)
			SL_TileReader.theDisplayMesh.colors = SL_TileReader.MeshColors;

		if(popChange)
			SL_TileReader.theSecondMesh.colors = SL_TileReader.SecondMeshColors;
	}


	
	// Update is called once per frame
	void Update () {

		updateTilesVisibility();

		if (WantWaves && SL_TileReader.Tiles.Count > 0)
		{
			MakeWaves(wavenum);
		} 

		if (WantLife && SL_TileReader.Tiles.Count > 0)
		{
			MakeLife();
		} 

		updateCounter += 1;


		if(Input.GetMouseButtonDown(0))
		{
			// Only if we hit something, do we continue 
			RaycastHit hit; 
			
			if (!Physics.Raycast (Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
				return; 
			
			var td = hit.collider.gameObject.name;
			
			MeshCollider meshCollider = hit.collider as MeshCollider; 
			if (meshCollider == null || meshCollider.sharedMesh == null) 
				return; 
			
			if(td != "HexSphere")
				return;
			
			Mesh mesh = meshCollider.sharedMesh;
			BoneWeight[] bw	= mesh.boneWeights; //boneweights hold tile info
			int[] triangles = mesh.triangles;	//get all collider triangles
			int subMeshNum = (int)bw[triangles[hit.triangleIndex * 3 + 0]].weight0; //weight0 holds tile index (submesh index)
			int num = subMeshNum;

			clicked = true;

			if (touchType == 0 && touchType != lastTouchType)
			{
				WantWaves = false;
				WantLife = false;
				resetTiles();
			} else if (touchType == 1){
				if(WantWaves == false)
				{
					WantWaves = true;
					wavenum = num;
				} else {
					resetTiles();
					Time = 0;
					wavenum = num;
				}
			} else if (touchType == 2) {
				if(WantLife == false)
				{
					WantLife = true;
					SL_TileReader.Population[num] = 10;
				} else {
					WantLife = false;
					resetTiles();
				}
			}

			if(num < SL_TileReader.Tiles.Count && num >= 0 && clicked)
			{
				if(num != lastPickedHex)
				{
					SL_TileReader.NextHexColors[num] = SelectedHexColor;
					SL_TileReader.Selected[num] = true;
					
					SL_TileReader.NextHexColors[lastPickedHex] = SL_TileReader.GroundColors[lastPickedHex];
					SL_TileReader.Selected[lastPickedHex] = false;
					
					//set slider values
					UISlider A = BioMetricA.GetComponent<UISlider>() as UISlider;
					UISlider B = BioMetricB.GetComponent<UISlider>() as UISlider;
					UISlider C = BioMetricC.GetComponent<UISlider>() as UISlider;
					UISlider D = BioMetricD.GetComponent<UISlider>() as UISlider;
					UISlider E = BioMetricE.GetComponent<UISlider>() as UISlider;
					
					A.sliderValue = SL_TileReader.GroundColors[num].r;
					B.sliderValue = SL_TileReader.GroundColors[num].g;
					C.sliderValue = SL_TileReader.GroundColors[num].b;
					D.sliderValue = (float)Random.Range(0.0f,1.0f);
					E.sliderValue = (float)Random.Range(0.0f,1.0f);
					
					//move camera to center on selected hex
					/*
				Vector3 vNorm_CONV = new Vector3(SL_TileReader.TileNormals[num].x,SL_TileReader.TileNormals[num].y,SL_TileReader.TileNormals[num].z);
				Vector3 ext = vNorm_CONV * 1.6f;
				Vector3 v3 = new Vector3();
				v3 = mesh.vertices[triangles[hit.triangleIndex * 3 + 0]];
				v3 = v3 + ext;

				Cam.transform.position = v3;
				Cam.transform.LookAt(Vector3.zero);
				*/
				}
				
				lastPickedHex = num;
				clicked = false;
			}

			if(Input.GetKeyDown("1"))
			{
				if(WantWaves == false)
				{
					WantWaves = true;
					wavenum = num;
				} else {
					resetTiles();
					Time = 0;
					wavenum = num;
				}
			}
			
			if(Input.GetKeyDown("2"))
			{
				if(WantLife == false)
				{
					WantLife = true;
					SL_TileReader.Population[num] = 10;
				} else {
					WantLife = false;
					resetTiles();
				}
			}

		}

		lastTouchType = touchType;

		if(Input.GetMouseButtonUp(0))
		{
			clicked = false;
		}

	}

	private void resetTiles()
	{
		for(int i = 0; i < SL_TileReader.Tiles.Count; i++)
		{
			var tile = SL_TileReader.Tiles[i];
			SL_TileReader.Population[i] = 0;
			SL_TileReader.LastPopulation[i] = -1;
			SL_TileReader.NextHexColors[i] = SL_TileReader.GroundColors[i];
			tile.SimState = 0;
			tile.NextSimState = 0;
		}
	}


	public void MakeWaves(int HexNum)
	{
		int i;
		int j;
		SL_Tile tile;
		SL_Tile neighbor;
		int numNeighbors;
		
		tile = SL_TileReader.Tiles[HexNum];
		tile.SimState = 1;
		WaveCenter = HexNum;
		

		if(WaveUpdateRate < 1) //can't go faster than this. 
			WaveUpdateRate = 1;

		if ((Time % WaveUpdateRate) == 0)
		{
			
			for (i = 0; i < SL_TileReader.Tiles.Count; i++)
			{
				tile = SL_TileReader.Tiles[i];
				tile.NextSimState = tile.SimState;
				
				
				numNeighbors = SL_TileReader.NumNeighbors( tile );
				if (tile.SimState == 0 || tile.SimState == 1)
				{
					
					for (j = 1; j < numNeighbors; j++)
					{
						neighbor = SL_TileReader.Tiles[ tile.Neighbors[j] ];
						if (neighbor.SimState == 1)
						{
							tile.NextSimState = tile.SimState + 1;
							break;
						}
					}
				}
				else if (tile.SimState == 2)
				{
					for (j = 1; j < numNeighbors; j++)
					{
						neighbor = SL_TileReader.Tiles[ tile.Neighbors[j] ];
						if (neighbor.SimState == 1)
							break;
					}
					if (j == numNeighbors)
						tile.NextSimState = 0;
				}
			}
			

			for (i = 0; i < SL_TileReader.Tiles.Count; i++)
			{
				tile = SL_TileReader.Tiles[i];

				if(tile.SimState != tile.NextSimState)
				{
					tile.SimState = tile.NextSimState;
					if (i != WaveCenter)
					{
						if (tile.SimState == 1)
						{	
							SL_TileReader.NextHexColors[i] = WaveColor;
						} else {
							SL_TileReader.NextHexColors[i] = SL_TileReader.GroundColors[i];
						}
					}
				}
			}
		}
		
		Time++;
		if(Time > WaveTime)
		{
			WantWaves = false;
			Time = 0;
			resetTiles();
		}
	}


	public void MakeLife()
	{
		int i;
		int j;
		SL_Tile tile;
		SL_Tile neighbor;
		int numNeighbors;
	
		
		if ((Time % 1) == 0)
		{
			
			for (i = 0; i < SL_TileReader.Tiles.Count; i++)
			{
				if(SL_TileReader.Population[i] > 9)
				{
					bool success = false;
					int t = Random.Range(0, 100);
					bool land = true;
					if(SL_TileReader.GroundColors[i].r < 0.4f && SL_TileReader.GroundColors[i].b > 0.3)
					{
						land = false;
					}


					if(t < 4 && land)
					{
						success = true;
					} else  {
						success = false;
					}

					if(success)
					{
						float result = 0;
						result = Random.Range(0, (float)SL_TileReader.Population[i]/2.0f);
						SL_TileReader.Population[i] += (int)result;

						if(SL_TileReader.Population[i] > 1000)
							SL_TileReader.Population[i] = 1000;

						SL_Tile theTile = SL_TileReader.Tiles[i];
						numNeighbors = SL_TileReader.NumNeighbors(theTile);
						for (j = 1; j < numNeighbors; j++)
						{
							float nresult = 0;
							nresult = Random.Range(0, (float)SL_TileReader.Population[i]/2.0f);
							SL_TileReader.Population[theTile.Neighbors[j]] += (int)nresult;

							if(SL_TileReader.Population[theTile.Neighbors[j]] > 1000)
								SL_TileReader.Population[theTile.Neighbors[j]] = 1000;
						}
					}
				}
			}



			/*
			for (i = 0; i < SL_TileReader.Tiles.Count; i++)
			{
				if(SL_TileReader.Population[i] != SL_TileReader.LastPopulation[i])
				{
					Color pop = new Color(0,(float)SL_TileReader.Population[i]/1000.0f,0, 0.3f);
					SL_TileReader.NextHexColors[i] = pop;

					if(SL_TileReader.Population[i] == 0)
					{
						SL_TileReader.NextHexColors[i] = SL_TileReader.GroundColors[i];
					}
				}
				SL_TileReader.LastPopulation[i] = SL_TileReader.Population[i];

			}*/
		}
		Time++;

	}

}
#endif

