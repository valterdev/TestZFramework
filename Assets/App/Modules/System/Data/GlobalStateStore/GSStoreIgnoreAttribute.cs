using System;

/// <summary>
/// Атрибут [GSStoreIgnore] позволяет игнорировать переменную и не заносить ее в глобальное хранилище.
/// </summary>
public class GSStoreIgnoreAttribute : Attribute
{
    public GSStoreIgnoreAttribute()
    {

    }
}
