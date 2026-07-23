using System;
using System.Collections.Generic;
using UnityEngine;
using AYellowpaper.SerializedCollections;
using System.Buffers;
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
    
    public bool IsUnderRootRotation
    {
        get;
        set;
    }


    public List<RaycastInfo> raycastPoints; 
    public LayerMask targetVaultLayer;
    [SerializedDictionary("Count","Triggers")]
    [SerializeField] SerializedDictionary<int , VaultTriggerData> animationTriggerNames;
    readonly Dictionary<Type, IPlayerState> stateRegistry = new();
    IPlayerState currentState;
    InputManager _inputManager;
    bool isChangingState;

    public Vector3 CurrentVelocity, LastVelocity;
    Vector3 finalVelocity;
    Vector3 finalMoveVector;
    Camera cam;
    float turn;
    Matrix4x4 lastTransformMatrixData;
    RaycastHit[] buffer;
    VaultContext vContext;
    

    void Awake()
    {   
        _inputManager = GameManager.GetModule.Invoke(typeof(InputManager)) as InputManager;
        cam ??= Camera.main;
        IsUnderRootRotation = Animator.hasRootMotion;
        InitializeStateRegistry();
    }

    void OnEnable()
    {   
        buffer = ArrayPool<RaycastHit>.Shared.Rent(5);
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
    public void VaultCheckPass()
    {   
        Vector3 startPoint = this.transform.position;
        int c = 0;
        float minPointDistance = float.MaxValue;
        foreach(var i in raycastPoints)
        {
            Vector3 point = startPoint + Vector3.up * Mathf.Lerp(0 , cc.height , i.t);
            Array.Clear(buffer, 0, buffer.Length);
            int count = Physics.RaycastNonAlloc(point,artTransform.forward,buffer,i.distance,targetVaultLayer,QueryTriggerInteraction.Ignore);
            if(count > 0)
            {
                c++;
                for(int j = 0; j< count; j++)
                {
                    minPointDistance = Mathf.Min(minPointDistance , buffer[j].distance);
                }
            }
        }

        if(animationTriggerNames.ContainsKey(c))
        {   
            VaultTriggerData data = animationTriggerNames[c];
            TriggerInfo triggerInfo = data.GetRandomTrigger();

            // Yes I am in range of an obstacle where I can perform the animation
            if(minPointDistance >= triggerInfo.minTriggerAnimationDistance && minPointDistance <= triggerInfo.maxTriggerAnimationDistance)
            {   
                
                Debug.LogWarning("Min Distance : "+minPointDistance);
                vContext = new VaultContext(){trigger = triggerInfo};
                switch(currentState)
                {
                    case RunState:
                        vContext.lastStateType = typeof(RunState);
                        break;
                    case JogState:
                        vContext.lastStateType = typeof(JogState);
                        break;
                    default:
                        break;
                }
                data.UpdateRandomIndex();
                InitiateStateChange(typeof(VaultState));
            }
        }

    }

    public VaultContext GetVaultContext() => this.vContext;

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
        if(this.IsUnderRootRotation)
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
        if(buffer != null)
        {
            ArrayPool<RaycastHit>.Shared.Return(buffer);
            buffer = null;
        }
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

[Serializable]
public struct RaycastInfo
{
    [Range(0f , 1f)] public float t;
    [Min(0f)] public float distance;
}

[System.Serializable]
public struct VaultContext
{
    public TriggerInfo trigger;
    public Type lastStateType;
}

[System.Serializable]
public class VaultTriggerData
{   
    public TriggerInfo[] possibleTriggers;
    int randomIndex;

    public VaultTriggerData(TriggerInfo[] a = null, int b = 0)
    {   
        possibleTriggers = a;
        randomIndex = b;
        UpdateRandomIndex();
    }

    public TriggerInfo GetRandomTrigger()
    {   
        if(possibleTriggers == null || possibleTriggers.Length == 0)
        {
            Debug.LogError("Trigger Array is null");
            return default;
        }
        return possibleTriggers[randomIndex];
    }

    public void UpdateRandomIndex() => randomIndex = UnityEngine.Random.Range(0 , possibleTriggers.Length);
}



[System.Serializable]
public struct TriggerInfo
{
    public string triggerName;
    public float minTriggerAnimationDistance;
    public float maxTriggerAnimationDistance;
}