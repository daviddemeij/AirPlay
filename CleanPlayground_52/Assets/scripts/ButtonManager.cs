using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ButtonManager : MonoBehaviour {
    public Text sliderText;
    public Slider stepsSlider;
    public Button[] levels;
    public int[] unlockLevel;
    public Button[] nrPlayersButtons;
    private int oldNrPlayers = 1;
    private int nrPlayers = 0;
    public Color pressedColor;
    void Start()
    {
        nrPlayers = PlayerPrefs.GetInt("NrPlayers");
        print(nrPlayers);
        stepsSlider.value = PlayerPrefs.GetFloat("stappen");
        if (nrPlayers == 0)
        {
            nrPlayers = 4;
            setNrPlayers(4);
        }
        else
        {
            setNrPlayers(nrPlayers);
        }
    }
    public void exitGame()
    {
        Application.Quit();
    }
    public void updateSlider() {
        sliderText.text = "Stappen: " + (stepsSlider.value*100).ToString("0");
        PlayerPrefs.SetFloat("stappen",stepsSlider.value);
        if (nrPlayers > 1)
        {
            levels[0].interactable = false;
            for (int i = 1; i < unlockLevel.Length; i++)
            {
                levels[i].interactable = (stepsSlider.value * 100 >= unlockLevel[i]);
                print(stepsSlider.value * 100);
            }
        }
        else
        {
            for (int i = 1; i < unlockLevel.Length; i++)
            {
                levels[i].interactable = false;
            }
            levels[0].interactable = true;
        }

    }
    public void setLevel(int level)
    {
        PlayerPrefs.SetInt("Level", level);
        SceneManager.LoadScene("airplay");
    }
    public void setNrPlayers(int nrPlay)
    {
        nrPlayersButtons[nrPlayers - 1].image.color = Color.white;
        PlayerPrefs.SetInt("NrPlayers", nrPlay);
        PlayerPrefs.SetInt("NrTaggers", nrPlay / 2);
        nrPlayers = nrPlay;
        nrPlayersButtons[nrPlayers - 1].Select();
        nrPlayersButtons[nrPlayers - 1].image.color = pressedColor;
        updateSlider();
    }

}
