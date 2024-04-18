using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Rendering.PostProcessing;


// MoveBehaviour inherits from GenericBehaviour. This class corresponds to basic walk and run behaviour, it is the default behaviour.

public class MoveBehaviour : GenericBehaviour
{
    public float walkSpeed = 0.15f;                 // Default walk speed.
    public float runSpeed = 1.0f;                   // Default run speed.
    public float sprintSpeed = 2.0f;                // Default sprint speed.
    public float speedDampTime = 0.1f;              // Default damp time to change the animations based on current speed.
    public string jumpButton = "Jump";              // Default jump button.
    public float jumpHeight = 1.5f;                 // Default jump height.
    public float jumpInertialForce = 10f;          // Default horizontal inertial force when jumping.
    public float attackRange = 1f;
    public Transform attackPoint;
    public Text playerHP;
    public Text playerMaxHP;
    public AudioManager sound;

    private bool inWater = false;

    private float lastDownAttackTime;

    

    
    private bool isRunningSoundPlaying = false;
    private bool isSprintingSoundPlaying = false;
    private bool isWaterSoundPlaying = false;

    public int healthKitHealthAmount = 30;

    private bool victoryGruntPlayed = false;

    public LayerMask enemyLayers;
    public LayerMask playerLayers;
    public int attackDamage;
    public int maxHealth;
    private int currentHealth;
    public Collider[] weaponColliders;
    public float attackCooldown = 1f;
    public float attackCooldownPlayer = 1f;// Adjust as needed
    private float lastAttackTime;
    private bool isDead;
    private bool isBlocking;
    private bool isBlockingPlayed = false;
    private bool isResumed = false;

    public GameObject ResumePanel;
    public GameObject VictoryPanel;
    public GameObject Clue;
    public Button backFromClue;
    public GameObject treasure;
    public GameObject piratesKilled;

    private int enemiesKilled = 0;
    public Text kills;
    
    public GENO[] enemies;
    
    private Animator anim;
    private float speed, speedSeeker;               // Moving speed.
    private int jumpBool;                           // Animator variable related to jumping.
    private int groundedBool;                       // Animator variable related to whether or not the player is on ground.
    private bool jump;                              // Boolean to determine whether or not the player started a jump.
    private bool isColliding;                       // Boolean to determine if the player has collided with an obstacle.

    public Collider underwaterCollider; // Reference to the collider representing underwater area
    public GameObject underwaterEffectObject; // Reference to the post-processing volume for underwater effect
    // Start is always called after any Awake functions.

    void Start()
    {
        // Set up the references.
        jumpBool = Animator.StringToHash("Jump");
        groundedBool = Animator.StringToHash("Grounded");
        behaviourManager.GetAnim.SetBool(groundedBool, true);

        // Subscribe and register this behaviour as the default behaviour.
        behaviourManager.SubscribeBehaviour(this);
        behaviourManager.RegisterDefaultBehaviour(this.behaviourCode);
        speedSeeker = runSpeed;
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        sound = GetComponent<AudioManager>();
        treasure = GameObject.Find("all_treasure");
        

        enemies = FindObjectsOfType<GENO>();
        PopulateWeaponColliders();

        UpdateHealthText();
        



    }

    // Update is used to set features regardless the active behaviour.
    void Update()
    {
        if (!jump && Input.GetButtonDown(jumpButton) && behaviourManager.IsCurrentBehaviour(this.behaviourCode) && !behaviourManager.IsOverriding())
        {
            jump = true;
            FindAnyObjectByType<AudioManager>().Play("Jump");
        }

        // Check if the character is not jumping before playing running and sprinting sounds
        if (!behaviourManager.GetAnim.GetBool(jumpBool))
        {
            if (speed > 0 && speedSeeker <= runSpeed && !isRunningSoundPlaying && !behaviourManager.IsSprinting())
            {
                // Play running sound only when speed is greater than 0 and the character is not sprinting
                FindAnyObjectByType<AudioManager>().Play("running");
                isRunningSoundPlaying = true;
            }
            // Check if the character has stopped moving or started sprinting
            else if ((speed == 0 || speedSeeker > runSpeed || behaviourManager.IsSprinting()) && isRunningSoundPlaying)
            {
                // Stop running sound if speed is 0, the character is sprinting, or exceeded run speed
                FindAnyObjectByType<AudioManager>().Stop("running");
                isRunningSoundPlaying = false;
            }

            if (speed > 0 && speedSeeker > walkSpeed && !isSprintingSoundPlaying && behaviourManager.IsSprinting())
            {
                // Play sprinting sound only when speed is greater than 0 and the character is sprinting
                FindAnyObjectByType<AudioManager>().Play("sprinting");
                isSprintingSoundPlaying = true;
            }
            // Check if the character has stopped moving or stopped sprinting
            else if ((speed == 0 || speedSeeker <= walkSpeed || !behaviourManager.IsSprinting()) && isSprintingSoundPlaying)
            {
                // Stop sprinting sound if speed is 0, the character is not sprinting, or exceeded walk speed
                FindAnyObjectByType<AudioManager>().Stop("sprinting");
                isSprintingSoundPlaying = false;
            }
        }
        if (!jump && Input.GetMouseButtonDown(0) && behaviourManager.IsCurrentBehaviour(this.behaviourCode) && !behaviourManager.IsOverriding())

        {
            DownAttack();
        }
        foreach (GENO enemyAI in enemies)
        {
            if (Time.time - lastAttackTime >= attackCooldown && !enemyAI.isDead)
            {
                foreach (Collider weaponCollider in weaponColliders)
                {
                    if (weaponCollider != null)
                    {
                        // Check if the weaponCollider is hitting the player
                        Collider[] hitPlayers = Physics.OverlapSphere(weaponCollider.transform.position, attackRange, playerLayers);

                        foreach (Collider playerCollider in hitPlayers)
                        {
                            Debug.Log("Player hit by enemy attack");
                            MoveBehaviour playerBehaviour = playerCollider.GetComponent<MoveBehaviour>();

                            // Check if the component is not null before calling TakeDamage
                            if (playerBehaviour != null && !isBlocking)
                            {
                                // Call the TakeDamage method on the player component
                                playerBehaviour.TakeDamage(enemyAI.attackDamage);
                                //*sound.Play("Hit");
                            }
                        }

                        // Update the last attack time
                        lastAttackTime = Time.time;
                    }
                }
            }
        }
        if (Input.GetMouseButtonDown(1))
        {
            BlockAttack();
        }
        if (Input.GetMouseButtonUp(1))
        {
            // If mouse button released, stop blocking
            isBlocking = false;
            isBlockingPlayed = false;
        }


        // Check if any enemy is alive
        bool anyEnemyAlive = false;
        foreach (GENO enemy in enemies)
        {
            if (!enemy.isDead)
            {
                anyEnemyAlive = true;
                break;
            }
        }

        // Modify the condition to use anyEnemyDead
        if (isDead)
        {
            Invoke("ResumeScreen", 4f);
        }
        if (!anyEnemyAlive)
        {
            treasure.SetActive(true);
            piratesKilled.SetActive(true);
        }
        if(inWater)
        {
            WaterSound();
        }
        else
        {
            FindAnyObjectByType<AudioManager>().Stop("swim");
        }
    }

    // LocalFixedUpdate overrides the virtual function of the base class.
    public override void LocalFixedUpdate()
    {
        // Call the basic movement manager.
        MovementManagement(behaviourManager.GetH, behaviourManager.GetV);

        // Call the jump manager.
        JumpManagement();
    }

    // Execute the idle and walk/run jump movements.
    void JumpManagement()
    {
        // Start a new jump.
        if (jump && !behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.IsGrounded())
        {
            // Set jump related parameters.
            behaviourManager.LockTempBehaviour(this.behaviourCode);
            behaviourManager.GetAnim.SetBool(jumpBool, true);
            // Is a locomotion jump?
            if (behaviourManager.GetAnim.GetFloat(speedFloat) > 0.1)
            {
                // Temporarily change player friction to pass through obstacles.
                GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
                GetComponent<CapsuleCollider>().material.staticFriction = 0f;
                // Remove vertical velocity to avoid "super jumps" on slope ends.
                RemoveVerticalVelocity();
                // Set jump vertical impulse velocity.
                float velocity = 2f * Mathf.Abs(Physics.gravity.y) * jumpHeight;
                velocity = Mathf.Sqrt(velocity);
                behaviourManager.GetRigidBody.AddForce(Vector3.up * velocity, ForceMode.VelocityChange);
            }
        }
        // Is already jumping?
        else if (behaviourManager.GetAnim.GetBool(jumpBool))
        {
            // Keep forward movement while in the air.
            if (!behaviourManager.IsGrounded() && !isColliding && behaviourManager.GetTempLockStatus())
            {
                behaviourManager.GetRigidBody.AddForce(transform.forward * (jumpInertialForce * Physics.gravity.magnitude * sprintSpeed), ForceMode.Acceleration);
            }
            // Has landed?
            if ((behaviourManager.GetRigidBody.velocity.y < 0) && behaviourManager.IsGrounded())
            {
                behaviourManager.GetAnim.SetBool(groundedBool, true);
                // Change back player friction to default.
                GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
                GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
                // Set jump related parameters.
                jump = false;
                behaviourManager.GetAnim.SetBool(jumpBool, false);
                behaviourManager.UnlockTempBehaviour(this.behaviourCode);
            }
        }
    }

    // Deal with the basic player movement
    void MovementManagement(float horizontal, float vertical)
    {
        // On ground, obey gravity.
        if (behaviourManager.IsGrounded())
            behaviourManager.GetRigidBody.useGravity = true;

        // Avoid takeoff when reached a slope end.
        else if (!behaviourManager.GetAnim.GetBool(jumpBool) && behaviourManager.GetRigidBody.velocity.y > 0)
        {
            RemoveVerticalVelocity();
        }

        // Call function that deals with player orientation.
        Rotating(horizontal, vertical);

        // Set proper speed.
        Vector2 dir = new Vector2(horizontal, vertical);
        speed = Vector2.ClampMagnitude(dir, 1f).magnitude;
        // This is for PC only, gamepads control speed via analog stick.
        speedSeeker += Input.GetAxis("Mouse ScrollWheel");
        speedSeeker = Mathf.Clamp(speedSeeker, walkSpeed, runSpeed);
        speed *= speedSeeker;
        if (behaviourManager.IsSprinting())
        {
            speed = sprintSpeed;
        }

        // Only apply movement if the "w" key is pressed (forward movement).
        if (vertical > 0)
        {
            if (inWater)
            {
                // Apply faster swim speed underwater.
                speed *= 2;
            }
            behaviourManager.GetRigidBody.MovePosition(transform.position + transform.forward * speed * Time.deltaTime);
        }

        // Set the animator parameter for speed.
        behaviourManager.GetAnim.SetFloat(speedFloat, speed, speedDampTime, Time.deltaTime);

       

    }

    // Remove vertical rigidbody velocity.
    private void RemoveVerticalVelocity()
    {
        Vector3 horizontalVelocity = behaviourManager.GetRigidBody.velocity;
        horizontalVelocity.y = 0;
        behaviourManager.GetRigidBody.velocity = horizontalVelocity;
    }

    // Rotate the player to match correct orientation, according to camera and key pressed.
    Vector3 Rotating(float horizontal, float vertical)
    {
        // Get camera forward direction, without vertical component.
        Vector3 forward = behaviourManager.playerCamera.TransformDirection(Vector3.forward);

        // Player is moving on ground, Y component of camera facing is not relevant.
        forward.y = 0.0f;
        forward = forward.normalized;

        // Calculate target direction based on camera forward and direction key.
        Vector3 right = new Vector3(forward.z, 0, -forward.x);
        Vector3 targetDirection = forward * vertical + right * horizontal;

        // Lerp current direction to calculated target direction.
        if ((behaviourManager.IsMoving() && targetDirection != Vector3.zero))
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);

            Quaternion newRotation = Quaternion.Slerp(behaviourManager.GetRigidBody.rotation, targetRotation, behaviourManager.turnSmoothing);
            behaviourManager.GetRigidBody.MoveRotation(newRotation);
            behaviourManager.SetLastDirection(targetDirection);
        }
        // If idle, Ignore current camera facing and consider last moving direction.
        if (!(Mathf.Abs(horizontal) > 0.9 || Mathf.Abs(vertical) > 0.9))
        {
            behaviourManager.Repositioning();
        }

        return targetDirection;
    }

    // Collision detection.
    private void OnCollisionStay(Collision collision)
    {
        isColliding = true;
        // Slide on vertical obstacles
        if (behaviourManager.IsCurrentBehaviour(this.GetBehaviourCode()) && collision.GetContact(0).normal.y <= 0.1f)
        {
            GetComponent<CapsuleCollider>().material.dynamicFriction = 0f;
            GetComponent<CapsuleCollider>().material.staticFriction = 0f;
        }
    }
    private void OnCollisionExit(Collision collision)
    {
        isColliding = false;
        GetComponent<CapsuleCollider>().material.dynamicFriction = 0.6f;
        GetComponent<CapsuleCollider>().material.staticFriction = 0.6f;
    }
    private void DownAttack()
    {
        // Check if enough time has passed since the last attack
        if (Time.time - lastDownAttackTime >= attackCooldownPlayer)
        {
            // Trigger the attack animation
            anim.SetTrigger("attack");

            // Find enemies within the attack range
            Collider[] hitEnemies = Physics.OverlapSphere(attackPoint.position, attackRange, enemyLayers);

            if (hitEnemies.Length == 0 && !isDead && !isResumed)
            {
                // No enemies were hit, play "miss enemy" audio effect
                FindAnyObjectByType<AudioManager>().Play("Miss enemy");
            }
            else
            {
                foreach (Collider enemy in hitEnemies)
                {
                    Debug.Log("SMACKED " + enemy.name);
                    GENO enemyAI = enemy.GetComponent<GENO>();

                    // Check if the component is not null before calling TakeDamage
                    if (enemyAI != null && !isDead && !isResumed)
                    {
                        // Call the TakeDamage method on the EnemyAI component
                        enemyAI.TakeDamage(attackDamage);
                        Invoke("PlayHitSound", 0.5f);
                    }
                }
            }

            // Update the last attack time
            lastDownAttackTime = Time.time;
        }
        else
        {
            // Attack is on cooldown, you can add some feedback or simply ignore the attack
            Debug.Log("Attack is on cooldown");
        }
    }
    void PlayHitSound()
    {
        FindAnyObjectByType<AudioManager>().Play("Hit");
    }
    public void TakeDamage(int damage)
    {
        if (isDead)
        {
            return; // Do nothing if the player is already dead
        }


        currentHealth -= damage;

        UpdateHealthText();
        if (currentHealth <= 0)
        {
            isDead = true;
            PlayerDeath();
        }
    }
    private void PlayerDeath()
    {

        isDead = true;
        GetComponent<Collider>().enabled = false;
        anim.SetTrigger("death");


        FindAnyObjectByType<AudioManager>().Play("player dead");
        // GAME OVER SCREEN
        Invoke("ResumeScreen", 4f);

    }
    private void GameOverScreen()
    {
        Time.timeScale = 0f;
        isResumed = true;
    }
    public bool IsDead()
    {
        return isDead;
    }
    private void UpdateHealthText()
    {
        if (playerHP != null)
        {
            playerHP.text = currentHealth.ToString();
            if (!isBlocking)
                playerMaxHP.text = "/" + maxHealth.ToString();
        }
    }
    public void EnemyKilled()
    {
        enemiesKilled++;
        Debug.Log("Enemy killed! Total enemies killed: " + enemiesKilled);
    }
    public void UpdateKillText()
    {
        if (kills != null)
        {
            kills.text = enemiesKilled.ToString();
            
        }
    }
    public void BlockAttack()
    {
        foreach (GENO enemy in enemies)
        {
            if (IsEnemyInFront(enemy))
            {
                anim.SetTrigger("block");
                isBlocking = true;
                if (!isBlockingPlayed)
                {
                    FindAnyObjectByType<AudioManager>().Play("Block");
                    isBlockingPlayed = true;
                }


            }
        }
    }

    bool IsEnemyInFront(GENO enemy)
    {
        // Calculate the direction vector from the player to the enemy
        Vector3 toEnemy = enemy.transform.position - transform.position;

        // Normalize the vectors to get the direction
        Vector3 playerForward = transform.forward;
        Vector3 toEnemyNormalized = toEnemy.normalized;

        // Calculate the dot product between the player's forward direction and the vector to the enemy
        float dotProduct = Vector3.Dot(playerForward, toEnemyNormalized);

        // Set a threshold angle to determine if the enemy is in front (adjust as needed)
        float angleThreshold = 0.8f;

        // Check if the dot product is greater than the cosine of the threshold angle
        return dotProduct > Mathf.Cos(angleThreshold);
    }

    public void ResumeScreen()

    {
        ResumePanel.SetActive(true);
        GameOverScreen();
    }
    public void VictoryScreen()

    {
        VictoryPanel.SetActive(true);
        GameOverScreen();
    }
    public void ResumeGame()
    {
        Time.timeScale = 1f;
        ResumePanel.SetActive(false);
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            inWater = true;
            anim.SetBool("isSwimming", true);
            

        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Water"))
        {
            inWater = false;
            anim.SetBool("isSwimming", false);
            ExitWater();
            
            
        }
    }
    private void OnCollisionEnter(Collision collision)
    {
        
        if (collision.gameObject.CompareTag("kit"))
        {
            
            
            currentHealth += healthKitHealthAmount;

            // Clamp health to maxHealth
            currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

            // Update health UI text
            UpdateHealthText();

            FindAnyObjectByType<AudioManager>().Play("health");
            // Destroy the health kit object
            Destroy(collision.gameObject);
        }
        if (collision.gameObject.CompareTag("clue"))
        {


            Clue.SetActive(true);

            Time.timeScale = 0f;

            FindAnyObjectByType<AudioManager>().Play("health");
            
            Destroy(collision.gameObject);
            
        }
        if (collision.gameObject.CompareTag("treasure"))
        {
            inWater = false;
            anim.SetBool("isSwimming", false);
            anim.SetBool("foundTreasure", true);
            underwaterEffectObject.SetActive(false);

            if (!victoryGruntPlayed)
            {
                Invoke("PlayVictoryAudio", 0.5f);
                victoryGruntPlayed = true; // Set the flag to true indicating that the audio has been played
            }
            Invoke("VictoryScreen", 6f);
        }
       
    }
    private void PlayVictoryAudio()
    {
        // Play the victory audio clips
        FindAnyObjectByType<AudioManager>().Play("victory grunt");
        FindAnyObjectByType<AudioManager>().Play("victory");
    }
    void PopulateWeaponColliders()
    {
        // Find all enemies with the GENO component
        GENO[] enemies = FindObjectsOfType<GENO>();

        // Initialize weaponColliders array with the size of the enemies array
        weaponColliders = new Collider[enemies.Length];

        for (int i = 0; i < enemies.Length; i++)
        {
            // Assign weaponCollider of each enemy to the array
            weaponColliders[i] = enemies[i].weaponCollider;
        }
    }
    public void BackFromClue()
    {
        Destroy(Clue);
        Time.timeScale = 1f;
    }
    private void WaterSound()
    {
        FindAnyObjectByType<AudioManager>().Stop("running");
        FindAnyObjectByType<AudioManager>().Stop("sprinting");
        if (!isWaterSoundPlaying)
        {
            FindAnyObjectByType<AudioManager>().Play("swim");
            isWaterSoundPlaying = true;
        }
        
    }
    private void ExitWater()
    {
        FindAnyObjectByType<AudioManager>().Stop("swim");
        isWaterSoundPlaying = false;
    }

}

