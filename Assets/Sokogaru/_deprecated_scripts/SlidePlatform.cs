using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidePlatform : MonoBehaviour
{
    [SerializeField] SlideDirection slideDirection = SlideDirection.slideX;
    [SerializeField] bool positiveDirectionAtStart = true;
    [SerializeField] float distance = 1;
    [SerializeField] float speed = 0.5f;

    private string playerTag = "Player";
    private enum SlideDirection
    {
        slideX,
        slideY
    };

    private bool positiveDirection;
    private Vector2 startPosition;

    private void Start()
    {
        this.startPosition = gameObject.transform.position;
        this.positiveDirection = this.positiveDirectionAtStart;
    }

    void Update()
    {
        if (this.slideDirection == SlideDirection.slideX)
        {
            // check the border is not reached
            float currentPosX = gameObject.transform.position.x;
            float startPosX = startPosition.x;
            float currentDistance = this.positiveDirection
                ? currentPosX - startPosX
                : startPosX - currentPosX;
            if (currentDistance >= this.distance)
            {
                this.positiveDirection = !this.positiveDirection;
            }
            Vector2 vectorDirection = this.positiveDirection ? Vector2.right : Vector2.left;
            transform.Translate(vectorDirection * this.speed * Time.deltaTime);
        } else
        {
            // check the border is not reached
            float currentPosY = gameObject.transform.position.y;
            float startPosY = startPosition.y;
            float currentDistance = this.positiveDirection
                ? currentPosY - startPosY
                : startPosY - currentPosY;
            if (currentDistance >= this.distance)
            {
                this.positiveDirection = !this.positiveDirection;
            }
            Vector2 vectorDirection = this.positiveDirection ? Vector2.up : Vector2.down;
            transform.Translate(vectorDirection * this.speed * Time.deltaTime);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == this.playerTag)
        {
           collision.collider.transform.SetParent(gameObject.transform);
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.tag == this.playerTag)
        {
            collision.collider.transform.SetParent(null);
        }
    }
}
