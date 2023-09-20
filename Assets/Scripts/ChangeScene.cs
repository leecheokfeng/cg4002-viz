using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChangeScene : MonoBehaviour
{
    // Go to different scene
    public void MoveToScene(int sceneId)
    {
        // For AR scene
        if (sceneId == 0)
        {
            var xrManagerSettings = UnityEngine.XR.Management.XRGeneralSettings.Instance.Manager;
            xrManagerSettings.DeinitializeLoader();
            SceneManager.LoadScene(sceneId); // reload current scene
            xrManagerSettings.InitializeLoaderSync();
        }
        // For UI scene
        else
        {
            SceneManager.LoadScene(sceneId);
        }
        
    }
}
