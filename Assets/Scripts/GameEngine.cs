using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    int MAX_HEALTH = 100;
    int MAX_SHIELD_HP = 30;
    int MAX_SHIELDS = 3;
    int MAX_AMMO = 6;
    int MAX_GRENADES = 2;

    int[] health = { 100, 100 };
    int[] shieldHp = { 0, 0 };
    int[] shieldsLeft = { 3, 3 };
    int[] ammoLeft = { 6, 6 };
    int[] grenadesLeft = { 2, 2 };
    int[] deaths = { 0, 0 };

    public HudController hudController;

    //int p1_health = 100;
    //int p1_shieldHp = 0;
    //int p1_shieldsLeft = 3;
    //int p1_ammoLeft = 6;
    //int p1_grenadesLeft = 2;

    //int p2_health = 100;
    //int p2_shieldHp = 0;
    //int p2_shieldsLeft = 3;
    //int p2_ammoLeft = 6;
    //int p2_grenadesLeft = 2;

    //int p1_deaths = 0;
    //int p2_deaths = 0;



    // Start is called before the first frame update
    void Start()
    {
        health[0] = MAX_HEALTH;
        shieldHp[0] = 0;
        shieldsLeft[0] = MAX_SHIELDS;
        ammoLeft[0] = MAX_AMMO;
        grenadesLeft[0] = MAX_GRENADES;

        health[1] = MAX_HEALTH;
        shieldHp[1] = 0;
        shieldsLeft[1] = MAX_SHIELDS;
        ammoLeft[1] = MAX_AMMO;
        grenadesLeft[1] = MAX_GRENADES;

        deaths[0] = 0;
        deaths[1] = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleActionPlayer1(string action)
    {
        int player = 1;

        // Invalid/unrecognisable action
        if (isActionValid(action, player) == false) 
        { 
            return; 
        }

        // Valid damage actions

        // Valid reload

        // Valid shield

        // Valid shoot
        if (action == "shoot")
        {
            HandleShoot(player);
            SendActionToVisualiser(action);
            SendGameStateToVisualiser();
        }

        // Valid logout
    }


    // Send action to visualiser
    // Receive hit/miss for actions that require it
    void SendActionToVisualiser(string action)
    {
        hudController.HandleActionFromGameEngine(action);
    }

    // Update the game state of the visualiser
    // For now using function, eventually will use MQTT
    void SendGameStateToVisualiser()
    {
        int[] p1_state = { health[0], shieldHp[0], shieldsLeft[0], ammoLeft[0], grenadesLeft[0], deaths[0] };
        int[] p2_state = { health[1], shieldHp[1], shieldsLeft[1], ammoLeft[1], grenadesLeft[1], deaths[1] };
        hudController.ChangeGameState(p1_state, p2_state);
    }

    // Assume shoot and hit
    // HandleShoot(1) means p1 shot p2 and hit
    void HandleShoot(int player) 
    {
        int playerIndex = (player == 1) ? 0 : 1;
        int damagedPlayer = (player == 1) ? 2 : 1;

        ammoLeft[playerIndex] -= 1;
        
        // If hit
        TakeDamage(damagedPlayer, 10);
    }

    // Damage a player
    void TakeDamage(int player, int damage)
    {
        int playerIndex = (player == 1) ? 0 : 1;

        for (int i = 0; i < damage / 10; i++)
        {
            if (health[playerIndex] == 0)
            {
                RevivePlayer(player);
                return;
            }

            if (shieldHp[playerIndex] > 0)
            {
                shieldHp[playerIndex] -= 10;
            }
            else if (health[playerIndex] > 0)
            {
                health[playerIndex] -= 10;
            }
        }
    }

    // Revive a player
    void RevivePlayer(int player) 
    {
        int playerIndex = (player == 1) ? 0 : 1;

        deaths[playerIndex] += 1;
        health[playerIndex] = MAX_HEALTH;
        shieldHp[playerIndex] = 0;
        shieldsLeft[playerIndex] = MAX_SHIELDS;
        ammoLeft[playerIndex] = MAX_AMMO;
        grenadesLeft[playerIndex] = MAX_GRENADES;
    }

    // Return TRUE if action is valid, else return FALSE
    bool isActionValid(string action, int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;

        if (action == "grenade")
        { return grenadesLeft[playerIndex] > 0; }
        else if (action == "shield")
        { return shieldsLeft[playerIndex] > 0 && shieldHp[playerIndex] == 0; }
        else if (action == "reload")
        { return ammoLeft[playerIndex] == 0; }
        else if (action == "shoot")
        { return ammoLeft[playerIndex] > 0; }
        else { return false; }  
    }
}
