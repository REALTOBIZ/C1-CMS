using System;


namespace Composite.Forms
{
    /// <summary>
    /// Defines that a property should be visuble / accessible in the forms environment.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,AllowMultiple=false,Inherited=true)]
    internal sealed class FormsPropertyAttribute : Attribute
    {
    }
}