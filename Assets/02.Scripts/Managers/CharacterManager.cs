using System;
using UnityEngine;

public class CharacterManager : MonoBehaviour
{
    public static Player Player { get; private set; }
    public static void Register(Player player) => Player = player;
}
