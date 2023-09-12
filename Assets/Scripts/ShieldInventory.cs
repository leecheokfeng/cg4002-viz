using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShieldInventory : MonoBehaviour
{
    public TMP_Text shieldText;

    int shieldsLeft = 3;
    int MAX_SHIELDS = 3;

    // Start is called before the first frame update
    void Start()
    {
        shieldsLeft = MAX_SHIELDS;
    }

    // Update is called once per frame
    void Update()
    {
        // Display shields count text
        shieldText.text = shieldsLeft.ToString() + " / " + MAX_SHIELDS.ToString();
    }

    // Decrease player shields by 1
    public void ReduceShield()
    {
        if (shieldsLeft > 0)
        {
            shieldsLeft -= 1;
        }
    }

    // Getter
    public int GetShieldsLeft() { return shieldsLeft; }
}
