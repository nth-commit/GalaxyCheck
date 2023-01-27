using GalaxyCheck.Gens.Parameters;
using GalaxyCheck.Gens.Parameters.Internal;
using GalaxyCheck.Internal;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Runners.Check.StateMachine
{
    internal record SizingAspects<T>(
        Size InitialSize,
        ResizeStrategy<T> ResizeStrategy)
    {
        public static SizingAspects<T> Resolve(Size? declaredSize, int iterations)
        {
            if (declaredSize == null)
            {
                if (iterations >= 100)
                {
                    return new SizingAspects<T>(new Size(0), SuperStrategicResize());
                }
                else
                {
                    var (initialSize, nextSizes) = SamplingSize.SampleSize(iterations);
                    return new SizingAspects<T>(initialSize!, PlannedResize(nextSizes));
                }
            }

            return new SizingAspects<T>(declaredSize, NoopResize());
        }

        /// <summary>
        /// When a resize is called for, do a small increment if the iteration wasn't a counterexample. This increment
        /// may loop back to zero. If it was a counterexample, no time to screw around. Activate turbo-mode and
        /// accelerate towards max size, and never loop back to zero. Because we know this is a fallible property, we 
        /// should generate bigger values, because bigger values are more likely to be able to normalize.
        /// </summary>
        private static ResizeStrategy<T> SuperStrategicResize()
        {
            return (info) => info.Instance.Match(
                onInstance: instance =>
                {
                    if (info.CurrentCounterexample is null)
                    {
                        return instance.NextParameters.Size.Increment();
                    }

                    var nextSize = instance.NextParameters.Size.BigIncrement();

                    var incrementWrappedToZero = nextSize.Value < instance.NextParameters.Size.Value;
                    if (incrementWrappedToZero)
                    {
                        // Clamp to MaxValue
                        return new Size(100);
                    }

                    return nextSize;
                },
                onError: error => error.ReplayParameters.Size,
                onDiscard: discard => discard.NextParameters.Size);
        }

        /// <summary>
        /// Resize according to the given size plan. If we find a counterexample, switch to super-strategic resizing.
        /// </summary>
        private static ResizeStrategy<T> PlannedResize(IEnumerable<Size> plannedSizes)
        {
            var sizes = plannedSizes.ToImmutableList();
            return (info) =>
            {
                if (info.CurrentCounterexample is not null)
                {
                    return SuperStrategicResize()(info);
                }

                var size = sizes.Skip(info.CheckStateData.CompletedIterations - 1).FirstOrDefault() ?? new Size(0);
                return size;
            };
        }

        /// <summary>
        /// If you thought you were going to resize well have I got news for you. Resizing cancelled!
        /// </summary>
        private static ResizeStrategy<T> NoopResize() => (info) => info.Instance.NextParameters.Size;
    }
}
