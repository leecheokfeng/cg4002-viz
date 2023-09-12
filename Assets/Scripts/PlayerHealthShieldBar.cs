using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthShieldBar : MonoBehaviour
{
    public TMP_Text healthText;
    public Image healthBar;
    public Image[] healthUnits;
    public Image shieldBar;
    public Image[] shieldUnits;

    float health = 100;
    float maxHealth = 100;
    float shield = 0;
    float maxShield = 30;


    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        shield = 0;
    }

    // Update is called once per frame
    void Update()
    {
        // Take 10 damage
        if (Input.GetKeyDown(KeyCode.Z))
        {
            TakeDamage(10);
        }

        // Take 30 damage (grenade hit)
        if (Input.GetKeyDown(KeyCode.X))
        {
            TakeDamage(30);
        }

        // Activate Shield
        if (Input.GetKeyDown(KeyCode.C))
        {
            ActivateShield();
        }

        // Display HP text
        healthText.text = health.ToString();

        // Display HP units
        for (int i = 0; i < healthUnits.Length; i++)
        {
            healthUnits[i].enabled = IsHealthUnitDisplayed(health, i);
        }

        // Display SHIELD units
        for (int i = 0; i < shieldUnits.Length; i++)
        {
            shieldUnits[i].enabled = IsHealthUnitDisplayed(shield, i);
        }
    }

    // Return TRUE if HP/SHIELD unit should be displayed, else return FALSE
    bool IsHealthUnitDisplayed(float health, int unitIndex)
    {
        return (unitIndex * 10) < health;
    }

    public void TakeDamage(int damage)
    {
        for (int i = 0; i < damage/10; i++)
        {
            if (shield > 0)
            {
                shield -= 10;
            }
            else if (health > 0)
            {
                health -= 10;
            }
        }
    }

    public void ActivateShield() 
    { 
        if (shield == 0)
        {
            shield = maxShield;
        }
    }
}
