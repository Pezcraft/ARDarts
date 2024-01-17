using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabShower : MonoBehaviour {

    public GameObject[] PrefabObject;

    int count = 0;
    // Use this for initialization
    void Start()
    {

    }
    public void Run()
    { 
        if (PrefabObject == null || PrefabObject.Length ==0)
            return;

        GameObject selectedPrefab = PrefabObject[count % PrefabObject.Length];
        count++;
        GameObject.Instantiate(selectedPrefab); // create a new instance of the prefab.


    }
    // Update is called once per frame
    void Update()
    {

    }
}
