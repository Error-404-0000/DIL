using DIL.Interfaces;
using System;

namespace DIL.Attributes
{
    /// <summary>
    /// Attribute to convert a string parameter into a specific type using a custom converter.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter)]
    public class ConvertFromStringAttribute : Attribute
    {
        public IBindConvert IBindConvert { get; }

        /// <summary>
        /// Constructor that takes a converter type implementing IBindConvert.
        /// </summary>
        /// <param name="bind">The type of the converter that implements IBindConvert.</param>
        public ConvertFromStringAttribute(Type bind)
        {
            if (!typeof(IBindConvert).IsAssignableFrom(bind))
            {
                throw new ArgumentException($"The type {bind.FullName} must implement IBindConvert.");
            }

            // Create an instance of the converter
            IBindConvert = (IBindConvert)Activator.CreateInstance(bind)!
                ?? throw new Exception($"Failed to create an instance of {bind.FullName}.");
        }
    }
}
