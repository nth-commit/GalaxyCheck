# Decision Register

## The project is called "Galaxy Check"

Space puns, mostly. The meat of a property-based testing framework is centered around exploring example _spaces_ (not problem spaces, as an example is not necessarily a counterexample).

Also, I feel there's something magical/existential living inside of a good property-based testing framework, which also really encroaches on the essence of code at the same time. Like, when you write a function (or a module or an appplication), you are spinning up a little universe with some basic rules that apply. These rules may interact in weird ways that are measureable, but not encoded into the universe. Similarly, it's unlikely that if we look close enough at the universe, that we will read C = 299,792,458ms, but yet, that rule exists.

A property-based testing framework can tell us factually about the universes we create. It can also tell us the rules of a chaotic universes that already exist.

2021-01-01

## No post-initialisation shrink (yet)

From prior experience, shrinking after initialisation of an example space is hard. It involves some crazy unintuitive (to me) kind of tree transposition to get this working in an efficient way. The classic example is joining example spaces to create an example space of an array. When you do that, you want to maintain the shrinks of the original example spaces (i.e. you want the elements to shrink as they would individually). But, you also want to shrink the size of the array, and you want to do that first as it tends to cut down the example space pretty rapidly.

However, we can also achieve the same result by building the new shrinks into the new example space as we create it.

This is a less useful than applying any shrink function to any generator at any point in time, and be able to determine if the shrink function should happen before or after the existing shrinks. But, it probably covers most use-cases, and (whilst complex), simplifies things a lot.

2021-01-01

## Use FsCheck over Hedgehog for property tests requiring "arbitrary" values

FsCheck just seems easier to use with the `[Property]` attribute. However, I'm not sure how it flies when you're creating higher-order generators.

FsCheck is good for now, for terseness. But will experiment with Hedgehog further down the line. GalaxyCheck will be used when stable enough.

2021-01-01