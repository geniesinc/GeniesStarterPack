using UnityEngine;
using UnityEngine.InputSystem;

// Useful for UI updates
public enum GenieAnim : int
{
    Idle = 0,
    Walk = 1,
    Run = 2,
    Dancing = 3,
    Jump = 4,
    Peace = 5,
    Yaw = 6,
    Wave = 7
}


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

    private void Start()
    {
        geniePlayerActions = new GeniePlayerActions();

        moveAction = geniePlayerActions.Player.Move;
        moveAction.Enable();

        geniePlayerActions.Player.Look.Enable();

        geniePlayerActions.Player.Jump.performed += OnPlayerJump;
        geniePlayerActions.Player.Jump.Enable();

        // Sanity-check
        if (animator.applyRootMotion)
        {
            Debug.LogError($"{gameObject.name} has 'applyRootMotion' checkbox checked. Unchecking!");
            animator.applyRootMotion = false;
        }
    }

    private void OnDestroy()
    {
        geniePlayerActions.Player.Jump.performed -= OnPlayerJump;

        geniePlayerActions.Player.Move.Disable();
        geniePlayerActions.Player.Look.Disable();
        geniePlayerActions.Player.Jump.Disable();
    }

    private void FixedUpdate()
    {
        OnPlayerMove(moveAction.ReadValue<Vector2>());
    }

    private void OnPlayerJump(InputAction.CallbackContext obj)
    {
        Debug.Log("Jump!");
        SetGenieAnim((int)GenieAnim.Jump, isPosing: true);
    }

    private void OnPlayerMove(Vector2 moveDir)
    {
        if (moveDir.magnitude > 0)
        {
            Vector3 inputVector = new Vector3(moveDir.x,
                                              0f,
                                              moveDir.y).normalized;

            Vector3 rotatedVector = Quaternion.Euler(0,
                                                     Camera.main.transform.eulerAngles.y,
                                                     0) * inputVector;

            // Make it walk or run depending on how pushed is the joystick
            float moveSpeed = 0.5f;
            if (Input.GetKey(KeyCode.LeftShift))
            {
                moveSpeed = 1;
            }

            SetGenieAnim((int)GenieAnim.Walk);

            //Blending parameter between walk and run
            animator.SetFloat("MoveSpeed", moveSpeed);

            transform.position += rotatedVector * moveSpeed * 4 * transform.localScale.x * Time.deltaTime;

            // Target rotation should check if look rotation viewing vector is smaller/larger than zero,
            // or it will make a noisy warning (and have no visible result).
            Quaternion targetRotation = rotatedVector.magnitude > 0 ? Quaternion.LookRotation(rotatedVector, Vector3.up) :
                                                                      Quaternion.identity;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 10f);
        }
        else
        {
            //Stop Animation
            SetGenieAnim((int)GenieAnim.Idle);
        }
    }

    public void SetGenieAnim(int whichAnim, bool isPosing = false)
    {
        animator.SetInteger("GenieAnim", whichAnim);
        animator.SetBool("Posing", isPosing);
        if (isPosing && whichAnim != (int)GenieAnim.Jump)
        {
            animator.SetTrigger("SwitchToJoystick");
        }
    }
}
