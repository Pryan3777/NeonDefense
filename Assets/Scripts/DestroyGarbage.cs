using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyGarbage : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // destroys objects that get out of the camera view
        // depending on how intensive this is, may need to refactor eventually

        if (collision.gameObject.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.Destroy();
        }
    }
}
