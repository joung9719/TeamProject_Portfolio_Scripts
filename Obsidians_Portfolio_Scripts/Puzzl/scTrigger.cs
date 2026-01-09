using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scTrigger : MonoBehaviour
{
    public int index;

    public scGateOpen gateOpen;

    private void OnTriggerEnter(Collider coll)
    {
        if (coll.CompareTag("DoorKey"))
        {
            gateOpen.SetTrigger(index, true);
        }
    }

    private void OnTriggerExit(Collider doll)
    {
        if (doll.CompareTag("DoorKey"))
        {
            gateOpen.SetTrigger(index, false);
        }
    }

}
