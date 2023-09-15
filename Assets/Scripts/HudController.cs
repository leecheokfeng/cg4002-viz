using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HudController : MonoBehaviour
{
    public TMP_Text healthText;
    public Image healthBar;
    public Image[] healthUnits;
    public Image shieldBar;
    public Image[] shieldUnits;
    public Image oppHealthBar;
    public Image[] oppHealthUnits;

    public TMP_Text shieldText;

    public TMP_Text ammoText;

    public TMP_Text grenadeText;

    private int[] health = { 100, 100 };
    private int[] shieldHp = { 0, 0 };
    private int[] shieldsLeft = { 3, 3 };
    private int[] ammoLeft = { 6, 6 };
    private int[] grenadesLeft = { 2, 2 };
    private int[] deaths = { 0, 0 };

    string RESERVE_AMMO = "--";

    string action = "";

    // For now, we are always player 1, opponent is player 2
    int PLAYER = 1;
    int OPPONENT = 2;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        // Display HP text
        DisplayHpText(PLAYER);
        // Display HP units
        DisplayHpUnits(PLAYER);
        // Display SHIELD units
        DisplayShieldUnits(PLAYER);
        // Display Opponent HP
        DisplayOppHp(OPPONENT);

        // Display ammo count text
        DisplayAmmoCountText(PLAYER);
    }

    // Display ammo count text
    void DisplayAmmoCountText(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        ammoText.text = ammoLeft[playerIndex].ToString() + " / " + RESERVE_AMMO.ToString();
    }

    // Display Opponent HP
    void DisplayOppHp(int opponent)
    {
        int opponentIndex = (opponent == 1) ? 0 : 1;
        for (int i = 0; i < healthUnits.Length; i++)
        {
            oppHealthUnits[i].enabled = IsHealthUnitDisplayed(health[opponentIndex], i);
        }
    }

    // Display SHIELD units
    void DisplayShieldUnits(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        for (int i = 0; i < shieldUnits.Length; i++)
        {
            shieldUnits[i].enabled = IsHealthUnitDisplayed(shieldHp[playerIndex], i);
        }
    }

    // Display HP units
    void DisplayHpUnits(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        for (int i = 0; i < healthUnits.Length; i++)
        {
            healthUnits[i].enabled = IsHealthUnitDisplayed(health[playerIndex], i);
        }
    }

    // Display HP text
    void DisplayHpText(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        healthText.text = health[playerIndex].ToString();
    }

    // Return TRUE if HP/SHIELD unit should be displayed, else return FALSE
    bool IsHealthUnitDisplayed(int health, int unitIndex)
    {
        return (unitIndex * 10) < health;
    }

    // Display action sent by game engine
    // Eg. "shoot" has firing and hitting of bullet effect
    public void DisplayAction(string action)
    {
        // Display action for 5 seconds and stop
        Debug.Log("displaying action...");
    }

    // Game Engine calls this function to send action to visualiser
    // Handle what happens on visualiser for each action
    // Return hit/miss to game engine for actions that require it
    public void HandleActionFromGameEngine(string action)
    {
        this.action = action;

        if (action == "shoot")
        {
            DisplayAction(action);
        }
    }

    // Change game state based on received info from game engine
    public void ChangeGameState(int[] p1_state, int[] p2_state)
    {
        health[0] = p1_state[0];
        shieldHp[0] = p1_state[1];
        shieldsLeft[0] = p1_state[2];
        ammoLeft[0] = p1_state[3];
        grenadesLeft[0] = p1_state[4];
        deaths[0] = p1_state[5];

        health[1] = p2_state[0];
        shieldHp[1] = p2_state[1];
        shieldsLeft[1] = p2_state[2];
        ammoLeft[1] = p2_state[3];
        grenadesLeft[1] = p2_state[4];
        deaths[1] = p2_state[5];
    }
}
