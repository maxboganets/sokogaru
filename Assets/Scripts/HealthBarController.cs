using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarController : MonoBehaviour
{
    public Slider slider;

    private int maxHealth;

    public void Initiate(int maxHealth)
    {
        this.maxHealth = maxHealth;
        slider.maxValue = this.maxHealth;
        slider.value = this.maxHealth;
    }

    public void SetHealth(int health)
    {
        slider.value = health;
    }
}
