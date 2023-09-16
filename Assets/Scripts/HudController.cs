using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

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

    public TMP_Text killsText;
    public TMP_Text deathsText;
    public TMP_Text playerIdentifierText;

    public Image selfShieldOverlay;

    private ARTrackedImageManager trackedImageManager;

    private ARTrackedImage arTrackedImage;

    public TMP_Text debugText;

    private int[] health = { 100, 100 };
    private int[] shieldHp = { 0, 0 };
    private int[] shieldsLeft = { 3, 3 };
    private int[] ammoLeft = { 6, 6 };
    private int[] grenadesLeft = { 2, 2 };
    private int[] deaths = { 0, 0 };

    private int MAX_SHIELDS = 3;
    private int MAX_GRENADES = 2;
    private string RESERVE_AMMO = "--";

    string action = "";

    // Action names
    private string SHOOT = "shoot";
    private string RELOAD = "reload";
    private string SHIELD = "shield";
    private string GRENADE = "grenade";

    // For now, we are always player 1, opponent is player 2
    // Change these to view POV of each player's screen
    private int PLAYER = 1;
    private int OPPONENT = 2;

    // Tracking Opponent
    private void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
    }

    public void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
    }

    public void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
    }

    void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage trackedImage in eventArgs.added)
        {
            // Set trackedImage
            arTrackedImage = trackedImage;

            if (trackedImage.trackingState == TrackingState.Tracking)
            {
                // trackedImage is tracked
                Debug.Log("image is tracking");
                // Debugging purposes
                DisplayDebugText("image is tracking");
            }
            else
            {
                // trackedImage is lost
                Debug.Log("image is lost");
                // Debugging purposes
                DisplayDebugText("image is lost");
            }
            break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        DisplayPlayerIdentifierText(PLAYER);
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

        // Display Kills/Deaths
        DisplayScore(PLAYER);

        // Display ammo count text
        DisplayAmmoCountText(PLAYER);
        // Display shield count text
        DisplayShieldCountText(PLAYER);
        // Display shield count text
        DisplayGrenadeCountText(PLAYER);


        // Display AR shield for player when shieldHp[0] > 0
        DisplaySelfShieldOverlay(PLAYER);
        // Display AR shield for opponent when ShieldHp[1] > 0
        ////

        // Opponent detection
        if (arTrackedImage != null)
        {
            DisplayDebugText(arTrackedImage.trackingState.ToString());
            if (arTrackedImage.trackingState == TrackingState.Tracking)
            {
                DisplayDebugText("image detected!!");
            }
            else
            {
                DisplayDebugText("image not found");
            }
        }
    }

    // Display self shield overlay
    void DisplaySelfShieldOverlay(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        selfShieldOverlay.enabled = shieldHp[playerIndex] > 0;
    }

    // Display shield count text
    void DisplayGrenadeCountText(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        grenadeText.text = grenadesLeft[playerIndex].ToString() + " / " + MAX_GRENADES.ToString();
    }

    // Display shield count text
    void DisplayShieldCountText(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        shieldText.text = shieldsLeft[playerIndex].ToString() + " / " + MAX_SHIELDS.ToString();
    }

    // Display ammo count text
    void DisplayAmmoCountText(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        ammoText.text = ammoLeft[playerIndex].ToString() + " / " + RESERVE_AMMO.ToString();
    }

    // Display Kills/Deaths of player
    // Player's kills = opponent's deaths and vice versa
    void DisplayScore(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        int opponentIndex = (player == 1) ? 1 : 0;
        killsText.text = deaths[opponentIndex].ToString();
        deathsText.text = deaths[playerIndex].ToString();
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

    // Display Player Identifier text
    void DisplayPlayerIdentifierText(int player)
    {
        playerIdentifierText.text = "P" + player.ToString() + " (You)";
    }

    // Return TRUE if HP/SHIELD unit should be displayed, else return FALSE
    bool IsHealthUnitDisplayed(int health, int unitIndex)
    {
        return (unitIndex * 10) < health;
    }

    // Display action sent by game engine
    // Eg. "shoot" has firing and hitting of bullet effect
    // Shield not included as it has to remain on screen until shieldHp == 0
    public void DisplayAction(string action)
    {
        // Display action for 5 seconds and stop
        // PLACEHOLDER
        Debug.Log("displaying action...");

        // AR Effect for shoot

        // AR Effect for reload


    }

    // Game Engine calls this function to send action to visualiser
    // Handle what happens on visualiser for each action
    // Return hit/miss to game engine for actions that require it
    // Hit = 1, miss = 0
    public int HandleActionFromGameEngine(string action)
    {
        this.action = action;

        // Actions that require tracking
        if (action == GRENADE)
        {
            // If opponent detected, return hit, display grenade flying to opp
            // Else, return miss, display grenade flying straight

            // Return hit/miss
            return 1;
        }

        // Other actions (shoot, reload)
        else
        {
            DisplayAction(action);
            return 0;
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

    // Debugging purposes
    void DisplayDebugText(string msg)
    {
        debugText.text = msg;
    }
}
