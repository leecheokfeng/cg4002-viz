using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GrenadeInventory : MonoBehaviour
{
    public TMP_Text grenadeText;

    int grenadesLeft = 2;
    int MAX_GRENADES = 2;

    // Start is called before the first frame update
    void Start()
    {
        grenadesLeft = MAX_GRENADES;
    }

    // Update is called once per frame
    void Update()
    {
        // Display grenades count text
        grenadeText.text = grenadesLeft.ToString() + " / " + MAX_GRENADES.ToString();
    }

    // Reduct player grenades by 1
    public void ReduceGrenade()
    {
        if (grenadesLeft > 0)
        {
            grenadesLeft -= 1;
        }
    }
}
