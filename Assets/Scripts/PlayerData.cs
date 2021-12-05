using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public PlayerNumber playerNumber;

    public enum PlayerNumber {
        NoPlayer,
        PlayerOne,
        PlayerTwo
    }
}
