using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum AmmoType
{
    Stardust,
    Ferro,
    AntiMatter,
    Godsoul,
    Citrine,
    Lapus
}

public class PlayerController : MonoBehaviour
{
    #region Static
    /// <summary>
    /// Holds data related to storing ammo. Ammo types are linked to their relevant data in the "ammo" dictionary.
    /// </summary>
    private class AmmoData
    {
        private int currentAmmo;

        private int maxAmmo;

        /// <summary>
        /// UI meant to display an ammo type's current amount.
        /// </summary>
        private TMPro.TMP_Text[] ammoCounters;

        public AmmoData(int max, TMPro.TMP_Text[] ammoCountTexts = null, int current = 0)
        {
            maxAmmo = max;
            currentAmmo = current;
            ammoCounters = ammoCountTexts;

            UpdateAmmoCounters();
        }

        /// <summary>
        /// Update's every ammo counter to display the currunt ammo.
        /// </summary>
        private void UpdateAmmoCounters()
        {
            if(ammoCounters != null)
            {
                foreach (TMPro.TMP_Text counter in ammoCounters)
                {
                    counter.text = currentAmmo.ToString();
                }
            }     
        }

        /// <summary>
        /// Modifies the current ammo value by the given amount an updates relavant UI. Current ammo is always clamped between 0 and max ammo.
        /// </summary>
        /// <param name="amount"></param>
        public void ModifyAmmo(int amount)
        {
            currentAmmo = Mathf.Clamp(currentAmmo + amount, 0, maxAmmo);

            UpdateAmmoCounters();
        }

        public int GetCurrentAmmo()
        {
            return currentAmmo;
        }

        public int GetMaxAmmo()
        {
            return maxAmmo;
        }
    }

    /// <summary>
    /// The maximum slope that counts and being the "ground".
    /// </summary>
    public const float MaxWalkableAngle = 45;
    
    /// <summary>
    /// A coefficient for the player's acceleration used while they are in the air.
    /// </summary>
    private const float AirControlCoef = .425f;
    /// <summary>
    /// The time before the player loses their jump when dashing upwards off the ground. This grace period allows the player to perform a "Lunar Hop" which causes them to jump super high.
    /// </summary>
    private const float LunarHopGracePeriod = .1f;

    /// <summary>
    /// The maximum speed a player who is neither dashing nor affected by bonus movement speed may have. Calculate from the player's max speed at the beginning of runtime.
    /// </summary>
    private static float playerBaseMaxSpeed;
    #endregion

    #region Editor Parameters
    [Header("Character Movement")]
    [SerializeField, Tooltip("How much the player accelerates each seconds")]
    private float movSpeed = 1;
    [SerializeField]
    private float maxSpeed = 11;
    [SerializeField]
    private float jumpVelocity = 12;
    [SerializeField]
    private float aimSpeed = 10;
    /// <summary>
    /// How long the player's dash lasts. The player cannot move laterally while dashing. This does not control the dash speed, as it is an instant velocity change.
    /// </summary>
    [SerializeField]
    private float dashTime = 0.25f;
    [SerializeField]
    private float dashCooldown = 1;
    [SerializeField, Tooltip("The player's speed while in \"No-Clip\" mode.")]
    private float flightSpeed = 16;

    [Header("Component References")]
    [SerializeField]
    new private Camera camera;
    [SerializeField]
    private Transform attackSpawnPoint;
    [SerializeField]
    private GameObject lungeAttackPrefab;
    [SerializeField]
    private SphereCollider footCollider;

    [Header("UI References")]
    [SerializeField]
    private Slider healthBar;
    [SerializeField]
    private FlashingImage healthBarFlash;
    [SerializeField]
    private TMPro.TMP_Text healthText;
    [SerializeField]
    private TMPro.TMP_Text weaponNameText;
    [SerializeField]
    private TMPro.TMP_Text ammoText;
    [SerializeField]
    private Slider dashBar;
    [SerializeField]
    private FlashingImage dashBarFlash;
    [SerializeField]
    private Image lungeImage;
    [SerializeField]
    private TMPro.TMP_Text[] stardustAmmoCounters;
    [SerializeField]
    private TMPro.TMP_Text[] ferroAmmoCounters;
    [SerializeField]
    private TMPro.TMP_Text[] nihilAmmoCounters;
    [SerializeField]
    private TMPro.TMP_Text[] godSoulAmmoCounters;
    [SerializeField]
    private TMPro.TMP_Text[] citrineAmmoCounters;
    [SerializeField]
    private FadingImage tutorialFadingBackground;
    [SerializeField]
    private FadingText tutorialFadingText;
    [SerializeField]
    private TMPro.TMP_Text tutorialText;
    [SerializeField]
    public PickupMessageManager pickupMsgManager;
    [SerializeField]
    private GameObject uiRoot;

    [Header("Audio Clips")]
    [SerializeField]
    private AudioClip dashAudio;
    [SerializeField]
    private AudioClip lunarHopAudio;
    /// <summary>
    /// Played when the player maintains a higher than max speed with a jump.
    /// </summary>
    [SerializeField, Tooltip("Played when the player maintains a higher than max speed with a jump.")]
    private AudioClip bHopAudio;
    [SerializeField]
    private AudioClip damageAudio;
    [SerializeField]
    private AudioClip staggerDamageAudio;

    [Header("Test/Debug")]
    [SerializeField]
    private WeaponBehavior startingWeapon;
    [SerializeField]
    private WeaponBehavior startingWeapon2;
    [SerializeField]
    private bool infiniteAmmo;
    [SerializeField]
    private GameObject testMenu;
    [SerializeField]
    private TutorialMessage dieMessage;
    [SerializeField]
    private TutorialMessage nightmareMessage;
    #endregion

    #region Private Fields
        //Components
    private Rigidbody rb;

    private Dictionary<AmmoType, AmmoData> ammo;
    /// <summary>
    /// The friction of the players footCollider while they are on the ground. Set from the player's physics material on Awake.
    /// </summary>
    private float baseDynamicFriction;
    private Transform cameraTransform;
    private WeaponBehavior currentWeapon;

    /// <summary>
    /// A cooldown before the player can swap their weapon again with axis controls (i.e. the mouse wheel). Used to prevent accidental "double swapping".
    /// </summary>
    private float axisSwapCooldown = .1f;
    /// <summary>
    /// Tracks the seconds before the player can swap their weapon using axis controls again.
    /// </summary>
    private float axisSwapCooldownTracker;
    /// <summary>
    /// The velocity the player is set to have when they dash. The upward velocity is capped at jumpVelocity.
    /// </summary>
    private float dashSpeed = 23;
    /// <summary>
    /// Tracks the seconds before the player's dash ends. The player is considered to be dashing while this is above 0.
    /// </summary>
    private float dashTimeTracker;
    /// <summary>
    /// Tracks the seconds before the player may dash again.
    /// </summary>
    private float dashCooldownTracker = 0;
    /// <summary>
    /// The physics material used the the player's collider which connects them to the ground.
    /// </summary>
    private PhysicMaterial footMaterial;
    private float flightSpeedMultiplier = 1f;
    private GameManager gameManager;
    private int hitpoints = 100;
    //The amount of times the player can jump. Resets when the player touches anything with the "Ground" tag.
    private int jumps = 1;
    /// <summary>
    /// The collider for the player's dash attack. Is null if the player either isn't dashing/diving or already hit an enemy with the lunge.
    /// </summary>
    private PlayerLunge lungeAttack;
    /// <summary>
    /// If true, the player flies and does not collide with anything. Dashing while this is true instead cycles the flight speed.
    /// </summary>
    private bool noClip;
    private bool onGround = true;
    /// <summary>
    /// While greater than 0, the player is staggered and cannot move without lunging.
    /// </summary>
    private float staggerTracker = 0;

    /// <summary>
    /// Stores the player's weapon inventory.
    /// </summary>
    private WeaponBehavior[] weapons = new WeaponBehavior[8];

    /// <summary>
    /// The normal of the ground the player is standing on. It is 0 while the player is in the air.
    /// </summary>
    private Vector3 slopeNormal;
    /// <summary>
    /// A vector representing the direction of the ground's horizontal axis relative to the direction of its normal.
    /// </summary>
    private Vector3 slopeAxleAxis;
    #endregion

    #region Unity Events
    private void Awake()
    {
            //Static Initialization (There should only be one player!)
        //Initialize base max speed.
        playerBaseMaxSpeed = maxSpeed;

        //Component caching
        rb = GetComponent<Rigidbody>();
        cameraTransform = camera.transform;

        footMaterial = footCollider.material;
        baseDynamicFriction = footCollider.material.dynamicFriction;

        //Make sure the cursor does not leave the game window.
        Cursor.lockState = CursorLockMode.Locked;

        //Initialize Ammo
        ammo = new Dictionary<AmmoType, AmmoData>
        {
            { AmmoType.Stardust, new AmmoData(300, stardustAmmoCounters, 40)},
            { AmmoType.Ferro, new AmmoData(40, ferroAmmoCounters)},
            { AmmoType.AntiMatter, new AmmoData(40, nihilAmmoCounters) },
            { AmmoType.Godsoul, new AmmoData(300, godSoulAmmoCounters) },
            { AmmoType.Citrine, new AmmoData(6, citrineAmmoCounters) },
            { AmmoType.Lapus, new AmmoData(6) },
        };
    }

    void Start()
    {
        gameManager = GameManager.gameManager;

        //Add the starting weapon to the player's inventory.
        AddWeapon(startingWeapon);
        EquipWeapon(startingWeapon);
    }

    // Update is called once per frame
    void Update()
    {
        
        if(!IsPaused())
        {
            //Stagger tracking
            if (staggerTracker > 0)
            {
                staggerTracker -= Time.deltaTime;

                if (staggerTracker <= 0)
                {
                    ClearStagger();
                }
            }

                //Perform a sphereCast to test if the player has walked off a ledge and is no longer on the ground.

            //Raise the starting point of the ground test a little in case the foot collider is inside of the ground.
            var groundTestOffset = .2f;
            //How far downward the ground test should travel. We add the offset so that the distance is relative to the foot collider's center.
            var groundTestDist = .25f + groundTestOffset;

            //A sphereCast starting from the position of the player's foot collider (with an offset) and travelling groundTestDist downward. The radius of this check is the same as the foot collider's radius.
            bool groundTest = Physics.SphereCast(transform.position + footCollider.center + Vector3.up * groundTestOffset, footCollider.radius, Vector3.down, out RaycastHit groundHit, groundTestDist, GameManager.GetRaycastLayerMask(), QueryTriggerInteraction.Ignore);

            //Test if the player is still on the ground via a raycast. The player loses their jump if they are no longer on the ground. Leaving the ground via a lunge has a grace period before the jump is loss so that the player can "Lunar Hop".
            //NOTE: If the player's collision detection is set to Discrete (which it shouldn't be), there is a chance this check misses the ground at high landing velocities.
            if (!IsDashing() && (!groundTest || !IsGroundCollision(groundHit)))
            {
                //Flatten the players velocity to the X/Z plane if they walk off a slope onto flat ground. This makes sure the player stays on the ground (retaining their momentum) instead of being flung into the air. The distance check is to make sure the player is moving onto the ground instead of the air, in which case the upward momentum is preserved to make the movement feel right.
                if (!OnFlatGround() && groundHit.distance == 0)
                {
                    rb.velocity = Vector3.ProjectOnPlane(rb.velocity/2, Vector3.up);
                }

                LeaveGround();

                SetFeetFrictionless(true);

                jumps = 0;
            }

                //Aiming

            var yaw = transform.eulerAngles.y + Input.GetAxis("Mouse X") * aimSpeed;
            var pitch = cameraTransform.localEulerAngles.x + -Input.GetAxis("Mouse Y") * aimSpeed;
            //The player's entire body yaws (so that they walk in the correct direction) while only the camera is pitched.
            transform.eulerAngles = new Vector3(0, yaw, 0);
            camera.transform.localEulerAngles = new Vector3(pitch, 0, 0);

            #region Controls
            if (noClip)
            {
                //Cycle noclip flight speed
                if (Input.GetButtonDown("Lunge"))
                {
                    if(flightSpeedMultiplier < 4)
                    {
                        flightSpeedMultiplier *= 2;
                    } else
                    {
                        flightSpeedMultiplier = 1;
                    }

                    pickupMsgManager.AddPickupMessage($"Flight Speed: x{flightSpeedMultiplier}");
                }

                //noclip flight
                CharacterFlightMovement("Vertical", Vector3.forward, true);
                CharacterFlightMovement("Horizontal", Vector3.right, true);
            } else
            {
                //Walking
                CharacterMovement("Vertical", transform.forward);
                CharacterMovement("Horizontal", transform.right);

                #region Dash Logic
                //End the dash when the tracker reaches 0. This gives the player back control over their movement.
                if (dashTimeTracker > 0)
                {
                    dashTimeTracker -= Time.deltaTime;

                    //Destroys the player's lunge collision and rests their foot friction. If the player is not touching the ground, their lunge is extended until the player either lands or moves, making it up to CollisionEnter or CharacterMovement to end the lunge.
                    //The second clause clears the player's jump if they leave the ground with a dash. This has a grace period so that the player can "Moon Jump" by ascending with both a jump and dash at the same time.
                    if (dashTimeTracker <= 0 && onGround)
                    {
                        SetFeetFrictionless(false);
                        ClearLunge();
                    }
                    else if (!onGround && jumps > 0 && dashTimeTracker < dashTime - LunarHopGracePeriod)
                    {
                        jumps = 0;
                    }
                }

                //Dash Cooldown
                if (dashCooldownTracker > 0)
                {
                    dashCooldownTracker -= Time.deltaTime;
                    dashBar.value = 1 - (dashCooldownTracker / dashCooldown);

                    //Change the dash meter's color back to normal when the dash comes off of cooldown.
                    if (dashCooldownTracker <= 0)
                    {
                        //dashBarFlash.FlashImage(Color.white, new Color(.6f, .94f, 1), .25f);
                        dashBarFlash.SetBaseColor(new Color(.6f, .94f, 1));
                    }
                }

                //Dashing (a.k.a lunging)
                if (Input.GetButtonDown("Lunge") && dashCooldownTracker <= 0)
                {
                    //Lunging clears the player out of their stagger.
                    ClearStagger();

                    Vector3 dashDirection;

                    //Keep the player from losing their dash speed due to aiming downward or doing a tiny hop while on the ground.  
                    if (!onGround || (cameraTransform.rotation.eulerAngles.x >= 270 && cameraTransform.rotation.eulerAngles.x <= 355))
                    {
                        dashDirection = cameraTransform.forward;
                        LeaveGround();
                    }
                    else
                    {
                        dashDirection = transform.forward;
                    }

                    //Frictionless feet let the player dash along the ground with a single velocity change without the dash velocity needing to be absurd.
                    SetFeetFrictionless(true);

                    var dashVel = dashDirection * dashSpeed;

                    //Limit the player's upward dash velocity to their jump velocity.
                    if (dashVel.y > jumpVelocity)
                    {
                        rb.velocity = new Vector3(dashVel.x, jumpVelocity, dashVel.z);
                    }
                    else
                    {
                        rb.velocity = dashVel;
                    }

                    //If the player already has a lunge hitbox active, it is cleared.
                    ClearLunge();

                    //Create and initialize the lunge collision.
                    lungeAttack = Instantiate(lungeAttackPrefab).GetComponent<PlayerLunge>();
                    lungeAttack.SetOwningPlayer(this);
                    lungeAttack.GetComponent<PlayerAttack>().SetAttackOwner(this);

                    dashTimeTracker = dashTime;

                    dashCooldownTracker = dashCooldown;

                        //Lunge UI and FX
                    //Show the screen effect for the player lunging.
                    lungeImage.enabled = true;
                    //Empty the lunge meter.
                    dashBar.value = 0;
                    //Turn the lunge meter grey
                    dashBarFlash.SetBaseColor(Color.gray);
                    //Play lunge audio.
                    AudioSource.PlayClipAtPoint(dashAudio, transform.position);
                }
                #endregion

                //Jumping. The player cannot jump while staggered.
                if (jumps > 0 && staggerTracker <= 0 && Input.GetButton("Jump"))
                {
                    //Lunar Hop and B-Hop audio. Lunar hops are detected by the player jumping while not considered on the ground. B-hops are detected by the player's horizontal velocity being higher than their max speed, though the sound will won't trigger if the player is off the ground to keep it from overlapping with the lunar hop sound.
                    if (!onGround)
                    {
                        AudioSource.PlayClipAtPoint(lunarHopAudio, transform.position);

                    }
                    else if (Vector3.ProjectOnPlane(rb.velocity, Vector3.up).magnitude > maxSpeed + .25f)
                    {
                        AudioSource.PlayClipAtPoint(bHopAudio, transform.position, .25f);
                    }

                    //If the player is on a ramp (and not lunar hoping) dampen their upward velocity some so their jump isn't super-boosted.
                    if (!OnFlatGround() && onGround)
                    {
                        rb.velocity = new Vector3(rb.velocity.x, rb.velocity.y * .25f, rb.velocity.z);
                    }

                    //Add the jump velocity.
                    rb.AddForce(Vector3.up * jumpVelocity, ForceMode.VelocityChange);

                    LeaveGround();

                    jumps = 0;

                    //Turn off the friction so the player keeps some of their momentum when they land. This is turned back on when they touch the ground.
                    SetFeetFrictionless(true);
                }
            }

                //Firing Weapons
            if (Input.GetButtonDown("Fire1"))
            {
                currentWeapon.StartFiringPrimary();
            }

            if (Input.GetButtonUp("Fire1"))
            {
                currentWeapon.StopFiringPrimary();
            }

            if(Input.GetButtonDown("Fire2"))
            {
                currentWeapon.FireSecondary();
            }

                //Weapon Swapping
            var weaponScroll = Input.GetAxis("Mouse ScrollWheel");

            //The axis weapon swapping has a cooldown so the the player cannot "double swap" by accident.
            if (axisSwapCooldownTracker <= 0 && weaponScroll != 0)
            {
                axisSwapCooldownTracker = axisSwapCooldown;

                //Swap to the previous/next weapon based on the axis value.
                if (weaponScroll > 0)
                {
                    SwapWeapon();
                }
                else if (weaponScroll < 0)
                {
                    SwapWeapon(true);
                }
            } else
            {
                axisSwapCooldownTracker -= Time.deltaTime;
            }
        
        }
        #endregion

        #region Cheat Controls
/*        if (Input.GetKeyDown(KeyCode.Alpha1))
            TriggerableBehavior.TriggerGroup("Wave1");

        if (Input.GetKeyDown(KeyCode.Alpha2))
            TriggerableBehavior.TriggerGroup("Wave2");

        if (Input.GetKeyDown(KeyCode.Alpha3))
            TriggerableBehavior.TriggerGroup("Wave3");

        if (Input.GetKeyDown(KeyCode.Alpha4))
            TriggerableBehavior.TriggerGroup("Wave4");

        if (Input.GetKeyDown(KeyCode.Alpha5))
            TriggerableBehavior.TriggerGroup("Wave5");

        if (Input.GetKeyDown(KeyCode.Alpha6))
            TriggerableBehavior.TriggerGroup("Wave6");*/

        //Heal
        if (Input.GetKeyDown(KeyCode.Z))
            HealPlayer(999);

        if (Input.GetKeyDown(KeyCode.L))
            infiniteAmmo = !infiniteAmmo;

        //Turn on NIGHTMARE MODE (setting the enemy multiplier to 2).
        if (Input.GetKeyDown(KeyCode.Minus))
        {
            GameManager.enemyMultiplier = 2;
            nightmareMessage.Trigger();
        }

        //Noclip
        if (Input.GetKeyDown(KeyCode.BackQuote))
        {
            SetNoClip(!noClip);
        }

        //Disable HUD
        if (Input.GetKeyDown(KeyCode.H))
            uiRoot.SetActive(!uiRoot.activeSelf);

        //Deleto (kills all enemies)
        if (Input.GetKey(KeyCode.Backspace))
        {
            foreach (Destructable enemy in FindObjectsByType<Destructable>(FindObjectsSortMode.None))
                enemy.Die();
        }

        //Restart the level
        if(Input.GetKeyDown(KeyCode.R) && testMenu.activeSelf)
        {
            //Make sure the game is unpaused!
            Time.timeScale = 1;

            SceneManager.LoadScene(0);
        }

        //Quit ze game
        if (Input.GetKeyDown(KeyCode.Escape) && testMenu.activeSelf)
        {
            Application.Quit();
        }

        //Toggle test menu Menu
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            testMenu.SetActive(!testMenu.activeSelf);

            //Pause/Unpause the game when the menu is opened/closed.
            if (testMenu.activeSelf)
            {
                Time.timeScale = 0;
            }
            else
            {
                Time.timeScale = 1;
            }
        }
        #endregion
    }

    private void OnCollisionEnter(Collision collision)
    {
        //If the player collides with the ground,restore their jump, restore their foot friction, and end their lunge if it was exended due to them being in the air.
        if (IsGroundCollision(collision))
        {
            jumps = 1;
            onGround = true;

            CalculateSlopeData(collision.GetContact(0).normal);

            SetFeetFrictionless(false);

            //End the player's lunge if it was extended by them being in the air.
            if (!IsDashing())
            {
                ClearLunge();
            }
        }
    }
    #endregion

    /// <summary>
    /// Set the slopeNormal and slopeAxisAngle based on the provided surface normal. This tells the player how to move in response to the slope of the surface they are walking on. If this method is given a zero-vector (e.g. they are in the air), the values will still function properly.
    /// </summary>
    /// <param name="surfaceNormal"></param>
    private void CalculateSlopeData(Vector3 surfaceNormal)
    {
        //Set the normal of the surface the player is supposed to be walking on.
        slopeNormal = surfaceNormal;

        //Project the surface onto the x/z plane to get it's horizontal direction.
        var horDir = Vector3.ProjectOnPlane(surfaceNormal, Vector3.up).normalized;
        //Swapping the x and z values rotates the vector 90 degrees about the y axis so that it faces the side instead of forward.
        slopeAxleAxis = new Vector3(-horDir.z, 0, horDir.x);
    }

    #region Static Methods
    /// <summary>
    /// Returns the maximum speed that player can move at, without taking into account lunging or having bonus movement speed. Automatically set on Awake.
    /// </summary>
    public static float GetPlayeraseMaxSpeed()
    {
        return playerBaseMaxSpeed;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Applies a force that moves the player in a given direction while clamping the player's speed at their Max Speed. This function does nothing while the player is dashing or staggered.
    /// </summary>
    /// <param name="direction">The direction to move in. This is NOT relative to the player transform.</param>
    private void CharacterMovement(string axisName, Vector3 direction)
    {
        var axisInput = Input.GetAxis(axisName);
        //Stores if the player is actually pressing the axis input, as opposed to it having a value due to smoothing.
        var axisButtonHeld = Input.GetAxisRaw(axisName) != 0;
        //If the character is on a slope, rotate the movement direction so that it is parallel with the slope.
        direction = Quaternion.AngleAxis(-Vector3.Angle(Vector3.up, slopeNormal), slopeAxleAxis) * direction;
        
        if (axisInput != 0 && !IsDashing() && staggerTracker <= 0)
        {
            //Reduce the acceleration if the player is in the air (makes the controls a tad less slippery).
            var airControlScalar = onGround ? 1 : AirControlCoef;

            var finalVelocity = rb.velocity + airControlScalar * movSpeed * axisInput * Time.deltaTime * direction;
            var finalHorVelocity = Vector3.ProjectOnPlane(finalVelocity, Vector3.up);

            //If the player is moving faster than their max speed, clamp their horizontal velocity to the maxSpeed.
            if (finalHorVelocity.magnitude > maxSpeed)
            {
               //The player's velocity should only be clamped if they are actually inputing a movement! The second check keeps the player from jerking when going up ramps.
               if(axisButtonHeld && OnFlatGround())
               {
                    rb.velocity = Vector3.ClampMagnitude(finalHorVelocity, maxSpeed) + new Vector3(0, finalVelocity.y, 0);
               }
            }
            else
            {
                rb.velocity = finalVelocity;
            }

            //Clear the lunge if the player moves while in the air. Axis Raw is used to that the player is only knocked out of their dash while they are pressing a movement key and can't be knocked out of their lunge by the axis value lingering due to smoothing.
            if (!onGround && axisButtonHeld)
            {
                ClearLunge();
            }
        }
    }

    /// <summary>
    /// Translates the players position according to the given axis input, with a maximum speed of flightSpeed units/s.
    /// </summary>
    private void CharacterFlightMovement(string axisName, Vector3 direction, bool relative)
    {
        if (relative)
        {
            direction = camera.transform.rotation * direction;
        }
        
        var motion = Input.GetAxis(axisName) * flightSpeed * flightSpeedMultiplier * Time.deltaTime * direction;
        
        transform.Translate(motion, Space.World);
    }

    /// <summary>
    /// Tests if the lunge hitbox exists, destroying it if it does. Whenever the lunge collider is destroyed, the gameobject also calls the player's ClearLungeEffect function.
    /// </summary>
    private void ClearLunge()
    {
        if (lungeAttack)
        {
            lungeAttack.lungeCleared = true;

            Destroy(lungeAttack.gameObject);
            lungeAttack = null;
            ClearLungeEffect();
        }
    }

    /// <summary>
    /// Instantly ends the player being staggered.
    /// </summary>
    private void ClearStagger()
    {
        if (onGround && !IsDashing())
        {
            SetFeetFrictionless(false);
        }

        if(staggerTracker > 0)
        {
            staggerTracker = 0;
        }
    }

    /// <summary>
    /// True if the given collision is with something that counts as the ground.
    /// </summary>
    private bool IsGroundCollision(Collision collision)
    {
        return Vector3.Angle(Vector3.up, collision.GetContact(0).normal) <= MaxWalkableAngle;
    }

    /// <summary>
    /// True if the given raycast hit something that counts as the ground.
    /// </summary>
    private bool IsGroundCollision(RaycastHit hit)
    {
        return Vector3.Angle(Vector3.up, hit.normal) <= MaxWalkableAngle;
    }

    private bool IsPaused()
    {
        return testMenu.activeSelf;
    }

    private bool IsSecondaryAmmoType(AmmoType type)
    {
        return (int)type >= 4;
    }

    /// <summary>
    /// Sets on ground to false and resets the vectors used to help the player move up/down slopes. Note that this does not automatically make the player lose their jump.
    /// </summary>
    private void LeaveGround()
    {
        onGround = false;

        //Reset the ground slope data so that the player cannot fly after jumping from a slope.
        slopeNormal = Vector3.up;
        slopeAxleAxis = Vector3.zero;
    }

    /// <summary>
    /// True if the normal of the surface the player is standing on is straight upwards.
    /// </summary>
    /// <returns></returns>
    private bool OnFlatGround()
    {
        return slopeNormal == Vector3.up;
    }

    /// <summary>
    /// If true, the physics material of the player's ground collider has its dynamic friction set to 0. Otherwise, it is set to baseDynamicFriction.
    /// </summary>
    private void SetFeetFrictionless(bool isFrictionless)
    {
        if(isFrictionless)
        {
            footMaterial.dynamicFriction = 0;
        } else
        {
            footMaterial.dynamicFriction = baseDynamicFriction;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="noClipEnabled"></param>
    private void SetNoClip(bool noClipEnabled)
    {
        noClip = noClipEnabled;

        //Disable/re-enable gravity (so the player does not fall into an infinite void when they turn on noclip)
        rb.useGravity = !noClip;

        //Noclip flight does not use physics to move, so velocity must be zeroed.
        rb.velocity = Vector3.zero;

        //Disablr/Re-enable collision
        foreach(Collider col in gameObject.GetComponents<Collider>())
        {
            col.enabled = !noClip;
        }
    }

    /// <summary>
    /// Switches the equipped weapon to the next/previous weapon in the player's inventory.
    /// </summary>
    /// <param name="prevWeapon">If true, this method will swap to the previous weapon instead of the next one.</param>
    private void SwapWeapon(bool prevWeapon = false)
    {
        WeaponBehavior toSelect;

        //The weapon swap starts from the player's current weapon.
        var i = currentWeapon.GetWeaponSlot();

        //Loop through the players weapons until a non-empty slot is found or the current slot is returned to. The latter case prevents an infinite loop if the player only has one weapon.
        do
        {
            if(prevWeapon)
            {
                i--;

                //Wrap i's value if the beginning of the player's weapon inventory was reached.
                if (i < 0)
                {
                    i = weapons.Length - 1;
                }
            } else
            {
                i++;

                //Wrap i's value if the end of the player's weapon inventory was reached.
                if (i == weapons.Length)
                {
                    i = 0;
                }
            }

            toSelect = weapons[i];

        } while (toSelect == null && i != currentWeapon.GetWeaponSlot());

        //Keep the player from swapping into their current weapon.
        if(toSelect != currentWeapon)
        {
            EquipWeapon(toSelect);
        }   
    }

    private void UpdateAmmoUI(AmmoType type)
    {
        var ammoData = ammo[type];
        ammoText.text = ammoData.GetCurrentAmmo().ToString();
    }

    /// <summary>
    /// Updates the meter and number displaying the player's health. The bar flashing is handled by the DamagePlayer function.
    /// </summary>
    private void UpdateHealthUI()
    {
        healthBar.value = hitpoints / 100.0f;
        healthText.text = hitpoints.ToString();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Gives ammo to the player and displays a pickup message. If ammo must be removed, ExpendAmmo should be used instead unless you want a message to be displayed!
    /// </summary>
    public void AddAmmo(int amount, AmmoType ammoType)
    {
        ammo[ammoType].ModifyAmmo(amount);

        pickupMsgManager.AddPickupMessage($"+{amount} {ammoType}");

        //Update the ammo UI if the ammo type is the same as the weapon held.
        if(ammoType == currentWeapon.GetPrimaryAmmoType())
        {
            UpdateAmmoUI(ammoType);
        }
    }

    /// <summary>
    /// Adds the given weapon to the player's inventory, setting it in its hard-coded weapon slot.
    /// </summary>
    /// <param name="weapon"></param>
    public void AddWeapon(WeaponBehavior weapon)
    {
        weapons[weapon.GetWeaponSlot()] = weapon;
    }

    /// <summary>
    /// Returns true if the given ammo type is storing its maximum amount of ammo.
    /// </summary>
    /// <param name="ammoType"></param>
    /// <returns></returns>
    public bool AmmoMaxed(AmmoType ammoType)
    {
        var ammoData = ammo[ammoType];
        return ammoData.GetCurrentAmmo() == ammoData.GetMaxAmmo();
    }

    /// <summary>
    /// Damages the player. If the given attack's staggerTime is greater than 0, the player is also staggered away from sourcePos.
    /// </summary>
    public void ApplyEnemyAttack(EnemyAttackData attackData, Vector3 sourcePos)
    {
        DamagePlayer(attackData.GetDamage());

        //If attack has a staggerTime, stagger the player. Being damages with and without stagger has different audio.
        if (attackData.GetStaggerTime() > 0)
        {
            AudioSource.PlayClipAtPoint(staggerDamageAudio, transform.position, .4f);
            StaggerPlayer(attackData.GetStaggerForce(), attackData.GetStaggerTime(), sourcePos);
        } else
        {
            AudioSource.PlayClipAtPoint(damageAudio, transform.position, .4f);
        }
    }

    /// <summary>
    /// Clears the screen effects/audio present while the lunge collision is active.
    /// </summary>
    public void ClearLungeEffect()
    {
        if(lungeImage)
        {
            lungeImage.enabled = false;
        }
        
    }

    /// <summary>
    /// Substracts the given amount from the player's hitpoints and updates the UI. This should not be used to heal the player, as this method does not clamp the player's health.
    /// </summary>
    public void DamagePlayer(int amount)
    {
        //Show a death message if the attack "killed" the player.
        if (hitpoints > 0 && hitpoints - amount <= 0)
        {
            dieMessage.Trigger();
        }

        hitpoints -= amount;

        //Update UI
        UpdateHealthUI();
        healthBarFlash.FlashImage(new Color(1, .7f, .7f), .75f);
    }

    /// <summary>
    /// Sets a weapon as the player's current weapon and updates the UI.
    /// </summary>
    /// <param name="weapon"></param>
    public void EquipWeapon(WeaponBehavior weapon)
    {
        currentWeapon = weapon;
        weaponNameText.text = weapon.GetWeaponName().ToUpper();

        UpdateAmmoUI(weapon.GetPrimaryAmmoType());
    }

    /// <summary>
    /// Expends the given amount of ammo of the given type. Use for cases where it is already known that the player has enough ammo.
    /// </summary>
    public void ExpendAmmo(int amount, AmmoType type)
    {
        if(!infiniteAmmo)
        {
            ammo[type].ModifyAmmo(-amount);
        }

        //Don't update the HUD ammo with secondary types.
        if(!IsSecondaryAmmoType(type))
        {
            UpdateAmmoUI(type);
        }
    }

    /// <summary>
    /// Damages the player and applies stagger based on how far the player is from sourcePos. Partial stagger is applied if the player is less than 70% within the radius of the explosion, reducing the stagger by up to 70%.
    /// </summary>
    /// <param name="attackData"></param>
    /// <param name="sourcePos"></param>
    public void ExplosiveDamage(EnemyAttackData attackData, Vector3 sourcePos, float sourceRadius)
    {
        DamagePlayer(attackData.GetDamage());

        if (attackData.GetStaggerTime() > 0)
        {
            var reductionDistance = sourceRadius * .7f;
            var finalStaggerForce = attackData.GetStaggerForce();
            var playerDist = Vector3.Distance(transform.position, sourcePos);

            //Modify the stagger force if the player is far enough away from the explosion's center.
            if (playerDist > reductionDistance)
            {
                finalStaggerForce *= 1 - ((playerDist - reductionDistance) / sourceRadius);
            }

            StaggerPlayer(finalStaggerForce, attackData.GetStaggerTime(), sourcePos, false);
        }
    }

    public Transform GetAttackSpawnPoint()
    {
        return attackSpawnPoint;
    }

    /// <summary>
    /// Returns the player's velocity clamped to the given magnitude. Useful if you would like something to stop tracking the player properly once they surpass a certain speed.
    /// </summary>
    public Vector3 GetClampedVelocity(float maxMagnitude)
    {
        var playerVel = rb.velocity;
        var magnitude = playerVel.magnitude;

        return (magnitude > maxMagnitude) ? Vector3.ClampMagnitude(playerVel, maxMagnitude) : playerVel;
    }

    /// <summary>
    /// Returns the players velocity with the magnitude clamped down their base speed. Useful if you want something to track the player, but only if they are not lunging and lack any bonus movement speed.
    /// </summary>
    /// <returns></returns>
    public Vector3 GetClampedVelocity()
    {
        return GetClampedVelocity(playerBaseMaxSpeed);
    }

    public int GetHitpoints()
    {
        return hitpoints;
    }

    /// <summary>
    /// Returns the player's current velocity.
    /// </summary>
    public Vector3 GetVelocity()
    {
        return rb.velocity;
    }

    /// <summary>
    /// Restore the given amount of health to the player. Unless overheal is true, the healing cannot take a player's health above 100.
    /// </summary>
    /// <param name="overheal">Whether or not the healing can raise a player's health above 100. There is no limit to the amount of overheal a player can recieve.</param>
    public void HealPlayer(int amount, bool overheal = false)
    {
        //If the healing cannot overheal and would take the players health above 100, simply set the player's health to 100. If the player's health is >100, non-overheal does nothing.
        if(overheal || hitpoints + amount <= 100)
        {
            hitpoints += amount;

        } else if (hitpoints < 100)
        {
            hitpoints = 100;
        }

        UpdateHealthUI();
    }

    /// <summary>
    /// True if the player is lunging. Note that this does not correspond with the player's lunge collider being active due to the dive mechanic.
    /// </summary>
    public bool IsDashing()
    {
        return dashTimeTracker > 0;
    }

    /// <summary>
    /// Pushes the player away from sourcePos with the given force. The player's non-lunge movement are disabled for staggerTime seconds.
    /// </summary>
    /// <param name="force">The magnitude of the stagger velocity.</param>
    /// <param name="staggerTime">Disables the character's non-lunge movment for this amount of seconds.</param>
    /// <param name="sourcePos">The point in space the player is staggered away from.</param>
    /// <param name="clampYVel">If true, the y velocity is clamped to the player's jump velocity.</param>
    public void StaggerPlayer(float force, float staggerTime, Vector3 sourcePos, bool clampYVel = true)
    {
        staggerTracker = staggerTime;

        SetFeetFrictionless(true);

        var staggerVel = force * (transform.position - sourcePos).normalized;
        //Clamp the y velocity, if this should be done.
        if(clampYVel && staggerVel.y > jumpVelocity)
        {
            staggerVel.y = jumpVelocity;
        }

        rb.velocity = staggerVel;
    }

    /// <summary>
    /// Displays the given message at the top of the screen.
    /// </summary>
    public void ShowTutorialMessage(string line1, string line2)
    {
        tutorialFadingBackground.PlayFade();

        tutorialFadingText.PlayFade();

        tutorialText.text = line1 + '\n' + line2;
    }

    /// <summary>
    /// Displays the given message at the top of the screen.
    /// </summary>
    public void ShowTutorialMessage(string message)
    {
        tutorialFadingBackground.PlayFade();

        tutorialFadingText.PlayFade();

        tutorialText.text = message;
    }

    /// <summary>
    /// Substracts the given amount of ammo from the player's reserves, but only if they have that much ammo to use.
    /// </summary>
    /// <returns>True if the player has the requested amount of ammo of the given type.</returns>
    public bool TryUseAmmo(int amount, AmmoType type)
    {
        if(TestAmmo(amount, type))
        {
            ExpendAmmo(amount, type);
            return true;
        } else
        {
            return false;
        }
    }

    /// <summary>
    /// Returns true if the player has at least the given amount of ammo of the given type.
    /// </summary>
    public bool TestAmmo(int amount, AmmoType type)
    {
        return infiniteAmmo || ammo[type].GetCurrentAmmo() >= amount;
    }
    #endregion
}
