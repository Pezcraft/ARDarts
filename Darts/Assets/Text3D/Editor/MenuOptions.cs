using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class MenuOptions
{
    [MenuItem("Assets/Create/3D Font")]
    public static void Add3DFont()
    {
        string path = "Assets";

        foreach (UnityEngine.Object obj in Selection.GetFiltered(typeof(UnityEngine.Object), SelectionMode.Assets))
        {
            path = AssetDatabase.GetAssetPath(obj);
            if (File.Exists(path))
            {
                path = Path.GetDirectoryName(path);
            }
            break;
        }

        path += "/3DFont.asset";
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        Font3DManager asset = ScriptableObject.CreateInstance<Font3DManager>();
        AssetDatabase.CreateAsset(asset, path);

        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        AssetDatabase.SaveAssets();
    }

    static void InstainciateResource(string path)
    {
        Transform SelectedObject = null;
        if (Selection.activeTransform != null && Selection.activeTransform.gameObject != null)
            SelectedObject = Selection.activeTransform.gameObject.transform;
        GameObject obj = Resources.Load<GameObject>(path);
        GameObject newObj = null;
        if (SelectedObject !=null)
            newObj = (GameObject)GameObject.Instantiate(obj,SelectedObject);
        else
            newObj = (GameObject)GameObject.Instantiate(obj);

        newObj.name = newObj.name.Replace("(Clone)", "");
        Undo.RegisterCreatedObjectUndo(newObj, "Create Object");
    }



    [MenuItem("GameObject/3D Object/3D Text Effects/Text Element")]
    public static void Add3DText()
    {
        InstainciateResource("3D Text");
    }

    [MenuItem("GameObject/3D Object/3D Text Effects/Text Element In Container")]
    public static void Add3DTextInContainer()
    {
        InstainciateResource("3D Text Container");
    }
    [MenuItem("GameObject/3D Object/3D Text Effects/Text Input")]
    public static void Add3DTextInput()
    {
        InstainciateResource("TextInput");
    }


}
