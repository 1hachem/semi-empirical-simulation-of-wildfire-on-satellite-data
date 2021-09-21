using UnityEngine;
using UnityEditor;
using System;
using System.IO;

public static class FileReader
{
    public static float[] Array1DFromFile(string path, Vector2 dimensions)
    {
        float[] array = new float[(int)dimensions.x * (int)dimensions.y];

        StreamReader reader = new StreamReader(path); 
        string[] text = reader.ReadToEnd().Split();
        reader.Close();

        for(int i = 0, x = 0; x < dimensions.x; x++)
        {
            for(int y = 0; y < dimensions.y; y++)
            {
                try
                {
                    array[i] = float.Parse(text[i]);
                }
                catch (Exception e)
                {
                    //Debug.Log("Error at postion " + x.ToString() + ", " + y.ToString() + ". Path: " + path);
                    array[i] = -1;
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

    public static T ChangeType<T>(this object obj)
    {
        return (T)Convert.ChangeType(obj, typeof(T));
    }
}