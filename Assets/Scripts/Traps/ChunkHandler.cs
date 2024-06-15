using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkHandler : MonoBehaviour
{
    private GameObject[] objects;
    private void Awake()
    {
        objects = new GameObject[transform.childCount];
        for(int i = 0; i < transform.childCount; i++)
            objects[i] = transform.GetChild(i).gameObject;
    }
    private void FixedUpdate()
    {
        checkIfAnyobjIsInScreen();
    }
    private void checkIfAnyobjIsInScreen()
    {
        bool isInScreen = false;

        foreach (GameObject obj in objects)
        {
            Vector2 objPosition = obj.transform.position;
            Vector2 _vp = Camera.main.WorldToViewportPoint(objPosition);

            if (_vp.x >= 0f && _vp.x <= 1f && _vp.y >= 0f && _vp.y <= 1f)
            {
                isInScreen = true;
                break;
            }
        }

        foreach (GameObject obj in objects)
            obj.SetActive(isInScreen);
    }
}
