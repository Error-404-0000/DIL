namespace DIL.Attributes
{
    /// <summary>
    /// Specifies a regular expression pattern that this method should match to be invoked.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class RegexUseAttribute : Attribute
    {
        public string Pattern { get; }

        public RegexUseAttribute(string pattern)
        {
            Pattern = pattern;
        }
    }
}
