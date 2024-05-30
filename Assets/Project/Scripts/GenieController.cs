using UnityEngine;
using UnityEngine.InputSystem;


/// <summary>
/// For now, we are only considering that there will ever be one Genie in play!
/// </summary>
public class GenieController : MonoBehaviour
{
    [SerializeField] Animator animator;
    [SerializeField] public BoxCollider boxCollider;

    // Note: We assume all renderers have the same list of blendshapes on them,
    //       in the same order.
    [SerializeField] SkinnedMeshRenderer[] skinnedMeshRenderers;

    private GeniePlayerActions geniePlayerActions;
    private InputAction moveAction;

    private float velocity = 0f;
    private float acceleration_anim = 1f;
    private float acceleration_translation = 5f;
    private float acceleration_rotation = 20f;

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
        geniePlayerActions.Player.Jump.performed -= OnPlayerJump;
        geniePlayerActions.Player.Move.Disable();
        geniePlayerActions.Player.Look.Disable();
        geniePlayerActions.Player.Jump.Disable();
    }

    private void OnPlayerJump(InputAction.CallbackContext obj)
    {
        Debug.Log("Jump!");
    }

    private void OnPlayerMove(Vector2 moveDir)
    {
        Vector3 moveDirRelative = new Vector3(moveDir.x,
                                               0f,
                                               moveDir.y).normalized;

        Vector3 moveDirWorld = Quaternion.Euler(0,
                                                Camera.main.transform.eulerAngles.y,
                                                0) * moveDirRelative;

        // Clear velocity
        if(moveDir.magnitude == 0)
        {
            velocity = 0;
        }
        // Add to velocity
        else if (velocity < 1)
        {
            velocity += moveDir.magnitude * Time.deltaTime * acceleration_anim;
        }

        //Blending parameter between walk and run
        animator.SetFloat("Velocity", velocity);

        transform.position += moveDirWorld * velocity * acceleration_translation
                                * transform.localScale.x * Time.deltaTime;

        // Target rotation should check if look rotation viewing vector is smaller/larger than zero,
        // or it will make a noisy warning (and have no visible result).
        Quaternion targetRotation = moveDirWorld.magnitude > 0 ? Quaternion.LookRotation(moveDirWorld, Vector3.up) :
                                                                  Quaternion.identity;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * acceleration_rotation);
    }
}
