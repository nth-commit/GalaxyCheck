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

        private static string RenderValue(object? value)
        {
            if (value is Delegate del)
            {
                return RenderType(del.GetType());
            }

            return JsonSerializer.Serialize(
                value,
                    new JsonSerializerOptions(new JsonSerializerOptions()
                    {
                        Converters =
                        {
                            new DelegateConverterFactory(),
                            new TupleConverterFactory()
                        }
                    }));
        }

        private static bool IsDelegateType(Type type) => typeof(Delegate).IsAssignableFrom(type);

        private static string RenderType(Type type) => type.IsGenericType ? RenderGenericType(type) : type.Name;

        private static string RenderGenericType(Type type)
        {
            var genericTypeName = type.GetGenericTypeDefinition().Name;

            var genericParameters = type
                .GetGenericArguments()
                .Select(t => RenderType(t));

            return $"{genericTypeName}[{string.Join(",", genericParameters)}]";
        }

        private class DelegateConverterFactory : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert) => IsDelegateType(typeToConvert);

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => new DelegateConverter();

            private class DelegateConverter : JsonConverter<Delegate>
            {
                public override Delegate? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                    throw new InvalidOperationException();

                public override void Write(Utf8JsonWriter writer, Delegate value, JsonSerializerOptions options)
                {
                    writer.WriteStringValue("Delegate");
                    writer.WriteCommentValue(RenderType(value.GetType()));
                }
            }
        }

        private class TupleConverterFactory : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert) =>
                typeToConvert.GetInterface("System.Runtime.CompilerServices.ITuple") != null;

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options) => new ValueTupleConverter();

            private class ValueTupleConverter : JsonConverter<object>
            {
                public override object Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                    throw new InvalidOperationException();

                public override void Write(Utf8JsonWriter writer, object value, JsonSerializerOptions options)
                {
                    writer.WriteStartObject();

                    foreach (var field in value.GetType().GetFields())
                    {
                        writer.WritePropertyName(field.Name);
                        JsonSerializer.Serialize(writer, field.GetValue(value), options);
                    }

                    foreach (var property in value.GetType().GetProperties())
                    {
                        writer.WritePropertyName(property.Name);
                        JsonSerializer.Serialize(writer, property.GetValue(value), options);
                    }

                    writer.WriteEndObject();
                }
            }
        }
    }
}
