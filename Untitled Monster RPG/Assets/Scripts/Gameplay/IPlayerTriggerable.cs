public interface IPlayerTriggerable
{
    void OnPlayerTriggered(PlayerController player);

    bool TriggerRepeatedly { get; }
}
