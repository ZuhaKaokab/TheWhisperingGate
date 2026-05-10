using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    // Start is called before the first frame update
   public void NewGame()
    {
        SceneManager.LoadSceneAsync(1);
    }
    public void Continue()
    {

    }
    public void Exit()
    {
        Application.Quit();
    }
}
