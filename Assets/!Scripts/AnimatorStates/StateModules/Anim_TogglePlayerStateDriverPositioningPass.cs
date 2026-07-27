public class Anim_TogglePlayerStateDriverPositioningPass : AnimModuleBase
{
    PlayerStateDriver driver;
    bool toggleValue;
    public Anim_TogglePlayerStateDriverPositioningPass(PlayerStateDriver driver, bool toggleValue)
    {
        this.driver = driver;
        this.toggleValue = toggleValue;
    }

    public void Refresh()
    {
        //noop
    }

    public void Process()
    {
        driver.IsUnderSimulatedPositioning = toggleValue;
    }
}
