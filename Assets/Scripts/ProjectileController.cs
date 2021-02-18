using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    void OnCollisionEnter2D(Collision2D otherObj)
    {
        destroyProjectile(0);
    }

    void destroyProjectile(float delay)
    {
        Destroy(gameObject, delay);
    }
}
