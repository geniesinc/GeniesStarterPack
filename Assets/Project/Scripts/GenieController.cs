using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// For now, we are only considering that there will ever be one Genie in play!
/// </summary>
public class GenieController : MonoBehaviour
{
    [SerializeField] Animator animator;

    private GeniePlayerActions geniePlayerActions;
    private InputAction moveAction;

    private float velocity = 0f;
    private float acceleration_anim = 1f;
    private float acceleration_translation = 5f;
    private float acceleration_rotation = 20f;

    private const string jumpTriggerName = "Jump";
    private const string waveTriggerName = "Wave";

    private void Start()
    {
        // Setup the input system
        geniePlayerActions = new GeniePlayerActions();
        // Listen to move input
        moveAction = geniePlayerActions.Player.Move;
        moveAction.Enable();
        // Listen for jump input
        geniePlayerActions.Player.Jump.performed += OnPlayerJump;
        geniePlayerActions.Player.Jump.Enable();
        // Listen for jump input
        geniePlayerActions.Player.Wave.performed += OnPlayerWave;
        geniePlayerActions.Player.Wave.Enable();

        // Sanity-check
        if (animator.applyRootMotion)
        {
            Debug.LogError($"{gameObject.name} has 'applyRootMotion' checkbox checked. Unchecking!");
            animator.applyRootMotion = false;
        }
    }

    private void FixedUpdate()
    {
        OnPlayerMove(moveAction.ReadValue<Vector2>());
    }

    private void OnDestroy()
    {
        // Clean up listeners
        geniePlayerActions.Player.Move.Disable();
        geniePlayerActions.Player.Jump.performed -= OnPlayerJump;
        geniePlayerActions.Player.Jump.Disable();
        geniePlayerActions.Player.Wave.performed -= OnPlayerWave;
        geniePlayerActions.Player.Wave.Enable();
    }

    private void OnPlayerJump(InputAction.CallbackContext obj)
    {
        animator.SetTrigger(jumpTriggerName);
    }

    private void OnPlayerWave(InputAction.CallbackContext obj)
    {
        animator.SetTrigger(waveTriggerName);
    }

    private void OnPlayerMove(Vector2 moveDir)
    {
        // Convert from Vector2d to Vector3d
        Vector3 moveDirRelative = new Vector3(moveDir.x,
                                               0f,
                                               moveDir.y).normalized;

        // Convert from D-Pad to world direction
        Vector3 moveDirWorld = Quaternion.Euler(0,
                                                Camera.main.transform.eulerAngles.y,
                                                0) * moveDirRelative;

        // Modify velocity based on input
        if(moveDir.magnitude == 0)
        {
            velocity = 0;
        }
        else if (velocity < 1)
        {
            velocity += moveDir.magnitude * Time.deltaTime * acceleration_anim;
        }

        //Blending parameter between walk and run
        animator.SetFloat("Velocity", velocity);

        // Face the direction you're walking in
        if(moveDir.magnitude > 0)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                    Quaternion.LookRotation(moveDirWorld, Vector3.up),
                                                    Time.deltaTime * acceleration_rotation);
        }

        // Translate in the direction you're walking in
        transform.position += moveDirWorld * velocity * acceleration_translation
                                * transform.localScale.x * Time.deltaTime;
    }
}
