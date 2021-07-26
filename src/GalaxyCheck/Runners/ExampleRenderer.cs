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
            const int DepthLimit = 10;
            const int BreadthLimit = 10;

            var handler = new CompositeExampleRendererHandler(
                new List<IExampleRendererHandler>
                {
                    new StringExampleRendererHandler(),
                    new PrimitiveExampleRendererHandler(),
                    new EnumerableExampleRendererHandler(elementLimit: BreadthLimit),
                    new TupleRendererHandler(),
                    new ObjectRendererHandler(propertyLimit: BreadthLimit)
                },
                DepthLimit);

            return handler.Render(obj, handler, ImmutableList.Create<object?>());
        }

        private interface IExampleRendererHandler
        {
            bool CanRender(object? obj);

            string Render(object? obj, IExampleRendererHandler renderer, ImmutableList<object?> path);
        }

        private class CompositeExampleRendererHandler : IExampleRendererHandler
        {
            private readonly IReadOnlyCollection<IExampleRendererHandler> _innerHandlers;
            private readonly int _depthLimit;

            public CompositeExampleRendererHandler(
                IReadOnlyCollection<IExampleRendererHandler> innerHandlers, int depthLimit)
            {
                _innerHandlers = innerHandlers;
                _depthLimit = depthLimit;
            }

            public bool CanRender(object? obj) => true;

            public string Render(object? obj, IExampleRendererHandler renderer, ImmutableList<object?> path)
            {
                if (path.Contains(obj))
                {
                    return "<Circular reference>";
                }

                if (path.Count >= _depthLimit)
                {
                    return "...";
                }

                try
                {
                    var innerHandler = _innerHandlers.Where(h => h.CanRender(obj)).FirstOrDefault();

                    if (innerHandler == null)
                    {
                        return obj?.ToString() ?? "null";
                    }

                    return innerHandler.Render(obj, renderer, path.Add(obj));
                }
                catch
                {
                    return "<Rendering failed>";
                }
            }
        }

        private class StringExampleRendererHandler : IExampleRendererHandler
        {
            public bool CanRender(object? obj) => obj is string;

            public string Render(object? obj, IExampleRendererHandler renderer, ImmutableList<object?> path) =>
                "\"" + (string)obj! + "\"";
        }

        private class PrimitiveExampleRendererHandler : IExampleRendererHandler
        {
            private static readonly ImmutableHashSet<Type> ExtraPrimitiveTypes =
                ImmutableHashSet.Create(typeof(string), typeof(decimal), typeof(DateTime));

            public bool CanRender(object? obj) =>
                obj is null ||
                obj.GetType().IsPrimitive ||
                obj.GetType().IsEnum ||
                ExtraPrimitiveTypes.Contains(obj.GetType()) ||
                obj is Delegate;

            public string Render(object? obj, IExampleRendererHandler renderer, ImmutableList<object?> path) => obj?.ToString() ?? "null";
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
                obj.GetType().GetInterfaces().Any(t =>
                    t.IsGenericType && t.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            public string Render(object? obj, IExampleRendererHandler renderer, ImmutableList<object?> path)
            {
                var elements = ((IEnumerable)obj!).Cast<object>();

                var elementsToRender = elements.Take(_elementLimit).ToList();
                var hasMoreElements = elements.Skip(_elementLimit).Any();

                var renderedElements = Enumerable.Concat(
                    elementsToRender.Select(x => renderer.Render(x, renderer, path)),
                    hasMoreElements ? new[] { "..." } : Enumerable.Empty<string>());

                return "[" + string.Join(", ", renderedElements) + "]";
            }
        }

        private class TupleRendererHandler : IExampleRendererHandler
        {
            public bool CanRender(object? obj) =>
                obj is not null &&
                obj.GetType().GetInterface("System.Runtime.CompilerServices.ITuple") != null;

            public string Render(object? obj, IExampleRendererHandler renderer, ImmutableList<object?> path)
            {
                return obj!.GetType().GetFields().Any()
                    ? RenderTupleLiteral(obj, renderer, path)
                    : RenderClassTuple(obj, renderer, path);
            }

            private static string RenderTupleLiteral(object obj, IExampleRendererHandler renderer, ImmutableList<object?> path)
            {
                var fields = obj!.GetType().GetFields();

                var fieldsToRender = fields.Where(f => f.Name != "Rest").ToList();
                var hasMoreFields = fields.Any(f => f.Name == "Rest");

                var renderedFields = Enumerable.Concat(
                    fieldsToRender.Select(f => $"{f.Name} = {renderer.Render(f.GetValue(obj), renderer, path)}"),
                    hasMoreFields ? new[] { "Rest = ..." } : Enumerable.Empty<string>());

                return "(" + string.Join(", ", renderedFields) + ")";
            }

            private static string RenderClassTuple(object obj, IExampleRendererHandler renderer, ImmutableList<object?> path)
            {
                var properties = obj!.GetType().GetProperties();

                var renderedProperties = properties.Select(p => $"{p.Name} = {renderer.Render(p.GetValue(obj), renderer, path)}");

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

            public string Render(object? obj, IExampleRendererHandler renderer, ImmutableList<object?> path)
            {
                var properties = obj!.GetType().GetProperties();

                var propertiesToRender = properties.Take(_propertyLimit).ToList();
                var hasMoreProperties = properties.Skip(_propertyLimit).Any();

                var renderedProperties = Enumerable.Concat(
                    propertiesToRender.Select(p => $"{p.Name} = {renderer.Render(p.GetValue(obj), renderer, path)}"),
                    hasMoreProperties ? new[] { "..." } : Enumerable.Empty<string>());

                return "{ " + string.Join(", ", renderedProperties) + " }";
            }
        }
    }
}
