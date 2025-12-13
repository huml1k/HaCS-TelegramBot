using HaCSBot.DataBase.Enums;

public static class EnumExtensions
{
    public static string GetDisplayName(this StreetsType type) => type switch
    {
        StreetsType.Улица => "ул.",
        StreetsType.Проспект => "пр-кт",
        StreetsType.Переулок => "пер.",
        StreetsType.Бульвар => "б-р",
        StreetsType.Площадь => "пл.",
        StreetsType.Проезд => "проезд",
        StreetsType.Шоссе => "ш.",
        _ => type.ToString()
    };
}
