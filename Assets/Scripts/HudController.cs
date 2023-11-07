using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using static UnityEngine.GraphicsBuffer;

public class HudController : MonoBehaviour
{
    // Health and shields HUD
    public TMP_Text healthText;
    public Image healthBar;
    public Image[] healthUnits;
    public Image shieldBar;
    public Image[] shieldUnits;
    public Image oppHealthBar;
    public Image[] oppHealthUnits;

    // Inventory HUD
    public TMP_Text shieldText;
    public TMP_Text ammoText;
    public TMP_Text grenadeText;

    // Score and Identity HUD
    public TMP_Text killsText;
    public TMP_Text deathsText;
    public TMP_Text playerIdentifierText;

    // Shield objects
    public Image selfShieldOverlay;
    public GameObject opponentShieldPrefab;
    private GameObject opponentShieldObject;

    // Tracked image manager
    private ARTrackedImageManager trackedImageManager;
    private ARTrackedImage arTrackedImage;

    // Grenade objects
    public GameObject grenadePrefab;
    private GameObject grenadeObject;            
    private Vector3 grenadeMissTargetPos;        // Target position for missed projectiles
    public GameObject explosionPrefab;

    // Marvel objects
    private Vector3 marvelMissTargetPos;
    public GameObject punchPrefab;
    public GameObject webPrefab;
    public GameObject portalPrefab;
    public GameObject hammerPrefab;
    public GameObject spearPrefab;
    private GameObject punchObject;
    private GameObject webObject;
    private GameObject portalObject;
    private GameObject hammerObject;
    private GameObject spearObject;

    // Other AR effects
    public GameObject muzzleFlashPrefab;
    public GameObject bulletSparksPrefab;

    public TMP_Text debugText;

    // Action flags
    private bool throwGrenadeFlag = false;
    private bool marvelPunchFlag = false;
    private bool marvelWebFlag = false;
    private bool marvelPortalFlag = false;
    private bool marvelHammerFlag = false;
    private bool marvelSpearFlag = false;
    //private bool[] marvelFlags = { false, false, false, false, false };     // Action to index map found in Start() dictionary

    // Opponent detection
    private bool isOpponentDetected = false;
    private Vector3 opponentPosition;

    // Vuforia
    public GameObject vuforiaTrackedObject;

    // Logout Screen
    public GameObject logoutOverlay;

    // Black Screen surrounding cameras to cover background when logout screen is disabled
    public GameObject blackScreen;

    // Visualiser game state
    private int[] health = { 100, 100 };
    private int[] shieldHp = { 0, 0 };
    private int[] shieldsLeft = { 3, 3 };
    private int[] ammoLeft = { 6, 6 };
    private int[] grenadesLeft = { 2, 2 };
    private int[] deaths = { 0, 0 };

    private int MAX_SHIELDS = 3;
    private int MAX_GRENADES = 2;
    private string RESERVE_AMMO = "--";

    // Simulation speed = 0.005f to 0.03f
    // Real speed = 0.05f to 0.07f
    private float GRENADE_VELOCITY = 0.05f;
    private float MARVEL_VELOCITY = 0.07f;

    //##//
    // This var causes delays in updating the HUD
    // Purpose: to allow animations to finish before updating HUD values
    // Eg. Grenade shd fly and explode, then deal damage. Instead of dealing damage then flying.
    // True -> HUD not updating, waiting for animation. False -> HUD updating normally.
    private bool isWaitingForAnimation = false;
    //##//

    /*
    // Action names (pre-integration)
    private string SHOOT = "shoot";
    private string RELOAD = "reload";
    private string SHIELD = "shield";
    private string GRENADE = "grenade";
    private static string PUNCH = "punch";
    private static string WEB = "web";
    private static string PORTAL = "portal";
    private static string HAMMER = "hammer";
    private static string SPEAR = "spear";
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

    // Debug mode: Change these to view POV of each player's screen
    // Play mode: We select our player in main menu
    private int PLAYER = ChangeScene.player;
    private int OPPONENT = (ChangeScene.player == 1) ? 2 : 1;

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
            break;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        Debug.Log(ChangeScene.gamemode + " " + ChangeScene.player);

        logoutOverlay.gameObject.SetActive(false);
        blackScreen.gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (isWaitingForAnimation == false)
        {
            // Display player identity
            DisplayPlayerIdentifierText(PLAYER);

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
            HandleOpponentShieldObject(OPPONENT);
        }

        // Opponent detection (ARCore)
        DetectOpponent();

        // Opponent detection (vuforia)
        VuforiaDetection();

        // Handle grenade instantiation and physics
        HandleGrenadeObject();

        // Handle marvel objects instantiation and physics
        HandleMarvelObjects();
    }

    // Handle all marvel objects inside Update function
    void HandleMarvelObjects()
    {
        // If marvel action event is called, instantiate marvel object
        HandlePunchObject();
        HandleWebObject();
        HandlePortalObject();
        HandleHammerObject();
        HandleSpearObject();
    }

    // Handle spear object. Put this inside HandleMarvelObjects
    void HandleSpearObject()
    {
        // Instantiate spear object
        if (marvelSpearFlag)
        {
            // Instantiate 0.5m below camera
            float xPos = Camera.main.transform.position.x;
            float yPos = Camera.main.transform.position.y - 0.5f;
            float zPos = Camera.main.transform.position.z;
            Vector3 currPos = new Vector3(xPos, yPos, zPos);
            //spearObject = Instantiate(spearPrefab, currPos, Quaternion.identity);
            spearObject = Instantiate(spearPrefab, currPos, Quaternion.AngleAxis(90, Vector3.right));
            // set flag to false again
            marvelSpearFlag = false;
        }

        // spear trajectory if spearObject exists
        if (spearObject != null)
        {
            // spear behaviour if opponent on screen
            if (isOpponentDetected)
            {
                spearObject.transform.position = Vector3.MoveTowards(spearObject.transform.position, opponentPosition, MARVEL_VELOCITY);
                float spearToOppDistance = Vector3.Distance(spearObject.transform.position, opponentPosition);

                spearObject.transform.rotation = Camera.main.transform.rotation;
                spearObject.transform.Rotate(90, 0, 0);

                // If spear has reached opponent, destroy spearObject
                if (spearToOppDistance < 0.01f)
                {
                    Destroy(spearObject);
                    isWaitingForAnimation = false;
                    // Call function to generate explosion directly on opponent
                    GenerateExplosion();
                }
            }
            // spear behaviour if opponent not found
            // Unfreeze update since no damage being done
            else
            {
                isWaitingForAnimation = false;
                spearObject.transform.position = Vector3.MoveTowards(spearObject.transform.position, marvelMissTargetPos, 3 * MARVEL_VELOCITY);
                float spearToCameraDistance = Vector3.Distance(spearObject.transform.position, Camera.main.transform.position);

                spearObject.transform.rotation = Camera.main.transform.rotation;
                spearObject.transform.Rotate(90, 0, 0);

                // If spear is >10m from us, destroy spearObject
                if (spearToCameraDistance > 10f)
                {
                    Destroy(spearObject);
                }
            }
        }
    }

    // Handle hammer object. Put this inside HandleMarvelObjects
    void HandleHammerObject()
    {
        // Instantiate hammer object
        if (marvelHammerFlag)
        {
            // Instantiate 0.5m below camera
            float xPos = Camera.main.transform.position.x;
            float yPos = Camera.main.transform.position.y - 0.5f;
            float zPos = Camera.main.transform.position.z;
            Vector3 currPos = new Vector3(xPos, yPos, zPos);
            //hammerObject = Instantiate(hammerPrefab, currPos, Quaternion.identity);
            hammerObject = Instantiate(hammerPrefab, currPos, Quaternion.AngleAxis(90, Vector3.right));
            // set flag to false again
            marvelHammerFlag = false;
        }

        // hammer trajectory if hammerObject exists
        if (hammerObject != null)
        {
            // hammer behaviour if opponent on screen
            if (isOpponentDetected)
            {
                hammerObject.transform.position = Vector3.MoveTowards(hammerObject.transform.position, opponentPosition, MARVEL_VELOCITY);
                float hammerToOppDistance = Vector3.Distance(hammerObject.transform.position, opponentPosition);

                hammerObject.transform.rotation = Camera.main.transform.rotation;
                hammerObject.transform.Rotate(90, 0, 0);

                // If hammer has reached opponent, destroy hammerObject
                if (hammerToOppDistance < 0.01f)
                {
                    Destroy(hammerObject);
                    isWaitingForAnimation = false;
                    // Call function to generate explosion directly on opponent
                    GenerateExplosion();
                }
            }
            // hammer behaviour if opponent not found
            // Unfreeze update since no damage being done
            else
            {
                isWaitingForAnimation = false;
                hammerObject.transform.position = Vector3.MoveTowards(hammerObject.transform.position, marvelMissTargetPos, 3 * MARVEL_VELOCITY);
                float hammerToCameraDistance = Vector3.Distance(hammerObject.transform.position, Camera.main.transform.position);

                hammerObject.transform.rotation = Camera.main.transform.rotation;
                hammerObject.transform.Rotate(90, 0, 0);

                // If hammer is >10m from us, destroy hammerObject
                if (hammerToCameraDistance > 10f)
                {
                    Destroy(hammerObject);
                }
            }
        }
    }

    // Handle portal object. Put this inside HandleMarvelObjects
    void HandlePortalObject()
    {
        // Instantiate portal object
        if (marvelPortalFlag)
        {
            // Instantiate 0.5m below camera
            float xPos = Camera.main.transform.position.x;
            float yPos = Camera.main.transform.position.y - 0.5f;
            float zPos = Camera.main.transform.position.z;
            Vector3 currPos = new Vector3(xPos, yPos, zPos);
            portalObject = Instantiate(portalPrefab, currPos, Quaternion.identity);
            // set flag to false again
            marvelPortalFlag = false;
        }

        // portal trajectory if portalObject exists
        if (portalObject != null)
        {
            // portal behaviour if opponent on screen
            if (isOpponentDetected)
            {
                portalObject.transform.position = Vector3.MoveTowards(portalObject.transform.position, opponentPosition, MARVEL_VELOCITY);
                float portalToOppDistance = Vector3.Distance(portalObject.transform.position, opponentPosition);

                portalObject.transform.rotation = Camera.main.transform.rotation;

                // If portal has reached opponent, destroy portalObject
                if (portalToOppDistance < 0.01f)
                {
                    Destroy(portalObject);
                    isWaitingForAnimation = false;
                    // Call function to generate explosion directly on opponent
                    GenerateExplosion();
                }
            }
            // portal behaviour if opponent not found
            // Unfreeze update since no damage being done
            else
            {
                isWaitingForAnimation = false;
                portalObject.transform.position = Vector3.MoveTowards(portalObject.transform.position, marvelMissTargetPos, 3 * MARVEL_VELOCITY);
                float portalToCameraDistance = Vector3.Distance(portalObject.transform.position, Camera.main.transform.position);

                portalObject.transform.rotation = Camera.main.transform.rotation;

                // If portal is >10m from us, destroy portalObject
                if (portalToCameraDistance > 10f)
                {
                    Destroy(portalObject);
                }
            }
        }
    }

    // Handle web object. Put this inside HandleMarvelObjects
    void HandleWebObject()
    {
        // Instantiate web object
        if (marvelWebFlag)
        {
            // Instantiate 0.5m below camera
            float xPos = Camera.main.transform.position.x;
            float yPos = Camera.main.transform.position.y - 0.5f;
            float zPos = Camera.main.transform.position.z;
            Vector3 currPos = new Vector3(xPos, yPos, zPos);
            webObject = Instantiate(webPrefab, currPos, Quaternion.identity);
            // set flag to false again
            marvelWebFlag = false;
        }

        // web trajectory if webObject exists
        if (webObject != null)
        {
            // web behaviour if opponent on screen
            if (isOpponentDetected)
            {
                webObject.transform.position = Vector3.MoveTowards(webObject.transform.position, opponentPosition, MARVEL_VELOCITY);
                float webToOppDistance = Vector3.Distance(webObject.transform.position, opponentPosition);

                webObject.transform.rotation = Camera.main.transform.rotation;
                webObject.transform.Rotate(90, 0, 0);

                // If web has reached opponent, destroy webObject
                if (webToOppDistance < 0.01f)
                {
                    Destroy(webObject);
                    isWaitingForAnimation = false;
                    // Call function to generate explosion directly on opponent
                    GenerateExplosion();
                }
            }
            // web behaviour if opponent not found
            // Unfreeze update since no damage being done
            else
            {
                isWaitingForAnimation = false;
                webObject.transform.position = Vector3.MoveTowards(webObject.transform.position, marvelMissTargetPos, 3 * MARVEL_VELOCITY);
                float webToCameraDistance = Vector3.Distance(webObject.transform.position, Camera.main.transform.position);

                webObject.transform.rotation = Camera.main.transform.rotation;
                webObject.transform.Rotate(90, 0, 0);

                // If web is >10m from us, destroy webObject
                if (webToCameraDistance > 10f)
                {
                    Destroy(webObject);
                }
            }
        }
    }

    // Handle punch object. Put this inside HandleMarvelObjects
    void HandlePunchObject()
    {
        // Instantiate punch object
        if (marvelPunchFlag)
        {
            // Instantiate 0.5m below camera
            float xPos = Camera.main.transform.position.x;
            float yPos = Camera.main.transform.position.y - 0.5f;
            float zPos = Camera.main.transform.position.z;
            Vector3 currPos = new Vector3(xPos, yPos, zPos);
            //punchObject = Instantiate(punchPrefab, currPos, Quaternion.identity);
            punchObject = Instantiate(punchPrefab, currPos, Quaternion.AngleAxis(90, Vector3.right));
            // set flag to false again
            marvelPunchFlag = false;
        }

        // Punch trajectory if punchObject exists
        if (punchObject != null)
        {
            // Punch behaviour if opponent on screen
            if (isOpponentDetected)
            {
                punchObject.transform.position = Vector3.MoveTowards(punchObject.transform.position, opponentPosition, MARVEL_VELOCITY);
                float punchToOppDistance = Vector3.Distance(punchObject.transform.position, opponentPosition);

                punchObject.transform.rotation = Camera.main.transform.rotation;
                punchObject.transform.Rotate(90, 0, 0);

                // If punch has reached opponent, destroy punchObject
                if (punchToOppDistance < 0.01f)
                {
                    Destroy(punchObject);
                    isWaitingForAnimation = false;
                    // Call function to generate explosion directly on opponent
                    GenerateExplosion();
                }
            }
            // Punch behaviour if opponent not found
            // Unfreeze update since no damage being done
            else
            {
                isWaitingForAnimation = false;
                punchObject.transform.position = Vector3.MoveTowards(punchObject.transform.position, marvelMissTargetPos, 3 * MARVEL_VELOCITY);
                float punchToCameraDistance = Vector3.Distance(punchObject.transform.position, Camera.main.transform.position);

                punchObject.transform.rotation = Camera.main.transform.rotation;
                punchObject.transform.Rotate(90, 0, 0);

                // If punch is >10m from us, destroy punchObject
                if (punchToCameraDistance > 10f)
                {
                    Destroy(punchObject);
                }
            }
        }
    }

    // Generate explosion AR effect on opponent marker
    void GenerateExplosion()
    {
        Instantiate(explosionPrefab, opponentPosition, Quaternion.identity);
    }

    // Handle grenade object inside Update function
    // Create and launch grenade at opponent if detected, else launch straight
    void HandleGrenadeObject()
    {
        // If throw grenade action is called, instantiate grenade
        if (throwGrenadeFlag)
        {
            // Instantiate 0.5m below camera
            float xPos = Camera.main.transform.position.x;
            float yPos = Camera.main.transform.position.y - 0.5f;
            float zPos = Camera.main.transform.position.z;
            Vector3 currPos = new Vector3(xPos, yPos, zPos);
            grenadeObject = Instantiate(grenadePrefab, currPos, Quaternion.identity);
            // set flag to false again
            throwGrenadeFlag = false;
        }

        // Grenade trajectory if grenadeObject exists
        if (grenadeObject != null)
        {
            // Grenade behaviour if opponent on screen
            if (isOpponentDetected)
            {
                grenadeObject.transform.position = Vector3.MoveTowards(grenadeObject.transform.position, opponentPosition, GRENADE_VELOCITY);
                float grenadeToOppDistance = Vector3.Distance(grenadeObject.transform.position, opponentPosition);

                // If grenade has reached opponent, destroy grenadeObject
                if (grenadeToOppDistance < 0.01f)
                {
                    Destroy(grenadeObject);
                    isWaitingForAnimation = false;
                    // Call function to generate explosion directly on opponent
                    GenerateExplosion();
                }
            }
            // Grenade behaviour if opponent not found
            // Unfreeze update since no damage being done
            else
            {
                isWaitingForAnimation = false;
                grenadeObject.transform.position = Vector3.MoveTowards(grenadeObject.transform.position, grenadeMissTargetPos, 2 * GRENADE_VELOCITY);
                float grenadeToCameraDistance = Vector3.Distance(grenadeObject.transform.position, Camera.main.transform.position);

                // If grenade is >10m from us, destroy grenadeObject
                if (grenadeToCameraDistance > 10f)
                {
                    Destroy(grenadeObject);
                }
            }            
        }
    }

    // Handles rendering of opponent's shield object
    void HandleOpponentShieldObject(int opponent)
    {
        int opponentIndex = (opponent == 1) ? 0 : 1;
        if (isOpponentDetected == true && shieldHp[opponentIndex] > 0)
        {
            opponentShieldObject.SetActive(true);
            opponentShieldObject.transform.position = opponentPosition;

            // Set opponent shield's rotation to match camera's rotation, then rotate shield 180 around y-axis.
            opponentShieldObject.transform.rotation = Camera.main.transform.rotation;
            opponentShieldObject.transform.Rotate(0, 180, 0);

        }
        else
        {
            if (opponentShieldObject != null)
            {
                opponentShieldObject.SetActive(false);
            }
        }
    }

    // Checks if opponent is on camera
    // Sets isOpponentDetected to TRUE/FALSE accordingly
    // Instantiate opponent's shield object if it doesn't already exist
    void DetectOpponent()
    {
        if (arTrackedImage != null)
        {
            opponentPosition = arTrackedImage.transform.position;
            if (arTrackedImage.trackingState == TrackingState.Tracking)
            {
                arTrackedImage.gameObject.SetActive(true);
                DisplayDebugText("image detected!! " + opponentPosition.ToString());
                isOpponentDetected = true;

                if (opponentShieldObject == null)
                {
                    // Instantiate at opponent position and (0, 180, 0) rotation.
                    opponentShieldObject = Instantiate(opponentShieldPrefab, opponentPosition, Quaternion.AngleAxis(180, Vector3.up));
                }
            }
            else
            {
                // hide arTrackedImage cube
                arTrackedImage.gameObject.SetActive(false);
                //arTrackedImage.enabled = false;
                //arTrackedImage.enabled = arTrackedImage.trackingState == TrackingState.Tracking ? true : false;
                //arTrackedImage = false;
                DisplayDebugText("image not found " + opponentPosition.ToString());
                isOpponentDetected = false;
            }
        }
    }

    // Display self shield overlay
    void DisplaySelfShieldOverlay(int player)
    {
        int playerIndex = (player == 1) ? 0 : 1;
        selfShieldOverlay.enabled = shieldHp[playerIndex] > 0;
    }

    // Display grenade count text
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
        playerIdentifierText.text = "PLAYER " + player.ToString();
    }

    // Return TRUE if HP/SHIELD unit should be displayed, else return FALSE
    bool IsHealthUnitDisplayed(int health, int unitIndex)
    {
        return (unitIndex * 10) < health;
    }

    // Event which is called when action == PUNCH || action == WEB || action == PORTAL || action == HAMMER || action == SPEAR
    // Set respective flags to true
    // Set camera transform at point of attack
    void MarvelAttackEvent(string action)
    {
        if (action == PUNCH)
        {
            marvelPunchFlag = true;
        }
        else if (action == WEB)
        {
            marvelWebFlag = true;
        }
        else if (action == PORTAL)
        {
            marvelPortalFlag = true;
        }
        else if (action == HAMMER)
        {
            marvelHammerFlag = true;
        }
        else if (action == SPEAR)
        {
            marvelSpearFlag = true;
        }

        // returns a point exactly 20 meters in front of the camera, at centre of screen (0.5, 0.5):
        marvelMissTargetPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 20f));
    }

    // Event which is called when action == GRENADE
    // Set flag to true to indicate grenade should be thrown
    // Set camera orientation for when grenade was thrown
    void ThrowGrenadeEvent()
    {
        throwGrenadeFlag = true;

        // Edit this to change grenadeMissStartpoint
        //float xTargetMiss = Camera.main.transform.position.x;
        //float yTargetMiss = Camera.main.transform.position.y;
        //float zTargetMiss = Camera.main.transform.position.z;
        //grenadeMissEndpoint = new Vector3(xTargetMiss, yTargetMiss, zTargetMiss);
        //grenadeMissStartTransform = Camera.main.transform;

        // returns a point exactly 20 meters in front of the camera, at centre of screen (0.5, 0.5):
        grenadeMissTargetPos = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 20f));
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
        // Hit
        if (action == SHOOT_HIT)
        {
            // generate muzzle flash
            GenerateMuzzleFlash();
            // generate sparks on opponent
            GenerateBulletSparks();
        }
        // Miss
        if (action == SHOOT_MISS)
        {
            // generate muzzle flash
            GenerateMuzzleFlash();
        }

        // AR Effect for reload
        if (action == RELOAD)
        {
            StartCoroutine(ReloadWaiter());   
        }

    }

    // Generate muzzle flash AR effect
    void GenerateMuzzleFlash()
    {
        Instantiate(muzzleFlashPrefab, Camera.main.ViewportToWorldPoint(new Vector3(0.25f, 0.40f, 0.1f)), Quaternion.identity);
    }

    // Generate bullet sparks AR effect
    void GenerateBulletSparks()
    {
        if (isOpponentDetected == true)
        {
            Instantiate(bulletSparksPrefab, opponentPosition, Quaternion.identity);
        }
    }

    // Execute reload animation
    IEnumerator ReloadWaiter()
    {
        ammoText.color = Color.red;
        isWaitingForAnimation = true;
        yield return new WaitForSeconds(2);
        ammoText.color = Color.white;
        isWaitingForAnimation = false;
    }

    /*
     * PRE-INTEGRATION
    - Game Engine calls this function to send action to visualiser
    - Handle what happens on visualiser for each action
    - This means opponent's actions should NOT be handled here since we do not 
    want to display their actions on OUR visualiser.
    - Return hit/miss to game engine for actions that require it
    - Hit = 1, miss = 0, ignore = -1
    - Also FREEZE UPDATE to wait for animation
    */
    public int HandleActionFromGameEngine(string action, int player)
    {
        if (player != PLAYER)
        {
            return -1;
        }

        // Actions that require tracking
        // Grenade
        if (action == GRENADE)
        {
            ThrowGrenadeEvent();
            // freeze update /////////////////////////////
            isWaitingForAnimation = true;

            // If opponent detected, return hit, display grenade flying to opp
            // Else, return miss, display grenade flying straight
            return isOpponentDetected ? 1 : 0;
        }
        // Marvel actions
        else if (action == PUNCH || action == WEB || action == PORTAL ||
            action == HAMMER || action == SPEAR)
        {
            MarvelAttackEvent(action);
            // freeze update /////////////////////////////
            isWaitingForAnimation = true;

            return isOpponentDetected ? 1 : 0;
        }
        // Logout action
        else if (action == EXIT)
        {
            logoutOverlay.gameObject.SetActive(true);

            blackScreen.gameObject.SetActive(false);

            return isOpponentDetected ? 1 : 0;
        }

        // Other actions (shoot, reload)
        else
        {
            DisplayAction(action);
            return isOpponentDetected ? 1 : 0;
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

    public void SwitchPlayerPov()
    {
        if (PLAYER == 1)
        {
            PLAYER = 2;
            OPPONENT = 1;

            ChangeScene.player = 2;
        }
        else if (PLAYER == 2)
        {
            PLAYER = 1;
            OPPONENT= 2;

            ChangeScene.player = 1;
        }
    }

    // Closes logout screen, go back to game
    public void CloseLogoutScreen()
    {
        logoutOverlay.SetActive(false);

        blackScreen.gameObject.SetActive(true);
    }

    // Opponent detection using vuforia (better range than ARCore)
    void VuforiaDetection()
    {
        if (vuforiaTrackedObject != null)
        {
            opponentPosition = vuforiaTrackedObject.gameObject.transform.position;

            if (vuforiaTrackedObject.gameObject.activeSelf == true)
            {
                DisplayDebugText("vuforia detected");
                isOpponentDetected = true;

                if (opponentShieldObject == null)
                {
                    // Instantiate at opponent position and (0, 180, 0) rotation.
                    opponentShieldObject = Instantiate(opponentShieldPrefab, opponentPosition, Quaternion.AngleAxis(180, Vector3.up));
                }
            }
            else
            {
                DisplayDebugText("vuforia not found");
                isOpponentDetected = false;
            }
        }
    }

    public void VuforiaOnTargetFound()
    {
        vuforiaTrackedObject.gameObject.SetActive(true);
    }

    public void VuforiaOnTargetLost()
    {
        vuforiaTrackedObject.gameObject.SetActive(false);
    }
}
