#if false
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using  System.IO;
//using  System.IO.Stream;
using  System.Runtime.Serialization;
using  System.Runtime.Serialization.Formatters.Binary;

public class SL_TileReader : MonoBehaviour {
	
	public string FileName;
	public Color DefaultHexColor;
	public float Amplitude;

	//save data
	public static List<SL_Tile> 	Tiles 				= new List<SL_Tile>();
	public static List<Vector3S> 	TileVertices 		= new List<Vector3S>();
	public static List<Vector3S> 	TileNormals 		= new List<Vector3S>();
	public static List<GameObject> 	TileGameObjects 	= new List<GameObject>();

	public static int[] HexIndices 			= new int[] {0,1,2, 0,2,3, 0,3,4, 0,4,5, 0,5,6, 0,6,1};
	public static int[] PentIndices 		= new int[] {0,1,2, 0,2,3, 0,3,4, 0,4,5, 0,5,1};

	//added tile data
	public static Material[]		TileMaterials;
	public static int[]				Population;	
	public static int[]				LastPopulation;
	public static Color[]			HexColors;		//used by TilePicker as "current color of tile"
	public static Color[]			NextHexColors;	//used by tilePicker, if different from "hexcolors" then it updates
	public static Color[]			GroundColors; 	//the tile colors from a material 

	public static Color[]			MeshColors;		//the tile colors, in vertice form
	public static Color[]			SecondMeshColors; // same for the 'secondMesh'
	public static Vector2S[]		SL_UVs;		//the uv co-ordinates for tiles
	public static Vector2S[]		SL_MeshUVs;	//the uv co-ordinates for each vert

	private Material				mat;
	private Material 				mat2;
	private CombineInstance[] 		combine;
	private CombineInstance[]		combine2;
	private BoneWeight[]			boneWeights;
	private int						boneCount;
	public 	static Mesh				theMesh;			//the mesh used for the mesh collider
	public 	static Mesh				theDisplayMesh;		//the mesh used for the mesh renderer
	public 	static Mesh				theSecondMesh;		//the mesh used for the 'secondmesh;

	public static bool[]			Selected;			//if tile is clicked on it is set to true in tilePicker
	public static bool[]			LastSelected;		//yadda yadda
	public bool 					writeUvMap; //when true, UV co-ordinates are created for the current subdivision selected

	// Use this for initialization
	void Start () {

		Tiles 			= LoadTiles(FileName);
		TileVertices 	= LoadVertices(FileName + "Vertices");
		TileNormals 	= LoadNormals(FileName + "Normals");
		SL_UVs 			= LoadUVmap(FileName + "_uv_map");


		combine = new CombineInstance[Tiles.Count];
		combine2 = new CombineInstance[Tiles.Count];

		boneWeights = new BoneWeight[TileVertices.Count];
		MeshColors = new Color[TileVertices.Count];
		SecondMeshColors = new Color[TileVertices.Count];

		boneCount = 0;
		theMesh = new Mesh();
		theDisplayMesh = new Mesh();
		theSecondMesh = new Mesh();

		TileMaterials 	= new Material[Tiles.Count];
		Population 		= new int[Tiles.Count];
		LastPopulation 	= new int [Tiles.Count];
		HexColors 		= new Color[Tiles.Count];
		NextHexColors 	= new Color[Tiles.Count];
		GroundColors 	= new Color[Tiles.Count];
		Selected		= new bool[Tiles.Count];
		LastSelected	= new bool[Tiles.Count];


		mat = new Material(Shader.Find("TerraViz/Earth-NightLights_Bill"));
		mat2 = new Material(Shader.Find("VertexAlpha"));

		int i;
		for(i = 0; i <  Tiles.Count; i++){
			DrawHexTile(i);
			Population[i] = 0;
			LastPopulation[i] = 0;
			Selected[i] = false;
			LastSelected[i] = false;
		}

		MakeSphere();
		MakeSphere2();
	}


	public void MakeSphere()
	{
		//combine all the hex meshes and assign to single game object 
		TileGameObjects.Add(new GameObject("HexSphere"));
		TileGameObjects[TileGameObjects.Count - 1].AddComponent("MeshRenderer");
		TileGameObjects[TileGameObjects.Count - 1].AddComponent("MeshFilter");

		MeshCollider meshc = TileGameObjects[TileGameObjects.Count - 1].AddComponent("MeshCollider") as MeshCollider;

		theMesh.CombineMeshes(combine, false, false);
		theMesh.boneWeights = boneWeights;
		meshc.sharedMesh = theMesh;

		theDisplayMesh.CombineMeshes(combine, true, false);
		theDisplayMesh.boneWeights = boneWeights;

		if(writeUvMap) { 
			SaveFile ("/Resources/" + FileName + "_uv_map.bytes" , SL_UVs);
		} else {
			int count = 0;
	
			GroundColors = GetColors2D("april");

			Vector2[] uv = new Vector2[TileVertices.Count];

			foreach(Color c in GroundColors)
			{
				int[] sub_triangles = theMesh.GetTriangles(count);
				foreach (int v in sub_triangles)
				{
					MeshColors[v] = c;
					uv[v].x = SL_UVs[count].x; //only works becuase we've called GetColors2D, which loads up SL_UVs
					uv[v].y = SL_UVs[count].y;
				}
				HexColors[count] = c;
				NextHexColors[count] = c;
				count++;
			}
			theDisplayMesh.colors = MeshColors;
			theDisplayMesh.uv = uv;
		}

		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshFilter>().sharedMesh = theDisplayMesh;
		TileGameObjects[TileGameObjects.Count - 1].transform.parent = GameObject.Find("_TILES").transform;
		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshRenderer>().sharedMaterial = mat;
		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshRenderer>().renderer.castShadows = false;
		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshRenderer>().renderer.receiveShadows = false;
		TileGameObjects[TileGameObjects.Count - 1].layer = 8;
		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshRenderer>().enabled = true;
		TileGameObjects[TileGameObjects.Count - 1].isStatic = true;
	}

	public void MakeSphere2()
	{
		//combine all the hex meshes and assign to single game object 
		TileGameObjects.Add(new GameObject("HexSphere2"));
		TileGameObjects[TileGameObjects.Count - 1].AddComponent("MeshRenderer");
		TileGameObjects[TileGameObjects.Count - 1].AddComponent("MeshFilter");

		theSecondMesh.CombineMeshes(combine2, true, false);
		int count = 0;
		foreach(Vector3 v in theSecondMesh.vertices)
		{
			SecondMeshColors[count] = new Color(1.0f,1.0f,1.0f,0.0f);
			count +=1;
		}
		theSecondMesh.colors = SecondMeshColors;

		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshFilter>().sharedMesh = theSecondMesh;
		TileGameObjects[TileGameObjects.Count - 1].transform.parent = GameObject.Find("_TILES").transform;
		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshRenderer>().sharedMaterial = mat2;
		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshRenderer>().renderer.castShadows = false;
		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshRenderer>().renderer.receiveShadows = false;
		TileGameObjects[TileGameObjects.Count - 1].layer = 13;
		TileGameObjects[TileGameObjects.Count - 1].GetComponent<MeshRenderer>().enabled = true;
		TileGameObjects[TileGameObjects.Count - 1].isStatic = true;
	}


	public Color[] GetColors2D(string textureFileName)
	{
		Texture2D tex = (Texture2D)Resources.Load(textureFileName);

		Color[] theColors = new Color[SL_UVs.Length];

		int count = 0;
		foreach(Vector2S v in SL_UVs)
		{
			Vector2 pixelUV = new Vector2();
			pixelUV.x = v.x;
			pixelUV.y = v.y;
			pixelUV.x *= tex.width;
			pixelUV.y *= tex.height;
			
			theColors[count] = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
			theColors[count].a = Amplitude;
			count++;
		}
		return theColors;
	}

	public void DrawHexTile(int index)
	{
		if( Tiles.Count <= index){
			return;
		}
		
		SL_Tile tile = Tiles[index];

		var vNorm_CONV = new Vector3(TileNormals[index].x,TileNormals[index].y,TileNormals[index].z);

		var ext = vNorm_CONV * 0.04f;
		var ext2 = vNorm_CONV * 1.041f;

		Mesh newMesh = new Mesh ();
		Vector3[] 	theMeshVerts;
		Vector2[] 	theMeshUVs;
		Vector3[]	theMeshNormals;

		Mesh SecondMesh = new Mesh();
		Vector3[]	SecondMeshVerts;

		if(NumNeighbors(tile) == 6)
		{
			theMeshVerts = new Vector3[7];
			theMeshUVs = new Vector2[7];
			theMeshNormals = new Vector3[7];
			SecondMeshVerts = new Vector3[7];


			for(int i = 0; i < 7; ++i)
			{
				Vector3 v2_conv = new Vector3(TileVertices[ tile.Vertices[i] ].x, TileVertices[ tile.Vertices[i] ].y, TileVertices[ tile.Vertices[i] ].z);
				theMeshVerts[i] = v2_conv + ext;
				theMeshNormals[i] = vNorm_CONV;


				Vector3 scaled = (v2_conv - vNorm_CONV);
				scaled.x = scaled.x * 0.5f;
				scaled.y = scaled.y * 0.5f;
				scaled.z = scaled.z * 0.5f;
				scaled = scaled + ext2;
				SecondMeshVerts[i] = scaled;

				//using boneweights to contain tile info
				//in TilePicker retrieve this to get submesh index
				boneWeights[boneCount].boneIndex0 = 6; //numNeighbors
				boneWeights[boneCount].weight0 = (float)index; //tile index
				boneWeights[boneCount].boneIndex1 = tile.Neighbors[0]; //neighbor indexes - not used
				boneWeights[boneCount].weight1 = (float)tile.Neighbors[1];
				boneWeights[boneCount].boneIndex2 = tile.Neighbors[2];
				boneWeights[boneCount].weight2 = (float)tile.Neighbors[3];
				boneWeights[boneCount].boneIndex3 = tile.Neighbors[4];
				boneWeights[boneCount].weight3 = (float)tile.Neighbors[5];
				boneCount++;

			}
			newMesh.vertices = theMeshVerts;
			newMesh.triangles = HexIndices; 
			newMesh.normals = theMeshNormals;
			//newMesh.RecalculateNormals();                               
			//newMesh.RecalculateBounds();
			newMesh.Optimize(); 

			SecondMesh.vertices = SecondMeshVerts;
			SecondMesh.uv = theMeshUVs;
			SecondMesh.triangles = HexIndices; 
			//SecondMesh.RecalculateNormals();                               
			SecondMesh.RecalculateBounds();
			SecondMesh.Optimize(); 

		} else {
			theMeshVerts = new Vector3[6];
			theMeshUVs 	= new Vector2[6];
			theMeshNormals = new Vector3[6];
			SecondMeshVerts = new Vector3[6];

			for(int i = 0; i < 6; ++i)
			{
				Vector3 v2_conv = new Vector3(TileVertices[ tile.Vertices[i] ].x, TileVertices[ tile.Vertices[i] ].y, TileVertices[ tile.Vertices[i] ].z);
				theMeshVerts[i] = v2_conv + ext;
				theMeshNormals[i] = vNorm_CONV;

				Vector3 scaled = (v2_conv - vNorm_CONV);
				scaled.x = scaled.x * 0.5f;
				scaled.y = scaled.y * 0.5f;
				scaled.z = scaled.z * 0.5f;
				scaled = scaled + ext2;
				SecondMeshVerts[i] = scaled;

				//using boneweights to contain tile info
				//in TilePicker retrieve this to get submesh index
				boneWeights[boneCount].boneIndex0 = 5;
				boneWeights[boneCount].weight0 = (float)index;
				boneWeights[boneCount].boneIndex1 = tile.Neighbors[0]; //neighbor indexes - not used
				boneWeights[boneCount].weight1 = (float)tile.Neighbors[1];
				boneWeights[boneCount].boneIndex2 = tile.Neighbors[2];
				boneWeights[boneCount].weight2 = (float)tile.Neighbors[3];
				boneWeights[boneCount].boneIndex3 = tile.Neighbors[4];
				boneWeights[boneCount].weight3 = -1.0f;
				boneCount++;
			}
			newMesh.vertices = theMeshVerts;
			newMesh.normals = theMeshNormals;
			newMesh.triangles = PentIndices; 

			//newMesh.RecalculateNormals();                               
			//newMesh.RecalculateBounds();
			newMesh.Optimize();

			SecondMesh.vertices = SecondMeshVerts;
			SecondMesh.uv = theMeshUVs;
			SecondMesh.triangles = PentIndices; 
			SecondMesh.RecalculateNormals();                               
			//SecondMesh.RecalculateBounds();
			SecondMesh.Optimize();
		}


		if(writeUvMap)
		{
			//color the hex tiles based on planet texture
			//cast ray to 0,0,0 and get the planet material color at the collision
			RaycastHit theHit;

			//Color[] meshColors = new Color[theMeshVerts.Length];
			//Color[] secondMeshColors = new Color[theMeshVerts.Length];

			int count=0;
			foreach(int tileVert in tile.Vertices)
			{
				Vector3S v3s = TileVertices[ tile.Vertices[count] ];
				Vector3 v3 = new Vector3(v3s.x,v3s.y,v3s.z);

				int layerMask = 1 << 11; //only cast rays against layer 11
				if(Physics.Linecast(v3 * 2, Vector3.zero, out theHit, layerMask))
				{
					var td = theHit.collider.gameObject.name;
					if(td == "Planet")
					{
						Renderer renderer = theHit.collider.renderer;
						//Texture2D tex = renderer.material.GetTexture("_difColor") as Texture2D;
						Texture2D tex = renderer.material.GetTexture("_MainTex") as Texture2D;
						Vector2 pixelUV = theHit.textureCoord;
						pixelUV.x *= tex.width;
						pixelUV.y *= tex.height;

						theMeshUVs[count] = pixelUV;

						if(count == 0)
						{
							SL_UVs[index] = new Vector2S();
							SL_UVs[index].x = pixelUV.x;
							SL_UVs[index].y = pixelUV.y;

							/*
							GroundColors[index] = tex.GetPixel((int)pixelUV.x, (int)pixelUV.y);
							GroundColors[index].a = Amplitude;
							HexColors[index] = GroundColors[index];
							NextHexColors[index] = GroundColors[index];

							/*
							for(int i=0; i < theMeshVerts.Length; i++)
							{
								meshColors[i] = GroundColors[index];
								secondMeshColors[i] = GroundColors[index];
							}*/
						} 
					} 
				}
				count++;
			}
			//newMesh.colors = meshColors;
		}
		combine[index].mesh = newMesh;
		combine2[index].mesh = SecondMesh;
	}


    public static int NumNeighbors( SL_Tile tile )
	{
		int result = 0;
		
		for (int i = 5; i != 0; i--)
		{
			if (tile.Neighbors[i] != -1)
			{
				result = i + 1;
				break;
			}
		}
		return result;
	}


	
	public List<SL_Tile> LoadTiles(string filename) {
		TextAsset ta = Resources.Load(filename) as TextAsset;
		Stream s = new MemoryStream(ta.bytes);
		BinaryFormatter formatter = new BinaryFormatter();
		var obj = (List<SL_Tile>)formatter.Deserialize(s);
		s.Close();
		return obj;
	}

	public List<Vector3S> LoadVertices(string filename) {
		TextAsset ta = Resources.Load(filename) as TextAsset;
		Stream s = new MemoryStream(ta.bytes);
		BinaryFormatter formatter = new BinaryFormatter();
		var obj = (List<Vector3S>)formatter.Deserialize(s);
		s.Close();
		return obj;
	}

	public List<Vector3S> LoadNormals(string filename) {
		TextAsset ta = Resources.Load(filename) as TextAsset;
		Stream s = new MemoryStream(ta.bytes);
		BinaryFormatter formatter = new BinaryFormatter();
		var obj = (List<Vector3S>)formatter.Deserialize(s);
		s.Close();
		return obj;
	}

	public Vector2S[] LoadUVmap(string filename) {
		TextAsset ta = Resources.Load(filename) as TextAsset;
		Stream s = new MemoryStream(ta.bytes);
		BinaryFormatter formatter = new BinaryFormatter();
		var obj = (Vector2S[])formatter.Deserialize(s);
		s.Close();
		return obj;
	}

	public void SaveFile(string filename, object obj) {
		Stream fileStream = File.Open (Application.dataPath + filename, FileMode.Create);
		BinaryFormatter formatter = new BinaryFormatter();
		formatter.Serialize(fileStream, obj);
		fileStream.Close();
	}

}
#endif