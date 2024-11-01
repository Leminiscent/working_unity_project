public interface IPlayerTriggerable
{
    bool TriggerRepeatedly { get; }
    
    void OnPlayerTriggered(PlayerController player);
}
