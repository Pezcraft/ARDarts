//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//using UnityEditor;
//using System.Linq;

//[InitializeOnLoad]
//public static class DevMenu
//{

//    private const string MENU_ASSERT_NAME = "Tools/TextStudio3D/Integrity/Enable Assert";
//    private const string MENU_ASSERT_PERFORMANCE_NAME = "Tools/TextStudio3D/Integrity/Enable Performance Intensive Asserts";

//    private const string AssertName = "DevAssert";
//    private const string AssertNamePerformance = "DevAssertPerformance";

//    static DevMenu()
//    {
//        EditorApplication.delayCall += () => {
//            CheckMenuEnabled(MENU_ASSERT_NAME, AssertName);
//            CheckMenuEnabled(MENU_ASSERT_PERFORMANCE_NAME, AssertNamePerformance);
//        };
//    }

//    static bool IsDefined(string name)
//    {
//        var target = EditorUserBuildSettings.activeBuildTarget;
//        var group = BuildPipeline.GetBuildTargetGroup(target);
//        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
//        foreach (string s in defines.Split(';'))
//        {
//            if (s == name)
//                return true;
//        }
//        return false;
//    }

//    static string ToogleOption(string options, string toogle)
//    {
//        var items = options.Split(';');
//        int toogleIndex = -1;
//        for (int i = 0; i < items.Length; i++)
//        {
//            if (items[i] == toogle)
//            {
//                toogleIndex = i;
//                break;
//            }
//        }
//        if (toogleIndex == -1)
//            return string.Join(";", items.Concat(new string[] { toogle }).ToArray());
//        items[toogleIndex] = items[0];  // put the last define where this one was to delete it.
//        var skipItem = items.Skip(1); // skip the first item because it was copied
//        return string.Join(";", skipItem.ToArray());
//    }

//    static void ToogleDefine(string option)
//    {
//        var target = EditorUserBuildSettings.activeBuildTarget;
//        var group = BuildPipeline.GetBuildTargetGroup(target);
//        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(group);
//        string toogled = ToogleOption(defines, option);
//        PlayerSettings.SetScriptingDefineSymbolsForGroup(group, toogled);
//    }

//    [MenuItem(DevMenu.MENU_ASSERT_NAME)]
//    private static void ToggleAssertAction()
//    {
//        ToogleDefine(AssertName);
//        CheckMenuEnabled(MENU_ASSERT_NAME, AssertName);
//        var target = EditorUserBuildSettings.activeBuildTarget;
//    }


//    [MenuItem(DevMenu.MENU_ASSERT_PERFORMANCE_NAME)]
//    private static void TogglePerformanceAssertAction()
//    {
//        ToogleDefine(AssertNamePerformance);
//        CheckMenuEnabled(MENU_ASSERT_PERFORMANCE_NAME, AssertNamePerformance);
//    }

//    private static void ValidateDefineMenu(string menu, string defineName)
//    {
//        bool enabled = IsDefined(defineName);
//        Menu.SetChecked(menu, enabled);
//    }

//    [MenuItem(DevMenu.MENU_ASSERT_NAME, true)]
//    private static bool ToggleAssertActionValidate()
//    {
//        CheckMenuEnabled(MENU_ASSERT_NAME, AssertName);
//        return true;
//    }

//    [MenuItem(DevMenu.MENU_ASSERT_PERFORMANCE_NAME, true)]
//    private static bool ToggleAssertPerformanceActionValidate()
//    {
//        CheckMenuEnabled(MENU_ASSERT_PERFORMANCE_NAME, AssertNamePerformance);
//        return true;
//    }

//    /// <summary>
//    /// sets the menu visiblity
//    /// </summary>
//    public static void CheckMenuEnabled(string menuName, string defineName)
//    {
//        bool enabled = IsDefined(defineName);
//        // Debug.Log("enabled" + enabled);
//        Menu.SetChecked(menuName, enabled);
//        EditorPrefs.SetBool(menuName, enabled);
//        //     mEnabled = enabled;
//    }

//    public static void CheckAssertEnabled()
//    {
//        bool enabled = IsDefined(AssertName);
//        // Debug.Log("enabled" + enabled);
//        Menu.SetChecked(MENU_ASSERT_NAME, enabled);
//        EditorPrefs.SetBool(MENU_ASSERT_NAME, enabled);
//        //     mEnabled = enabled;
//    }

//}
