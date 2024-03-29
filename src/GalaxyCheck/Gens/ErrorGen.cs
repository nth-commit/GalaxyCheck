﻿namespace GalaxyCheck
{
    using GalaxyCheck.Gens;
    using System;

    public static partial class Gen
    {
        public static partial class Advanced
        {
            /// <summary>
            /// As an alternative to throwing exceptions, creates a generator that always errors. This is useful when
            /// writing custom generators - if any of the inputs were invalid, switching to an error generator ensures
            /// that GalaxyCheck will handle the error through its normal error code path. Otherwise, exceptions might
            /// bubble up and provide less-interesting diagnostics.
            /// </summary>
            /// <param name="message">A message explaining what this error is, and what it might have been caused by.
            /// </param>
            /// <returns>A new generator that errors when it runs.</returns>
            public static IGen<T> Error<T>(string message) => new ErrorGen<T>(message);

            internal static IGen Error(Type type, string message)
            {
                var genericErrorGen = typeof(Advanced)
                    .GetMethod(nameof(Advanced.Error))!
                    .MakeGenericMethod(type);

                return (IGen)genericErrorGen.Invoke(null, new object?[] { message })!;
            }
        }
    }
}

namespace GalaxyCheck.Gens
{
    using GalaxyCheck.Gens.Internal;
    using GalaxyCheck.Gens.Internal.Iterations;
    using GalaxyCheck.Gens.Iterations.Generic;
    using GalaxyCheck.Gens.Parameters;
    using System.Collections.Generic;

    internal record ErrorGen<T> : BaseGen<T>, IGen<T>
    {
        private readonly string _message;

        public ErrorGen(string message)
        {
            _message = message;
        }

        protected override IEnumerable<IGenIteration<T>> Run(GenParameters parameters)
        {
            while (true)
            {
                yield return GenIterationFactory.Error<T>(parameters, _message);
            }
        }
    }
}
