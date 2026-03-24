using UnityEngine;
using System;

public class PlayerMoney : MonoBehaviour
{
    public int currentMoney = 0;

    public event Action OnMoneyChanged;

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        OnMoneyChanged?.Invoke();
        Debug.Log("Š‹à: " + currentMoney);
    }
}