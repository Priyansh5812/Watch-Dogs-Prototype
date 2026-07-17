using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStateDriver : MonoBehaviour
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
    
    public bool IsUnderRootRotation
    {
        get;
        set;
    }

    readonly Dictionary<Type, IPlayerState> stateRegistry = new();
    IPlayerState currentState;
    InputManager _inputManager;
    bool isChangingState;

    public Vector3 CurrentVelocity, LastVelocity;
    Vector3 finalMoveVector;
    Camera cam;
    void Awake()
    {   
        _inputManager = GameManager.GetModule.Invoke(typeof(InputManager)) as InputManager;
        cam ??= Camera.main;
        IsUnderRootRotation = Animator.hasRootMotion;
        InitializeStateRegistry();
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

    void CalculateFinalMoveVector()
    {
        Vector3 finalVelocity = cam.transform.TransformVector(CurrentVelocity); 
        finalMoveVector = Vector3.ProjectOnPlane(finalVelocity , Vector3.up);
    }

    void ApplyRotation()
    {   
        if(_inputManager.GetInput().sqrMagnitude <= 0.001f)
            return;

        if(this.IsUnderRootRotation)
            return;

        Vector3 inputVec = cam.transform.TransformVector(_inputManager.GetInput().normalized); 
        Vector3 intendedMoveDir = Vector3.ProjectOnPlane(inputVec , Vector3.up);

        if(Vector3.Dot(intendedMoveDir , artTransform.forward) <= -0.85f)
        {
            Animator.SetTrigger("TurnWalk");
            return;
        }

        Quaternion targetRotation = Quaternion.LookRotation(finalMoveVector , transform.up);
        artTransform.rotation = Quaternion.Slerp(artTransform.rotation,targetRotation,Data.bodyTurningSpeed * Time.deltaTime);
    }
        
    void MoveCharacter()
    {   
        if(this.IsUnderRootRotation)
            return;
        cc.Move(finalMoveVector * Time.deltaTime);
    }

    void InitializeStateRegistry()
    {
        stateRegistry.Clear();
        RegisterState<IdleState>(new IdleState(this , _inputManager));
        RegisterState<WalkState>(new WalkState(this , _inputManager));
        RegisterState<JogState>(new JogState(this));
        RegisterState<RunState>(new RunState(this));
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
}
