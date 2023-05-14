using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] private Slider blackHoleSlider;
    [SerializeField] private Text blackHoleCountText;

    // Start is called before the first frame update
    void Start()
    {
        UpdateSliderText();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    // load the game
    public void LoadGame()
    {
        GridManager.SetNumBlackHoles((int)blackHoleSlider.value);
        SceneManager.LoadScene("main");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void UpdateSliderText()
    {
        blackHoleCountText.text = blackHoleSlider.value.ToString();
    }
}
