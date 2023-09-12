using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class AmmoInventory : MonoBehaviour
{
    public TMP_Text ammoText;

    int ammoLeft = 6;
    int MAX_AMMO = 6;

    string RESERVE_AMMO = "--";

    // Start is called before the first frame update
    void Start()
    {
        ammoLeft = MAX_AMMO;
    }

    // Update is called once per frame
    void Update()
    {
        // Display ammo count text
        ammoText.text = ammoLeft.ToString() + " / " + RESERVE_AMMO.ToString();
    }

    // Reduce player ammo by 1
    public void ReduceAmmo()
    {
        if (ammoLeft > 0)
        {
            ammoLeft -= 1;
        }
    }

    // Reload to max ammo
    public void Reload()
    {
        if (ammoLeft == 0)
        {
            ammoLeft = MAX_AMMO;
        }
    }
}
