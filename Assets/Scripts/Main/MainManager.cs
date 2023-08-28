using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainManager : MonoBehaviour {

    
    public void PlayGame() {
        SceneManager.LoadScene("Gameplay");
    }

    public void Option() {
        Debug.Log("Create a scene for Option");
    }

    public void QuitGame() {
        Application.Quit();
    }
}
