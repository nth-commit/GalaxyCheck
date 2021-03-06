# Decision Register

## The project is called "Galaxy Check"

Space puns, mostly. The meat of a property-based testing framework is centered around exploring example _spaces_ (not problem spaces, as an example is not necessarily a counterexample).

Also, I feel there's something magical/existential living inside of a good property-based testing framework, which also really encroaches on the essence of code at the same time. Like, when you write a function (or a module or an application), you are spinning up a little universe with some basic rules that apply. These rules may interact in weird ways that are measurable, but not encoded into the universe. Similarly, it's unlikely that if we look close enough at the universe, that we will read C = 299,792,458ms, but yet, that rule exists.

A property-based testing framework can tell us factually about the universes we create. It can also tell us the rules of chaotic universes that already exist.

2021-01-01

## No post-initialisation shrink (yet)

From prior experience, shrinking after initialisation of an example space is hard. It involves some crazy unintuitive (to me) kind of tree transposition to get this working in an efficient way. The classic example is joining example spaces to create an example space of an array. When you do that, you want to maintain the shrinks of the original example spaces (i.e. you want the elements to shrink as they would individually). But, you also want to shrink the size of the array, and you want to do that first as it tends to cut down the example space pretty rapidly.

However, we can also achieve the same result by building the new shrinks into the new example space as we create it.

This is less useful than applying any shrink function to any generator at any point in time, and being able to determine if the shrink function should happen before or after the existing shrinks. But, it probably covers most use-cases, and (whilst complex), simplifies things a lot.

2021-01-01

## Use FsCheck over Hedgehog for property tests requiring "arbitrary" values

FsCheck just seems easier to use with the `[Property]` attribute. However, I'm not sure how it flies when you're creating higher-order generators.

FsCheck is good for now, for terseness. But will experiment with Hedgehog further down the line. GalaxyCheck will be used when stable enough.

2021-01-01

## Randomness is treated with prudence, and its consumption measured

With all generators, we should be careful to use randomness as sparingly and as deliberately as possible. Not necessarily because of efficiency, although that might be a factor, but because of simplicity. Each iteration of a generator should announce what RNG it was seeded with (for repetition purposes), but also what the RNG the next iteration should use. For a primitive generator like `Gen.Constant`, the initial RNG and the next RNG should be equivalent, denoting no randomness was consumed. For something like `Gen.Int32`, the RNG should only progress by one step for an iteration, denoting that one random integer was generated in the process.

This metric can be used as a mechanism to solve for the shrinking challenge ["Length List"](https://github.com/jlink/shrinking-challenge/blob/main/challenges/lengthlist.md) because it allows us to analyse how much randomness was consumed. In our bound shrinks, when we notice the randomness consumed lessens, we can try different permutations of the RNG that fit into the original consumed space. For example, if the original instance consumed randomness twice, and the shrunk consumed randomness once, we can generate a second shrink by skipping the first RNG.

The metric also gives some nice introspection into how generators behave, for testing purposes.

The RNG model in GalaxyCheck should therefore behave like a single linked-list. The seed that an RNG uses is not only used to generate values from that RNG, but also to create the next RNG. For contrast, in other frameworks, to ensure distribution of values, the RNG is split and then one part of it is handed off to a generator, whether or not the generator needs to consume that randomness or not. The other part is used to seed the next RNG.

2021-01-03

## Test generators through an aggregation function

A generator's behaviour is mostly defined by the implementation of it's `Run` function. Therefore, there is an argument to testing this function directly.

However, it's a lot of effort to use this function. The "more public" API is one of the aggregation functions, like `Sample` or `Minimal` - which will handle the cases of a generated iteration (discards, errors), have infinite loop protection built in, and more.

It's also nice to test the public API, and means we probably won't need to test the aggregation functions directly.

2021-01-03

## Test generators with a hard-coded seed

Most of the generator behaviour is itself tested through properties.

It becomes tricky when you have to manage two levels of randomness. That's two levels of replication required when a test fails, one to control the test itself, and one to control the generator-under-test. We still get good variation in our tests if the other parameters are injected through properties.

2021-01-03

## Generators should never throw

Instead, they should produce an error token, so that the feedback can bubble up to the consumers in the same way as if a generator was exhausted.

An example of a case where we might consider crashing, is if the given origin of a range is outside it's bounds. However, we cannot presume all the ways in which consumers might construct generators. For example, a consumer might be generating random ranges and binding those to a random integer. It is feasible that, due to an error in their generator, the origin of their range is outside the range's bounds. For the consumer's perspective, this error is equatable to say - trying to filter with an impossible predicate - so they should receive feedback in the same way.

This means we need to place an error signal into the stream, so that the consumer of that stream can handle any errors in a consistent manner.

2021-01-03

## Generators should never terminate

It's a feature that generators produce streams rather than single values. It enables infinite loop handling (we can write discard tokens into the stream, then the consumer can give up after seeing a certain density of discards). Because a generator needs to produce an arbitrary amount of iterations whenever it's called, then consumers should be prepared for excruciatingly large amounts of iterations. To ensure they can handle excruciatingly large streams, then we should always be generating infinite streams. For the stream aggregating code to be regular, then streams should always be infinite. This includes when streams terminate by error - rather than terminating they should repeat the error token.

2021-01-03

## Properties are generators

This is an aspirational decision.

Properties are basically a boolean generator, indicating whether the the iteration passed or failed.

However, there is a fair bit more to them. For instance, they terminate when falsified, they need to be repeatable without re-shrinking, they need to run with a varying size. We should hopefully be able to either lift these special behaviours into the generator behaviour, or pull it out into the `Check` aggregation function.

If we are successful, it means we will get sampling and printing of properties for free, and there will be less code to maintain.

2021-01-05

## Property iterations are lazy

A property is a generator.

However, it's facets, some input values and a function, should be stored on the generator iteration, rather than storing the evaluated result.

This allows a property to leverage operators that are present on generators, such as `Where()` in useful ways, as they can operate on the inputs.

2021-01-10

## Convention: Size > 50 = Chaos mode

We want to get a really good variance of data from a generator, to use in our properties.

An example of how we do this when generating integers, if below a certain size, we just simply size the bounds (1.), and generate a random int within that bounds. When the generation exceeds a certain size, we bias towards the extremes. We do this by first pulling a random int, which drives whether we generate an int from the sized bounds, or if we just return an extreme (2.). Generating an int from the sized bounds will consume randomness again, but return an extreme won't. As the size increases thereafter, it becomes more likely that we'll just return an extreme.

You might have noted, that the amount of randomness consumed could become chaotic over that certain size. That is, over the "chaos size", to generate integer we may consume the randomness either once or twice. This makes tests that leverage the integer generator incidentally more complicated, if they want to make other assertions about how randomness is consumed (3.).

Therefore, we should establish a convention that when size > 50, generators are allowed to behave "chaotically". This means it's OK to test other things at a size <= 50, if those tests would become to complex to manage with chaos mode activated.

1. For example, if the bounds are [-10, 10], and size is 20, we might size the bounds to be [-2, 2].
2. An extreme is one of the edges of the bounds. For example, if the bounds are [-10, 10], the extremes are -10 and 10.
3. For example, the list generation tests use the integer generator for it's elements, and check that randomness is consumed for generating the length of the list, and for generating the elements.

2021-03-03
