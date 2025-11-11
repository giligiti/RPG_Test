using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class ScriptDirection : Editor
{
    [MenuItem("EditorTool/文件夹初始化")]
    public static void FloderCreat()
    {
        List<string> strings = new()
        {
            Path.Combine(Application.dataPath, "Resources"),   //资源文件夹
            Application.streamingAssetsPath,
            Path.Combine(Application.dataPath, "BundleRes"),   //需要打包的资源
            Path.Combine(Application.dataPath, "ArtRes/Animation"),
            Path.Combine(Application.dataPath, "ArtRes/ImportRes")//外界导入的美术资源
        };
        foreach (var item in strings)
        {
            Debug.Log("执行了一次");
            Directory.CreateDirectory(item);
        }

    }
}
