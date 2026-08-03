using UnityEngine;

public class Anim_MatchRootLocation : AnimModuleBase
{   
    Animator anim;
    PlayerStateDriver driver;
    MatchRootPoseConfig config;
    Vector3 pose;
    public Anim_MatchRootLocation(Animator anim, PlayerStateDriver driver, MatchRootPoseConfig config)
    {
        this.anim = anim;
        this.driver = driver;
        this.config = config;
    }
    
    public void Refresh()
    {
        pose = driver.transform.position;
    }

    public void Process()
    {   
        if(config.MatchX)
            pose.x = anim.transform.position.x;
        if(config.MatchY)
            pose.y = anim.transform.position.y;
        if(config.MatchZ)
            pose.z = anim.transform.position.z;

        driver.transform.position = pose;
    }
}

[System.Serializable]
public struct MatchRootPoseConfig
{
    public bool MatchX;
    public bool MatchY;
    public bool MatchZ;
}