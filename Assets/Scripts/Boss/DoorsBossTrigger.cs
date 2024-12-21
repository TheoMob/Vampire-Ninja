using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorsBossTrigger : MonoBehaviour
{
    private Animator[] doorsAnim = new Animator[2];

    private void Awake()
    {
        doorsAnim[0] = transform.GetChild(0).GetComponent<Animator>();
        doorsAnim[1] = transform.GetChild(1).GetComponent<Animator>();
    }

    public void MoveDoors(bool close)
    {
        if (close)
        {
            doorsAnim[0].Play("PortaDesce");
            doorsAnim[1].Play("PortaDesce");
        }
        else
        {
            doorsAnim[0].Play("PortaSobe");
            doorsAnim[1].Play("PortaSobe");
        }
    }

    public void ResetDoors()
    {
        doorsAnim[0].Play("PortaIdle");
        doorsAnim[1].Play("PortaIdle"); 
    }
}
