namespace KeeperDomain;

public enum Durations
{
    Years, Months, Days, Hours, Minutes, Seconds,
}

[Serializable]
public class Duration
{
    public bool IsPerpetual { get; set; } // term-less
    public int Value { get; set; }
    public Durations Scale { get; set; }

    public Duration()
    {
        IsPerpetual = true;
    }

    public Duration(int value, Durations scale)
    {
        Value = value;
        Scale = scale;
        IsPerpetual = false;
    }

    // Duration or Repetition
    public string Dump(bool isDuration = true)
    {
        if (IsPerpetual)
        {
            return isDuration ? "Perpetual" : "Once";
        }
        else
        {
            return Value + "-" + Scale;
        }
    }

    public string ToRussianString(bool isDuration = true)
    {
        if (IsPerpetual)
        {
            return isDuration ? "Бессрочно" : "Однократно";
        }
        else
        {
            string scaleStr = Scale switch
            {
                Durations.Years => Value.YearsNumber(),
                Durations.Months => Value.MonthsNumber(),
                Durations.Days => Value.DaysNumber(),
                Durations.Hours => Value.HoursNumber(),
                Durations.Minutes => Value.MinutesNumber(),
                Durations.Seconds => Value.SecondsNumber(),
                _ => Scale.ToString()
            };
            return $"{Value} {scaleStr}";
        }
    }

    
    public Duration FromString(string s)
    {
        if (string.IsNullOrEmpty(s))
        {
            return new Duration();
        }

        if (s.Equals("Perpetual", StringComparison.OrdinalIgnoreCase) || s.Equals("Once", StringComparison.OrdinalIgnoreCase))
        {
            IsPerpetual = true;
            return this;
        }

        var ss = s.Split(new[] { '-' }, StringSplitOptions.RemoveEmptyEntries);
        IsPerpetual = false;
        Value = int.Parse(ss[0]);
        Scale = (Durations)Enum.Parse(typeof(Durations), ss[1]);
        return this;
    }

    public Duration Clone()
    {
        return (Duration)MemberwiseClone();
    }
}

