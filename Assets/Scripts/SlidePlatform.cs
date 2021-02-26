using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlidePlatform : MonoBehaviour
{
    [SerializeField] SlideDirection slideDirection = SlideDirection.slideX;
    [SerializeField] float distance = 1;
    [SerializeField] float speed = 0.5f;

    private enum SlideDirection
    {
        slideX,
        slideY
    };
    private bool positiveDirection = true;

    private Vector2 startPosition;

    private void Start()
    {
        this.startPosition = gameObject.transform.position;
    }

    void Update()
    {
        if (this.slideDirection == SlideDirection.slideX)
        {
            // check the border is not reached
            float currentPosX = gameObject.transform.position.x;
            float startPosX = startPosition.x;
            if (Mathf.Abs(startPosX - currentPosX) >= this.distance)
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
            if (Mathf.Abs(startPosY - currentPosY) >= this.distance)
            {
                this.positiveDirection = !this.positiveDirection;
            }
            Vector2 vectorDirection = this.positiveDirection ? Vector2.up : Vector2.down;
            transform.Translate(vectorDirection * this.speed * Time.deltaTime);
        }
    }
}
