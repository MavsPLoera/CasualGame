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
    public float score = 0f;
    public float playerSpeed;
    public float currentMax;
    public float jumpForce;
    public float slideTimer = 0.0f;
    public float slideTime;

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
    public ParticleSystem doubleJumpParticles;

    [Header("Player Conditional Traits")]
    public bool playerCanInput = true;
    public bool isBulletTime;
    public bool canDoubleJump = true;
    public bool storedJump = false;
    public bool invulnerable = false;
    public bool isFacingRight;
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
    public float bulletTimeDuration;
    public int ammo = 6;
    public int maxAmmo = 10;
    public float shootingCooldown = .2f;
    public float shootTime = .05f;

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
    public static Player_Controller instance;
    public Transform lastTouched;
    public GameObject gameUI;
    public GameUI_Controller controller;
    public Rigidbody2D rb;
    public Collider2D col;
    public BetterJump_Controller BetterJump_Controller;
    public Camera playerCamera;
    public ParticleSystem dustParticle;
    public Animator animator;
    public Vector2 boxSize;
    public Transform crouchBulletSpawn;
    public SpriteRenderer spriteRenderer;
    public float boxCastDistance;
    public float jumpingBoxCastDistance;
    public bool beenHit = false;
    public bool isDead = false;
    public bool storedVelocity;
    public float initialVelocity;

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
        spriteRenderer = GetComponent<SpriteRenderer>();
        currentSpawnLocation = bulletSpawnLocation.transform;

        //To avoid weird bug where sometimes bullet will be magenta instead of green
        SpriteRenderer temp = bullet.GetComponent<SpriteRenderer>();
        temp.color = Color.white;

        //Set variables 
        uncrouchedHitBoxOffset = playerHitBox.offset;
        uncrouchedHitBoxSize = playerHitBox.size;
        jumpForceButtonReleased = jumpForceHeld / 2f;
        doubleJumpForce = jumpForce;
        ammo = maxAmmo;
        instance = this;
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

        //Move the player based on player speed and keep current y linear velocity
        flip(x_raw);

        /*
         * Walking and Coruching.
         * 
         * if the player is pressing s the current velocity of the player will be lerped to zero and the aniamtor will trigger a crouch animation.
         * 
         * otherwise the player will run at full speed on the ground or the player will move at air speed if not.
         */
        if(y_raw == -1f && onGround)
        {
            if (Mathf.Abs(rb.linearVelocityX) > 0f)
            {
                slideTimer += Time.deltaTime;
                if(!storedVelocity)
                {
                    initialVelocity = rb.linearVelocityX;
                    storedVelocity = true;
                }

                rb.linearVelocityX = Mathf.Lerp(initialVelocity, 0f, (slideTimer / slideTime));
            }

            animator.SetBool("isCrouching", true);
            animator.SetBool("isRunning", false);

            //Shrink player hitbox to be the correct size of the sprite and change the bullet spawn position as well.
            playerHitBox.offset = crouchedHitBoxOffset;
            playerHitBox.size = crouchedHitBoxSize;
            currentSpawnLocation = crouchedBulletSpawnLocation.transform;
        }
        else
        {
            /*
             * Reset the values used for coruching if the player is not longer holding s
             * Also update the hit box and bullet spawn position from what is was when crouched.
             */

            initialVelocity = 0.0f;
            storedVelocity = false;
            slideTimer = 0.0f;
            currentSpawnLocation = bulletSpawnLocation.transform;
            playerHitBox.offset = uncrouchedHitBoxOffset;
            playerHitBox.size = uncrouchedHitBoxSize;

            //If statement is to change the functionality of the players movement while in the air or on the ground. Player has two different speeds.
            if (onGround)
            {
                rb.linearVelocity = new Vector2(Mathf.Clamp(direction.x * playerSpeed, -maxWalkingSpeed, maxWalkingSpeed), rb.linearVelocityY);
            }
            else
            {
                rb.linearVelocity = new Vector2(direction.x * playerSpeed, rb.linearVelocityY);
            }

            animator.SetBool("isRunning", true);
            animator.SetBool("isCrouching", false);
        }

        /*
         * Dont know why this is set up like this but if the player is not doing anything (Not pressing any keys). Then set the correct values for the aniamtor to go back to idle.
         */
        if (x_raw == 0f && onGround)
        {
            if (Mathf.Abs(rb.linearVelocityX) > 0f)
            {
                slideTimer += Time.deltaTime;
                if (!storedVelocity)
                {
                    initialVelocity = rb.linearVelocityX;
                    storedVelocity = true;
                }

                rb.linearVelocityX = Mathf.Lerp(initialVelocity, 0f, (slideTimer / slideTime));
            }
            else
            {
                storedVelocity = false;
            }
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
        * 
        * 3/20/2025: Jump buffering removed due to player being allowed to shoot during any time now.
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
            //playSound(doubleJumpSFX);
            doubleJumpParticles.Play();
            canDoubleJump = false;
        }
        else if (Input.GetKeyDown(KeyCode.Space) && !onGround)
        {
            if (movementBufferCheck())
            {
                storedJump = true;
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
        if (Input.GetButtonDown("Fire1"))
        {
            if(!isReloading)
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
    }

    private IEnumerator shoot()
    {
        //Modify this to be coroutine to shoot and play animation.
        //Add if statement for bullet time
        if(!isBulletTime)
        {
            ammo--;
        }

        animator.SetBool("isShooting", true);
        Instantiate(bullet, currentSpawnLocation.position, bulletSpawnLocation.transform.rotation);
        playerAudioSource.PlayOneShot(shootGunSFX);
        yield return new WaitForSeconds(shootTime);
        animator.SetBool("isShooting", false);

        if (ammo == 0 && isReloading == false)
        {
            StartCoroutine(reload());
        }

        yield return null;
    }

    private IEnumerator dash(float x_raw)
    {
        Physics2D.IgnoreLayerCollision(8, 7, true);
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
        Physics2D.IgnoreLayerCollision(8, 7, false);
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
        playerAudioSource.PlayOneShot(realodedSFX);
        yield return null;
    }

    public void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("enemy") || collision.gameObject.CompareTag("deadzone"))
        {
            if (playerLives == 0)
            {
                playerCamera.GetComponent<CameraFollow_Controller>().enabled = false;
                rb.gravityScale = 0.0f;
                BetterJump_Controller.enabled = false;
                playerCanInput = false;
                rb.linearVelocity = Vector2.zero;
                animator.SetTrigger("isDead");

                //Comeback and fix this, a .8f delay is not good and should rely on the animator
                Destroy(gameObject, .8f);


                //more game over stuff
            }
            else
            {
                rb.linearVelocity = Vector2.zero;
                StartCoroutine(respawn());
            }
        }
    }

    public IEnumerator respawn()
    {
        playerCanInput = false;
        animator.SetTrigger("isDead");
        yield return new WaitForSeconds(.8f);

        animator.Play("Idle", 0, 0f);
        playerLives--;
        controller.updatePlayerLives();
        transform.position = lastTouched.position;
        playerCamera.GetComponent<CameraFollow_Controller>().cameraToPlayer();
        yield return null;
        playerCanInput = true;
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
                playSound(playerLandSFX);
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
            return false;
        }
    }

    //Simple method to be able to call, to send a ray from the players position to see if the player hit a key close enought to the ground. Although the way the program ahs devloped the player only uses this for jump buffering.
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

    /*
     * Flip the player based on the x_raw horizontal input.
     * When we do so we not only rotate the player like the in class example but we play a little dust particle effect as well.
     * Depending on the direction the player is facing we will change the velocity of the particle effect to go in the opposite direction the player is moving.
     * values are hard coded
     */
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

    //Used for playing sounds that we want to range the pitch of. Otherwise the player can call the non changing pitch audio source
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

    //Call this when the player gets hit by enemy. Mirroring castlvania style knockback.
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
        Physics2D.IgnoreLayerCollision(9, 7, true);
        invulnerable = true;
        yield return new WaitForSeconds(.1f);
        invulnerable = false;
        Physics2D.IgnoreLayerCollision(9, 7, false);
        yield return null;
    }

    //Main used for the pick up to be able to start a corutine. Why cant I call a corutine in another script??
    public void bulletPickUp()
    {
        StartCoroutine(bulletTime());
    }

    //Power up mode for the player. Still need to add some sounds and what not to this.
    public IEnumerator bulletTime()
    {
        /*
         * Refill player ammo back to full and update the UI to show accordingly
         * Change the color of the player and UI bullets to magenta (for now) to show the player event change has happened.
         * 
         * We only need to call controller.updateAmmoImageUI due to the fact we are resetting ammo back to full.
         */
        isBulletTime = true;
        ammo = maxAmmo;
        controller.updateAmmoImageUI();
        controller.changeAmmoImageColor(Color.magenta);
        spriteRenderer.color= Color.magenta;
        SpriteRenderer temp = bullet.GetComponent<SpriteRenderer>();
        temp.color = Color.magenta;

        yield return new WaitForSeconds(bulletTimeDuration);

        /*
         * Once bullet time is over we revert the colors back and turn bulletTime bool back to false
         */
        isBulletTime = false;
        controller.changeAmmoImageColor(Color.white);
        temp.color = Color.white;
        spriteRenderer.color = Color.white;
        yield return null;
    }

    public void increaseScore(float amount)
    {
        score += amount;
        controller.updateScore();
    }
}
