using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCam : MonoBehaviour
{
    #region SINGLETON
    public static MainCam Instance;
    void Awake() => Instance = this;
    #endregion


    // Update is called once per frame
    void Update()
    {
        
    }
}
