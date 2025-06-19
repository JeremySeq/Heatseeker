using UnityEngine;

public class MainMenuUI : MonoBehaviour
{
    public void StartGame()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
    
    public void OpenSettings()
    {
        // Implement settings menu logic here
        Debug.Log("Settings menu opened.");
    }
    
    public void QuitGame()
    {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the editor
        #else
        Application.Quit(); // Quit the application in a build
        #endif
    }
}
