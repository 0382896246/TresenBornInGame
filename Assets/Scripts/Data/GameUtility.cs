using System.IO;
using System.Xml.Serialization;
using UnityEngine;

public class GameUtility
{
    public const string FileName = "Question";
    public static string fileDir
    {
        get
        {
            return Application.dataPath + "/";
        }
    }
}
[System.Serializable()]
public class Data
{
    public QuestionAsset[] Questions = new QuestionAsset[0];

    public Data() { }

    // chuyển dữ liệu sang XML và lưu trữ trong file đã chỉ định
    public static void Write(Data data, string path)
    {
        XmlSerializer serializer = new XmlSerializer(typeof(Data));
        using (Stream stream = new FileStream(path, FileMode.Create))
        {
            serializer.Serialize(stream, data);
        }
    }
    public static Data Fetch(string filePath)
    {
        return Fetch(out bool result, filePath);
    }

    // mở file XML và chuyển thành kiểu data
    public static Data Fetch(out bool result, string filePath)
    {
        if (!File.Exists(filePath)) { result = false; return new Data(); }
        XmlSerializer deserializer = new XmlSerializer(typeof(Data));
        using (Stream stream = new FileStream(filePath, FileMode.Open))
        {
            var data = (Data)deserializer.Deserialize(stream);
            result = true;
            return data;
        }

    }
}