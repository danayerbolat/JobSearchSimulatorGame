/// <summary>
/// Interface for windows that need to reset their state when opened.
/// Any component that implements this will have ResetState() called
/// automatically when its window opens.
/// </summary>
public interface IResettable
{
    void ResetState();
}