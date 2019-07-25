using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

//[InitializeOnLoad]
public class PruneEmptyFolders
{
    /*
    static bool found = false;

    //static PruneEmptyFolders()
    //{
    //        Prune();
    //}

    [MenuItem("VoK/Fix Ghost meta")]
    static void Prune()
    {
        found = false;
        PruneStep("Assets");
        AssetDatabase.Refresh();
        if (!found)
        {
            Debug.Log("<color=#00FF00>No ghost meta files found.</color>");
        }
        else
        {
            Debug.Log("<color=#FFFF00>Ghost meta files successfully autofixed.</color>");
        }
    }

    static void PruneStep(string path)
    {
        foreach (var item in Directory.GetDirectories(path))
        {
            //Debug.Log(item.ToString());
            PruneStep(item.ToString());
        }
        if(Directory.GetFiles(path).Length == 0)//&& Directory.GetDirectories(path).Length == 0)
        {
            Debug.Log("Deleted " + path + " folder since it's <b>empty</b>");
            Directory.Delete(path);

            File.Delete(path + ".meta");
            found = true;
        }
    }*/
}
