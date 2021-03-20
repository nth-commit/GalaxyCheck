GalaxyCheck is a modern property-based testing framework for .NET, in a similar vein to [FsCheck](https://github.com/fscheck/FsCheck). Although GalaxyCheck has been more directly influenced by [fast-check](https://github.com/dubzzz/fast-check), [jqwik](https://github.com/jlink/jqwik) is and [Hedgehog](https://github.com/hedgehogqa/fsharp-hedgehog).

The library heavily leans on LINQ expressions, allowing seamless between test-data generators and properties.

[![Nuget](https://img.shields.io/nuget/v/galaxycheck)](https://www.nuget.org/packages/GalaxyCheck/)

## Quickstart

The recommend way to getting started quickly with GalaxyCheck, is through [xunit](https://github.com/xunit/xunit), and using the integration package, [GalaxyCheck.Xunit](https://www.nuget.org/packages/GalaxyCheck.Xunit/).

```csharp
using GalaxyCheck;
using System.Linq;

public class AboutSort
{
    [Property]
    public IGen<Test> SortIsIdempotent() =>
        from xs in Gen.Int32().ListOf()
        select Property.ForThese(() => xs.Sort().SequenceEqual(xs.Sort().Sort()));
}
```

GalaxyCheck makes randomly-generated lists of integers available to the test function, to prove its correctness over many iterations and a high variance of input.

## Notable features

### Shrinking

GalaxyCheck does a pretty good job shrinking the counterexamples it finds to their minimal-reproduction. Unlike older flavours of QuickCheck, it also ensures that those shrinks conform to the same constraints that were specified when creating generators.

For example, given the (intentionally erroneous) property:

```csharp
[Property]
public IGen<Test> MyProperty() =>
    from x in Gen.Int32().GreaterThanEqual(1)
    from y in Gen.Int32().GreaterThanEqual(x + 5)
    select Property.ForThese(() => x < 10 || y < x);
```

`x` is sometimes less than 10, but `y` should never be less than `x`, so this property should fail. The tool can tell us what the minimal counterexample to this property was!

```
GalaxyCheck.Runners.PropertyFailedException :

    Falsified after 31 tests (5 shrinks)
    Reproduction: [Replay("H4sIAAAAAAAACjM3sDA0NDIysdQzNtAzAQDv8uhODgAAAA==")]
    Counterexample:
        [0] = 10
        [1] = 15

    Property function returned false
```

This example leans heavily on the fact that GalaxyCheck implements something called "integrated shrinking". In classical property-based testing frameworks, you needed to define your generation function and your shrinking function independently. This adds a lot of manual work, with a lot room for error, especially when when composing generators!

GalaxyCheck knows that the minimal counterexample is somewhere in the cross-product example space of `x` and `y`, and it has strategies to calculate that cross-product.

### Composability

It's really _really_ easy to write custom generators.

For example, here's generator that produces a point:

```csharp
public static IGen<(int x, int y)> Point =>
    from x in Gen.Int32()
    from y in Gen.Int32()
    select (x, y);
```

But now, what if instead of any point, we're only interested in points that do not lie in the negative space. We can simply constrain the generators using the builder functions provided by the API:

```csharp
public static IGen<(int x, int y)> Point =>
    from x in Gen.Int32().GreaterThanEqual(0)
    from y in Gen.Int32().GreaterThanEqual(0)
    select (x, y);
```

Hooooold up. Requirements have changed. We actually now need to generate a range, and the interesting thing about the kind of range is that the minimum and maximum bounds need to be in order

This would be a significant structural change in some other frameworks, but using LINQ, it's trivial:

```csharp
public static IGen<(int min, int max)> Range =>
    from min in Gen.Int32().GreaterThanEqual(0)
    from max in Gen.Int32().GreaterThanEqual(min)
    select (min, max);
```

### Repeatability

In our shrinking example above, you might have noticed that GalaxyCheck output an interesting little directive when it found a counterexample: `[Replay("H4sIAAAAAAAACjM3sDA0NDIysdQzNtAzAQDv8uhODgAAAA==")]`.

You can simply augment a property with this attribute, and GalaxyCheck will allow you to reproduce the exact values that caused your test to fail:

```csharp
[Property]
[Replay("H4sIAAAAAAAACjM3sDA0NDIysdQzNtAzAQDv8uhODgAAAA==")]
public IGen<Test> MyProperty() =>
    from x in Gen.Int32().GreaterThanEqual(1)
    from y in Gen.Int32().GreaterThanEqual(x + 5)
    select Property.ForThese(() => x < 10 || y < x);
```

Other frameworks have similar features, though often only supply you with the original seed. This means that when you want to debug, you'll have to step through numerous examples that weren't counterexamples. Then, if it shrunk, you'll need to step through all the test runs associated with those shrinks too (probably whilst assertions are being thrown as your test fails).

GalaxyCheck encodes the exact iteration and the exact shrink into the replay, which means starting the debugger will take you straight there.

### Visibility

Other frameworks provide sampling functionality, but it can be quite disruptive to get at whilst you're in there writing properties. GalaxyCheck provides a `SampleAttribute`, which can be hot-swapped with `PropertyAttribute`, so you can immediately get an intuition of what values your function is being called with.

For example:

```csharp
[Sample]
public IGen<Test> MyProperty() =>
    from x in Gen.Int32().GreaterThanEqual(1)
    from y in Gen.Int32().GreaterThanEqual(x + 5)
    select Property.ForThese(() => x < 10 || y < x);
```

Results in:

```
GalaxyCheck.SampleException : Test case failed to prevent false-positives.

    Sampled 100 values (0 discarded):

    Sample[0]:
        [0] = 1
        [1] = 6

    Sample[1]:
        [0] = 2
        [1] = 8

    ...

    Sample[99]:
        [0] = 2147483647
        [1] = -1944670218
```

Interestingly, sampling those values revealed a bug in my test. If `x` is close to `int.MaxValue`, `x + 5` may overflow. Therefore, I've learnt that for this test, I need to constrain the generator for `x` a bit more:

```csharp
[Sample]
public IGen<Test> MyProperty() =>
    from x in Gen.Int32().Between(1, int.MaxValue - 5)
    from y in Gen.Int32().GreaterThanEqual(x + 5)
    select Property.ForThese(() => x < 10 || x > y);
```
