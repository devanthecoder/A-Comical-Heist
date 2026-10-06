using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            // DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public Animator UIAnimator; // Reference to the UI Animator

    public void GotCaught()
    {
        UIAnimator.SetTrigger("Caught"); // Play the fade-out animation
    }

    public void Retry()
    {
        UIAnimator.SetTrigger("Fadeout");
        // Reload the current scene
        Invoke("LoadCurrentScene", 1f); // Delay the scene reload to allow the fade-out animation to play
    }

    public void QuitToMainMenu()
    {
        UIAnimator.SetTrigger("Fadeout");
        // Load the main menu scene
        Invoke("LoadMainMenu", 1f); // Delay the scene load to allow the fade-out animation to play
    }

    void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
    }
    void LoadCurrentScene()
    {
        Debug.Log("Loading current scene...");
        SceneManager.LoadScene(1);
        
    }

    public void Play()
    {
        UIAnimator.SetTrigger("Fadeout");
        Invoke("LoadCurrentScene", .5f);
    }
    public void Exit()
    {
        Application.Quit();
    }
}
