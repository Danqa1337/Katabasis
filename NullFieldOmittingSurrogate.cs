using System;
using System.Runtime.Serialization;
using System.Reflection;

public class NullFieldOmittingSurrogate : ISerializationSurrogate
{
    void ISerializationSurrogate.GetObjectData(object obj, SerializationInfo info, StreamingContext context)
    {
        if (obj != null)
        {
            foreach (FieldInfo field in obj.GetType().GetFields
                (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object fieldValue = field.GetValue(obj);
                if (fieldValue != null)
                {
                    info.AddValue(field.Name, fieldValue);
                }
            }
        }
    }

    object ISerializationSurrogate.SetObjectData(object obj, SerializationInfo info, StreamingContext context, ISurrogateSelector selector)
    {
        // The deserialization works without the need for this surrogate
        throw new NotImplementedException();
    }
}



