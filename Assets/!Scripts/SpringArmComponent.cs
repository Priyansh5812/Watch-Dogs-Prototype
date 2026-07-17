using Unity.VisualScripting;
using UnityEngine;

// Camera Handling includes boom logic that follows the player and avoids clipping
public class SpringArmComponent : MonoBehaviour
{
    Camera mainCamera;

    [Header("Position")]
    [SerializeField] float m_raycastDistance = 7.0f;
    [SerializeField] Vector3 originOffset = Vector3.up;
    [SerializeField] Vector3 direction = Vector3.up - Vector3.forward;
    [SerializeField] LayerMask layersToIgnore;

    [Header("Input")]
    [SerializeField, Range(0.1f , 1.5f)] float XSens = 1f;
    [SerializeField, Range(0.1f , 1.5f)] float YSens = 1f;
    [SerializeField, Range(0.5f , 2.0f)] float globalSensMultiplier = 1f;
    [SerializeField, Range(-180f, 180f)] float minXAngle;
    [SerializeField, Range(-180f, 180f)] float maxXAngle;


    [SerializeField] bool lazyUpdatePosition = true;
    [SerializeField] bool lazyUpdateRotation = false;
    [SerializeField] float lazyPositionUpdationSpeed = 10.0f;
    [SerializeField] float lazyRotationUpdationSpeed = 10.0f;

    float xInput, yInput;
    float currentX, currentY;
    Vector3 origin;
    Vector3 rayDirection;
    Vector3 finalPosition;
    Quaternion finalRotation;
    RaycastHit[] hitsInfo = new RaycastHit[1];
    
    
    
    private void OnEnable()
    {
        Application.targetFrameRate = -1;
    }


    private void Start()
    {
        Initialize();    
    }

   

    void Initialize()
    {
        mainCamera ??= Camera.main;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {   
        AddInput();
        ComputeRayParams();
        ComputeRotation();
        ComputePosition();
        ApplyPose();
    }

    void AddInput()
    {   
        
        xInput = Input.GetAxis("Mouse X");
        yInput = Input.GetAxis("Mouse Y");
        Vector3 rotation = this.transform.localRotation.eulerAngles;
        currentX += -yInput * XSens * globalSensMultiplier;
        currentY += xInput * YSens * globalSensMultiplier;
        currentX = Mathf.Clamp(currentX, minXAngle, maxXAngle);
        transform.localRotation = Quaternion.Euler(currentX, currentY, 0f);
    }


    void ComputeRayParams()
    {
        // Calculation of the world space origin and direction for the camera collision check
        origin = this.transform.position;
        origin += (this.transform.forward * originOffset.z) + (this.transform.up * originOffset.y) + (this.transform.right * originOffset.x);
        rayDirection = this.transform.rotation * direction.normalized;
    }

    // Final Position Calculation
    void ComputePosition()
    {
        float camDistance = m_raycastDistance;
        Vector3 normDir = rayDirection;
        LayerMask mask = ~layersToIgnore;
        
        if (Physics.SphereCastNonAlloc(origin, 0.25f, normDir, hitsInfo, m_raycastDistance, mask) > 0)
        {
            RaycastHit hitInfo = hitsInfo[0];
            camDistance = hitInfo.distance;
        }
        finalPosition = origin + (normDir * camDistance);
    }

    // Final Rotation Calculation
    void ComputeRotation()
    {
        
        finalRotation = Quaternion.LookRotation(-rayDirection, this.transform.up);

    }

    bool isFirstPass = true;

    // Pose Updation based on the type of updation has been set in editor
    void ApplyPose()
    {   
        if(isFirstPass)
        {
            isFirstPass = false;
            return;
        }

        float rt = 1f - Mathf.Exp(-lazyRotationUpdationSpeed * Time.deltaTime);
        float pt = 1f - Mathf.Exp(-lazyPositionUpdationSpeed * Time.deltaTime);
        mainCamera.transform.rotation = lazyUpdateRotation ? Quaternion.Slerp(mainCamera.transform.rotation, finalRotation, rt) : finalRotation;
        mainCamera.transform.position = lazyUpdatePosition ? Vector3.Lerp(mainCamera.transform.position, finalPosition, pt) : finalPosition;
    }





}