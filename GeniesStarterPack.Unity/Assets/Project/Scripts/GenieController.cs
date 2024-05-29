using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using TMPro;
using System.Collections;
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
    public delegate void AnimChangeEvent(GenieAnim ga);
    public event AnimChangeEvent OnAnimChange;

    public Texture thumbnailTexture;

    [SerializeField] Animator animator;
    [SerializeField] public BoxCollider boxCollider;

    // Note: We assume all renderers have the same list of blendshapes on them,
    //       in the same order.
    [SerializeField] SkinnedMeshRenderer[] skinnedMeshRenderers;

    private float pixelsToDegreesY = -0.5f;

    private bool isYawing = false;
    private float yawAnimTotalAngle = 90f;
    private float yawAnimDuration = 1.567f;

    //Movement Joystick 
    private float moveSpeed = 2f;

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

    private void OnPlayerJump(InputAction.CallbackContext obj)
    {
        Debug.Log("Jump!");
        SetGenieAnim((int)GenieAnim.Jump);
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
        if (!isYawing)
        {
            JoystickControl(moveAction.ReadValue<Vector2>());
        }
    }

    private Transform FindChildRecursively(Transform parent, string childName)
    {
        if (parent.name == childName)
        {
            return parent;
        }

        for (int i = 0; i < parent.childCount; i++)
        {
            Transform child = parent.GetChild(i);
            Transform result = FindChildRecursively(child, childName);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    private void ChangeLayersRecursively(Transform parent, int newLayer)
    {
        // Change the layer of the current object
        parent.gameObject.layer = newLayer;

        // Recursively change the layer of all child objects
        for (int i = 0; i < parent.childCount; i++)
        {
            ChangeLayersRecursively(parent.GetChild(i), newLayer);
        }
    }

    private void TranslateGenie(Vector2 deltaPixels, Vector2 currScreenPoint)
    {
        Camera cam = Camera.main;

        Vector3 planeNormal = Vector3.up;
        Vector3 pointOnPlane = new Vector3(0, transform.position.y, 0);
        Plane raycastCatcherPlane = new Plane(planeNormal, pointOnPlane);

        Vector2 currScreenPointOffset = cam.WorldToScreenPoint(transform.position);
        Ray ray = cam.ScreenPointToRay(currScreenPointOffset + deltaPixels);

        float distance;
        if (raycastCatcherPlane.Raycast(ray, out distance))
        {
            transform.position = ray.GetPoint(distance);
            /*Debug.Log("Plane Normal: " + planeNormal + ", Point: " + pointOnPlane);
            Debug.Log($"Move to point on ray at {distance}m distance: {transform.position}");
            Debug.DrawRay(ray.origin, ray.direction, Color.blue, 0.5f);*/
        }
    }

    private void YawGenie(Vector2 deltaPixels)
    {
        /*if (enableIk && geniesIKComponent.isIKActive)
        {
            return;
        }*/

        // Yaw the Genie based on touch input delta (horizontal swipe only)
        transform.rotation *= Quaternion.AngleAxis(deltaPixels.x * pixelsToDegreesY, Vector3.up);
        //Play animation while yawing if it has not started yet
        if (!isYawing)
        {
            animator.SetTrigger("Yaw");
            SetGenieAnim((int)GenieAnim.Yaw);
        }
        // Animation plays at different speeds depending of how many pixels swiped. 
        // Speed calculation: ((angle rotated/ total angle animation * animation lenght)/deltaTime)
        float speed = (((deltaPixels.x * pixelsToDegreesY) / yawAnimTotalAngle) * yawAnimDuration) / Time.deltaTime;
        animator.SetFloat("YawSpeed", speed);
        isYawing = true;
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

    private void SetGenieAnimSpeed(float animSpeed)
    {
        animator.speed = animSpeed;
    }

    private void JoystickControl(Vector2 moveDir)
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
            float joystickPushSpeed = Mathf.Abs(moveDir.y) + Mathf.Abs(moveDir.x);
            // Defining minimum speed, so it doesn't look like super slow motion
            if (joystickPushSpeed <= 0.3f)
            {
                joystickPushSpeed = 0.3f;
            }
            SetGenieAnim((int)GenieAnim.Walk);
            //Animation playback speed
            SetGenieAnimSpeed(joystickPushSpeed);
            //Blending parameter between walk and run
            animator.SetFloat("WalkRunSpeed", joystickPushSpeed);
            transform.position += rotatedVector * moveSpeed * joystickPushSpeed * transform.localScale.x * Time.deltaTime;

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
            SetGenieAnimSpeed(1);
        }
    }
}
