using UnityEngine;

public class Anim_MatchRootLocation : AnimModuleBase
{   
    Animator anim;
    PlayerStateDriver driver;

    public Anim_MatchRootLocation(Animator anim , PlayerStateDriver driver)
    {
        this.anim = anim;
        this.driver = driver;
    }
    
    public void Refresh()
    {
        
    }

    public void Process()
    {
        driver.transform.position = anim.gameObject.transform.position;
    }
}
