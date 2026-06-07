using Backend.Core.Models.Interfaces;

namespace Backend.Core.Models.Results;

public static class Extensions
{
    public static void ToDto<T>(this List<T> list)
    {
        foreach (var item in list)
            if (item is IDtoConvertible<T> dto)
                dto.ToDto();
    }
}