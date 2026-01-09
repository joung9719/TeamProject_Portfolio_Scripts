using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class scGateOpen : MonoBehaviour
{
    private GameObject door;
    private bool[] trigger = new bool[8];
    private Animator aime;
    void Awake()
    {
        aime = GetComponent<Animator>();
    }

    public void SetTrigger(int index, bool door)
    {
        trigger[index] = door;

        bool Open = true;
        for (int i = 0; i < trigger.Length; i++)
        {
            if (!trigger[i])
            {
                Open = false;
               
                break;
            }   
        }
         if (Open)
        {
            aime.SetTrigger("Open");
        }
    }


}