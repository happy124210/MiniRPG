using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController Controller { get; private set; }
    public StatHandler StatHandler { get; private set; }
    public Inventory Inventory { get; private set; }

    private void Awake()
    {
        Controller = GetComponent<PlayerController>();
        StatHandler = GetComponent<StatHandler>();
        Inventory = GetComponent<Inventory>();
        
        if (Controller == null) Debug.LogError("Controller is null");
        if (StatHandler == null) Debug.LogError("StatHandler is null");
        if (Inventory == null) Debug.LogError("Inventory is null");
        
        CharacterManager.Register(this);
    }
}
