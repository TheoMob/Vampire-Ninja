using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestVisibility : MonoBehaviour
{
    private SpriteRenderer _spr;
    [SerializeField] private float _potato = 0;

    private void Start() 
    {
        _spr = GetComponent<SpriteRenderer>();
    }

    private void Update() 
    {
        // Debug.Log("Visibility1");

        // _potato = _potato + 1;

        // if (_potato > 10)
        //     _potato = 0;
            
        // // if (_spr.isVisible)
        // // {
        // //     Debug.Log("Visible");
        // // }    
        // // else
        // //     Debug.Log("Invisible");

        // Debug.Log("Visibility2");
    }
    private void OnBecameInvisible() 
    {
        Debug.Log("OnBecameInvisible");
    }

    private void OnBecameVisible() 
    {
        Debug.Log("OnBecameVisible");
    }
}
