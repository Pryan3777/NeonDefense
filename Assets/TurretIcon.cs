using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TurretIcon : MonoBehaviour
{
    [SerializeField] private Text destroyedCount;
    [SerializeField] private Image image;

    // Start is called before the first frame update
    public void Init(int numDestroyed, Color imageColor)
    {
        destroyedCount.text = numDestroyed.ToString();
        imageColor.a = .8f;
        image.color = imageColor;
    }
}
