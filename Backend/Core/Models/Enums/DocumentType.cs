namespace Backend.Core.Models.Enums;

/// <summary>
/// Types of identification documents commonly used in Colombia.
/// </summary>
public enum DocumentType
{
    /// <summary>
    /// CC - Cédula de ciudadanía: national identity card for Colombian citizens (adults).
    /// </summary>
    CC = 1,

    /// <summary>
    /// CE - Cédula de extranjería: identification card issued to foreigners residing in Colombia.
    /// </summary>
    CE = 2,

    /// <summary>
    /// TI - Tarjeta de identidad: identity card issued to minors (usually under 18) in Colombia.
    /// </summary>
    TI = 3,

    /// <summary>
    /// NIT - Número de Identificación Tributaria: tax identification number for businesses or individuals for tax purposes.
    /// </summary>
    NIT = 4,

    /// <summary>
    /// OTRO - Other: any other type of document not covered by the previous values.
    /// </summary>
    OTRO = 5
}