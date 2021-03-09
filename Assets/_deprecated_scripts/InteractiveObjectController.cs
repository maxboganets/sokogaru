using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractiveObjectController : MonoBehaviour
{
    [SerializeField] int maxHealth = 3;
    [SerializeField] int hitPower = 2;
    [SerializeField] Color onHitColor = new Color(0.7f, 0.7f, 0.7f, 1);

    private string projectileTag = "Projectile";
    private float onHitAnimationTime = 0.2F;
    private float dieAnimationTime = 0.25F;
    private int magnitude = 1000;
    private Color originalTintColor;
    private int currentHealth;

    private void Start()
    {
        this.originalTintColor = gameObject.GetComponent<SpriteRenderer>().color;
        this.UpdateHealth(this.maxHealth);
    }

    private IEnumerator AnimateOnHit()
    {
        gameObject.GetComponent<Renderer>().material.color = this.onHitColor;
        yield return new WaitForSeconds(this.onHitAnimationTime);
        gameObject.GetComponent<Renderer>().material.color = this.originalTintColor;
    }

    private bool GotHit(Collision2D collisionObject)
    {
        // It's not a weapon and can't hit hard
        if (!collisionObject.gameObject.GetComponent<WeaponController>())
        {
            return false;
        }
        return (collisionObject.gameObject.tag == projectileTag)
            ? true
            : false;
    }

    public int GetHitPower()
    {
        return this.hitPower;
    }

    private int GetHealth()
    {
        return this.currentHealth;
    }

    private void UpdateHealth(int healthDelta)
    {
        this.currentHealth += healthDelta;
    }

    void OnCollisionEnter2D(Collision2D otherObj)
    {
        if (this.GotHit(otherObj))
        {
            // Decrease current health, stun, maybe die
            int attackPower = otherObj.gameObject.GetComponent<WeaponController>().GetAttackPower();
            this.UpdateHealth(-attackPower);
            // push object in opposite direction from the hit
            var force = transform.position - otherObj.transform.position;
            force.Normalize();
            GetComponent<Rigidbody2D>().AddForce(force * magnitude);
            // Animate
            StartCoroutine(this.AnimateOnHit());
            if (this.GetHealth() <= 0)
            {
                StartCoroutine(this.DoDie());
            }
        }
    }

    private IEnumerator DoDie()
    {
        gameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Kinematic;
        gameObject.GetComponent<Collider2D>().enabled = false;


        var explodableComponent = gameObject.GetComponent<Explodable>();
        if (explodableComponent)
        {
            explodableComponent.explode();
            //ExplosionForce ef = GameObject.FindObjectOfType<ExplosionForce>();
            //ef.doExplosion(transform.position);
        }


        yield return new WaitForSeconds(dieAnimationTime);
        Destroy(gameObject);
    }
}
