using CymaticLabs.Unity3D.Amqp.SimpleJSON;
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

    /*
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
    */

    // Action names (post-integration with MQTT)
    private static string SHIELD = "1";
    private static string RELOAD = "2";
    private static string WEB = "3";
    private static string PORTAL = "4";
    private static string PUNCH = "5";
    private static string SPEAR = "6";
    private static string HAMMER = "7";
    private static string GRENADE = "8";
    private static string EXIT = "9";
    private static string SHOOT_MISS = "10";
    private static string SHOOT_HIT = "11";

    // Dummy game state from eval server
    private string DUMMY_P1 = "{\"hp\":90, \"bullets\":5, \"grenades\":2, " +
        "\"shield_hp\":20, \"shields\":2, \"deaths\":3}";
    private string DUMMY_P2 = "{\"hp\":80, \"bullets\":4, \"grenades\":1, " +
        "\"shield_hp\":10, \"shields\":1, \"deaths\":2}";

    // Class for decoding JSON from Eval Server
    public class PlayerInfo
    {
        public int hp;
        public int bullets;
        public int grenades;
        public int shield_hp;
        public int shields;
        public int deaths;

        // Convert JSON to object
        public static PlayerInfo CreateFromJSON(string jsonString)
        {
            return JsonUtility.FromJson<PlayerInfo>(jsonString);
        }
    }


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

        // Valid shoot hit
        if (action == SHOOT_HIT)
        {
            HandleShoot(player, true);
            SendActionToVisualiser(action, player);
            SendGameStateToVisualiser();
        }

        // Valid shoot miss
        if (action == SHOOT_MISS)
        {
            HandleShoot(player, false);
            SendActionToVisualiser(action, player);
            SendGameStateToVisualiser();
        }

        // Valid logout
        if (action == EXIT)
        {
            SendActionToVisualiser(action, player);
        }
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
        if (action == SHOOT_HIT)
        {
            HandleShoot(player, true);
            SendActionToVisualiser(action, player);
            SendGameStateToVisualiser();
        }

        // Valid shoot miss
        if (action == SHOOT_MISS)
        {
            HandleShoot(player, false);
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

    // Simulate receiving correct game state from Eval Server
    // This function sets game state to values found in dummy data from Eval Server
    // Then sends the game state to visualiser
    public void ReceiveFromEvalServer()
    {
        PlayerInfo player1Obj = PlayerInfo.CreateFromJSON(DUMMY_P1);
        PlayerInfo player2Obj = PlayerInfo.CreateFromJSON(DUMMY_P2);

        health[0] = player1Obj.hp;
        shieldHp[0] = player1Obj.shield_hp;
        shieldsLeft[0] = player1Obj.shields;
        ammoLeft[0] = player1Obj.bullets;
        grenadesLeft[0] = player1Obj.grenades;
        deaths[0] = player1Obj.deaths;

        health[1] = player2Obj.hp;
        shieldHp[1] = player2Obj.shield_hp;
        shieldsLeft[1] = player2Obj.shields;
        ammoLeft[1] = player2Obj.bullets;
        grenadesLeft[1] = player2Obj.grenades;
        deaths[1] = player2Obj.deaths;

        SendGameStateToVisualiser();
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
    void HandleShoot(int player, bool hit) 
    {
        int playerIndex = (player == 1) ? 0 : 1;
        int damagedPlayer = (player == 1) ? 2 : 1;

        ammoLeft[playerIndex] -= 1;

        // If hit
        if (hit == true)
        {
            TakeDamage(damagedPlayer, 10);
        }
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
        else if (action == SHOOT_HIT || action == SHOOT_MISS)
        { return ammoLeft[playerIndex] > 0; }
        else { return true; }  
    }
}
