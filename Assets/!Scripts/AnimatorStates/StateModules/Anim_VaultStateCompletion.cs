using UnityEngine;

public class Anim_VaultStateCompletion : AnimModuleBase
{
    PlayerStateDriver driver;

    public Anim_VaultStateCompletion(PlayerStateDriver driver)
    {
        this.driver = driver;
    }

    public void Refresh()
    {

    }

    public void Process()
    {
        VaultState state = driver.GetCurrentState() as VaultState;

        if (state == null)
        {
            Debug.LogError("State is not in Vault State while performing vault animation!!!");
            return;
        }
        state.SetStateCompletion();
    }

    public void Dispose()
    {
    }
}
