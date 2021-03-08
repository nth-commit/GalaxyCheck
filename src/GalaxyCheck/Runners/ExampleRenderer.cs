using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GalaxyCheck.Runners
{
    public abstract record ExampleViewModel
    {
        public record Nullary : ExampleViewModel;

        public record Unary(object? Value) : ExampleViewModel;

        public record Multiary(object[] Values) : ExampleViewModel;

        public static ExampleViewModel Infer(object? value)
        {
            if (value is Array arr)
            {
                return new Multiary(arr.Cast<object>().ToArray());
            }

            return new Unary(value);
        }
    }

    public static class ExampleRenderer
    {
        public static IEnumerable<string> Render(ExampleViewModel example) => example switch
        {
            ExampleViewModel.Nullary _ => new string[] { "(no value)" },
            ExampleViewModel.Unary unary => new string[] { RenderValue(unary.Value) },
            ExampleViewModel.Multiary multiary => multiary.Values.Select((value, index) => $"[{index}] = {RenderValue(value)}"),
            _ => throw new NotSupportedException("Unhandled switch")
        };

        private static string RenderValue(object? value) => JsonSerializer.Serialize(
            value,
            new JsonSerializerOptions(new JsonSerializerOptions()
            {
                Converters =
                {
                    new DelegateConverterFactory()
                }
            }));

        private class DelegateConverterFactory : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert)
            {
                return typeof(Delegate).IsAssignableFrom(typeToConvert);
            }

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                return new DelegateConverter();
            }

            private class DelegateConverter : JsonConverter<Delegate>
            {
                public override Delegate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
                {
                    throw new InvalidOperationException();
                }

                public override void Write(Utf8JsonWriter writer, Delegate value, JsonSerializerOptions options)
                {
                    var parameterTypes = value.Method.GetParameters().Select(pi => pi.ParameterType.Name);
                    var returnType = value.Method.ReturnType.Name;

                    var stringRepresentation = $"Function ({string.Join(",", parameterTypes)}) => {returnType}";

                    writer.WriteStringValue(stringRepresentation);
                }
            }
        }
    }
}
