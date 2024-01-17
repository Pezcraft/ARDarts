using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuEventPropagator : MonoBehaviour
{

    public string ItemName;

    void OnMouseDown()
    {
        MenuExample example = GetComponentInParent<MenuExample>();
        example.OnClick(ItemName,gameObject);
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
