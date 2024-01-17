using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestroy : MonoBehaviour {

    public float DestoryTime = 5f;
	// Use this for initialization
	void Start () {
        Destroy(gameObject, DestoryTime);
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
