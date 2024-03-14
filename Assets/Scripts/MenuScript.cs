using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuScript : MonoBehaviour
{
    public static bool gameStarted = false;
    public static Button newGameButton;
    public static Button resumeButton;
    public static GameObject menu0;

    public void getComponents()
    {
        newGameButton = GameObject.Find("NewGame").GetComponent<Button>();
        resumeButton = GameObject.Find("Resume").GetComponent<Button>();
        menu0 = GameObject.Find("Menu0");
}
    public void onClickNewGame()
    {
        if (!gameStarted)
        {
            gameStarted = true;
            toggleResume(true);
            onClickResume();
        }
        else
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void onClickResume()
    {
        menu0.SetActive(!menu0.activeInHierarchy);
        gameStarted = true;
        UIManager.soundUI.Play();
    }

    public void onClickExitGame()
    {
        Application.Quit();
    }
    public void toggleResume(bool state)
    {
        resumeButton.enabled = state;
        resumeButton.gameObject.GetComponent<Image>().enabled = state;
        resumeButton.gameObject.GetComponentInChildren<Text>().enabled = state;
    }
    private void Awake()
    {
        getComponents();
        gameStarted = false;
        toggleResume(false);
    }
}
