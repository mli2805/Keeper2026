using System;
using System.Linq;

namespace KeeperModels;

public static class UtilityMetersExt
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="dataModel"></param>
    /// <returns>если в бд уже есть данные для текущего месяца, то вернет их, иначе создаст новый инстанс</returns>
    public static UtilityMetersModel GetLastOrNew(this KeeperDataModel dataModel)
    {
        UtilityMetersModel lastReadings = dataModel.UtilityMeters.Last();
        var now = DateTime.Now;
        bool isCurrentMonth = lastReadings.ReadingsDate.Year == now.Year
            && lastReadings.ReadingsDate.Month == now.Month;

        UtilityMetersModel monthReadings;
        if (isCurrentMonth)
        {
            monthReadings = lastReadings;
        }
        else
        {
            monthReadings = new UtilityMetersModel
            {
                ReadingsDate = now,
            };
            monthReadings.ColdWater.From = lastReadings.ColdWater.To;
            monthReadings.ColdWater.To = lastReadings.ColdWater.To;
            monthReadings.HotWater.From = lastReadings.HotWater.To;
            monthReadings.HotWater.To = lastReadings.HotWater.To;
            monthReadings.Electricity.From = lastReadings.Electricity.To;
            monthReadings.Electricity.To = lastReadings.Electricity.To;
            monthReadings.Electricity.Tariff = lastReadings.Electricity.Tariff;
            monthReadings.Gas.From = lastReadings.Gas.To;
            monthReadings.Gas.To = lastReadings.Gas.To;
            monthReadings.Gas.Tariff = lastReadings.Gas.Tariff;
        }
        return monthReadings;
    }
}
