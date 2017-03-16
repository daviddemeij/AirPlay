using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class ButtonManager : MonoBehaviour {
    public Text sliderText;
    public Text levelText;
    public Image[] stars;
    public Image currentLevelImage;
    public Sprite[] starsHighlighted;
    public Sprite[] starsGray;
    public Slider stepsSlider;
    public Button[] levels;
    public int[] unlockLevel;
    public Button[] nrPlayersButtons;
    private int oldNrPlayers = 1;
    private int nrPlayers = 0;
    public Color pressedColor;
    void Start()
    {
        levels[0].interactable = true;
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
        float nextLevel = 0;
        currentLevelImage.sprite = starsHighlighted[0];
        for (int i = 1; i < unlockLevel.Length; i++)
        {
            if (stepsSlider.value * 100 >= unlockLevel[i])
            {
                stars[i].sprite = starsHighlighted[i];
                currentLevelImage.sprite = starsHighlighted[i];
            }
            else
            {
                stars[i].sprite = starsGray[i];
                if(nextLevel == 0)
                {
                    nextLevel = unlockLevel[i] - (stepsSlider.value * 100);
                }
            }
 
            levels[i].interactable = (stepsSlider.value * 100 >= unlockLevel[i]);
            print(stepsSlider.value * 100);
        }
        
      
        if (nextLevel == 0) { levelText.text = "Je bent nu level         Gefeliciteerd!"; }
        else { levelText.text = "Je bent nu level         nog " + nextLevel.ToString("0") + " stappen voor het volgende level!"; }
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
