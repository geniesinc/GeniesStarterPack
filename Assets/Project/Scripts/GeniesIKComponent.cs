using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class GeniesIKComponent : MonoBehaviour
{
    //public InputManager inputManager;

    float touchedColliderDistance;
    private float colliderRadius = 0.12f;

    //Rig Components
    private GameObject ikRig;

    private GameObject ikrightArm;
    private GameObject rightArmTarget;
    private GameObject rightHandJoint;
    private GameObject rightForearmJoint;
    private TwoBoneIKConstraint rightArmConstraint;

    private GameObject ikleftArm;
    private GameObject leftArmTarget;
    private GameObject leftHandJoint;
    private GameObject leftForearmJoint;
    private TwoBoneIKConstraint leftArmConstraint;

    private GameObject ikrightLeg;
    private GameObject rightLegTarget;
    private GameObject rightLegJoint;
    private TwoBoneIKConstraint rightLegConstraint;

    private GameObject ikleftLeg;
    private GameObject leftLegTarget;
    private GameObject leftLegJoint;
    private TwoBoneIKConstraint leftLegConstraint;


    // CREATE COLLIDERS
    private GameObject rightArmCollider;
    private GameObject leftArmCollider;
    private GameObject rightFootCollider;
    private GameObject leftFootCollider;

    private bool isRightArmTouched = false;
    private bool isLeftArmTouched = false;
    private bool isRightFootTouched = false;
    private bool isLeftFootTouched = false;

    //For other functions to know this is running
    public bool isIKActive = false;


    // Start is called before the first frame update
    void Start()
    {
        CreateGenieIKRig();

        /*inputManager.OnTouchDown += ProcessIKTouch;
        inputManager.OnTouchUp += ClearIKTouchedState;*/

        rightArmConstraint.weight = 0;
        leftArmConstraint.weight = 0;
        rightLegConstraint.weight = 0;
        leftLegConstraint.weight = 0;

    }

    // Update is called once per frame
    void Update()
    {
        PlaceColliders();
    }

    void CreateGenieIKRig()
    {
        //RigBuilder
        gameObject.AddComponent<RigBuilder>();
        //gameObject.AddComponent<Animator>();

#if UNITY_EDITOR
        // Adding boneRenderer for easier visualization
        if (true)
        {
            gameObject.AddComponent<BoneRenderer>();
            var bonesList = new List<Transform>();
            foreach (Transform child in FindDeepChild(this.gameObject, "Root").GetComponentsInChildren<Transform>())
            {
                bonesList.Add(child);
            }
            Transform[] bonesArray = bonesList.ToArray();
            gameObject.GetComponent<BoneRenderer>().transforms = bonesArray;
        }
#endif

        // IKs RIG
        ikRig = new GameObject("IKRigs");
        ikRig.transform.parent = this.gameObject.transform;
        ikRig.AddComponent<Rig>();

        rightHandJoint = FindDeepChild(this.gameObject, "RightHand").gameObject;
        leftHandJoint = FindDeepChild(this.gameObject, "LeftHand").gameObject;
        rightLegJoint = FindDeepChild(this.gameObject, "RightFoot").gameObject;
        leftLegJoint = FindDeepChild(this.gameObject, "LeftFoot").gameObject;

        gameObject.GetComponent<RigBuilder>().layers.Add(new RigLayer(ikRig.GetComponent<Rig>(), true));

        // HeadToBodyRotationOffset
        //headToBodyRotationOffset = new GameObject("HeadToBodyRotationOffset");
        //headToBodyRotationOffset.transform.parent = this.gameObject.transform;
        //headToBodyRotationOffset.transform.position = this.transform.FindChildRecursive("Head").position;
        //headToBodyRotationOffset.transform.rotation = this.transform.FindChildRecursive("Head").rotation;


        // Head
        //GameObject ikhead = new GameObject("IKHead");
        //ikhead.transform.parent = ikRig.gameObject.transform;
        //ikhead.transform.position = FindDeepChild(this.gameObject, "Head").position;
        //ikhead.transform.rotation = FindDeepChild(this.gameObject, "Head").rotation;

        //ikhead.AddComponent<MultiParentConstraint>();
        //ikhead.GetComponent<MultiParentConstraint>().data.constrainedObject = FindDeepChild(this.gameObject, "Head");
        //ikhead.GetComponent<MultiParentConstraint>().data.sourceObjects = new WeightedTransformArray { new WeightedTransform(ikhead.transform, 1) };
        //ikhead.GetComponent<MultiParentConstraint>().data.maintainPositionOffset = true;
        //ikhead.GetComponent<MultiParentConstraint>().data.maintainRotationOffset = true;
        //ikhead.GetComponent<MultiParentConstraint>().data.constrainedPositionXAxis = true;
        //ikhead.GetComponent<MultiParentConstraint>().data.constrainedPositionYAxis = true;
        //ikhead.GetComponent<MultiParentConstraint>().data.constrainedPositionZAxis = true;
        //ikhead.GetComponent<MultiParentConstraint>().data.constrainedRotationXAxis = true;
        //ikhead.GetComponent<MultiParentConstraint>().data.constrainedRotationYAxis = true;
        //ikhead.GetComponent<MultiParentConstraint>().data.constrainedRotationZAxis = true;


        //Right Arm
        ikrightArm = new GameObject("IKRightArm");
        ikrightArm.transform.parent = ikRig.gameObject.transform;
        rightArmConstraint = ikrightArm.AddComponent<TwoBoneIKConstraint>();

        rightArmTarget = new GameObject("Target");
        rightArmTarget.transform.parent = ikrightArm.gameObject.transform;
        rightArmTarget.transform.position = rightHandJoint.transform.position;
        rightArmTarget.transform.rotation = rightHandJoint.transform.rotation;

        GameObject rightArmHint = new GameObject("Hint");
        rightArmHint.transform.parent = ikrightArm.gameObject.transform;
        rightForearmJoint = FindDeepChild(this.gameObject, "RightForeArm").gameObject;
        rightArmHint.transform.position = rightForearmJoint.transform.position;
        rightArmHint.transform.rotation = rightForearmJoint.transform.rotation;
        rightArmHint.transform.Translate(new Vector3(0, -0.2f, 0), Space.Self);

        rightArmConstraint.data.root = FindDeepChild(this.gameObject, "RightArm");
        rightArmConstraint.data.mid = rightForearmJoint.transform;
        rightArmConstraint.data.tip = rightHandJoint.transform;
        rightArmConstraint.data.target = rightArmTarget.transform;
        rightArmConstraint.data.hint = rightArmHint.transform;
        rightArmConstraint.data.targetPositionWeight = 1;
        rightArmConstraint.data.targetRotationWeight = 1;
        rightArmConstraint.data.hintWeight = 1;


        // Left Arm
        ikleftArm = new GameObject("IKLeftArm");
        ikleftArm.transform.parent = ikRig.gameObject.transform;
        leftArmConstraint = ikleftArm.AddComponent<TwoBoneIKConstraint>();

        leftArmTarget = new GameObject("Target");
        leftArmTarget.transform.parent = ikleftArm.gameObject.transform;
        leftArmTarget.transform.position = leftHandJoint.transform.position;
        leftArmTarget.transform.rotation = leftHandJoint.transform.rotation;

        GameObject leftArmHint = new GameObject("Hint");
        leftArmHint.transform.parent = ikleftArm.gameObject.transform;
        leftForearmJoint = FindDeepChild(this.gameObject, "LeftForeArm").gameObject;
        leftArmHint.transform.position = leftForearmJoint.transform.position;
        leftArmHint.transform.rotation = leftForearmJoint.transform.rotation;
        leftArmHint.transform.Translate(new Vector3(0, 0.2f, 0), Space.Self);

        leftArmConstraint.data.root = FindDeepChild(this.gameObject, "LeftArm");
        leftArmConstraint.data.mid = FindDeepChild(this.gameObject, "LeftForeArm");
        leftArmConstraint.data.tip = leftHandJoint.transform;
        leftArmConstraint.data.target = leftArmTarget.transform;
        leftArmConstraint.data.hint = leftArmHint.transform;
        leftArmConstraint.data.targetPositionWeight = 1;
        leftArmConstraint.data.targetRotationWeight = 1;
        leftArmConstraint.data.hintWeight = 1;


        // Left Leg
        ikleftLeg = new GameObject("IKLeftLeg");
        ikleftLeg.transform.parent = ikRig.gameObject.transform;
        leftLegConstraint = ikleftLeg.AddComponent<TwoBoneIKConstraint>();

        leftLegTarget = new GameObject("Target");
        leftLegTarget.transform.parent = ikleftLeg.gameObject.transform;
        leftLegTarget.transform.position = FindDeepChild(this.gameObject, "LeftFoot").position;
        leftLegTarget.transform.rotation = FindDeepChild(this.gameObject, "LeftFoot").rotation;

        GameObject leftLegHint = new GameObject("Hint");
        leftLegHint.transform.parent = ikleftLeg.gameObject.transform;
        leftLegHint.transform.position = FindDeepChild(this.gameObject, "LeftLeg").position;
        leftLegHint.transform.rotation = FindDeepChild(this.gameObject, "LeftLeg").rotation;
        leftLegHint.transform.Translate(new Vector3(0.4f, 0.4f, 0), Space.Self);

        leftLegConstraint.data.root = FindDeepChild(this.gameObject, "LeftUpLeg");
        leftLegConstraint.data.mid = FindDeepChild(this.gameObject, "LeftLeg");
        leftLegConstraint.data.tip = FindDeepChild(this.gameObject, "LeftFoot");
        leftLegConstraint.data.target = leftLegTarget.transform;
        leftLegConstraint.data.hint = leftLegHint.transform;
        leftLegConstraint.data.targetPositionWeight = 1;
        leftLegConstraint.data.targetRotationWeight = 1;
        leftLegConstraint.data.hintWeight = 1;

        // Right Leg
        ikrightLeg = new GameObject("IKRightLeg");
        ikrightLeg.transform.parent = ikRig.gameObject.transform;
        rightLegConstraint = ikrightLeg.AddComponent<TwoBoneIKConstraint>();

        rightLegTarget = new GameObject("Target");
        rightLegTarget.transform.parent = ikrightLeg.gameObject.transform;
        rightLegTarget.transform.position = FindDeepChild(this.gameObject, "RightFoot").position;
        rightLegTarget.transform.rotation = FindDeepChild(this.gameObject, "RightFoot").rotation;

        GameObject rightLegHint = new GameObject("Hint");
        rightLegHint.transform.parent = ikrightLeg.gameObject.transform;
        rightLegHint.transform.position = FindDeepChild(this.gameObject, "RightLeg").position;
        rightLegHint.transform.rotation = FindDeepChild(this.gameObject, "RightLeg").rotation;
        rightLegHint.transform.Translate(new Vector3(-0.4f, -0.4f, 0), Space.Self);

        rightLegConstraint.data.root = FindDeepChild(this.gameObject, "RightUpLeg");
        rightLegConstraint.data.mid = FindDeepChild(this.gameObject, "RightLeg");
        rightLegConstraint.data.tip = FindDeepChild(this.gameObject, "RightFoot");
        rightLegConstraint.data.target = rightLegTarget.transform;
        rightLegConstraint.data.hint = rightLegHint.transform;
        rightLegConstraint.data.targetPositionWeight = 1;
        rightLegConstraint.data.targetRotationWeight = 1;
        rightLegConstraint.data.hintWeight = 1;

        var LeftFootIK = leftLegTarget.transform;
        var RightFootIK = rightLegTarget.transform;

        // BUILD RIG
        gameObject.GetComponent<RigBuilder>().Build();

        // CREATE COLLIDERS
        rightArmCollider = new GameObject("rightArmCollider");
        SphereCollider rightArmSpherecollider = rightArmCollider.AddComponent<SphereCollider>();
        rightArmSpherecollider.radius = colliderRadius;
        rightArmCollider.transform.parent = ikrightArm.transform;
        rightArmCollider.transform.position = rightHandJoint.transform.position;
        rightArmCollider.transform.rotation = rightHandJoint.transform.rotation;
        //rightArmCollider.layer = (int)Layers.IKColliders;

        leftArmCollider = new GameObject("leftArmCollider");
        SphereCollider leftArmSpherecollider = leftArmCollider.AddComponent<SphereCollider>();
        leftArmSpherecollider.radius = colliderRadius;
        leftArmCollider.transform.parent = ikleftArm.transform;
        leftArmCollider.transform.position = leftHandJoint.transform.position;
        leftArmCollider.transform.rotation = leftHandJoint.transform.rotation;
        //leftArmCollider.layer = (int)Layers.IKColliders;

        rightFootCollider = new GameObject("rightFootCollider");
        SphereCollider rightFootSpherecollider = rightFootCollider.AddComponent<SphereCollider>();
        rightFootSpherecollider.radius = colliderRadius;
        rightFootCollider.transform.parent = ikrightLeg.transform;
        rightFootCollider.transform.position = rightLegJoint.transform.position;
        rightFootCollider.transform.rotation = rightLegJoint.transform.rotation;
        //rightFootCollider.layer = (int)Layers.IKColliders;

        leftFootCollider = new GameObject("leftFootCollider");
        SphereCollider leftFootSpherecollider = leftFootCollider.AddComponent<SphereCollider>();
        leftFootSpherecollider.radius = colliderRadius;
        leftFootCollider.transform.parent = ikleftLeg.transform;
        leftFootCollider.transform.position = leftLegJoint.transform.position;
        leftFootCollider.transform.rotation = leftLegJoint.transform.rotation;
        //leftFootCollider.layer = (int)Layers.IKColliders;

    }

    private Transform FindDeepChild(GameObject parent, string childName)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>();
        foreach (Transform child in children)
        {
            if (child.name == childName)
            {
                return child;
            }
        }
        return null;
    }

    private void PlaceColliders()
    {
        if (isRightArmTouched)
        {
            rightArmConstraint.weight = 1;
            MoveCollider(rightArmCollider, rightForearmJoint.transform.rotation.eulerAngles);
            rightArmTarget.transform.SetPositionAndRotation(rightArmCollider.transform.position, rightArmCollider.transform.rotation);
        }
        else
        {
            rightArmCollider.transform.position = rightHandJoint.transform.position;
            rightArmCollider.transform.rotation = rightHandJoint.transform.rotation;
        }

        if (isLeftArmTouched)
        {
            leftArmConstraint.weight = 1;
            MoveCollider(leftArmCollider, leftForearmJoint.transform.rotation.eulerAngles);
            leftArmTarget.transform.SetPositionAndRotation(leftArmCollider.transform.position, leftArmCollider.transform.rotation);
        }
        else
        {
            leftArmCollider.transform.position = leftHandJoint.transform.position;
            leftArmCollider.transform.rotation = leftHandJoint.transform.rotation;
        }

        if (isRightFootTouched)
        {
            rightLegConstraint.weight = 1;
            MoveCollider(rightFootCollider);
            rightLegTarget.transform.SetPositionAndRotation(rightFootCollider.transform.position, rightFootCollider.transform.rotation);
        }
        else
        {
            rightFootCollider.transform.position = rightLegJoint.transform.position;
            rightFootCollider.transform.rotation = rightLegJoint.transform.rotation;
        }
        if (isLeftFootTouched)
        {
            leftLegConstraint.weight = 1;
            MoveCollider(leftFootCollider);
            leftLegTarget.transform.SetPositionAndRotation(leftFootCollider.transform.position, leftFootCollider.transform.rotation);
        }
        else
        {
            leftFootCollider.transform.position = leftLegJoint.transform.position;
            leftFootCollider.transform.rotation = leftLegJoint.transform.rotation;
        }

    }

    private void ProcessIKTouch(Vector2 touchPos)
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(touchPos), out hit, 100f))
        {
            if (hit.collider.gameObject == rightArmCollider)
            {
                isRightArmTouched = true;
                isIKActive = true;
            }
            else if (hit.collider.gameObject == leftArmCollider)
            {
                isLeftArmTouched = true;
                isIKActive = true;
            }
            else if (hit.collider.gameObject == rightFootCollider)
            {
                isRightFootTouched = true;
                isIKActive = true;
            }
            else if (hit.collider.gameObject == leftFootCollider)
            {
                isLeftFootTouched = true;
                isIKActive = true;
            }
            touchedColliderDistance = Vector3.Distance(Camera.main.transform.position, hit.transform.position);
        }
    }
    private void ClearIKTouchedState(Vector2 lastTouchPos)
    {
        if (isRightArmTouched)
        {
            StartCoroutine(FadeOutConstraintWeight(rightArmConstraint));
            isRightArmTouched = false;
        }
        if (isLeftArmTouched)
        {
            StartCoroutine(FadeOutConstraintWeight(leftArmConstraint));
            isLeftArmTouched = false;
        }
        if (isRightFootTouched)
        {
            StartCoroutine(FadeOutConstraintWeight(rightLegConstraint));
            isRightFootTouched = false;
        }
        if (isLeftFootTouched)
        {
            StartCoroutine(FadeOutConstraintWeight(leftLegConstraint));
            isLeftFootTouched = false;
        }
        if (!isRightArmTouched && !isLeftArmTouched && !isRightFootTouched && !isLeftFootTouched)
        {
            isIKActive = false;
        }
    }

    private void MoveCollider(GameObject movingCollider, Vector3 colliderRotation = default)
    {
        if (colliderRotation == default)
        {
            colliderRotation = movingCollider.transform.rotation.eulerAngles;
        }
#if UNITY_EDITOR
        if (Input.GetMouseButton(0))
        {
            Vector3 touchWorldPosition = Camera.main.ScreenToWorldPoint(
                                new Vector3(Input.mousePosition.x, Input.mousePosition.y, touchedColliderDistance));
            movingCollider.transform.position = touchWorldPosition;
            movingCollider.transform.rotation = Quaternion.Euler(colliderRotation);

        }

        #elif UNITY_IOS
        if (Input.touchCount > 0)
        {
            for (int i = 0; i < Input.touchCount; i++)
            {
                Touch touch = Input.GetTouch(0);
                Vector3 touchWorldPosition = Camera.main.ScreenToWorldPoint(
                    new Vector3(touch.position.x, touch.position.y, touchedColliderDistance));
                movingCollider.transform.position = touchWorldPosition;
                movingCollider.transform.rotation = Quaternion.Euler(colliderRotation);
            }
        }
#endif
    }

    private IEnumerator FadeOutConstraintWeight(TwoBoneIKConstraint twoBoneIKConstraint)
    {
        float elapsedTime = 0f;
        float duration = 1;
        float startValue = 1;
        float endValue = 0;

        yield return new WaitForSeconds(1);
        while (elapsedTime < duration)
        {
            // Calculate the interpolation factor (0 to 1)
            float t = elapsedTime / duration;

            // Perform linear interpolation (lerp) between startValue and endValue
            float lerpedValue = Mathf.Lerp(startValue, endValue, t);

            // Optionally, do something with the lerped value
            twoBoneIKConstraint.weight = lerpedValue;

            // Increment the elapsed time
            elapsedTime += Time.deltaTime;

            // Wait for the next frame
            yield return null;
        }
        twoBoneIKConstraint.weight = 0;
    }
}
