#if UNITY_2018_3_OR_NEWER
#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public class PrefabOverride : UnityEditor.AssetPostprocessor
{
    static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
    {
        foreach (string path in importedAssets)
        {
            string lowPath = path.ToLower();
            if (lowPath.EndsWith(".prefab"))
            {
                if (ContainsPath(lowPath) == false)
                {
                  //  Debug.Log(lowPath);
                    EditorApplication.delayCall+= ()=> CleanPrefab(path);
                    AddPath(lowPath);
                }
            }

        }
        foreach(string path in deletedAssets)
        {
            RemovePath( path.ToLower());
        }
    }
    [MenuItem("EditorPrefs/Clear all Editor Preferences")]
    static void deleteAllExample()
    {
        EditorPrefs.DeleteAll();
    }
    static bool ContainsPath(string path)
    {
        return EditorPrefs.GetBool("TextEffects3DPrefabOverride$" +path, false);
    }
    static void RemovePath(string path)
    {
        EditorPrefs.DeleteKey("TextEffects3DPrefabOverride$" + path);
    }
    static void AddPath(string path)
    {
        EditorPrefs.SetBool("TextEffects3DPrefabOverride$" + path, true);
    }
    static void CleanPrefab(string path)
    {
        GameObject obj = PrefabUtility.LoadPrefabContents(path);
        //AssetDatabase.DeleteAsset(path);
        foreach (var item in obj.GetComponentsInChildren<TextEffectsItem>(true))
        {
            //          if (item == null)
            //              continue;
            //            if (item.gameObject == null)
            //                continue;
           // Debug.Log("destroy " + item.gameObject.name);
            while(item.gameObject.transform.childCount > 0)
            {
                var innerObj = item.gameObject.transform.GetChild(0).gameObject;
                if (innerObj != null && innerObj.GetComponent<TextEffectsItem>() != null)
                {
                    GameObject.DestroyImmediate(innerObj);
                }
            }


        }

        PrefabUtility.SaveAsPrefabAsset(obj,path);
        PrefabUtility.UnloadPrefabContents(obj);
    }

}
#endif
#endif
