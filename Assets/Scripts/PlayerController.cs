using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Animator), typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    public float laneDistance = 2.5f;
    public float laneChangeSpeed = 10f;
    public float jumpForce = 7f;

    [Header("Speed Up UI")]
    public GameObject speedSliderParent;
    public Slider speedSlider;

    [Header("Health UI")]
    public Slider healthSlider;
    public int maxHealth = 10;
    private int currentHealth;

    public int currentLane = 1;
    private float verticalVelocity = 0f;
    private bool isJumping = false;
    private bool isSliding = false;

    private Animator animator;
    private CharacterController controller;

    private Coroutine speedUpCoroutine;

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

        // Set initial CharacterController size
        SetColliderHeight(2f, 1f);

        animator.Play("Run");

        if (speedSliderParent != null)
            speedSliderParent.SetActive(false);

        currentHealth = maxHealth;
        UpdateHealthUI();

        if (stepSoundPlayer != null)
        {
            stepSoundPlayer.stepInterval = 0.4f; // Normal step interval
            stepSoundPlayer.StartSteps();        // Start footsteps on start
        }

        DialogueAudio.PlayRandomDialogue1();

        // Start calling PlayDialogue every 5–10 seconds randomly
        Invoke("StartDialogueLoop", 1f);
    }

    void Update()
    {
        HandleInput();
        MovePlayer();

        // Debug line for raycast visualization (optional)
        Debug.DrawRay(transform.position + Vector3.up * 0.5f, transform.forward * 2f, Color.red);
    }

    void HandleInput()
    {
        if (isSliding)
            return;  // Ignore all input during slide (or at least lane changes)

        if (Input.GetKeyDown(KeyCode.LeftArrow) && currentLane > 0)
            currentLane--;

        if (Input.GetKeyDown(KeyCode.RightArrow) && currentLane < 2)
            currentLane++;

        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            verticalVelocity = jumpForce;
            animator.SetTrigger("Jump");
        }

        if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
        {
            animator.SetTrigger("Slide");
            StartCoroutine(SlideRoutine());
        }
    }

    IEnumerator SlideRoutine()
    {
        isSliding = true;

        // Change collider for sliding
        SetColliderHeight(0f, 0.3f);

        yield return new WaitForSeconds(2f); // Slide duration including reset delay

        // Reset collider to original size
        SetColliderHeight(2f, 1f);

        isSliding = false;
    }

    void MovePlayer()
    {
        Vector3 targetPosition = transform.position.z * Vector3.forward;

        if (currentLane == 0)
            targetPosition += Vector3.left * laneDistance;
        else if (currentLane == 2)
            targetPosition += Vector3.right * laneDistance;

        float xDiff = targetPosition.x - transform.position.x;
        float xMove = xDiff * laneChangeSpeed;

        if (isJumping)
            verticalVelocity += Physics.gravity.y * Time.deltaTime;

        Vector3 move = new Vector3(xMove, verticalVelocity, 0f);
        controller.Move(move * Time.deltaTime);

        if (isJumping && controller.isGrounded)
        {
            isJumping = false;
            verticalVelocity = -1f;
        }
    }

    void OnAnimatorMove()
    {
        Vector3 forwardMove = animator.deltaPosition;
        forwardMove.x = 0f;
        forwardMove.y = 0f;
        controller.Move(forwardMove);
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
        animator.SetBool("Sprint", true);

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

        animator.SetBool("Sprint", false);

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
        animator.SetTrigger("Die");
        enabled = false; // Stop controller updates
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

    private void SetColliderHeight(float height, float centerY)
    {
        if (controller != null)
        {
            controller.height = height;
            controller.center = new Vector3(0f, centerY, 0f);
        }
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

}