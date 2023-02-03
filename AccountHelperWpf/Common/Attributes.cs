namespace AccountHelperWpf.Common;

[AttributeUsage(AttributeTargets.Property)]
public class WidthAttribute : Attribute
{
    public double Width { get; set; }

    public WidthAttribute(double width) => Width = width;
}

[AttributeUsage(AttributeTargets.Property)]
public class StringFormatAttribute : Attribute
{
    public string Format { get; set; }

    public StringFormatAttribute(string format) => Format = format;
}