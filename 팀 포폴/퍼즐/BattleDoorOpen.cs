using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class BattleDoorOpen : MonoBehaviour
{
    public Transform battleDoor;
    public float doorSpeed=1f;
    public float doorDist=5f;
    private bool doorOpen=false;

    private Vector3 startPos;
    private Vector3 endPos;

    void Start()
    {
        if(battleDoor==null)
        {
            battleDoor=transform;
        }
        startPos=battleDoor.localPosition;
        endPos=startPos+new Vector3(0,0,doorDist);
    }
    // Update is called once per frame
    void Update()
    {
       if(doorOpen)
        {
            battleDoor.localPosition=Vector3.Lerp(battleDoor.localPosition,endPos,Time.deltaTime*doorSpeed);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            doorOpen=true;
        }
    }

}
