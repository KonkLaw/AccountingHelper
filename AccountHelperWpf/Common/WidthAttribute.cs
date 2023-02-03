namespace AccountHelperWpf.Common;

[AttributeUsage(AttributeTargets.Property)]
public class WidthAttribute : Attribute
{
    public double Width { get; set; }

    public WidthAttribute(double width) => Width = width;
}