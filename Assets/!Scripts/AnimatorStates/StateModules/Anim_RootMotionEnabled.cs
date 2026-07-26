using UnityEngine;
public class Anim_RootMotionEnabled : AnimModuleBase
{   
    Animator anim;
    PlayerStateDriver driver;

    public Anim_RootMotionEnabled(Animator anim , PlayerStateDriver driver)
    {
        this.anim = anim;
        this.driver = driver;
    }
    
    public void Refresh()
    {
        
    }

    public void Process()
    {
        anim.gameObject.transform.SetParent(null);
        driver.IsUnderRootRotation = true;
        anim.applyRootMotion = true;
    }
}
