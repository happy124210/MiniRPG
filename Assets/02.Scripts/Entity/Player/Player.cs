using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController controller { get; private set; }
    public StatHandler StatHandler { get; private set; }

    private void Awake()
    {
        controller = GetComponent<PlayerController>();
        StatHandler = GetComponent<StatHandler>();
        
        CharacterManager.Register(this);
    }
}
