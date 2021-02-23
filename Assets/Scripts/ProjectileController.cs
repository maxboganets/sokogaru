using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] float ExplosionDelay = 0.15F;

    public int attackPower = 1;

    void OnCollisionEnter2D(Collision2D otherObj)
    {
        StartCoroutine(this.doExplode());
    }

    private IEnumerator doExplode () {
        // Run explosion animation
        gameObject.GetComponent<Animator>().SetBool("explode", true);
        // Remove Rigidbody
        Destroy(transform.GetComponent<Rigidbody>());
        // Stop Moving
        gameObject.GetComponent<Rigidbody2D>().velocity = new Vector2(0, 0);
        // Destroy object after delay
        yield return new WaitForSeconds(ExplosionDelay);
        destroyProjectile(0);
    }

    void destroyProjectile(float delay)
    {
        Destroy(gameObject, delay);
    }

    public void SetAttackPower(int attackPower)
    {
        this.attackPower = attackPower;
    }

    public int GetAttackPower()
    {
        return this.attackPower;
    }
}
