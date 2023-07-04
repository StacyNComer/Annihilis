using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TurretAI : EnemyAIBase
{
    /// <summary>
    ///The maximum degrees/s the turret can turn a second.
    /// </summary>
    [Header("Turret AI")]
    [SerializeField]
    protected float baseTurnSpeed;
    [SerializeField, Tooltip("The max speed in units/s that the laser may chase the player at. Used to slow the turret's turn speed at long ranges and perfectly tracking the player.")]
    protected float maxAngularSpeed = 8;
    [SerializeField]
    protected float damageInterval;
    [SerializeField]
    protected float maxRange = 100;
    [SerializeField]
    protected EnemyAttackData laserAttack;
    [SerializeField]
    protected float attackDelay = 1.25f;

    protected float attackDelayTracker;
    protected float damageIntervalTracker;
    /// <summary>
    /// NOTE: By default, the line renderer should be set to use local space. If you use SetLaserEndpoint to move the laser's endpoint, this should be accounted for.
    /// </summary>
    protected LineRenderer laserRenderer;
    /// <summary>
    /// The distance the player must be from the turret before it turns extra slow.
    /// </summary>
    protected float slowTurnDistance;

    new protected void Start()
    {
        base.Start();

        laserRenderer = GetComponent<LineRenderer>();
        attackDelayTracker = attackDelay;
        SetLaserFireEffectActive(false);

        slowTurnDistance = (360 * maxAngularSpeed) / (2 * Mathf.PI * baseTurnSpeed);
    }

    protected void Update()
    {
        if (damageIntervalTracker > 0)
        {
            damageIntervalTracker -= Time.deltaTime;
        }
    }

    /// <summary>
    /// Aims the turret at the player using its base speed. Dampens the rotation speed if the player is far away so that the angular speed doesn't outrun the player.
    /// </summary>
    protected void AimAtPlayerWithDampening()
    {
        if (Vector3.Distance(transform.position, playerT.position) >= slowTurnDistance)
        {
            var slowedTurnSpeed = (360 * maxAngularSpeed) / (2 * Mathf.PI * Vector3.Distance(transform.position, playerT.position));
            AimAtPlayer(slowedTurnSpeed);
        }
        else
        {
            AimAtPlayer(baseTurnSpeed);
        }
    }

    protected void SetLaserFireEffectActive(bool firingEffectActive)
    {
        if (firingEffectActive)
        {
            laserRenderer.startWidth = laserRenderer.endWidth = .5f;
            laserRenderer.startColor = laserRenderer.endColor = Color.red;
        }
        else
        {
            laserRenderer.startWidth = laserRenderer.endWidth = .1f;
            laserRenderer.startColor = laserRenderer.endColor = Color.yellow;
        }
    }

    //Fires the laser for a frame.
    protected void FireLaser()
    {
        if (Physics.SphereCast(transform.position, 0.5f, transform.forward, out RaycastHit laserAttackHit, Mathf.Infinity, GetEnemyAttackLayerMask(), QueryTriggerInteraction.Ignore))
        {
            SetLaserLength(laserAttackHit.distance);

            if (damageIntervalTracker <= 0)
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
    }

    protected void SetLaserLength(float length)
    {
        var endpoint = Vector3.forward * (length / 2);
        laserRenderer.SetPosition(1, endpoint);
    }
}
