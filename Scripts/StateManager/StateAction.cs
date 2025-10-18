
//TODO: rename Type to "HandlerName", rename Value to "Payload", add an "ignores" argument.
public record struct StateAction(string Type, object Value);