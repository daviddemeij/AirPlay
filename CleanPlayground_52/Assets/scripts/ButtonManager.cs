using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
public class ButtonManager : MonoBehaviour {
    public void newGame(string gameLevel)
    {
        SceneManager.LoadScene(gameLevel);
    }
    public void exitGame()
    {
        Application.Quit();
    }
}
