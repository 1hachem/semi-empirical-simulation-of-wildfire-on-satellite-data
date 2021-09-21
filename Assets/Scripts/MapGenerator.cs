using UnityEngine;
using System.Collections;
using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;

public class MapGenerator : MonoBehaviour
{
	public const int mapChunkSize = 241;
	[Range(0, 6)]
	public int editorPreviewLOD;

	public float meshHeightMultiplier;
	public AnimationCurve meshHeightCurve;

	public bool autoUpdate;

	//public TerrainType[] regions;

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

		return new MapData(heightMap, vegetationMap);
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
	public float height;
	public Color colour;
}

public struct MapData
{
	public float[,] elevationMap;
	public int[,] vegetationMap;
	public Color[] colourMap;

	public MapData(float[,] elevationMap, int[,] vegetationMap)
	{
		this.elevationMap = elevationMap;
		this.vegetationMap = vegetationMap;

		this.colourMap = new Color[elevationMap.GetLength(0) * elevationMap.GetLength(1)];
		for (int i = 0, x = 0; x < elevationMap.GetLength(0); x++)
		{
			for (int y = 0; y < elevationMap.GetLength(1); y++)
			{
				switch(vegetationMap[x, y])
				{
					case 0:
						colourMap[i] = new Color(51f/255, 61f/255, 41f/255);
						break;
					
					case 1:
						colourMap[i] = new Color(147f/255, 102f/255, 57f/255);
						break;

					case 2:
						colourMap[i] = new Color(164f/255, 172f/255, 134f/255);
						break;

					case 3:
						colourMap[i] = new Color(194f/255, 197f/255, 170f/255);
						break;

					default:
						colourMap[i] = Color.white;
						break;
				}
				
				i++;
			}
		}
	}
}

