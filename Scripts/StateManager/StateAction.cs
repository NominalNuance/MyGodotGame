
public class StateAction(string newType, object newValue)
{
    public string Type { get; private set; } = newType;
    public object Value { get; private set; } = newValue;
}
