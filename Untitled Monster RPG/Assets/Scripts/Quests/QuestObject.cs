using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] private QuestBase _questToCheck;
    [SerializeField] private ObjectActions _onStart;
    [SerializeField] private ObjectActions _onComplete;

    private QuestList _questList;

    private void Start()
    {
        _questList = QuestList.GetQuestList();
        _questList.OnUpdated += UpdateObjectStatus;

        UpdateObjectStatus();
    }

    private void OnDestroy()
    {
        _questList.OnUpdated -= UpdateObjectStatus;
    }

    public void UpdateObjectStatus()
    {
        if (_onStart != ObjectActions.DoNothing && _questList.IsStarted(_questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (_onStart == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);

                    if (child.TryGetComponent(out SavableEntity savable))
                    {
                        SavingSystem.Instance.RestoreEntity(savable);
                    }
                }
                else if (_onStart == ObjectActions.Disable)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }

        if (_onComplete != ObjectActions.DoNothing && _questList.IsCompleted(_questToCheck.Name))
        {
            foreach (Transform child in transform)
            {
                if (_onComplete == ObjectActions.Enable)
                {
                    child.gameObject.SetActive(true);


                    if (child.TryGetComponent(out SavableEntity savable))
                    {
                        SavingSystem.Instance.RestoreEntity(savable);
                    }
                }
                else if (_onComplete == ObjectActions.Disable)
                {
                    child.gameObject.SetActive(false);
                }
            }
        }
    }
}

public enum ObjectActions
{
    DoNothing,
    Enable,
    Disable
}