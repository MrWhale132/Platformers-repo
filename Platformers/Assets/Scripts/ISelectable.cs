
public interface ISelectable
{
    void HandleMouseMessage(MouseMessage msg);

    bool ValidSwitch(object newSelection);

    void Switch();
}
