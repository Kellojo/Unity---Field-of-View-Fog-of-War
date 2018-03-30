
/// <summary>
/// Interface that needs to be implemented by any object that gets affected by the Field of View of the player.
/// </summary>
public interface IHideable {

    void OnFOVEnter();
    void OnFOVLeave();
}
