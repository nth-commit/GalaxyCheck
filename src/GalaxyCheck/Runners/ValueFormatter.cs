using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace GalaxyCheck.Runners
{
    public static class ValueFormatter
    {
        public static string FormatValue(object? value)
        {
            return JsonSerializer.Serialize(value, new JsonSerializerOptions(new JsonSerializerOptions()
            {
                Converters =
                {
                    new DelegateConverterFactory()
                }
            }));
        }

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
                    writer.WriteStringValue(value.GetType().FullName);
                }
            }
        }
    }
}
