using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace GalaxyCheck.Runners
{
    public static class ExampleRenderer
    {
        public static IEnumerable<string> Render(object?[] example) => example.Length switch
        {
            0 => new string[] { "(no value)" },
            1 => new string[] { RenderValue(example.Single()) },
            _ => example.Select((value, index) => $"[{index}] = {RenderValue(value)}"),
        };

        private static string RenderValue(object? obj)
        {
            const int BreadthLimit = 10;

            var handler = new CompositeExampleRendererHandler(new List<IExampleRendererHandler>
            {
                new PrimitiveExampleRendererHandler(),
                new EnumerableExampleRendererHandler(elementLimit: BreadthLimit),
                new TupleRendererHandler(),
                new ObjectRendererHandler(propertyLimit: BreadthLimit)
            });

            return handler.Render(obj, handler);
        }

        private interface IExampleRendererHandler
        {
            bool CanRender(object? obj);

            string Render(object? obj, IExampleRendererHandler renderer);
        }

        private class CompositeExampleRendererHandler : IExampleRendererHandler
        {
            private readonly IReadOnlyCollection<IExampleRendererHandler> _innerHandlers;

            public CompositeExampleRendererHandler(
                IReadOnlyCollection<IExampleRendererHandler> innerHandlers)
            {
                _innerHandlers = innerHandlers;
            }

            public bool CanRender(object? obj) => true;

            public string Render(object? obj, IExampleRendererHandler renderer)
            {
                try
                {
                    var innerHandler = _innerHandlers.Where(h => h.CanRender(obj)).FirstOrDefault();

                    if (innerHandler == null)
                    {
                        return obj?.ToString() ?? "null";
                    }

                    return innerHandler.Render(obj, renderer);
                }
                catch
                {
                    return "<Rendering failed>";
                }
            }
        }

        private class PrimitiveExampleRendererHandler : IExampleRendererHandler
        {
            private static readonly ImmutableHashSet<Type> ExtraPrimitiveTypes =
                ImmutableHashSet.Create(typeof(string), typeof(decimal));

            public bool CanRender(object? obj) =>
                obj is null ||
                obj.GetType().IsPrimitive ||
                obj.GetType().IsEnum ||
                ExtraPrimitiveTypes.Contains(obj.GetType()) ||
                obj is Delegate;

            public string Render(object? obj, IExampleRendererHandler renderer) => obj?.ToString() ?? "null";
        }

        private class EnumerableExampleRendererHandler : IExampleRendererHandler
        {
            private readonly int _elementLimit;

            public EnumerableExampleRendererHandler(int elementLimit)
            {
                _elementLimit = elementLimit;
            }

            public bool CanRender(object? obj) =>
                obj is not null &&
                obj is not string &&
                obj.GetType().GetInterfaces().Any(t =>
                    t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            public string Render(object? obj, IExampleRendererHandler renderer)
            {
                var elements = ((IEnumerable)obj!).Cast<object>();

                var elementsToRender = elements.Take(_elementLimit).ToList();
                var hasMoreElements = elements.Skip(_elementLimit).Any();

                var renderedElements = Enumerable.Concat(
                    elementsToRender.Select(x => renderer.Render(x, renderer)),
                    hasMoreElements ? new[] { "..." } : Enumerable.Empty<string>());

                return "[" + string.Join(", ", renderedElements) + "]";
            }
        }

        private class TupleRendererHandler : IExampleRendererHandler
        {
            public bool CanRender(object? obj) =>
                obj is not null &&
                obj.GetType().GetInterface("System.Runtime.CompilerServices.ITuple") != null;

            public string Render(object? obj, IExampleRendererHandler renderer)
            {
                return obj!.GetType().GetFields().Any()
                    ? RenderTupleLiteral(obj, renderer)
                    : RenderClassTuple(obj, renderer);
            }

            private static string RenderTupleLiteral(object obj, IExampleRendererHandler renderer)
            {
                var fields = obj!.GetType().GetFields();

                var fieldsToRender = fields.Where(f => f.Name != "Rest").ToList();
                var hasMoreFields = fields.Any(f => f.Name == "Rest");

                var renderedFields = Enumerable.Concat(
                    fieldsToRender.Select(f => $"{f.Name} = {renderer.Render(f.GetValue(obj), renderer)}"),
                    hasMoreFields ? new[] { "Rest = ..." } : Enumerable.Empty<string>());

                return "(" + string.Join(", ", renderedFields) + ")";
            }

            private static string RenderClassTuple(object obj, IExampleRendererHandler renderer)
            {
                var properties = obj!.GetType().GetProperties();

                var renderedProperties = properties.Select(p => $"{p.Name} = {renderer.Render(p.GetValue(obj), renderer)}");

                return "(" + string.Join(", ", renderedProperties) + ")";
            }
        }

        private class ObjectRendererHandler : IExampleRendererHandler
        {
            private readonly int _propertyLimit;

            public ObjectRendererHandler(int propertyLimit)
            {
                _propertyLimit = propertyLimit;
            }

            public bool CanRender(object? obj) =>
                obj is not null;

            public string Render(object? obj, IExampleRendererHandler renderer)
            {
                var properties = obj!.GetType().GetProperties();

                var propertiesToRender = properties.Take(_propertyLimit).ToList();
                var hasMoreProperties = properties.Skip(_propertyLimit).Any();

                var renderedProperties = Enumerable.Concat(
                    propertiesToRender.Select(p => $"{p.Name} = {renderer.Render(p.GetValue(obj), renderer)}"),
                    hasMoreProperties ? new[] { "..." } : Enumerable.Empty<string>());

                return "{ " + string.Join(", ", renderedProperties) + " }";
            }
        }
    }
}
