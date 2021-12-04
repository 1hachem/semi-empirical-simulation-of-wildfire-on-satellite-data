using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
	public const int mapChunkSize = 483;
	[Range(0, 6)]
	public int editorPreviewLOD;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	public TerrainType[] regions;

	public Vector2 mapPos;

	Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
	Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();

	public void DrawMapInEditor()
	{
		MapData mapData = GenerateMapData(mapPos);

		MapDisplay display = FindObjectOfType<MapDisplay>();
		display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.elevationMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
	}

	public void RequestMapData(Vector2 centre, Action<MapData> callback)
	{
		ThreadStart threadStart = delegate {
			MapDataThread(centre, callback);
		};

		new Thread(threadStart).Start();
	}

	void MapDataThread(Vector2 centre, Action<MapData> callback)
	{
		MapData mapData = GenerateMapData(centre);
		lock (mapDataThreadInfoQueue)
		{
			mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
		}
	}

	public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
	{
		ThreadStart threadStart = delegate {
			MeshDataThread(mapData, lod, callback);
		};

		new Thread(threadStart).Start();
	}

	void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
	{
		MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.elevationMap, meshHeightMultiplier, meshHeightCurve, lod);
		lock (meshDataThreadInfoQueue)
		{
			meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
		}
	}

	void Update()
	{
		if (mapDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}

		if (meshDataThreadInfoQueue.Count > 0)
		{
			for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
			{
				MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
				threadInfo.callback(threadInfo.parameter);
			}
		}	
	}

	MapData GenerateMapData(Vector2 centre)
	{
		string heights_path = "Assets/Resources/elevation/" + centre.x.ToString() 
															+ " " + centre.y.ToString() + ".txt";
		if (!System.IO.File.Exists(heights_path))
			heights_path = "Assets/Resources/elevation/DEFAULT.txt";
		float[,] heightMap = FileReader.Array2DFromFile<float>(heights_path, new Vector2(mapChunkSize, mapChunkSize));


		string vegetation_path = "Assets/Resources/vegetation/" + centre.x.ToString() + " " 
														+ centre.y.ToString() + ".txt";
		if (!System.IO.File.Exists(vegetation_path))
			vegetation_path = "Assets/Resources/vegetation/DEFAULT.txt";
		int[,] vegetationMap = FileReader.Array2DFromFile<int>(vegetation_path, new Vector2(mapChunkSize, mapChunkSize));


		string moisture_path = "Assets/Resources/moisture/" + centre.x.ToString() 
															 + " " + centre.y.ToString() + ".txt";
		if (!System.IO.File.Exists(moisture_path))
			moisture_path = "Assets/Resources/moisture/DEFAULT.txt";
		float[,] moistureMap = FileReader.Array2DFromFile<float>(moisture_path, new Vector2(mapChunkSize, mapChunkSize));


		string wind_x_path = "Assets/Resources/wind_x/" + centre.x.ToString() 
															 + " " + centre.y.ToString() + ".txt";
		if (!System.IO.File.Exists(wind_x_path))
			wind_x_path = "Assets/Resources/wind_x/DEFAULT.txt";
		float[,] windXMap = FileReader.Array2DFromFile<float>(wind_x_path, new Vector2(mapChunkSize, mapChunkSize));


		string wind_z_path = "Assets/Resources/wind_z/" + centre.x.ToString() 
															 + " " + centre.y.ToString() + ".txt";
		if (!System.IO.File.Exists(wind_z_path))
			wind_z_path = "Assets/Resources/wind_z/DEFAULT.txt";
		float[,] windZMap = FileReader.Array2DFromFile<float>(wind_z_path, new Vector2(mapChunkSize, mapChunkSize));

		string r_path = "Assets/Resources/RGB/R/" + centre.x.ToString() 
															+ " " + centre.y.ToString() + ".txt";
		if (!System.IO.File.Exists(r_path))
			r_path = "Assets/Resources/RGB/R/DEFAULT.txt";

		string g_path = "Assets/Resources/RGB/G/" + centre.x.ToString() 
															+ " " + centre.y.ToString() + ".txt";
		if (!System.IO.File.Exists(g_path))
			g_path = "Assets/Resources/RGB/G/DEFAULT.txt";

		string b_path = "Assets/Resources/RGB/B/" + centre.x.ToString() 
															+ " " + centre.y.ToString() + ".txt";
		if (!System.IO.File.Exists(b_path))
			b_path = "Assets/Resources/RGB/B/DEFAULT.txt";

		Color[] rgbMap = FileReader.ColorsFromRGBFiles(r_path, g_path, b_path, new Vector2(mapChunkSize, mapChunkSize));

		return new MapData(heightMap, vegetationMap, moistureMap, windXMap, windZMap, rgbMap, regions);
	}

	struct MapThreadInfo<T>
	{
		public readonly Action<T> callback;
		public readonly T parameter;

		public MapThreadInfo(Action<T> callback, T parameter)
		{
			this.callback = callback;
			this.parameter = parameter;
		}

	}

}

[System.Serializable]
public struct TerrainType
{
	public string name;
	public int cluster_number;
	public Color colour;
}

public struct MapData
{
	public float[,] elevationMap;

	public int[,] vegetationMap;

	public float[,] moistureMap;

	public float[,] windXMap;
	public float[,] windZMap;
	
	public Color[] rgbMap;
	public Color[] colourMap;
	TerrainType[] regions;

	bool toggleColor;

	public MapData(float[,] elevationMap, int[,] vegetationMap, float[,] moistureMap, 
				   float[,] windXMap, float[,] windZMap, Color[] rgbMap, TerrainType[] regions)
	{
		this.elevationMap = elevationMap;
		this.vegetationMap = vegetationMap;
		this.moistureMap = moistureMap;
		this.windXMap = windXMap;
		this.windZMap = windZMap;
		this.rgbMap = rgbMap;
		this.colourMap = rgbMap;
		this.regions = regions;

		toggleColor = true;
	}

	public void SwitchColor()
	{
		if (toggleColor)
		{
			this.colourMap = new Color[elevationMap.GetLength(0) * elevationMap.GetLength(1)];
			for (int i = 0, x = 0; x < elevationMap.GetLength(0); x++)
			{
				for (int y = 0; y < elevationMap.GetLength(1); y++)
				{
					colourMap[i] = Color.white;
					foreach (var r in this.regions)
					{
						if (vegetationMap[x, y] == r.cluster_number)
							colourMap[i] = r.colour;
					}
					
					i++;
				}
			}

			toggleColor = false;
		}

		else
		{
			this.colourMap = this.rgbMap;

			toggleColor = true;
		}
	}
}

