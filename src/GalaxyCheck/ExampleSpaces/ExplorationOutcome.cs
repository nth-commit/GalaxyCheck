using System;

internal record ExplorationOutcome
{
    public record SuccessData;

    public record DiscardData;

    public record FailData(Exception? Exception);

    private readonly SuccessData? _success;
    private readonly DiscardData? _discard;
    private readonly FailData? _fail;

    private ExplorationOutcome(
        SuccessData? success,
        DiscardData? discard,
        FailData? fail)
    {
        _success = success;
        _discard = discard;
        _fail = fail;
    }

    public static ExplorationOutcome Success() => new ExplorationOutcome(new SuccessData(), null, null);
    public static ExplorationOutcome Discard() => new ExplorationOutcome(null, new DiscardData(), null);
    public static ExplorationOutcome Fail(Exception? exception) => new ExplorationOutcome(null, null, new FailData(exception));

    public T Match<T>(
        Func<T> onSuccess,
        Func<T> onDiscard,
        Func<Exception?, T> onFail)
    {
        if (_success != null) return onSuccess();
        if (_discard != null) return onDiscard();
        if (_fail != null) return onFail(_fail.Exception);
        throw new NotSupportedException();
    }
}