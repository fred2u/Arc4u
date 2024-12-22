using System.Text.Json;
using System.Text.Json.Serialization;

namespace Arc4u.ServiceModel;

/// <summary>
/// This class is used with json to Deserialize a <see cref="Message"/> class and change the Category from a 
/// <see cref="string"/> to a <see cref="MessageCategory"/> type.
/// </summary>
public class MessageCategoryConverter : JsonConverter<Message>
{
    public override Message Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var code = string.Empty;
        var text = string.Empty;
        var subject = string.Empty;
        var category = MessageCategory.Technical;
        var type = MessageType.Information;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.PropertyName)
            {
                var propertyName = reader.GetString();

                reader.Read();

                if (!string.IsNullOrWhiteSpace(propertyName))
                {
                    if (propertyName.Equals("code", StringComparison.CurrentCultureIgnoreCase))
                    {
                        code = reader.GetString();
                    }
                    else if (propertyName.Equals("text", StringComparison.CurrentCultureIgnoreCase))
                    {
                        text = reader.GetString();
                    }
                    else if (propertyName.Equals("subject", StringComparison.CurrentCultureIgnoreCase))
                    {
                        subject = reader.GetString();
                    }
                    else if (propertyName.Equals("category", StringComparison.CurrentCultureIgnoreCase))
                    {
                        var categoryValue = reader.GetString();
                        if (categoryValue != null)
                        {
                            category = categoryValue.Equals("described", StringComparison.CurrentCultureIgnoreCase) ? MessageCategory.Business : MessageCategory.Technical;
                        }
                    }
                    else if (propertyName.Equals("type", StringComparison.CurrentCultureIgnoreCase))
                    {
                        type = (MessageType)reader.GetInt32();
                    }
                }
            }

            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return new Message(category, type, code, subject, text);
            }
        }

        return new Message();
    }

    public override void Write(Utf8JsonWriter writer, Message value, JsonSerializerOptions options)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(value);

        writer.WriteStartObject();

        if (!string.IsNullOrWhiteSpace(value.Code))
        {
            writer.WriteString("Code", value.Code);
        }

        if (!string.IsNullOrWhiteSpace(value.Subject))
        {
            writer.WriteString("Subject", value.Subject);
        }

        if (!string.IsNullOrWhiteSpace(value.Text))
        {
            writer.WriteString("Text", value.Text);
        }

        writer.WriteNumber("Type", (int)value.Type);

        writer.WriteString("Category", value.Category == MessageCategory.Business ? "Described" : "Undescribed");

        writer.WriteEndObject();
    }
}
