using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

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
                        new TupleConverterFactory(),
                        new DictionaryConverterFactory(),
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

        private class DictionaryConverterFactory : JsonConverterFactory
        {
            public override bool CanConvert(Type typeToConvert) =>
                typeToConvert.GetInterfaces().Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IDictionary<,>));

            public override JsonConverter? CreateConverter(Type typeToConvert, JsonSerializerOptions options)
            {
                var genericArgs = typeToConvert.GetGenericArguments();
                var genericDictionaryConvertor = typeof(DictionaryConverter<,>).MakeGenericType(genericArgs);

                return (JsonConverter)Activator.CreateInstance(genericDictionaryConvertor);
            }

            private class DictionaryConverter<TKey, TValue> : JsonConverter<IDictionary<TKey, TValue>>
            {
                public override IDictionary<TKey, TValue>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options) =>
                    throw new NotImplementedException();

                public override void Write(Utf8JsonWriter writer, IDictionary<TKey, TValue> dictionary, JsonSerializerOptions options)
                {
                    if (typeof(TKey) == typeof(string) || typeof(TKey) == typeof(int))
                    {
                        JsonSerializer.Serialize(writer, dictionary, options);
                    }
                    else
                    {
                        writer.WriteStartArray();

                        foreach (var (kvp, i) in dictionary.Select((kvp, i) => (kvp, i)))
                        {
                            writer.WriteCommentValue(JsonSerializer.Serialize(kvp.Key, options) + ":");
                            JsonSerializer.Serialize(writer, kvp.Value, options);
                        }

                        writer.WriteEndArray();
                    }
                }
            }
        }
    }
}
