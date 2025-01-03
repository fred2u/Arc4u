using Arc4u.Dependency.Attribute;

namespace Arc4u.NServiceBus;

[Export, Scoped]
public class MessagesToPublish
{
    private readonly List<object> messages = [];

    public static Func<Type, bool> EventsNamingConvention = _ => false;

    public static Func<Type, bool> CommandsNamingConvention = _ => false;

    /// <summary>
    /// Register a "ICommand" or "IEvent" to publish.
    /// </summary>
    /// <param name="message">Add a Command or event</param>
    public void Add(object message)
    {
        if (null == EventsNamingConvention || null == CommandsNamingConvention)
        {
            throw new InvalidOperationException("No conventions is defined for Commands or Events.");
        }

        if (EventsNamingConvention(message.GetType()) || CommandsNamingConvention(message.GetType()))
        {
            messages.Add(message);
        }
        else
        {
            throw new InvalidOperationException("Doesn't respect the namespace convention defined for events and commands.");
        }
    }

    /// <summary>
    /// Used to clear any messages already registered.
    /// </summary>
    public void Clear()
    {
        messages?.Clear();
    }

    public List<object> Events => messages.Where((m) => EventsNamingConvention(m.GetType())).ToList();

    public List<object> Commands => messages.Where((m) => CommandsNamingConvention(m.GetType())).ToList();
}
