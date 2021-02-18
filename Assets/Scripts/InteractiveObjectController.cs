using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObjectController : MonoBehaviour
{
    private string projectileTag = "Projectile";

    private bool collideWithProjectile(Collision2D gameObject)
    {
        return (gameObject.gameObject.tag == projectileTag)
            ? true
            : false;
    }

    void OnCollisionEnter2D(Collision2D otherObj)
    {
        if (collideWithProjectile(otherObj))
        {
            Destroy(gameObject);
        }
    }
}
