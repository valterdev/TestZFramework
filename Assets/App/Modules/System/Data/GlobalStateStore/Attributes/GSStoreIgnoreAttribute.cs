using System;

/// <summary>
/// Attribute [GSStoreIgnore] allows you to ignore the variable and not bring it into the global storage.
/// </summary>
public class GSStoreIgnoreAttribute : Attribute
{
    public GSStoreIgnoreAttribute()
    {

    }
}
