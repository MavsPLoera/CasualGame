using System.Collections;
using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class Player_Controller : MonoBehaviour
{
    [Header("Current Player Stats")]
    public float playerHealth;
    public int playerLives = 3;
    public float playerSpeed;
    public float currentMax;
    public float jumpForce;
    public float slideTimer = 0.0f;
    public float slideTime = .5f;
    public bool facingRight = true;
    public bool invulnerable = false;

    [Header("Conditional Player Stats")]
    public float walkSpeed;
    public float maxWalkingSpeed;
    public float airSpeed;
    public float maxAirSpeed;

    [Header("Player Jumping Traits")]
    public float jumpForceButtonReleased;
    public float jumpForceHeld;
    public float jumpHeight; 
    public float dashForce;
    public float doubleJumpForce;
    public float jumpBufferDistance;
    public float coyoteTimer;
    public float coyoteTime = .2f;

    [Header("Player Conditional Traits")]
    public bool playerCanInput = true;
    public bool canDoubleJump = true;
    public bool storedJump = false;
    public bool isFacingRight = true;
    public bool onGround;

    [Header("Player Dashing Traits")]
    public GameObject bullet;
    public Transform currentSpawnLocation;
    public GameObject crouchedBulletSpawnLocation;
    public GameObject bulletSpawnLocation;
    public bool isDashing;
    public bool canDash = true;
    public float dashSpeed = 20f;
    public float dashDuration = 0.1f;
    public float dashCoolDown = 0.1f;
    public TrailRenderer trailRenderer;

    [Header("Player Shooting Traits")]
    public bool isReloading;
    public bool isShooting;
    public int ammo = 6;
    public int maxAmmo = 10;
    public float shootingCooldown = .2f;
    public float shootTime = .05f;
    public bool storedShot;

    [Header("Player HitBox Traits")]
    public BoxCollider2D playerHitBox;
    public Vector2 uncrouchedHitBoxOffset;
    public Vector2 uncrouchedHitBoxSize;
    public Vector2 crouchedHitBoxOffset;
    public Vector2 crouchedHitBoxSize;

    [Header("Player Audio Clips")]
    public AudioSource playerAudioSource;
    public AudioSource playerChangingAudioSource;
    public float prevPitch;
    public AudioClip shootGunSFX;
    public AudioClip realodedSFX;
    public bool landAlreadyPlayed = false;
    public AudioClip playerLandSFX;
    public AudioClip jumpSFX;
    public AudioClip doubleJumpSFX;
    public AudioClip dashSFX;
    public GameObject footSteps;
    public AudioClip hitSoundEffectSFX;

    [Header("Player Miscellaneous")]
    public Transform lastTouched;
    public GameObject gameUI;
    public GameUI_Controller controller;
    public Rigidbody2D rb;
    public Collider2D col;
    public BetterJump_Controller BetterJump_Controller;
    public ParticleSystem dustParticle;
    public Animator animator;
    public Vector2 boxSize;
    public Transform crouchBulletSpawn; //NEEED TO ADD THIS
    public float boxCastDistance;
    public float jumpingBoxCastDistance;
    public bool beenHit = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //Get Components for player behaviour.
        rb = GetComponent<Rigidbody2D>();
        col = rb.GetComponent<Collider2D>();
        BetterJump_Controller = GetComponent<BetterJump_Controller>();
        trailRenderer = GetComponent<TrailRenderer>();
        animator = GetComponent<Animator>();
        playerHitBox = GetComponent<BoxCollider2D>();
        controller = gameUI.GetComponent<GameUI_Controller>();
        currentSpawnLocation = bulletSpawnLocation.transform;
        controller.updatePlayerLives();
        controller.updateScore(0);


        //Set variables 
        uncrouchedHitBoxOffset = playerHitBox.offset;
        uncrouchedHitBoxSize = playerHitBox.size;
        jumpForceButtonReleased = jumpForceHeld / 2f;
        doubleJumpForce = jumpForce * .9f;
        ammo = maxAmmo;
    }

    // Update is called once per frame
    void Update()
    {
        //Do not allow input if the player is dashing.
        if (isDashing || !playerCanInput)
        {
            return;
        }

        onGround = isGrounded();

        if (onGround == false)
        {
            coyoteTimer -= Time.deltaTime;
        }
        else
        {
            coyoteTimer = coyoteTime;
        }

        //Get direction the player wants to move.
        float x = Input.GetAxis("Horizontal");
        float y = Input.GetAxis("Vertical");
        float x_raw = Input.GetAxisRaw("Horizontal");
        float y_raw = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(x, 0);
        //Debug.DrawRay(transform.position, new Vector3(0, -jumpBufferDistance, 0), Color.red);

        //Move the player based on player speed and keep current y linear velocity
        flip(x_raw);

        /*
         * Walking and Coruching.
         * 
         * if the player is pressing s the current velocity of the player will be lerped to zero and the aniamtor will trigger a crouch animation.
         * 
         * otherwise the player will run at full speed on the ground or the player will move at air speed if not.
         */
        if(y_raw == -1f)
        {
            if(Mathf.Abs(rb.linearVelocityX) > 0f)
            {
                slideTimer += Time.deltaTime;
                rb.linearVelocityX = Mathf.Lerp(rb.linearVelocityX, 0.0f, slideTimer / slideTime);
            }

            animator.SetBool("isCrouching", true);
            animator.SetBool("isRunning", false);

            //Need to shrink hitbox
            playerHitBox.offset = crouchedHitBoxOffset;
            playerHitBox.size = crouchedHitBoxSize;
            currentSpawnLocation = crouchedBulletSpawnLocation.transform;
        }
        else
        {
            slideTimer = 0.0f;
            currentSpawnLocation = bulletSpawnLocation.transform;
            if (onGround)
            {
                playerHitBox.offset = uncrouchedHitBoxOffset;
                playerHitBox.size = uncrouchedHitBoxSize;
                rb.linearVelocity = new Vector2(Mathf.Clamp(direction.x * playerSpeed, -maxWalkingSpeed, maxWalkingSpeed), rb.linearVelocityY);
            }
            else
            {
                rb.linearVelocity = new Vector2(direction.x * playerSpeed, rb.linearVelocityY);
            }

            animator.SetBool("isRunning", true);
            animator.SetBool("isCrouching", false);
        }

        //Check for the animator to stop playing the running animation if the player is not inputing anything. Idk why this is here but the program works :P
        if(x_raw == 0f && y_raw == 0f && onGround)
        {
            animator.SetBool("isRunning", false);
        }

        /*
        * Jumping
        * 
        * The first if statement checks a few cases:
        * 1. if the player is ground on the ground and hit the jump button.
        * 2. if the player just revently ran off a platform and has coyote time
        * 3. if the player has a stored jump via. a jump buffering check and is on the ground.
        * 
        * in any of these cases we store the linearXvelocity and add a jump force to the player to send them into the air.
        * we then set some bools and timers to values that will not allow the player to jump while in mid air unless permitted.
        * 
        * second if statement is for double jumping
        * we will do the same thing as the first if statement but instead we will add doublejumpforce which is slightly less than jump.
        * double jumps will refresh once the player hits the ground again.
        * 
        * the third and fourth if statements are checks for buffering moves
        * they will call a raycast method and check to see if the player is close enough to the ground to store a move.
        * if they are they will change their respective bool value and the enxt time the onGround() check is true the move will trigger
        * 
        * jump buffering take prio over shoot buffering.
        */
        if ((Input.GetKeyDown(KeyCode.Space) && (onGround == true || coyoteTimer >= 0) || (storedJump == true && onGround == true)))
        {
            jumpForce = jumpForceHeld;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, 0);
            rb.linearVelocity += Vector2.up * jumpForce;

            onGround = false;
            storedJump = false;
            coyoteTimer = -1f;
        }
        else if((Input.GetKeyDown(KeyCode.Space) && !onGround && canDoubleJump)) //Might want to do the same but with shooting.
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, 0);
            rb.linearVelocity += Vector2.up * doubleJumpForce;
            playSound(doubleJumpSFX);
            canDoubleJump = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !onGround)
        {
            if (movementBufferCheck())
            {
                storedJump = true;
            }
        }
        else if((Input.GetButtonDown("Fire1") && !onGround))
        {
            if (movementBufferCheck())
            {
                storedShot = true;
            }
        }

        //Half the velocity of the jump when the player releases the jump button
        if (Input.GetKeyUp(KeyCode.Space) && !onGround)
        {
            rb.linearVelocityY *= .5f;
        }

        /*
         * Shooting
         * 
         * first check is to see if the player has hit the fire button or if the player has stored a shot to fire.
         * 
         * if the player is on the ground allow the player to shoot and set storedShot to false in the coroutine.
         * 
         * otherwise if the player is reloaded trigger the reloaded sequence and do not let the player shoot till ammo is refilled.
         */
        if (Input.GetButtonDown("Fire1") || storedShot)
        {
            if (ammo == 0 && isReloading == false)
            {
                StartCoroutine(reload());
            }
            else if(!isReloading && onGround)
            { 
                StartCoroutine(shoot());
            }
            controller.updateAmmoImageUI();

        }

        /*
         * Dashing
         * 
         * allow the player to dash as much as they want on the ground. Besides a small dashcooldown or only allow them to dash once in the air.
         * 
         * Dash is refreshed once the player hits the ground. Like Celeste
         */
        if(Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            StartCoroutine(dash(x_raw));
        }

        if (beenHit)
        {
            StartCoroutine(playerHit());
        }
    }

    private IEnumerator shoot()
    {
        //Modify this to be coroutine to shoot and play animation.
        ammo--;
        controller.updateAmmoImageUI();
        storedShot = false;
        animator.SetBool("isShooting", true);
        Instantiate(bullet, currentSpawnLocation.position, bulletSpawnLocation.transform.rotation);
        playerAudioSource.PlayOneShot(shootGunSFX);
        yield return new WaitForSeconds(shootTime);
        animator.SetBool("isShooting", false);
        yield return null;
    }

    private IEnumerator dash(float x_raw)
    {
        Physics2D.IgnoreLayerCollision(6, 7, true);
        canDash = false;
        isDashing = true;
        trailRenderer.emitting = true;
        BetterJump_Controller.enabled = false;
        if(x_raw == 0f)
        {
            if(isFacingRight)
            {
                rb.linearVelocity = new Vector2(dashSpeed, 0f);
            }
            else
            {
                rb.linearVelocity = new Vector2(-dashSpeed, 0f);
            }
        }
        else
        {
            rb.linearVelocity = new Vector2(dashSpeed * x_raw, 0f);
        }

        playSound(dashSFX);
        float prevGravity = rb.gravityScale;
        rb.gravityScale = 0.0f;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = new Vector2(0f, rb.linearVelocityY);
        trailRenderer.emitting = false;
        isDashing = false;
        BetterJump_Controller.enabled = true;
        Physics2D.IgnoreLayerCollision(6, 7, false);
        rb.gravityScale = prevGravity;
        yield return null;
    }

    private IEnumerator reload()
    {
        isReloading = true;
        yield return new WaitForSeconds(shootingCooldown);
        ammo = maxAmmo;
        isReloading = false;
        controller.updateAmmoImageUI();

        yield return null;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy"))
        {
            if (playerHealth <= 0f)
            {
                animator.SetBool("isDead", true);
                Destroy(gameObject, animator.GetCurrentAnimatorStateInfo(0).length);
                //more game over stuff
            }
            else
            {
                playerHealth -= 10f;
                StartCoroutine(temporaryInvulnerability());
            }
        }
    }

    public bool isGrounded()
    {
        if(Physics2D.BoxCast(transform.position, boxSize, 0, -transform.up, boxCastDistance, LayerMask.GetMask("ground")))
        {
            currentMax = maxWalkingSpeed;
            playerSpeed = walkSpeed;
            canDash = true;
            canDoubleJump = true;
            animator.SetBool("isJumping", false);

            if (!landAlreadyPlayed)
            {
                playerAudioSource.PlayOneShot(playerLandSFX);
                landAlreadyPlayed = true;
            }
            return true;
        }
        else
        {
            landAlreadyPlayed = false;
            currentMax = maxAirSpeed;
            playerSpeed = airSpeed;
            animator.SetBool("isJumping", true);
            lastTouched = gameObject.transform;
            return false;
        }
    }

    public bool movementBufferCheck()
    {
        if(Physics2D.Raycast(gameObject.transform.position, Vector2.down, jumpBufferDistance, LayerMask.GetMask("ground")))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void flip(float horizontalInput)
    {
        ParticleSystem.VelocityOverLifetimeModule velocity = dustParticle.velocityOverLifetime;

        if (isFacingRight && horizontalInput == -1f)
        {
            transform.rotation = Quaternion.Euler(0f, 180f, 0f);
            isFacingRight = false;
            if (onGround)
            {
                velocity.x = .5f;
                velocity.y = .25f;
                dustParticle.Play();
            }
        }

        if(!isFacingRight && horizontalInput == 1f)
        {
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            isFacingRight = true;

            if (onGround)
            {
                velocity.x = -.5f;
                velocity.y = .25f;
                dustParticle.Play();
            }       
        }
    }

    //Used for playing sounds that we want to range the pitch of
    public void playSound(AudioClip sound)
    {
        float temp = Random.Range(.9f, 1.1f);
        playerChangingAudioSource.pitch = temp;
        playerChangingAudioSource.PlayOneShot(sound);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireCube(transform.position - transform.up * boxCastDistance, boxSize);

        Gizmos.DrawRay(transform.position, -transform.up * jumpBufferDistance);
    }

    //Call this when the player gets hit by enemy
    public IEnumerator playerHit()
    {
        rb.linearVelocity = new Vector2(-10f, 5f);
        playerCanInput = false;
        beenHit = true;
        animator.SetBool("isHit", true);
        yield return new WaitForSeconds(.1f);
        rb.linearVelocity = Vector2.zero;
        playerCanInput = true;
        beenHit = false;
        animator.SetBool("isHit", false);
        yield return null;
    }


    //Add more to this
    public IEnumerator temporaryInvulnerability()
    {
        //Physics2D.IgnoreLayerCollision(6, 7, true); can use this
        invulnerable = true;
        yield return new WaitForSeconds(.1f);
        invulnerable = false;
        yield return null;
    }
}
