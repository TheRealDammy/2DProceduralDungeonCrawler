using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

/// <summary>
/// A class to control the top-down character.
/// Implements the player controls for moving and shooting.
/// Updates the player animator so the character animates based on input.
/// </summary>
public class TopDownCharacterController : MonoBehaviour
{
    #region Framework Variables

    //The inputs that we need to retrieve from the input system.
    private InputAction moveAction;
    private InputAction attackAction;
    private InputAction sprintAction;
    private InputAction rollAction;

    //The components that we need to edit to make the player move smoothly.
    private Animator animator;
    private Rigidbody2D m_rigidbody;
    
    //The direction that the player is moving in.
    private Vector2 playerDirection;
    private Vector2 lastDirection;
   

    [Header("Movement parameters")]
    //The speed at which the player moves
    [SerializeField] private float playerSpeed = 200f;
    //The maximum speed the player can move
    [SerializeField] private float playerMaxSpeed = 1000f;
    [SerializeField] private float sprintSpeed = 400f;

    #endregion

    private float minStamina = 0f;
    private float maxStamina = 100f;
    private float stamina = 100f;
    private float staminaRegen = 20f;
    private float sprintCost = 30f;
    private bool canSprint = false;

    [Header("Projectile parameters")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed;
    [SerializeField] private float fireRate;
    private float nextFireTime = 0f;

    

    /// <summary>
    /// When the script first initialises this gets called.
    /// Use this for grabbing components and setting up input bindings.
    /// </summary>
    private void Awake()
    {
        //bind movement inputs to variables
        moveAction = InputSystem.actions.FindAction("Move");
        attackAction = InputSystem.actions.FindAction("Attack");
        sprintAction = InputSystem.actions.FindAction("Sprint");
        rollAction = InputSystem.actions.FindAction("Roll");

        //get components from Character game object so that we can use them later.
        animator = GetComponent<Animator>();
        m_rigidbody = GetComponent<Rigidbody2D>();

    }

    /// <summary>
    /// Called after Awake(), and is used to initialize variables e.g. set values on the player
    /// </summary>
    void Start()
    {
        //not currently used - left here for demonstration purposes.
    }

    /// <summary>
    /// When a fixed update loop is called, it runs at a constant rate, regardless of pc performance.
    /// This ensures that physics are calculated properly.
    /// </summary>
    private void FixedUpdate()
    {

        //clamp the speed to the maximum speed for if the speed has been changed in code.
        float speed = playerSpeed > playerMaxSpeed ? playerMaxSpeed : playerSpeed;
        
        //apply the movement to the character using the clamped speed value.
        m_rigidbody.linearVelocity = playerDirection * (speed * Time.fixedDeltaTime);
    }
    
    /// <summary>
    /// When the update loop is called, it runs every frame.
    /// Therefore, this will run more or less frequently depending on performance.
    /// Used to catch changes in variables or input.
    /// </summary>
    void Update()
    {
        

        // store any movement inputs into m_playerDirection - this will be used in FixedUpdate to move the player.
        playerDirection = moveAction.ReadValue<Vector2>();
        
        // ~~ handle animator ~~
        // Update the animator speed to ensure that we revert to idle if the player doesn't move.
        animator.SetFloat("Speed", playerDirection.magnitude);
        
        // If there is movement, set the directional values to ensure the character is facing the way they are moving.
        if (playerDirection.magnitude > 0)
        {
            animator.SetFloat("Horizontal", playerDirection.x);
            animator.SetFloat("Vertical", playerDirection.y);
            canSprint = true;

            lastDirection = playerDirection;
        }
        else 
        {
            canSprint = false;
        }

        if (rollAction.WasPressedThisFrame())
        {
            animator.SetTrigger("Roll");
        }

        if (canSprint == true)
        {
            if (sprintAction.IsPressed())
            {
                
                stamina -= sprintCost * Time.deltaTime;
                stamina = Mathf.Clamp(stamina, minStamina, maxStamina);
                if (stamina <= 0f)
                {
                    canSprint = false;
                    Debug.Log("Out of stamina!");
                    playerSpeed = 200f;
                }
                else
                {
                    Debug.Log("Stamina: " + stamina);
                    playerSpeed = sprintSpeed;
                }
            }
        }
        else
        {
            if (stamina < 100f)
            {
                stamina += staminaRegen * Time.deltaTime;
                Debug.Log("Stamina: " + stamina);
            }
            playerSpeed = 200f;
        }
        // check if an attack has been triggered.
        if (attackAction.IsPressed() && Time.time > nextFireTime)
        {
            nextFireTime = Time.time + fireRate;
            Fire();
        }
    }

    private void Fire()
    {
        Vector2 mousePosition = Mouse.current.position.ReadValue();
        Vector3 mousePointOnScreen = Camera.main.ScreenToWorldPoint(mousePosition);

        Vector2 fireDirection = mousePointOnScreen;
        if (fireDirection == Vector2.zero)
        {
            fireDirection = Vector2.down; // Default direction if no movement
        }
        GameObject spawnedProjectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);

        Rigidbody2D projectileRB = spawnedProjectile.GetComponent<Rigidbody2D>();
        if (projectileRB != null)
        {
            projectileRB.AddForce(fireDirection.normalized * projectileSpeed, ForceMode2D.Impulse);
        }
    }
}
