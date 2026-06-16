using System;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    public event Action OnDied;

    public void Die()
    {
        OnDied?.Invoke();
    }
}
