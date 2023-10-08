using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{

    private static int DEBUG_MODE = 0;
    private static int PLAY_MODE = 1;

    private static int PLAYER_1 = 1;
    private static int PLAYER_2 = 2;

    // Depending on which mode, display different things in HudController.
    public static int gamemode = 1;
    public static int player = 1;

    // Go to different scene
    public void MoveToScene(int sceneId)
    {
        // For main menu scene
        if (sceneId == 0)
        {
            SceneManager.LoadScene(sceneId);
        }
        // For AR scene, debug mode
        else if (sceneId == 1)
        {
            var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
            xrManagerSettings.DeinitializeLoader();
            SceneManager.LoadScene(sceneId); // reload current scene
            xrManagerSettings.InitializeLoaderSync();
        }
        // For AR scene, play mode
        else if (sceneId == 2)
        {
            var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
            xrManagerSettings.DeinitializeLoader();
            SceneManager.LoadScene(sceneId); // reload current scene
            xrManagerSettings.InitializeLoaderSync();
        }
        else
        {
            SceneManager.LoadScene(sceneId);
        }
        
    }

    // Set gamemode: DEBUG or PLAY
    // Set player: 1 or 2
    public void SetModeAndPlayer(string buttonPressed)
    {
        if (buttonPressed == "debug")
        {
            gamemode = DEBUG_MODE;
            player = PLAYER_1;
        }
        else if (buttonPressed == "p1")
        {
            gamemode = PLAY_MODE;
            player = PLAYER_1;
        }
        else if (buttonPressed == "p2")
        {
            gamemode = PLAY_MODE;
            player = PLAYER_2;
        }
    }
}
