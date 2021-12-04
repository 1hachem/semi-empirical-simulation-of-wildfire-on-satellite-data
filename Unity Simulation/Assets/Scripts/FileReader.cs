using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public static class FileReader
{
    public static T[] Array1DFromFile<T>(string path, Vector2 dimensions)
    {
        T[] array = new T[(int)dimensions.x * (int)dimensions.y];

        StreamReader reader = new StreamReader(path); 
        string[] text = reader.ReadToEnd().Split();
        reader.Close();

        for(int i = 0, x = 0; x < dimensions.x; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                try
                {
                    array[i] = text[i].ChangeType<T>();
                }
                catch (Exception e)
                {
                    Debug.Log("Error at postion " + x.ToString() + ", " + y.ToString() + ". Path: " + path);
                    array[i] = (-1f).ChangeType<T>();
                }

                i++;
            }
        }

        return array;
    }
    
    public static T[,] Array2DFromFile<T>(string path, Vector2 dimensions)
    {
        T[,] array = new T[(int)dimensions.x, (int)dimensions.y];

        StreamReader reader = new StreamReader(path); 
        string[] text = reader.ReadToEnd().Split(' ', '\n');
        reader.Close();

        for(int i = 0, x = 0; x < dimensions.x; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                try
                {
                    array[x, y] = text[i].ChangeType<T>();
                }
                catch (Exception e)
                {
                    //Debug.Log("Error at postion " + x.ToString() + ", " + y.ToString() + ". Path: " + path);
                    array[x, y] = (-1f).ChangeType<T>();
                }

                i++;
            }
        }

        return array;
    }

    public static Color[] ColorsFromRGBFiles(string rPath, string gPath, string bPath, Vector2 dimensions)
    {
        float[,] rArray = Array2DFromFile<float>(rPath, dimensions);
        float[,] gArray = Array2DFromFile<float>(gPath, dimensions);
        float[,] bArray = Array2DFromFile<float>(bPath, dimensions);

        Color[] colors = new Color[(int)dimensions.x * (int)dimensions.y];

        for(int i = 0, x = 0; x < dimensions.x; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                
                colors[i] = new Color(rArray[x, y], gArray[x, y], bArray[x, y]);

                i++;
            }
        }

        return colors;
    }

    public static T ChangeType<T>(this object obj)
    {
        return (T)Convert.ChangeType(obj, typeof(T));
    }
}