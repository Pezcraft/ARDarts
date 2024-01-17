using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuExample : MonoBehaviour
{
    public GameObject MenuItemPrefab;
    public string[] MenuItems;
    public float yGap = 0.2f;
    public AnimationSequenceBase Selected;
    public AnimationSequenceBase NotSelected;
    GameObject[] MenuGameObjects;
    // Start is called before the first frame update
    void Start()
    {
        CreateMenu();
    }

    public void OnClick(string itemName,GameObject clicked)
    {
        Debug.Log(itemName);
        for(int i=0; i<MenuItems.Length; i++)
        {
            var obj = MenuGameObjects[i];
            var element = obj.GetComponentInChildren<DynamicTextElement>();
            if (element == null)
                continue;
            foreach(var prevSeq in obj.GetComponentsInChildren<AnimationSequenceBase>())
                Object.Destroy(prevSeq);
            AnimationSequenceBase seq;
            if (itemName == MenuItems[i])
            {
                seq = GameObject.Instantiate(Selected, element.transform);
            }
            else
            {
                seq = GameObject.Instantiate(NotSelected, element.transform);
            }
            seq.StartAnimationOn(element);
        }
    }
    public void DestoryMenu()
    {
        if(MenuGameObjects != null)
        {
            foreach(GameObject obj in MenuGameObjects)
            {
                if (obj != null)
                    GameObject.Destroy(obj);
            }
            MenuGameObjects = null;
        }
    }
    /// <summary>
    /// 
    /// </summary>
    public void CreateMenu()
    {
        DestoryMenu(); // make sure to destory the previous menu first
        if (MenuItems == null || MenuItems.Length == 0)
            return;
        float y = - yGap * (MenuItems.Length-1) * 0.5f;
        MenuGameObjects = new GameObject[MenuItems.Length];
        for (int i = 0; i < MenuItems.Length; i++)
        {
            string item = MenuItems[i];
            GameObject newObj = GameObject.Instantiate(MenuItemPrefab, transform);

            newObj.transform.localPosition = new Vector3(0f, y, 0f);
            newObj.transform.localRotation = Quaternion.identity;

            var element = newObj.GetComponentInChildren<DynamicTextElement>();
            if (element != null)
            {
                element.Text = item;
                var prop = element.gameObject.AddComponent<MenuEventPropagator>();
                prop.ItemName = item;

            }
            MenuGameObjects[i] = newObj;
            y += yGap;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
