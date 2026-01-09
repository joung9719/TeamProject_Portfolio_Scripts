using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{

    public GameObject muzzleFlash;
    public float flashDuration = 0.1f;

    private bool isFlashing = false;

    // Start is called before the first frame update
    void Start()
    {
        if (muzzleFlash != null)
            muzzleFlash.SetActive(false);
    }

    // Update is called once per frame
    public void OnMouseDown()
    {
        if (!isFlashing)
            StartCoroutine(ShowMuzzleFlash());
    }

    IEnumerator ShowMuzzleFlash()
    {
        isFlashing = true;

        muzzleFlash.SetActive(true);

        yield return new WaitForSeconds(flashDuration);

        muzzleFlash.SetActive(false);
        isFlashing = false;
    }
}
