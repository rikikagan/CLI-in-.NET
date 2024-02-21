using System.CommandLine.Invocation;

internal class CommandHandler
{
    internal static ICommandHandler? Create<T1, T2>(Action<object, object> value)
    {
        throw new NotImplementedException();
    }

    internal static ICommandHandler? Create<T>(Action<object> value)
    {
        throw new NotImplementedException();
    }

    internal static ICommandHandler? Create<T1, T2, T3, T4, T5, T6, T7, T8, T9>(Action<object, object, object, object, object, object, object, object> value)
    {
        throw new NotImplementedException();
    }

    internal static ICommandHandler? Create<T1, T2, T3, T4, T5, T6, T7>(Action<object, object, object, object, object, object, object> value)
    {
        throw new NotImplementedException();
    }
}