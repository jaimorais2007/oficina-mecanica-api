using System.Text.RegularExpressions;
using OficinaApi.Domain.Exceptions;

namespace OficinaApi.Domain.ValueObjects;

public class Plate
{
    public string Value { get; private set; } = string.Empty;

    protected Plate() { }

    public Plate(string value)
    {
        if (!IsValid(value))
            throw new DomainException("Placa inválida");

        Value = value.ToUpper();
    }

    private bool IsValid(string plate)
    {
        if (string.IsNullOrWhiteSpace(plate))
            return false;

        plate = plate.ToUpper();

        var patternOld = @"^[A-Z]{3}[0-9]{4}$";
        var patternNew = @"^[A-Z]{3}[0-9][A-Z][0-9]{2}$";

        return Regex.IsMatch(plate, patternOld) ||
               Regex.IsMatch(plate, patternNew);
    }

    public override string ToString() => Value;
}
