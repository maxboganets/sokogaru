using UnityEngine;

public class WeaponController : MonoBehaviour
{
    private int attackPower = 1;

    public void SetAttackPower(int attackPower)
    {
        this.attackPower = attackPower;
    }

    public int GetAttackPower()
    {
        return this.attackPower;
    }
}
