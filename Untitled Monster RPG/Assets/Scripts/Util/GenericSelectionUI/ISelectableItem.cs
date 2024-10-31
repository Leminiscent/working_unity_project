public interface ISelectableItem
{
    void Init();
    void Clear();

    void OnSelectionChanged(bool selected);
}
