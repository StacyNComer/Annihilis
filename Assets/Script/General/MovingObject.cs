using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingObject : MonoBehaviour
{
    private enum MovementMode
    {
        ToOriginal = -1,
        ToTarget = 1
    }

    /// <summary>
    /// The target relative position for this actor to move to.
    /// </summary>
    [SerializeField]
    private Vector3 targetPos;
    /// <summary>
    /// The time in seconds for the gameObject to complete its movement.
    /// </summary>
    [SerializeField]
    private float moveTime = .5f;

    private Vector3 worldTargetPos;
    /// <summary>
    /// How much mveAlpha should change per second. Calculate at Start.
    /// </summary>
    private float delta;
    private Vector3 originalPos;
    private float moveAlpha;
    private MovementMode movementMode = MovementMode.ToOriginal;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = transform.position;
        worldTargetPos = originalPos + targetPos;

        //Calculate delta
        CalculateDelta();
    }

    // Update is called once per frame
    void Update()
    {
        if((movementMode == MovementMode.ToOriginal && moveAlpha > 0) || (movementMode == MovementMode.ToTarget && moveAlpha < 1))
        {
            moveAlpha += Time.deltaTime * delta * (int)movementMode;

            transform.position = Vector3.Lerp(originalPos, worldTargetPos, moveAlpha);
        }
    }

    private void CalculateDelta()
    {
        delta = moveTime == 0 ? 1000 : (1 / moveTime);
    }

    private void SetMoveTime(float time)
    {
        moveTime = time;
        CalculateDelta();
    }

    public void TogglePosition()
    {
        if(movementMode == MovementMode.ToOriginal)
        {
            movementMode = MovementMode.ToTarget;
        } else
        {
            movementMode = MovementMode.ToOriginal;
        }
    }

    public void TogglePosition(float newMoveTime)
    {
        SetMoveTime(newMoveTime);

        TogglePosition();
    }
}
