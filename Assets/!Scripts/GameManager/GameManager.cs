using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;
using System;
public class GameManager : MonoBehaviour
{   
    private static GameManager _instance;
    private PlayerStateDriver player;
    List<IGameModule> gameModules;
    List<ITickableGameModule> tickableGameModules;

    [Header("Modules Data")]

    bool AreReferencesInitialized = false;
    bool AreModulesInitialized = false;

    public static Func<Type , IGameModule> GetModule;

    // Awake is dedicated to GameManager only
    void Awake()
    {   
        if(_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this.gameObject);
            return;
        }

        InitializeReferences();
        InitializeModules();
    }

    
    void InitializeReferences()
    {   
        if(AreReferencesInitialized)
            return;

        gameModules = ListPool<IGameModule>.Get();
        tickableGameModules = ListPool<ITickableGameModule>.Get();
        if(player == null)
            player = FindFirstObjectByType<PlayerStateDriver>();
        AreReferencesInitialized = true;
    }

    void InitializeModules()
    {
        if(AreModulesInitialized)
            return;

        tickableGameModules.Add(new InputManager());

        AreModulesInitialized = true;
    }

    // For its modules (Initialize)
    void OnEnable()
    {
        InitializeReferences();

        GetModule += GetGameModule;

        for (int i = 0; i < gameModules.Count; i++)
        {
            gameModules[i]?.Initialize();
        }

        for (int i = 0; i < tickableGameModules.Count; i++)
        {
            tickableGameModules[i]?.Initialize();
        }
    }

    // For its modules to be ready
    void Start()
    {
        for (int i = 0; i < gameModules.Count; i++)
        {
            gameModules[i]?.OnModuleReady();
        }

        for (int i = 0; i < tickableGameModules.Count; i++)
        {
            tickableGameModules[i]?.OnModuleReady();
        }
    }

    void Update()
    {
        if(tickableGameModules == null)
            return;

        for (int i = 0; i < tickableGameModules.Count; i++)
        {
            tickableGameModules[i]?.Tick(Time.deltaTime);
        }
    }


    // DeInitialize Modules
    void OnDisable()
    {   
        GetModule -= GetGameModule;

        for (int i = 0; i < gameModules.Count; i++)
        {
            gameModules[i]?.DeInitialize();
        }

        for (int i = 0; i < tickableGameModules.Count; i++)
        {
            tickableGameModules[i]?.DeInitialize();
        }
    }
    
    IGameModule GetGameModule(Type T)
    {
        foreach(var i in gameModules)
        {
            if(T.IsInstanceOfType(i))
            {
                return i;
            }
        }

        foreach(var i in tickableGameModules)
        {
            if(T.IsInstanceOfType(i))
            {
                return i;
            }
        }

        return null;
    }


    void OnDestroy()
    {   
        if(_instance != this)
        {
            return;
        }

        if(gameModules != null)
        {
            ListPool<IGameModule>.Release(gameModules);
            gameModules = null;
        }

        if(tickableGameModules != null)
        {
            ListPool<ITickableGameModule>.Release(tickableGameModules);
            tickableGameModules = null;
        }

        if(_instance == this)
        {
            _instance = null;
        }

        AreReferencesInitialized = false;
        _instance = null;
    }




}

