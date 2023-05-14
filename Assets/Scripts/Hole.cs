using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hole : MonoBehaviour
{
    // store the width and height for case of multipe dimension holes
    // this will be needed to calculate the tiles we need to make uninteractable
    [SerializeField] private Vector2 dimensions;
    private Vector2 gridPos;

    // Start is called before the first frame update
    public void Init()
    {
        gridPos = GridManager.WorldToGrid(transform.position);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public Vector2 GetDimensions()
    {
        return dimensions;
    } 

    public Vector2 GetGridPos()
    {
        return gridPos;
    }
}
