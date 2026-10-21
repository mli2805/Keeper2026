using System.Collections.Generic;
using System.Linq;

namespace KeeperWpf;

public class HouseholdPaymentsFactory(KeeperDataModel dataModel)
{
    public List<HouseholdPaymentTemplate> Create()
    {
        var result = new List<HouseholdPaymentTemplate>();

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "АСБ"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Погашение кредита ЖСК"),
            new List<AccountItemModel>()
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "ЖЭС"),
            dataModel.AcMoDict.Values.First(a => a.Name == "По жировке"),
            new List<AccountItemModel>()
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Белтелеком"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Дом телефон"),
            new List<AccountItemModel>()
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Энергосбыт"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Электричество"),
            new List<AccountItemModel>()
            {
                dataModel.AcMoDict.Values.First(a => a.Name == "Электроэнергия"),
            }
            ));

        result.Add(new HouseholdPaymentTemplate(
            dataModel.AcMoDict.Values.First(a => a.Name == "Газовщики"),
            dataModel.AcMoDict.Values.First(a => a.Name == "Газ"),
            new List<AccountItemModel>()
            {
                dataModel.AcMoDict[1082],
            }
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

