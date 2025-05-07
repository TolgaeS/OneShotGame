using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerMovement2 : MonoBehaviour
{

    public Rigidbody2D rb;
    bool isFacingRight = false;
    public ParticleSystem SmokeFx;
    public Transform RespawnPoint;
    public GameManager gameManager;
    public Animator animator;

    [Header ("Movement")]
    public float moveSpeed = 3f;
    float horizontalMovement;

    [Header("Dashing")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.1f;
    public float dashCooldown = 2;
    bool isDashing;
    bool canDash = true;
    TrailRenderer TrailRenderer;

    [Header("Jumping")]
    public float JumpPower = 6f;
    public int maxJumps = 2;
    int jumpsRemaining;

    [Header("Attack")]
    public Transform attackPoint;
    public float attackRange = 0.5f;
    public LayerMask enemyLayers;
    public float attackCooldown = 0.5f;
    private bool canAttack = true;
    private Vector2 attackPointPos;

    [Header("Health")]
    public int maxLives = 3;
    private int currentLives;

    [Header("Respawn")]
    public Transform respawnPoint;
    public float respawnSpeed = 2f; 

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
        TrailRenderer = GetComponent<TrailRenderer>();
        currentLives = maxLives;
        attackPointPos = attackPoint.localPosition;
    
    }

    void Update()
    {
        if (isDashing)
        {
            return;
        }

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

        animator.SetFloat("yVelocity",rb.linearVelocity.y);
        animator.SetFloat("Magnitude",rb.linearVelocity.magnitude);

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
            wallJumpDirection = transform.localScale.x;
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

    public void Dash2(InputAction.CallbackContext context)
    {
        if(context.performed && canDash)
        {
            StartCoroutine(DashCoroutine());
        }
    }

    private IEnumerator DashCoroutine()
    {
        Physics2D.IgnoreLayerCollision(7,8, true);

        canDash = false;
        isDashing = true;

        TrailRenderer.emitting = true;
        float dashDirection = isFacingRight ? -1f : 1f;

        rb.linearVelocity = new Vector2(dashDirection * dashSpeed, rb.linearVelocity.y); //dash mvovement

        yield return new WaitForSeconds(dashDuration);
        
        rb.linearVelocity = new Vector2(0f, rb.linearVelocity.y); // reset horizontal velocity

        isDashing = false;
        TrailRenderer.emitting = false;
        Physics2D.IgnoreLayerCollision(7,8, false);

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
    
    
    public void Jump2(InputAction.CallbackContext context)
{
    if(jumpsRemaining > 0)
    {
        if (context.performed)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, JumpPower);
            jumpsRemaining--;

            SmokeFx.Play();
            animator.SetTrigger("fleejump");
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
        SmokeFx.Play();
        
        Invoke (nameof(cancelWallJump), wallJumpTime + 0.1f); //wall jump = 0.f -- jum again 0.6f
    }
}
    public void Attack2(InputAction.CallbackContext context)
{

     Debug.Log("Attack tuşuna basıldı!");

    if (!context.performed || !canAttack) return;

    canAttack = false;
    
    // Saldırı kutusunu oluştur
    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

    foreach (Collider2D enemy in hitEnemies)
{
    Debug.Log("Düşmana vuruldu: " + enemy.name);
    enemy.GetComponent<PlayerMovement>()?.TakeDamage(1);
    StartCoroutine(MoveToRespawnPoint());
}

    StartCoroutine(AttackCooldown());
}

public void TakeDamage(int damage)
{
    currentLives--;
    Die();

    if (currentLives <= 0)
    {
        gameManager.GetComponent<GameManager>()?.GameOver();
    }
}

private void Die()
{
    Debug.Log(gameObject.name + " died!");

    // Ölüm animasyonu veya efektleri ekleyebilirsin
    StartCoroutine(MoveToRespawnPoint()); // Respawn noktasına hareket etmeyi başlat
}

    private IEnumerator MoveToRespawnPoint()
{
    float elapsedTime = 0f;
    Vector3 startingPosition = transform.position;

    // Respawn noktasına doğru hareket
    while (elapsedTime < respawnSpeed)
    {
        transform.position = Vector3.Lerp(startingPosition, RespawnPoint.position, elapsedTime / respawnSpeed);
        elapsedTime += Time.deltaTime;
        yield return null; // Bir sonraki frame'e geç
    }

    transform.position = RespawnPoint.position; // Tam olarak respawn noktasına git
}


    private IEnumerator AttackCooldown()
{
    yield return new WaitForSeconds(attackCooldown);
    canAttack = true;
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
        if (isFacingRight && horizontalMovement > 0 || !isFacingRight && horizontalMovement < 0)

        {
            isFacingRight = !isFacingRight;
            Vector3 ls = transform.localScale;
            ls.x *= -1f;
            transform.localScale =ls;

            if(rb.linearVelocity.y == 0)
            {
                SmokeFx.Play();
            }
        }
    }

        private void OnDrawGizmosSelected() 
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(wallCheckPos.position, wallCheckSize);

        if (attackPoint == null) return;
        {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }
    }

 }   
