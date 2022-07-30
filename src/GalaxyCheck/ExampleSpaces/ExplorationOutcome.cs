using System;

internal record ExplorationOutcome
{
    public record SuccessData;

    public record FailData(Exception? Exception);

    private readonly SuccessData? _success;
    private readonly FailData? _fail;

    private ExplorationOutcome(
        SuccessData? success,
        FailData? fail)
    {
        _success = success;
        _fail = fail;
    }

    public static ExplorationOutcome Success() => new ExplorationOutcome(new SuccessData(), null);
    public static ExplorationOutcome Fail(Exception? exception) => new ExplorationOutcome(null, new FailData(exception));

    public T Match<T>(
        Func<T> onSuccess,
        Func<Exception?, T> onFail)
    {
        if (_success != null) return onSuccess();
        if (_fail != null) return onFail(_fail.Exception);
        throw new NotSupportedException();
    }
}
