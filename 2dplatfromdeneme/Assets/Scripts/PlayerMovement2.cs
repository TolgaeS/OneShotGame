using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement2 : MonoBehaviour
{

    public Rigidbody2D rb;
    bool isFacingRight = true;

    [Header ("Movement")]
    public float moveSpeed = 3f;
    float horizontalMovement;

    [Header("Jumping")]
    public float JumpPower = 6f;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("GroundCheck")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.5f,0.05f);
    public LayerMask groundLayer;
    bool isGrounded;

     [Header("WallCheck")]
    public Transform wallCheckPos;
    public Vector2 wallCheckSize = new Vector2(0.5f,0.05f);
    public LayerMask wallLayer;

    [Header("Gravity")]
    public float baseGravity = 2f;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 1.5f;

    [Header("WallMovement")]
     public float wallSlideSpeed = 2;
     bool isWallSliding;

     //walljump
     bool isWallJumping;
     float wallJumpDirection;
     float wallJumpTime = 0.5f;
     float wallJumpTimer;
     public Vector2 wallJumpPower = new Vector2(5f, 8f);

    void Start()
    {
        isFacingRight = false;
        Vector3 ls = transform.localScale;
        ls.x = -Mathf.Abs(ls.x); // Sağa bakıyor gibi görünüp aslında sola baksın
        transform.localScale = ls;
    }

    void Update()
    {
        GroundCheck();
        Gravity();
        WallSlide();
        WallJump();
        Flip();

        if(!isWallJumping)
        {
            rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocity.y);
            Flip();
        }
    }

    public void Gravity()
    {
        if(rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }
    private void WallSlide()
    {
        if(!isGrounded && WallCheck() && horizontalMovement != 0)
        {
            isWallSliding = true;
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Max(rb.linearVelocity.y, -wallSlideSpeed));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if(isWallSliding)
        {
            isWallJumping = false;
            wallJumpDirection = - transform.localScale.x;
            wallJumpTimer = wallJumpTime;

            CancelInvoke(nameof(cancelWallJump));
        }
        else if (wallJumpTimer > 0f)
        {
            wallJumpTimer -= Time.deltaTime;
        }
    }

    private void cancelWallJump()
    {
        isWallJumping = false;
    }

    public void Move2(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }
    
    public void Jump2(InputAction.CallbackContext context)
{
    if(jumpsRemaining > 0)
    {
    if (context.performed)
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpPower);
        jumpsRemaining--;
    }
    else if (context.canceled)
    {
        // Zıplamayı kısa kesmek için yukarı doğru olan hızı azalt
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, rb.linearVelocity.y * 0.5f);
        jumpsRemaining--;
    }
    }

    //wall jump
    if(context.performed && wallJumpTimer > 0f)
    {
        isWallJumping = true;
        wallJumpTimer = 0;
        rb.linearVelocity = new Vector2(wallJumpDirection * wallJumpPower.x, wallJumpPower.y);
        
        Invoke (nameof(cancelWallJump), wallJumpTime + 0.1f); //wall jump = 0.f -- jum again 0.6f
    }
}

    private void GroundCheck()
    {
        if(Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer))
        {
            jumpsRemaining = maxJumps;
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
        
    }

    private bool WallCheck()
    {
        return Physics2D.OverlapBox(wallCheckPos.position, wallCheckSize, 0, wallLayer);
    }

    private void Flip()
    {
        if(isFacingRight && horizontalMovement < 0 || !isFacingRight && horizontalMovement > 0 )
        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale =ls;
        }

    }

        private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);
    }
 }   
