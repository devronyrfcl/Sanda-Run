using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float forwardSpeed = 14f;
    public float sprintSpeedMultiplier = 1.4f;
    public float laneDistance = 2.5f;
    public float laneChangeSpeed = 10f;
    public bool autoStart = true;               // false = wait for GameStart()

    [Header("Jump / Gravity")]
    public float jumpHeight = 1.5f;
    public float gravity = -20f;
    public Transform groundCheck;               // optional, falls back to CharacterController.isGrounded
    public LayerMask groundLayer = ~0;
    public float groundCheckRadius = 0.17f;

    [Header("Slide")]
    public float slideDuration = 0.8f;

    [Header("Speed Up UI")]
    public GameObject speedSliderParent;
    public Slider speedSlider;

    [Header("Health UI")]
    public Slider healthSlider;
    public int maxHealth = 10;
    private int currentHealth;

    [Header("State (read only)")]
    public int currentLane = 1;                 // 0 = left, 1 = mid, 2 = right
    public bool isGameOn;
    public bool isGrounded;
    public bool isSliding;

    private float xPos;
    private Vector3 velocity;
    private float colHeight;
    private float colCenterY;
    private bool sprinting;

    private Animator animator;
    private CharacterController controller;
    private HashSet<string> animParams;

    private Coroutine speedUpCoroutine;
    private Coroutine slideCoroutine;

    // Inputs queued by the public Left/Right/Jump/Slide methods (e.g. SwipeInputHandler)
    private bool queuedLeft, queuedRight, queuedJump, queuedSlide;

    [Header("Effects")]
    public GameObject obstacleHitEffectPrefab;
    public GameObject headStars; // Assign your HeadStars GameObject in Inspector

    [Header("Step Sound")]
    public StepSoundPlayer stepSoundPlayer;

    [Header("Game Over UI")]
    public GameObject gameOverUI; // Assign in inspector

    public DialogueAudio DialogueAudio;

    void Start()
    {
        Time.timeScale = 1f;
        animator = GetComponent<Animator>();
        controller = GetComponent<CharacterController>();

        animParams = new HashSet<string>();
        foreach (var p in animator.parameters)
            animParams.Add(p.name);

        colHeight = controller.height;
        colCenterY = controller.center.y;
        xPos = transform.position.x;

        if (speedSliderParent != null)
            speedSliderParent.SetActive(false);

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (autoStart)
            GameStart();
    }

    public void GameStart()
    {
        if (isGameOn)
            return;

        isGameOn = true;
        SetTrigger("isGameStarted");

        if (stepSoundPlayer != null)
        {
            stepSoundPlayer.stepInterval = 0.4f; // Normal step interval
            stepSoundPlayer.StartSteps();
        }

        if (DialogueAudio != null)
            DialogueAudio.PlayRandomDialogue1();

        // Start calling PlayDialogue every 5-10 seconds randomly
        Invoke(nameof(StartDialogueLoop), 1f);
    }

    void Update()
    {
        if (!isGameOn)
            return;

        HandleInput();
        MovePlayer();
    }

    void HandleInput()
    {
        bool left  = Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow)  || SwipeManager.swipeLeft  || queuedLeft;
        bool right = Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || SwipeManager.swipeRight || queuedRight;
        bool up    = Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.Space) || SwipeManager.swipeUp || queuedJump;
        bool down  = Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow)  || SwipeManager.swipeDown  || queuedSlide;
        queuedLeft = queuedRight = queuedJump = queuedSlide = false;

        if (left && currentLane > 0)
            currentLane--;
        else if (right && currentLane < 2)
            currentLane++;

        if (isGrounded)
        {
            if (up)
                DoJump();
            else if (down && !isSliding)
                slideCoroutine = StartCoroutine(SlideRoutine());
        }
        else if (down && !isSliding)
        {
            // Slam back down and slide on landing
            slideCoroutine = StartCoroutine(SlideRoutine());
            velocity.y = -10f;
        }
    }

    void DoJump()
    {
        EndSlide();
        SetTrigger("jump");
        velocity.y = Mathf.Sqrt(jumpHeight * 2f * -gravity);
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;
        SetBool("isSliding", true);

        controller.center = new Vector3(0f, colCenterY / 2f, 0f);
        controller.height = colHeight / 2f;

        yield return new WaitForSeconds(slideDuration);

        EndSlide();
    }

    void EndSlide()
    {
        if (slideCoroutine != null)
        {
            StopCoroutine(slideCoroutine);
            slideCoroutine = null;
        }

        if (controller != null)
        {
            controller.center = new Vector3(0f, colCenterY, 0f);
            controller.height = colHeight;
        }

        SetBool("isSliding", false);
        isSliding = false;
    }

    void MovePlayer()
    {
        // Sideways: smooth towards the lane's X
        float targetX = (currentLane - 1) * laneDistance;
        xPos = Mathf.Lerp(xPos, targetX, laneChangeSpeed * Time.deltaTime);

        // Ground check
        if (groundCheck != null)
            isGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer, QueryTriggerInteraction.Ignore);
        else
            isGrounded = controller.isGrounded;
        SetBool("isGrounded", isGrounded);

        // Gravity
        if (isGrounded && velocity.y < 0f)
            velocity.y = -1f;
        else if (!isGrounded)
            velocity.y += gravity * Time.deltaTime;

        velocity.z = forwardSpeed * (sprinting ? sprintSpeedMultiplier : 1f);

        Vector3 move = velocity * Time.deltaTime;
        move.x = xPos - transform.position.x;
        controller.Move(move);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Biriyani"))
        {
            CurrencyManager.Instance.AddCoins(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("SpeedUp"))
        {
            if (speedUpCoroutine != null)
                StopCoroutine(speedUpCoroutine);

            speedUpCoroutine = StartCoroutine(SpeedUpRoutine());
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("Obstacles"))
        {
            // Spawn particle effect 2f above the obstacle
            if (obstacleHitEffectPrefab != null)
            {
                Vector3 spawnPos = other.transform.position + Vector3.up * 2f;
                Instantiate(obstacleHitEffectPrefab, spawnPos, Quaternion.identity);
            }

            if (DialogueAudio != null)
                DialogueAudio.PlayRandomDialogue4();

            Destroy(other.gameObject);

            // Show HeadStars effect for 2 seconds
            if (headStars != null)
                StartCoroutine(ShowHeadStarsRoutine());

            // Only take damage if not sliding through PassOut
            if (!(IsPassOutAhead() && isSliding))
            {
                ChangeHealth(-1);
            }
        }

        else if (other.CompareTag("Health"))
        {
            ChangeHealth(1);
            Destroy(other.gameObject);
        }
        else if (other.CompareTag("PassOut"))
        {
            Debug.Log("Player passed out!");
            // Handle PassOut logic here
        }
    }

    private IEnumerator SpeedUpRoutine()
    {
        sprinting = true;
        SetBool("Sprint", true);

        if (speedSliderParent != null)
            speedSliderParent.SetActive(true);

        if (speedSlider != null)
            speedSlider.value = 0f;

        // Update step sound interval for sprint
        if (stepSoundPlayer != null)
            stepSoundPlayer.stepInterval = 0.25f;

        float duration = 5f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            if (speedSlider != null)
                speedSlider.value = Mathf.Clamp01(elapsed / duration);

            yield return null;
        }

        sprinting = false;
        SetBool("Sprint", false);

        if (speedSliderParent != null)
            speedSliderParent.SetActive(false);

        // Reset step interval back to normal after sprint
        if (stepSoundPlayer != null)
            stepSoundPlayer.stepInterval = 0.4f;
    }

    private void ChangeHealth(int amount)
    {
        currentHealth += amount;
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        UpdateHealthUI();

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void UpdateHealthUI()
    {
        if (healthSlider != null)
        {
            healthSlider.maxValue = maxHealth;
            healthSlider.value = currentHealth;
        }
    }

    private void Die()
    {
        Debug.Log("Player died!");
        SetTrigger("Die");
        enabled = false; // Stop controller updates
        if (DialogueAudio != null)
            DialogueAudio.PlayRandomDialogue3();

        // Stop time
        Time.timeScale = 0f;

        // Show Game Over UI
        if (gameOverUI != null)
            gameOverUI.SetActive(true);
    }


    private bool IsPassOutAhead()
    {
        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, transform.forward);
        RaycastHit hit;

        Debug.DrawRay(ray.origin, ray.direction * 2f, Color.red, 1f); // Show red ray in Scene view

        if (Physics.Raycast(ray, out hit, 2f))
        {
            Debug.Log("Raycast hit: " + hit.collider.name + ", Tag: " + hit.collider.tag);

            if (hit.collider.CompareTag("PassOut"))
            {
                Debug.Log("Raycast detected PassOut ahead!");
                return true;
            }
        }
        else
        {
            Debug.Log("Raycast did NOT hit anything.");
        }

        return false;
    }

    private IEnumerator ShowHeadStarsRoutine()
    {
        headStars.SetActive(true);
        yield return new WaitForSeconds(2f);
        headStars.SetActive(false);
    }

    void StartDialogueLoop()
    {
        InvokeRepeating(nameof(PlayDialogueWithRandomDelay), 0f, 1f); // check every 1 second
    }

    float timer = 0f;
    float nextTime = 5f;

    void PlayDialogueWithRandomDelay()
    {
        timer += 1f;

        if (timer >= nextTime)
        {
            if (DialogueAudio != null)
                DialogueAudio.PlayRandomDialogue2();

            timer = 0f;
            nextTime = Random.Range(5f, 10f); // next delay
        }
    }

    // Public hooks (e.g. for on-screen buttons or SwipeInputHandler); processed on the next Update
    public void Left()  { queuedLeft = true; }
    public void Right() { queuedRight = true; }
    public void Jump()  { queuedJump = true; }
    public void Slide() { queuedSlide = true; }

    // Only touch animator parameters that exist in the controller (DJ Alok has no Sprint / Die)
    private void SetBool(string name, bool value)
    {
        if (animParams != null && animParams.Contains(name))
            animator.SetBool(name, value);
    }

    private void SetTrigger(string name)
    {
        if (animParams != null && animParams.Contains(name))
            animator.SetTrigger(name);
    }
}
