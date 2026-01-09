using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamgeEvent : MonoBehaviour
{
    public PlayerShotPistol playerShotPistol;

    public PlayerShotRifle playerShotRifle;
    public enum Weapons { Pistol, Rifle };
    public Weapons weapons;

    void Awake()
    {
        if (playerShotPistol == null)
        {
            playerShotPistol = GetComponent<PlayerShotPistol>();
        }
        if (playerShotRifle == null)
        {
            playerShotRifle = GetComponent<PlayerShotRifle>();
        }
    }

    public void TakeDamge()
    {
        switch (weapons)
        {
            case Weapons.Pistol:
                playerShotPistol.FireOnce();
                break;
            case Weapons.Rifle:
                playerShotRifle.FireOnce();
                break;
        }


    }
}
