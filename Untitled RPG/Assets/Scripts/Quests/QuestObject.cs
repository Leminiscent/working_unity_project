using UnityEngine;

public class QuestObject : MonoBehaviour
{
    [SerializeField] private QuestBase _questToCheck;
    [SerializeField] private ObjectActions _onStart;
    [SerializeField] private ObjectActions _onComplete;

    private QuestList _questList;

    private void Start()
    {
        if (_questToCheck == null)
        {
            Debug.LogWarning("QuestObject: QuestToCheck is not assigned.", this);
            return;
        }

        _questList = QuestList.GetQuestList();
        if (_questList == null)
        {
            Debug.LogError("QuestObject: QuestList not found in the scene.", this);
            return;
        }

        _questList.OnUpdated += UpdateObjectStatus;
        UpdateObjectStatus();
    }

    private void OnDestroy()
    {
        if (_questList != null)
        {
            _questList.OnUpdated -= UpdateObjectStatus;
        }
    }

    public void UpdateObjectStatus()
    {
        ObjectActions actionToApply = ObjectActions.DoNothing;

        // Check quest status: completed takes precedence over started.
        if (_questList.IsCompleted(_questToCheck.Name) && _onComplete != ObjectActions.DoNothing)
        {
            actionToApply = _onComplete;
        }
        else if (_questList.IsStarted(_questToCheck.Name) && _onStart != ObjectActions.DoNothing)
        {
            actionToApply = _onStart;
        }

        // Apply the determined action to all child objects.
        if (actionToApply != ObjectActions.DoNothing)
        {
            foreach (Transform child in transform)
            {
                switch (actionToApply)
                {
                    case ObjectActions.Enable:
                        child.gameObject.SetActive(true);
                        if (child.TryGetComponent(out SavableEntity savable))
                        {
                            SavingSystem.Instance.RestoreEntity(savable);
                        }
                        break;
                    case ObjectActions.Disable:
                        child.gameObject.SetActive(false);
                        break;
                    case ObjectActions.DoNothing:
                        break;
                    default:
                        break;
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