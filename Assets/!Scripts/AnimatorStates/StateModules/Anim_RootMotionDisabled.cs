using UnityEngine;

public class Anim_RootMotionDisabled : AnimModuleBase
{
    Animator anim;
    PlayerStateDriver driver;

    public Anim_RootMotionDisabled(Animator anim , PlayerStateDriver driver)
    {
        this.anim = anim;
        this.driver = driver;
    }

    public void Refresh()
    {
        
    }

    public void Process()
    {   
        
        anim.transform.SetParent(driver.transform);
        driver.IsUnderRootRotation = false;
        anim.applyRootMotion = false;
        anim.transform.localScale = Vector3.one;
        anim.transform.localPosition = Vector3.zero;
    }
}
