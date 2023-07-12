using UnityEngine;

/// <summary>
/// The base class for floating, immobile turrets that which sweep a laser into the player. Includes methods for slowing the turret's turning speed as the player gets farther away so that the laser is not impossible to outrun
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public abstract class LaserTurretAIBase : EnemyAIBase
{
    protected enum LaserRenderingMode
    {
        Disabled,
        Targeting,
        Firing
    }

    /// <summary>
    /// The turret's normal turning speed in degrees/s.
    /// </summary>
    [Header("Base Laser Turret AI")]
    [SerializeField]
    protected float baseTurnSpeed;
    /// <summary>
    /// The max speed in units/s that the endpoint of the laser may follow the player at. Used to slow the turret's turn speed at long ranges so the player can still out run it.
    /// </summary>
    [SerializeField, Tooltip("The max speed in units/s that the endpoint of the laser may may follow the player at. Used to slow the turret's turn speed at long ranges so the player can still out run it.")]
    protected float maxAngularSpeed = 8;
    [SerializeField, Tooltip("The seconds between each instance of damage dealt by the laser.")]
    protected float damageInterval;
    [SerializeField]
    protected float maxRange = 100;
    [SerializeField]
    protected EnemyAttackData laserAttack;

    protected float damageIntervalTracker;
    /// <summary>
    /// NOTE: By default, the line renderer should be set to use local space. If you use SetLaserEndpoint to move the laser's endpoint, this must be accounted for.
    /// </summary>
    protected LineRenderer laserRenderer;
    /// <summary>
    /// The distance at which the laser's turning speed must be slowed to prevent it from being impossible to outrun. Calculated in start.
    /// </summary>
    protected float turnDampeningDistance;

    new protected void Start()
    {
        base.Start();

        laserRenderer = GetComponent<LineRenderer>();

        turnDampeningDistance = (360 * maxAngularSpeed) / (2 * Mathf.PI * baseTurnSpeed);
    }

    protected void Update()
    {
        if (damageIntervalTracker > 0)
        {
            damageIntervalTracker -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Aims the laser turret at the player using its base speed. Dampens the rotation speed if the player is far away so that the laser doesn't outrun the player.
    /// </summary>
    protected void AimAtPlayerWithDampening()
    {
        if (Vector3.Distance(transform.position, playerT.position) >= turnDampeningDistance)
        {
            var slowedTurnSpeed = (360 * maxAngularSpeed) / (2 * Mathf.PI * Vector3.Distance(transform.position, playerT.position));
            AimAtPlayer(slowedTurnSpeed);
        }
        else
        {
            AimAtPlayer(baseTurnSpeed);
        }
    }

    /// <summary>
    /// Changes the appearance of the laser renderer. "Diaabled", disables the laser, "Targeting" has a thinner yellow laser, and "Firing" has a full red laser.
    /// </summary>
    protected void SetLaserRenderingMode(LaserRenderingMode renderingMode)
    {
        if(renderingMode == LaserRenderingMode.Disabled)
        {
            laserRenderer.enabled = false;
        } else
        {
            //Make sure the laserRenderer is turned on.
            laserRenderer.enabled = true;

            //The laser's width and color changes based on whether it is Firing or only Targeting the player.
            switch(renderingMode)
            {
                case LaserRenderingMode.Targeting:
                    laserRenderer.startWidth = laserRenderer.endWidth = .1f;
                    laserRenderer.startColor = laserRenderer.endColor = Color.yellow;
                    break;

                case LaserRenderingMode.Firing:
                    laserRenderer.startWidth = laserRenderer.endWidth = .5f;
                    laserRenderer.startColor = laserRenderer.endColor = Color.red;
                    break;
            }
        }
    }

    /// <summary>
    /// Perform's the laser's raycast and updates its lenght based on how far it travelled before hitting something.
    /// </summary>
    /// <param name="damagePlayer"> Whether or not the laser should damage a player hit by it.</param>
    protected void UpdateLaser(bool damagePlayer)
    {
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit laserAttackHit, Mathf.Infinity, GetEnemyAttackLayerMask(), QueryTriggerInteraction.Ignore))
        {
            SetLaserLength(laserAttackHit.distance);

            if (damagePlayer && damageIntervalTracker <= 0)
            {
                if (laserAttackHit.collider.TryGetComponent(out PlayerController hitPlayer))
                {
                    hitPlayer.ApplyEnemyAttack(laserAttack, laserAttackHit.point);
                    damageIntervalTracker = damageInterval;
                }
            }
        }
        else
        {
            //If the ray did not hit anything, set the laser's endpoint at its maxRange.
            SetLaserLength(maxRange);
        }

        //Set's the laser to be a certain length.
        void SetLaserLength(float length)
        {
            var endpoint = Vector3.forward * (length / 2);
            laserRenderer.SetPosition(1, endpoint);
        }
    } 
}
