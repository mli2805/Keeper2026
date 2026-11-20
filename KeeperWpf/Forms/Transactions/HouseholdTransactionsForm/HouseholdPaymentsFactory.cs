using System;
using System.Collections.Generic;
using System.Linq;

namespace KeeperWpf;

public class HouseholdPaymentsFactory(KeeperDataModel dataModel)
{
    public List<HouseholdPaymentTemplate> Create(
        UtilityMetersModel utilityMetersModel, List<TransactionModel> previousPayments, decimal totalAccrued)
    {
        var thisMonthReadings = utilityMetersModel;

        var result = new List<HouseholdPaymentTemplate>();

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[241], // "АСБ"
            dataModel.AcMoDict[285], // "Погашение кредита ЖСК"
            0,
            9.0m
            ));

        var cold = thisMonthReadings.ColdWater;
        var hot = thisMonthReadings.HotWater;
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[266], // "ЖЭС"
            dataModel.AcMoDict[278], // "По жировке"
            totalAccrued, // из новой жировки
            totalAccrued, // из новой жировки
            new List<AccountItemModel>(),
            $"ХВ: {cold.To} - {cold.From} = {cold.Volume} куб м; " +
            $"ГВ: {hot.To} - {hot.From} = {hot.Volume} куб м"
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[269], // "Белтелеком"
            dataModel.AcMoDict[279], // "Дом телефон"
            previousPayments.First(t => t.Category != null && t.Category.Id == 279)?.Amount ?? 0m // по прошлому месяцу
            ));

        var el = thisMonthReadings.Electricity;
        decimal elAmountForecast; 
        if (el.Volume == 0) // еще не ввели показания за текущий месяц, тогда берем сумму по прошлому месяцу
        {
            var elPrev = previousPayments.First(t => t.Category != null && t.Category.Id == 280);
            elAmountForecast = elPrev?.Amount ?? 0m;
        }
        else
        {
            elAmountForecast = Math.Round((decimal)(el.Volume * el.Tariff), 2);
        }
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[267], // "Энергосбыт"
            dataModel.AcMoDict[280], // "Электричество"
            Math.Round((decimal)(el.Volume * el.Tariff), 2), // это тоже не факт, т.к. тариф может измениться,
                                                             // он будет вычислен, когда введут оплату
            elAmountForecast,
            new List<AccountItemModel>()
            {
                dataModel.AcMoDict[1081], // "Электроэнергия"
            },
            $"{el.To} - {el.From} = {el.Volume} кВт"
            ));

        var gas = thisMonthReadings.Gas;
        decimal gasAmountForecast;
        if (gas.Volume == 0) // еще не ввели показания за текущий месяц, тогда берем сумму по прошлому месяцу
        {
            var gasPrev = previousPayments.First(t => t.Category != null && t.Category.Id == 281);
            gasAmountForecast = gasPrev?.Amount ?? 0m;
        }
        else
        {
            gasAmountForecast = Math.Round((decimal)(gas.Volume * gas.Tariff), 2);
        }
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[268], // "Газовщики"
            dataModel.AcMoDict[281], // "Газ"
            Math.Round((decimal)(gas.Volume * gas.Tariff), 2), // это тоже не факт, т.к. тариф может измениться,
                                                               // он будет вычислен, когда введут оплату
            gasAmountForecast,
            new List<AccountItemModel>()
            {
                dataModel.AcMoDict[1082],
            },
            $"{gas.To} - {gas.From} = {gas.Volume} куб м"
            ));

        
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[584], // "МТС"
            dataModel.AcMoDict[282], // "Интернет"
            previousPayments.First(t => t.Category != null && t.Category.Id == 282)?.Amount ?? 0m // по прошлому месяцу
            ));

        // Дача
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[600], // "Воложинский ЖКХ"
            dataModel.AcMoDict[599], // "Дача-Вывоз мусора"
            previousPayments.First(t => t.Category != null && t.Category.Id == 599)?.Amount ?? 0m // по прошлому месяцу
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[267], // "Энергосбыт"
            dataModel.AcMoDict[364], // "Дача-Электричество"
            0,
            0,
            new List<AccountItemModel>()
            {
                dataModel.AcMoDict[1081], // "Электроэнергия"
            }
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[269], // "Белтелеком"
            dataModel.AcMoDict[519], // "Дача-Zala"
            previousPayments.First(t => t.Category != null && t.Category.Id == 519)?.Amount ?? 0m // по прошлому месяцу
            ));

        // Другое

        decimal prevMyVelcom = previousPayments // по прошлому месяцу
            .First(t => t.Counterparty != null && t.Counterparty.Id == 270 && t.Comment == "мой")?.Amount ?? 0m;
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[270], // "Велком"
            dataModel.AcMoDict[256], // "Прочи расходы"
            prevMyVelcom, 
            prevMyVelcom, 
            new List<AccountItemModel>(),
            "мой"
            ));

        decimal prevJuliaVelcom = previousPayments // по прошлому месяцу
            .First(t => t.Counterparty != null && t.Counterparty.Id == 270 && t.Comment == "Юля")?.Amount ?? 0m;
        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict[270], // "Велком"
            dataModel.AcMoDict[256], // "Прочи расходы"
            prevJuliaVelcom, 
            prevJuliaVelcom, 
            new List<AccountItemModel>(),
            "Юля"
            ));

        return result;
    }
}

