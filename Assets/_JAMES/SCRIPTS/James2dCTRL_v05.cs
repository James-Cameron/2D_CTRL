using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class James2dCTRL_v05 : MonoBehaviour
{
    /*
    2D Custom Character Controller by James H. Cameron |Copyright 2020
    Version2: (2020-04-13) Added Coyote Time functionality.  Split JumpAndFall fucntion into Jump and Fall functions respectively.
    Version3: (2020-04-15) Added Jump Buffering code to Jump function.
    Version4: (2020-04-30) Added Wall Cling, Wall SLide, and Wall Jump functionality
    Version5: (2020-06-15) Added Wall Grab functinality, the player now remains stationary wehen touching a wall and pressing  
                           the wall grab button.  Player can also Wall Slide and Wall Jump our of this state.

    NOTE: 
    Use with BoxCollider2D component.  
    Modify Physics2D.gravity to your needs in Edit > Project Settings > Physics 2D
    */

    // CHARACTER ATTRIBUTES
    [SerializeField, Tooltip("Max speed, in units per second, that the character moves.")]
    float speed = 9;

    [SerializeField, Tooltip("Acceleration while Grounded.")]
    float walkAcceleration = 75;

    [SerializeField, Tooltip("Acceleration while in the air.")]
    float airAcceleration = 30;

    [SerializeField, Tooltip("Deceleration applied when character is Grounded and not attempting to move.")]
    float groundDeceleration = 70;

    [SerializeField, Tooltip("Max height the character will jump regardless of gravity")]
    float jumpHeight = 4;

    [SerializeField]
    float fallMultiplier = 2.5f;

    [SerializeField]
    float lowJumpMultiplier = 2f;

    [SerializeField]
    float maxCoyoteTime = .04f;

    private float coyoteTimer;

    [SerializeField]
    float jumpPressRememberTime = .1f;

    private float jumpPressRemember = 0;

    // WALL JUMP

    [SerializeField]
    float wallJumpTime = .2f;

    [SerializeField]
    float wallSlideSpeed = .3f;

    [SerializeField]
    float wallDistance = .55f;

    public bool isWallSliding = false;

    public bool isWallGrabbing = false;

    RaycastHit2D wallCheckHit;

    private float jumpTime;

    private bool grabPressRemember = false;

    //COMPONENT REFERENCES
    private BoxCollider2D boxCollider;

    /// //////////////////////////////////////

    private Vector2 velocity;

    public bool isGrounded;

    private bool isFacingRight = true;

    public LayerMask groundLayer;


    /// //////////////////////////////////////

    private void Awake()
    {
        boxCollider = GetComponent<BoxCollider2D>();
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        MoveAbout();

        HandleCollision();

        Jump();

        Fall();

        Drift();

        FlipSprite();

        WallCling();

    }

    void MoveAbout()
    {
        float moveInput = Input.GetAxisRaw("Horizontal");
        // WHEN WE HAVE AN INPUT VALUE, APPROACH MAX SPEED WITH A RATE OF CHANGE OF WALKACCELERATION
        if (moveInput != 0)
        {
            velocity.x = Mathf.MoveTowards(velocity.x, speed * moveInput, walkAcceleration * Time.deltaTime);
        }
        // WHEN WE DONT HAVE AN INPUT VALUE, APPROACH 0 SPEED WITH A RATE OF CHANGE OF GROUNDDECELREATION
        else
        {
            velocity.x = Mathf.MoveTowards(velocity.x, 0, groundDeceleration * Time.deltaTime);
        }

        transform.Translate(velocity * Time.deltaTime);

    }

    void HandleCollision()
    {
        isGrounded = false;

        // Retrieve all colliders we have intersected after velocity has been applied.
        Collider2D[] hits = Physics2D.OverlapBoxAll(transform.position, boxCollider.size, 0);

        foreach (Collider2D hit in hits)
        {
            // Ignore our own collider.
            if (hit == boxCollider)
                continue;

            ColliderDistance2D colliderDistance = hit.Distance(boxCollider);

            // Ensure that we are still overlapping this collider.
            // The overlap may no longer exist due to another intersected collider pushing us out of this one. 
            if (colliderDistance.isOverlapped)
            {
                transform.Translate(colliderDistance.pointA - colliderDistance.pointB);

                // If we intersect an object beneath us, set Grounded to true. 
                if (Vector2.Angle(colliderDistance.normal, Vector2.up) < 90 && velocity.y < 0)
                {
                    isGrounded = true;
                }

            }

        }

    }

    void Jump()
    {
        if (isGrounded)
        {
            coyoteTimer = Time.time;

            velocity.y = 0;

        }

        jumpPressRemember -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            jumpPressRemember = jumpPressRememberTime;
        }

        if ((jumpPressRemember > 0) && ((Time.time - coyoteTimer) < maxCoyoteTime))
        {
            jumpPressRemember = 0;

            velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
        }

    }

    void Fall()
    {
        velocity.y += Physics2D.gravity.y * Time.deltaTime;

        if (velocity.y < 0)
        {
            velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (velocity.y > 0 && !Input.GetButton("Jump"))
        {
            velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
        }
    }

    void Drift()
    {
        // DEFINITELY WANT TO TOY WITH AIR ACCEL AND DECCEL
        float acceleration = isGrounded ? walkAcceleration : airAcceleration;
        float deceleration = isGrounded ? groundDeceleration : 0;

    }

    void FlipSprite()
    {
        // IF PLAYER'S X VELOCITY > EPSILON THEN THIS BOOL WILL RETURN AS TRUE
        bool hasHorizontalSpeed = Mathf.Abs(velocity.x) > Mathf.Epsilon;

        if (hasHorizontalSpeed)
        {
            transform.localScale = new Vector2(Mathf.Sign(velocity.x), 1f);
        }

    }

    void WallCling()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            grabPressRemember = true;
        }

        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            grabPressRemember = false;
        }

        float moveInput = Input.GetAxisRaw("Horizontal");

        if (moveInput < 0)
        {
            isFacingRight = false;
        }
        else
        {
            isFacingRight = true;
        }

        if (isFacingRight) // IF FACING RIGHT THEN CAST A RAY TO THE RIGHT TO CHECK FOR THE WALL
        {
            wallCheckHit = Physics2D.Raycast(transform.position, new Vector2(wallDistance, 0), wallDistance, groundLayer);
        }
        else // OTHERWISE CHECK LEFT
        {
            wallCheckHit = Physics2D.Raycast(transform.position, new Vector2(-wallDistance, 0), wallDistance, groundLayer);
        }

        if (wallCheckHit && !isGrounded && moveInput != 0)
        {
            isWallSliding = true;
            jumpTime = Time.time + wallJumpTime;
        }
        else if (jumpTime < Time.time)
        {
            isWallSliding = false;
        }

        if (wallCheckHit && grabPressRemember)
        {
            isWallGrabbing = true;
            jumpTime = Time.time + wallJumpTime;
        }
        else if (jumpTime < Time.time)
        {
            isWallGrabbing = false;
        }

        if (isWallSliding)
        {
            velocity = new Vector2(velocity.x, Mathf.Clamp(velocity.y, -wallSlideSpeed, float.MaxValue));
        }

        if (isWallSliding && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
        }

        if (isWallGrabbing)
        {
            velocity = new Vector2(velocity.x, Mathf.Clamp(velocity.y, 0, float.MaxValue));
        }

        if (isWallGrabbing && Input.GetButtonDown("Jump"))
        {
            velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
        }

    }






    // END OF FILE
}
