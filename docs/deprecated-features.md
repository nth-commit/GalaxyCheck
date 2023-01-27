# Deprecated Features

This doc is intended to capture (experimental) features that were discarded, and why.

## Preconditions

This was a convenience feature that allowed you to specify a precondition inside the test body, which simplified the way you would write some properties. Consider the following:

```
private static IGen<(int x, int y)> TwoDistinctIntegers =>
    from x in Gen.Int32()
    from y in Gen.Int32()
    where x != y
    select (x, y);

[Property]
public void DifferentIntegersHashDifferently([MemberGen(nameof(TwoDistinctIntegers))] (int x, int y) tupleOfInts)
{
    Assert.NotEqual(Hash(tupleOfInts.x), Hash(tupleOfInts.y));
}
```

This could be expressed with preconditions as:

```
[Property]
public void DifferentIntegersHashDifferently(int x, int y)
{
    Property.Precondition(x != y);

    Assert.NotEqual(Hash(x), Hash(y));
}
```

This was deprecated because it added significant complexity to GalaxyCheck. It is useful, and may be added in the future in a more considered way.

The main problems were:

1. Firstly, filtering generators is just hard. All generators have an input size, which determines the range of the values they produce. A size of 0 will always produce 0 values (empty lists, strings, or literally a `0` for integers). We have some rough heuristics for "resizing" if the generator fails to produce a value - for example the above property will never be satisfied if the size is 0 (both x and y will = 0).
2. Implementing pre-conditions into GalaxyCheck as it stands required separate, disconnected, resizing heuristics in the generators (such as in the first example above), and in the test runner (the `Check` function), which decorates generators and provides the state machine for iterating through test-cases. A future solution would consider a "holistic" filtering approach, where the behaviour is injected into both models, and we have a single state that holds filter metrics.
3. Samples. An core value of the library is that it should be observable, so that an appropriate level of trust can be built between a user and the library. One of the main features that enables this is the ability to easily sample what values _would have_ been passed to your test, and you can do this by swapping a `Sample` attribute in for a `Property` attribute. The interaction between sampling and preconditions needs special consideration (in implementation and desired behaviour), as a value is passed to your test, then it is discard. Samples of the two properties above would produce different results, if not especially considered by the library.
