using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerLunge : MonoBehaviour
{
    private PlayerController player;
    private Transform playerT;
    private Rigidbody playerRB;

    /// <summary>
    /// True if this lunge has been cleared already and shouldn't clear the lunge's VFX/audio.
    /// </summary>
    public bool lungeCleared = false;

    private void Update()
    {
        transform.position = playerT.position;

        if(playerRB.velocity.magnitude > 0)
        {
            var playerDir = playerRB.velocity.normalized;
            transform.rotation = Quaternion.LookRotation(playerDir);
        }
    }

    private void OnDestroy()
    {
        if(!lungeCleared)
        {
            player.ClearLungeEffect();
        }
    }

    public void SetOwningPlayer(PlayerController owner)
    {
        player = owner;
        playerT = owner.transform;
        playerRB = owner.GetComponent<Rigidbody>();
    }
}
