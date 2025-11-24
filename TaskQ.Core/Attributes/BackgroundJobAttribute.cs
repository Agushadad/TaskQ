namespace TaskQ.Core.Attributes
{
    public class BackgroundJobAttribute : Attribute
    {
        public string Name { get; }
        public BackgroundJobAttribute(string name)
        {
            Name = name;
        }
    }
}
