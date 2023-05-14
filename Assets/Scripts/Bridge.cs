using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bridge : Item
{
    private Transform _wallHolder;

    // Start is called before the first frame update
    void Start()
    {
        _wallHolder = GridManager.Instance.GetWallHolder();

        if (_wallHolder != null)
            transform.SetParent(_wallHolder);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
