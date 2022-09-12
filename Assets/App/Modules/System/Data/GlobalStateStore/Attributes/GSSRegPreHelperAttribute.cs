using System;

/// <summary>
/// Attribute [GSSRegPreHelper("Function name")] allows you to associate variables from the store with helper functions that can perform any action before passing the variable.
/// </summary>
public class GSSRegPreHelperAttribute : Attribute
{
    #region Fields

    // ---------------------------------------------------------------------------------------------------------
    // Public fields
    // ---------------------------------------------------------------------------------------------------------

    public string HelperMethodName;

    #endregion

    #region Object lifecycle

    public GSSRegPreHelperAttribute(string methodName)
    {
        HelperMethodName = methodName;
    }

    #endregion
}