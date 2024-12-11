namespace DIL.Attributes
{
    /// <summary>
    /// Specifies that a parameter value should be dynamically populated using a Regex group match.
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class FromRegexIndexAttribute : Attribute
    {
        public int Index { get; }

        public FromRegexIndexAttribute(int index)
        {
            Index = index;
        }
    }
}
