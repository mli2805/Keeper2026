using MigraDoc.DocumentObjectModel.Tables;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeeperWpf;

public class HouseholdPaymentsFactory(KeeperDataModel dataModel)
{
    private UtilityMetersModel? GetUtilityMetersReadings()
    {
        var now = DateTime.Now;
        var lastReadings = dataModel.UtilityMeters.Last();
        var isCurrentMonth = lastReadings.ReadingsDate.Year == now.Year
            && lastReadings.ReadingsDate.Month == now.Month;
        return isCurrentMonth ?  lastReadings : null;
    }

    public List<HouseholdPaymentTemplate>? Create()
    {
        var thisMonthReadings = GetUtilityMetersReadings();
        if (thisMonthReadings == null)
        {
            return null;
        }

        var result = new List<HouseholdPaymentTemplate>();

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "АСБ"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Погашение кредита ЖСК"),
            new List<AccountItemModel>()
            ));

        var cold = thisMonthReadings.ColdWater;
        var hot = thisMonthReadings.HotWater;
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "ЖЭС"),
            dataModel.AcMoDict.Values.First(a => a.Name == "По жировке"),
            new List<AccountItemModel>(),
            $"ХВ: {cold.To} - {cold.From} = {cold.Volume} куб м; " +
            $"ГВ: {hot.To} - {hot.From} = {hot.Volume} куб м"   
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Белтелеком"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Дом телефон"),
            new List<AccountItemModel>()
            ));

        var el = thisMonthReadings.Electricity;
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Энергосбыт"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Электричество"),
            new List<AccountItemModel>()
            {
                dataModel.AcMoDict.Values.First(a => a.Name == "Электроэнергия"),
            },
            $"{el.To} - {el.From} = {el.Volume} кВт"
            ));

        var gas = thisMonthReadings.Gas;
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Газовщики"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Газ"),
            new List<AccountItemModel>()
            {
                dataModel.AcMoDict[1082],
            },
            $"{gas.To} - {gas.From} = {gas.Volume} куб м"
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "МТС"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Интернет"),
            new List<AccountItemModel>()
            ));

        // Дача
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Воложинский ЖКХ"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Дача-Вывоз мусора"),
            new List<AccountItemModel>()
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Энергосбыт"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Дача-Электричество"),
            new List<AccountItemModel>()
            {
                dataModel.AcMoDict.Values.First(a => a.Name == "Электроэнергия"),
            }
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Белтелеком"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Дача-Zala"),
            new List<AccountItemModel>()
            ));

        // Другое
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Велком"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Прочи расходы"),
            new List<AccountItemModel>(),
            "мой"
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Велком"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Прочи расходы"),
            new List<AccountItemModel>(),
            "Юля"
            ));

        return result;
    }
}

