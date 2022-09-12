using System;

/// <summary>
/// Attribute [GSSRegPostHelper("Function name")] allows you to associate variables from the store with helper functions that can perform any action after giving the variable.
/// </summary>
public class GSSRegPostHelperAttribute : Attribute
{
    #region Fields

    // ---------------------------------------------------------------------------------------------------------
    // Public fields
    // ---------------------------------------------------------------------------------------------------------

    public string HelperMethodName;

    #endregion

    #region Object lifecycle

    public GSSRegPostHelperAttribute(string methodName)
    {
        HelperMethodName = methodName;
    }

    #endregion
}