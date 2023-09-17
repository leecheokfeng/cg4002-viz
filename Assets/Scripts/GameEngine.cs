using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameEngine : MonoBehaviour
{
    private int MAX_HEALTH = 100;
    private int MAX_SHIELD_HP = 30;
    private int MAX_SHIELDS = 3;
    private int MAX_AMMO = 6;
    private int MAX_GRENADES = 2;

    private int[] health = { 100, 100 };
    private int[] shieldHp = { 0, 0 };
    private int[] shieldsLeft = { 3, 3 };
    private int[] ammoLeft = { 6, 6 };
    private int[] grenadesLeft = { 2, 2 };
    private int[] deaths = { 0, 0 };

    public HudController hudController;

    // isOpponentDetected[0] = true -> p1 sees p2
    // isOpponentDetected[1] = true -> p2 sees p1
    private bool[] isOpponentDetected = { false, false };

    // Action names
    private string SHOOT = "shoot";
    private string RELOAD = "reload";
    private string SHIELD = "shield";
    private string GRENADE = "grenade";
    private string PUNCH = "punch";
    private string WEB = "web";
    private string PORTAL = "portal";
    private string HAMMER = "hammer";
    private string SPEAR = "spear";


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

    // Handle all actions for player 1.
    // 2 functions for action handling for both players cos unity buttons can't
    // link to functions with 2 parameters.
    public void HandleActionPlayer1(string action)
    {
        int player = 1;
        int playerIndex = (player == 1) ? 0 : 1;

        // Invalid/unrecognisable action
        if (isActionValid(action, player) == false) 
        { 
            return; 
        }

        // Marvel damage actions
        if (action == PUNCH || action == WEB || action == PORTAL || 
            action == HAMMER || action == SPEAR)
        {
            // Send action to visualiser
            SendActionToVisualiser(action, player);
            // Visualiser determines hit/miss and replies
            // Update game state accordingly
            HandleMarvel(player);
            SendGameStateToVisualiser();
            // Reset opponent detection
            isOpponentDetected[playerIndex] = false;
        }

        // Grenade
        if (action == GRENADE)
        {
            // Send action to visualiser
            SendActionToVisualiser(action, player);
            // Visualiser determines hit/miss and replies
            // Update game state accordingly
            HandleGrenade(player);
            SendGameStateToVisualiser();
            // Reset opponent detection
            isOpponentDetected[playerIndex] = false;
        }

        // Valid reload
        if (action == RELOAD)
        {
            HandleReload(player);
            SendActionToVisualiser(action, player);
            SendGameStateToVisualiser();
        }

        // Valid shield
        if (action == SHIELD)
        {
            ActivateShield(player);
            SendGameStateToVisualiser();
        }

        // Valid shoot
        if (action == SHOOT)
        {
            HandleShoot(player);
            SendActionToVisualiser(action, player);
            SendGameStateToVisualiser();
        }

        // Valid logout
    }

    // Handle all actions for player 2.
    public void HandleActionPlayer2(string action)
    {
        int player = 2;
        int playerIndex = (player == 1) ? 0 : 1;

        // Invalid/unrecognisable action
        if (isActionValid(action, player) == false)
        {
            return;
        }

        // Marvel damage actions
        if (action == PUNCH || action == WEB || action == PORTAL ||
            action == HAMMER || action == SPEAR)
        {
            SendActionToVisualiser(action, player);
            HandleMarvel(player);
            SendGameStateToVisualiser();
            // Reset opponent detection
            isOpponentDetected[playerIndex] = false;
        }

        // Grenade
        if (action == GRENADE)
        {
            SendActionToVisualiser(action, player);
            HandleGrenade(player);
            SendGameStateToVisualiser();
            // Reset opponent detection
            isOpponentDetected[playerIndex] = false;
        }

        // Valid reload
        if (action == RELOAD)
        {
            HandleReload(player);
            SendActionToVisualiser(action, player);
            SendGameStateToVisualiser();
        }

        // Valid shield
        if (action == SHIELD)
        {
            ActivateShield(player);
            SendGameStateToVisualiser();
        }

        // Valid shoot
        if (action == SHOOT)
        {
            HandleShoot(player);
            SendActionToVisualiser(action, player);
            SendGameStateToVisualiser();
        }

        // Valid logout
    }


    // Send action to visualiser
    // Receive hit/miss for actions that require it
    void SendActionToVisualiser(string action, int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        // Get response from visualiser based on which player made the action
        int response = hudController.HandleActionFromGameEngine(action, player);
        
        if (response == -1)
        {
            return;
        }

        // Actions that require tracking
        if (action == GRENADE || action == PUNCH || action == WEB || action == PORTAL ||
            action == HAMMER || action == SPEAR)
        {
            isOpponentDetected[playerIndex] = (response == 1) ? true : false;
        }
    }

    // Update the game state of the visualiser
    // For now using function, eventually will use MQTT
    void SendGameStateToVisualiser()
    {
        int[] p1_state = { health[0], shieldHp[0], shieldsLeft[0], ammoLeft[0], grenadesLeft[0], deaths[0] };
        int[] p2_state = { health[1], shieldHp[1], shieldsLeft[1], ammoLeft[1], grenadesLeft[1], deaths[1] };
        hudController.ChangeGameState(p1_state, p2_state);
    }

    // Marvel attack
    void HandleMarvel(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        if (isOpponentDetected[playerIndex] == true)
        {
            int damagedPlayer = (player == 1) ? 2 : 1;
            TakeDamage(damagedPlayer, 10);
        }
    }

    // Grenade thrown
    void HandleGrenade(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        grenadesLeft[playerIndex] -= 1;

        if (isOpponentDetected[playerIndex] == true)
        {
            int damagedPlayer = (player == 1) ? 2 : 1;
            TakeDamage(damagedPlayer, 30);
        }
    }

    // Activate Shield
    void ActivateShield(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        shieldHp[playerIndex] = MAX_SHIELD_HP;
        shieldsLeft[playerIndex] -= 1;
    }

    // Reload
    void HandleReload(int player) 
    {
        int playerIndex = (player == 1) ? 0 : 1;
        ammoLeft[playerIndex] = MAX_AMMO;
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
            if (shieldHp[playerIndex] > 0)
            {
                shieldHp[playerIndex] -= 10;
            }
            else if (health[playerIndex] > 0)
            {
                health[playerIndex] -= 10;
            }

            if (health[playerIndex] == 0)
            {
                RevivePlayer(player);
                return;
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

        if (action == GRENADE)
        { return grenadesLeft[playerIndex] > 0; }
        else if (action == SHIELD)
        { return shieldsLeft[playerIndex] > 0 && shieldHp[playerIndex] == 0; }
        else if (action == RELOAD)
        { return ammoLeft[playerIndex] == 0; }
        else if (action == SHOOT)
        { return ammoLeft[playerIndex] > 0; }
        else { return true; }  
    }
}
