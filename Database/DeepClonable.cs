using System;

[Serializable]
public abstract class DeepClonable
{
    public T DeepClone<T>() where T : DeepClonable
    {
        return BinarySerializer.MakeDeepClone<T>(this as T);
    }
}