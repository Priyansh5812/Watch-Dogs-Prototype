using System;
using System.Collections.Generic;
using UnityEngine;

public partial class PlayerStateDriver : MonoBehaviour
{   
    [field:SerializeField] public CharacterController cc
    {
        get; private set;
    }
    
    [field:SerializeField] public Transform artTransform
    {
        get; private set;
    }
    
    [field:SerializeField] public PlayerStatData Data
    {
        get; private set;
    }

    [field: SerializeField] public Animator Animator
    {
        get; private set;
    }

    public DecisionAsset<ResultAsset , VaultRequestParams> AnimationDatabase;

    [field : SerializeField] public RootMotionRuntime _RootMotionRuntime
    {
        get;
        private set;
    }

    public bool IsUnderRootRotation
    {
        get;
        set;
    }

    public bool IsUnderSimulatedPositioning
    {
        get;
        set;
    }

    [field: SerializeField] public List<RaycastInfo> raycastPoints
    {
        get; private set;
    }
    [field: SerializeField] public LayerMask targetVaultLayer
    {
        get; private set;
    }
    readonly Dictionary<Type, IPlayerState> stateRegistry = new();
    IPlayerState currentState;
    InputManager _inputManager;
    VaultModule vaultModule;
    bool isChangingState;
    [HideInInspector] public Vector3 CurrentVelocity, LastVelocity;
    Vector3 finalVelocity;
    Vector3 finalMoveVector;
    Camera cam;
    float turn;
    Matrix4x4 lastTransformMatrixData;


    void Awake()
    {   
        _inputManager = GameManager.GetModule.Invoke(typeof(InputManager)) as InputManager;
        cam ??= Camera.main;
        IsUnderRootRotation = Animator.hasRootMotion;
        InitializeStateRegistry();
    }

    void OnEnable()
    {   
        vaultModule ??= new(this , AnimationDatabase);
    }
    void Start()
    {   
        SetInitialState<IdleState>();
    }


    
     void Update()
    {   
        if (isChangingState || currentState == null)
        {
            return;
        }

        currentState.OnUpdate();
        currentState.AnimationUpdate();
        currentState.OnCheckTransition();
        ApplyRotation();
        MoveCharacter();
    }



    void FixedUpdate()
    {
        if (isChangingState || currentState == null)
        {
            return;
        }

        currentState.FixedUpdate();
        CalculateFinalMoveVector();

        LastVelocity = CurrentVelocity;
    }


    // Treating this as a shared logic , will be executed by the states
    public void VaultCheckPass() => vaultModule?.VaultCheckPass();

    public VaultContext GetVaultContext() => vaultModule == null ? default : vaultModule.GetVaultContext();

    void CalculateFinalMoveVector()
    {   
        if(_inputManager.GetInput().sqrMagnitude > 0.01 && currentState is not VaultState)
        {
            finalVelocity = cam.transform.TransformVector(CurrentVelocity);
            lastTransformMatrixData = cam.transform.localToWorldMatrix;
        }
        else
        {
            finalVelocity = lastTransformMatrixData.MultiplyVector(CurrentVelocity);
        }
        finalMoveVector = Vector3.ProjectOnPlane(finalVelocity , Vector3.up);
        Debug.DrawRay(this.transform.position , finalMoveVector, Color.red);
    }

    void ApplyRotation()
    {   
        if(finalMoveVector.sqrMagnitude <= 0.001f)
        {
            turn = Mathf.MoveTowards(turn , 0f , Data.bodyTurningSpeed* Data.animationTurnLerpSpeed * Time.deltaTime);
            return;
        }

        if(this.IsUnderRootRotation)
        {
            turn = Mathf.MoveTowards(turn , 0f , Data.bodyTurningSpeed* Data.animationTurnLerpSpeed * Time.deltaTime);

            return;
        }
        // if(Vector3.Dot(intendedMoveDir , artTransform.forward) <= -0.85f)
        // {
        //     Animator.SetTrigger("TurnWalk");
        //     return;
        // }
        
        float dot = Vector3.Dot(finalMoveVector.normalized , artTransform.right);


        float acc;

        switch(currentState)
        {
            case RunState:
                acc = Data.AccelarationToRun;
                break;
            
            case JogState:
                acc = Data.AccelarationToJog;
                break;

            default:
                acc = Data.AccelarationToWalk;
                break;
        }

        if(dot >= 0.15)
        {
            turn = Mathf.MoveTowards(turn , 1f , acc * Data.animationTurnLerpSpeed * Time.deltaTime);
        }
        else if(dot <= -0.15)
        {
            turn = Mathf.MoveTowards(turn , -1f , acc * Data.animationTurnLerpSpeed * Time.deltaTime);
            
        }
        else
        {
            turn = Mathf.MoveTowards(turn , 0f , acc * Data.animationTurnLerpSpeed * Time.deltaTime);
        }

        
        Animator.SetFloat("Turn" , turn);

        Quaternion targetRotation = Quaternion.LookRotation(finalMoveVector , transform.up);
        artTransform.rotation = Quaternion.Slerp(artTransform.rotation,targetRotation,acc * Time.deltaTime);
    }
        
    void MoveCharacter()
    {   
        if(this.IsUnderRootRotation || this.IsUnderSimulatedPositioning)
            return;

        cc.Move(finalMoveVector * Time.deltaTime);
    }

    void InitializeStateRegistry()
    {
        stateRegistry.Clear();
        RegisterState<IdleState>(new IdleState(this , _inputManager));
        RegisterState<WalkState>(new WalkState(this , _inputManager));
        RegisterState<JogState>(new JogState(this , _inputManager));
        RegisterState<RunState>(new RunState(this , _inputManager));
        RegisterState<VaultState>(new VaultState(this));
    }


    #region State Management Helpers
    void RegisterState<T>(T state) where T : class, IPlayerState
    {
        if (state == null)
        {
            throw new ArgumentNullException(nameof(state));
        }

        stateRegistry[typeof(T)] = state;
    }

    void SetInitialState<T>() where T : class, IPlayerState
    {
        InitiateStateChange(typeof(T), true);
    }


    public IPlayerState GetState<T>() where T : class, IPlayerState
    {
        stateRegistry.TryGetValue(typeof(T), out IPlayerState state);
        return state;
    }

    public IPlayerState GetCurrentState()
    {
        return currentState;
    }


    IPlayerState nextState;

    public void InitiateStateChange(Type stateType, bool forceEnter = false)
    {
        if (!stateRegistry.TryGetValue(stateType, out nextState))
        {
            Debug.LogWarning($"State '{stateType.Name}' is not registered on {nameof(PlayerStateDriver)}.");
            return;
        }

        if (!forceEnter && ReferenceEquals(currentState, nextState))
        {
            return;
        }

        isChangingState = true;

        if (currentState == null)
        {
            CompleteStateEnter();
            return;
        }

        currentState.OnExit(CompleteStateEnter);
    }

    void CompleteStateEnter()
    {
        currentState = nextState;
        currentState.OnEnter(SetChangingStateCompleted);
    }

    void SetChangingStateCompleted()
    {
        isChangingState = false;
    }
    #endregion

    void OnDisable()
    {   
        vaultModule?.Dispose();
    }

#if UNITY_EDITOR

    void OnDrawGizmos()
    {
        DrawRaycastGizmo();
    }

    void DrawRaycastGizmo()
    {   
        Vector3 startPoint = this.transform.position;

        foreach(var i in raycastPoints)
        {   
            Gizmos.color = Color.yellow;
            Vector3 point = startPoint + Vector3.up * Mathf.Lerp(0 , cc.height , i.t);
            Gizmos.DrawSphere(point, 0.05f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawRay(point, this.artTransform.forward * i.distance);
        }
    }

    #endif
}




