using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextInstance : MonoBehaviour {


    public GameObject PrefabObject;


	// Use this for initialization
	void Start () {

	}

	public void Run()
    {
        if (PrefabObject == null)
            return;

        string TextFormat = "+{0} Defense";
        int RandomNumber = Random.Range(1, 100); // select a random number. In a real life use case,  this number can be something relevant to your game instead of a random.
        string showText = string.Format(TextFormat, RandomNumber);

        DynamicTextElement.ShowTextEffect(PrefabObject, showText);


    }
    // Update is called once per frame
    void Update () {
		
	}
}
