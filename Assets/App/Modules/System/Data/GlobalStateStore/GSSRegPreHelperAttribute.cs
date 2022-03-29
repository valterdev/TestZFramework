using System;

/// <summary>
/// Атрибут [GSSRegPreHelper("Имя функции")] позволяющий связывать переменные из хранилища с функциями хелперами, которые могут производить какие-либо действия перед тем как отдать переменную
/// </summary>
public class GSSRegPreHelperAttribute : Attribute
{
    public string HelperMethodName;
    
    public GSSRegPreHelperAttribute(string methodName)
    {
        HelperMethodName = methodName;
    }
}