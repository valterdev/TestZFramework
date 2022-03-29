using System;

/// <summary>
/// Атрибут [GSSRegPostHelper("Имя функции")] позволяющий связывать переменные из хранилища с функциями хелперами, которые могут производить какие-либо действия после того как отдать переменную
/// </summary>
public class GSSRegPostHelperAttribute : Attribute
{
    public string HelperMethodName;

    public GSSRegPostHelperAttribute(string methodName)
    {
        HelperMethodName = methodName;
    }
}