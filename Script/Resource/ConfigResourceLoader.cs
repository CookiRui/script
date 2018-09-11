using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MiniXml;
using System.IO;

class ConfigResourceLoader : ResourceLoader, behaviac.IBeahaviacFileReader
{
    public static ConfigResourceLoader inst;

    void Awake()
    {
        inst = this;
    }

    public SecurityParser loadConfig(string path)
    {
        SecurityParser p = new SecurityParser();
        TextAsset textAsset = getTextAsset(path);
        string text = textAsset.text;
        p.LoadXml(text);

        return p;
    }

    public T loadJason<T>(string path)
       where T : new()
    {
        TextAsset textAsset = getTextAsset(path);
        string text = textAsset.text;
        T t = LitJson.JsonMapper.ToObject<T>(text);
        return t;
    }

    TextAsset getTextAsset(string path)
    {
        var index = path.LastIndexOf('.');
        path = index < 0 ? path : path.Substring(0, index);
        TextAsset textAsset = Resources.Load<TextAsset>(path);
        return textAsset;
    }

    public byte[] read(string path)
    {
        path = path.Substring(path.IndexOf("Resources/") + 10);
        path = path.Replace('\\', '/');

        return getFileData(path);
    }

    bool isFileExist(string filename)
    {
        return File.Exists(filename);
    }

    public byte[] getFileData(string path)
    {
        //先查看是否更新目录有文件
        string filePath = ResourceManager.inst.WriteablePath + path;

        byte[] temp = null;

        if (isFileExist(filePath))
        {
            FileStream fs = new FileStream(filePath, FileMode.Open);

            if (fs == null)
                return null;
            BinaryReader br = new BinaryReader(fs);
            if (br == null)
                return null;

            temp = br.ReadBytes((int)fs.Length);

            fs.Close();
        }
        else
        {
            Debuger.Log("...........File not Exist : " + filePath);

            TextAsset txtAsset = getTextAsset(path);
            if (txtAsset == null)
                return null;
            temp = txtAsset.bytes;
        }

        return temp;
    }
}
