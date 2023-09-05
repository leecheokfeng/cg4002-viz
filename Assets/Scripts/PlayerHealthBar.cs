using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerHealthBar : MonoBehaviour
{
    public TMP_Text healthText;
    public Image healthBar;
    public Image[] healthUnits;

    float health = 100;
    float maxHealth = 100;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            TakeDamage(10);
        }

        // Display HP text
        healthText.text = health.ToString();

        // Display HP units
        for (int i = 0; i < healthUnits.Length; i++)
        {
            healthUnits[i].enabled = IsHealthUnitDisplayed(health, i);
        }
    }

    // Return TRUE if HP unit should be displayed, else return FALSE
    bool IsHealthUnitDisplayed(float health, int unitIndex)
    {
        return (unitIndex * 10) < health;
    }

    void TakeDamage(int damage)
    {
        if (health > 0)
        {
            health -= damage;
        }
    }
}
