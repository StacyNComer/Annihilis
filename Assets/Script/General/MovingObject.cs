using UnityEngine;

/// <summary>
/// The base class for a GameObject that can translate its location to/from a give position (e.g. A sliding door).
/// </summary>
public class MovingObject : MonoBehaviour
{
    /// <summary>
    /// An enum type for whether the MovingObject should be moving toward or away from its original position. The values are made be coefficients for moveDelta that control whether moveAlpha increases or decreases during interpolation.
    /// </summary>
    private enum MovementMode
    {
        /// <summary>
        /// The GameObject is moving to its original position (The alpha should decrease during interpolation).
        /// </summary>
        ToOriginal = -1,
        /// <summary>
        /// The GameObject is moving to its give target position (The alpha should increase during interpolation).
        /// </summary>
        ToTarget = 1
    }

    /// <summary>
    /// The relative position for this GameObject to move to.
    /// </summary>
    [SerializeField, Tooltip("This position is relative to the attached GameObject.")]
    private Vector3 targetPos;
    /// <summary>
    /// The time in seconds for the GameObject to complete its movement. This should be set using SetMoveTime() so that moveDelta is set properly.
    /// </summary>
    [SerializeField]
    private float moveTime = .5f;

    private Vector3 worldTargetPos;
    /// <summary>
    /// How much moveAlpha should change per second so that this GameObject reaches its destination in moveTime seconds. Calculate at Start.
    /// </summary>
    private float moveDelta;
    /// <summary>
    /// The location this GameObject began play at.
    /// </summary>
    private Vector3 originalPos;
    /// <summary>
    /// The alpha used to interpolate this GameObject's position.
    /// </summary>
    private float moveAlpha;
    private MovementMode movementMode = MovementMode.ToOriginal;

    // Start is called before the first frame update
    void Start()
    {
        originalPos = transform.position;
        worldTargetPos = originalPos + targetPos;

        //Calculate delta
        CalculateMoveDelta();
    }

    // Update is called once per frame
    void Update()
    {
        if((movementMode == MovementMode.ToOriginal && moveAlpha > 0) || (movementMode == MovementMode.ToTarget && moveAlpha < 1))
        {
            //Increases moveAlpha at a rate of moveDelta/sec. If the movementMode is "toOriginal 
            moveAlpha += Time.deltaTime * moveDelta * (int)movementMode;

            //Lerp this GameObject toward its intended position
            transform.position = Vector3.Lerp(originalPos, worldTargetPos, moveAlpha);
        }
    }

    /// <summary>
    /// Calculates how much moveAlpha should change each second to achieve the desired moveTime. This value is saved as this object's moveDelta property.
    /// </summary>
    private void CalculateMoveDelta()
    {
        moveDelta = moveTime == 0 ? 1000 : (1 / moveTime);
    }

    /// <summary>
    /// Sets how long it should take this object to move to/from its target location. This also recalculates the moveDelta.
    /// </summary>
    private void SetMoveTime(float time)
    {
        moveTime = time;
        CalculateMoveDelta();
    }

    /// <summary>
    /// Toggles this object's intended position between its original and target location.
    /// </summary>
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

    /// <summary>
    /// Toggles the position after applying a new moveTime.
    /// </summary>
    /// <param name="newMoveTime"></param>
    public void TogglePosition(float newMoveTime)
    {
        SetMoveTime(newMoveTime);

        TogglePosition();
    }
}
