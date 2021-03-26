# StringGenTests

The tests for Gen.String() needn't be as stringent as the bar we have set for testing other generators, as we can use a generator for a list of chars as an oracle.

This way, the frequently tested aspects can be skipped:

- Specific value production
- Shrinking
- Snapshots

These are mostly asserted in AboutValueProduction.
