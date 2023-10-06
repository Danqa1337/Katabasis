using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class BinarySerializer
{
    public static T MakeDeepClone<T>(T data) where T : class
    {
        var ms = new MemoryStream();
        var binaryFormatter = new BinaryFormatter();

        binaryFormatter.Serialize(ms, data);
        ms.Position = 0;
        var copy = binaryFormatter.Deserialize(ms);
        ms.Close();
        return copy as T;
    }

    public static void Write<T>(T data, string path) where T : class
    {
        var fs = new FileStream(path, FileMode.Create);
        var binaryFormatter = new BinaryFormatter();

        binaryFormatter.Serialize(fs, data);
        fs.Close();
    }

    public static T Read<T>(TextAsset textAsset) where T : class
    {
        Stream stream = new MemoryStream(textAsset.bytes);
        BinaryFormatter formatter = new BinaryFormatter();
        T data = formatter.Deserialize(stream) as T;
        stream.Close();
        return data;
    }

    public static T Read<T>(string path) where T : class
    {
        var fs = new FileStream(path, FileMode.Open);
        var binaryFormatter = new BinaryFormatter();
        if (File.Exists(path))
        {
            T data = null;
            try
            {
                data = binaryFormatter.Deserialize(fs) as T;
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            fs.Close();
            return data;
        }
        fs.Close();
        throw new System.FieldAccessException("File not found on path " + path);
    }

    public static object Read(string path)
    {
        var fs = new FileStream(path, FileMode.Open);
        var binaryFormatter = new BinaryFormatter();
        if (File.Exists(path))
        {
            object data = null;
            try
            {
                data = binaryFormatter.Deserialize(fs);
            }
            catch (Exception e)
            {
                Console.WriteLine("Failed to deserialize. Reason: " + e.Message);
                throw;
            }
            fs.Close();
            return data;
        }
        fs.Close();
        throw new System.FieldAccessException("File not found on path " + path);
    }
}