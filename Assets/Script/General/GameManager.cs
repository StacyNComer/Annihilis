using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _gameManager;

    private static PlayerController _player;

    public static GameManager gameManager
    {
        private set { _gameManager = value; }
        get => _gameManager;
    }

    public static PlayerController Player
    {
        private set { _player = value; }
        get => _player;
    }

    public static float enemyMultiplier = 1;

    private void Awake()
    {
        gameManager = this;
        Player = FindAnyObjectByType<PlayerController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        enemyMultiplier = 1;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public static int GetRaycastLayerMask()
    {
        return ~LayerMask.GetMask("Ignore Raycast", "Projectile");
    }
}
