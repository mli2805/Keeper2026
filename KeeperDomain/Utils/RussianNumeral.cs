namespace KeeperDomain;

public static class RussianNumeral
{
    public static string DaysNumber(this int number)
    {
        switch (NumberOption(number))
        {
            case 1: return "день";
            case 2: return "дня";
            default:
                return "дней";
        }
    }

    public static string MonthsNumber(this int number)
    {
        switch (NumberOption(number))
        {
            case 1: return "месяц";
            case 2: return "месяца";
            default:
                return "месяцев";
        }
    }

    public static string YearsNumber(this int number)
    {
        switch (NumberOption(number))
        {
            case 1: return "год";
            case 2: return "года";
            default:
                return "лет";
        }
    }

    public static string HoursNumber(this int number)
    {
        switch (NumberOption(number))
        {
            case 1: return "час";
            case 2: return "часа";
            default:
                return "часов";
        }
    }

    public static string MinutesNumber(this int number)
    {
        switch (NumberOption(number))
        {
            case 1: return "минута";
            case 2: return "минуты";
            default:
                return "минут";
        }
    }

    public static string SecondsNumber(this int number)
    {
        switch (NumberOption(number))
        {
            case 1: return "секунда";
            case 2: return "секунды";
            default:
                return "секунд";
        }
    }

    private static int NumberOption(int number)
    {
        if (number > 10 && number < 14) return 3;
        if (number % 10 == 1) return 1;
        if (number % 10 > 1 && number % 10 < 5) return 2;
        return 3;
    }
}
