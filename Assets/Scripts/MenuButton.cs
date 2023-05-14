using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuButton : MonoBehaviour
{
    private Image buttonImage;
    private Color startingColor;
    [SerializeField] private Color hoverColor;
    

    // Start is called before the first frame update
    void Start()
    {
        buttonImage = GetComponent<Image>();
        startingColor = buttonImage.color;
    }

    private void OnMouseOver()
    {
        SetButtonColor(hoverColor);
    }

    private void OnMouseExit()
    {
        SetButtonColor(startingColor);
    }

    private void SetButtonColor(Color color)
    {
        buttonImage.color = color;
    }
}
