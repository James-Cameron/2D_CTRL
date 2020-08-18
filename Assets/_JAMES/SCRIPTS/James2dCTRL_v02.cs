using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class James2dCTRL_v02 : MonoBehaviour
{
    /*
    2D Custom Character Controller by James H. Cameron 2020

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
    float jumpPressRememberTime = .2f;

    private float jumpPressRemember = 0;

    //COMPONENT REFERENCES
    private BoxCollider2D boxCollider;

    /// //////////////////////////////////////
    
    private Vector2 velocity;

    private int amount = 3;

    public bool isGrounded;


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

        // CornerCorrection();


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
        /*
        if (Input.GetButtonDown("Jump") && ((Time.time - coyoteTimer) < maxCoyoteTime))
        {
            velocity.y = Mathf.Sqrt(2 * jumpHeight * Mathf.Abs(Physics2D.gravity.y));
        }
        */
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

    /*
    void CornerCorrection()
    {
        for (int i = 1; i <= amount; i++)
        {
            for(int j = 1; j >= -1; j -= 2)
            {
                if (!Collider2D.Check(new Point2(i * j, -1), Solid))
                {
                    MoveXExact(i * j);
                    MoveYExact(-1);
                    return true;
                }
            }

        }

    }
    */






    // END OF FILE
}
