namespace Backend.Core.Models.CustomAttributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class EncryptProperty : Attribute
{
}

[AttributeUsage(AttributeTargets.Property)]
public sealed class HashProperty : Attribute
{
}