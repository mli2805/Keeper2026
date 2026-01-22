using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using KeeperDomain;
using Newtonsoft.Json;

namespace KeeperWpf;

public static class ExchangeRatesFetcher
{
    public static async Task<List<ExchangeRates>?> Get(string bankTitle, int days)
    {
        var uri = $"http://192.168.96.19:11082/bali/get-some-last-days-for-bank?bankTitle={bankTitle}&days={days}";

        try
        {
            var response = await MyRequest.GetResponseAsync(uri);
            if (!response.Success)
                return null;
            var lines = (List<KomBankRatesLine>?)JsonConvert.DeserializeObject(response.Content, typeof(List<KomBankRatesLine>));
            return lines?.Select(l => l.ToExchangeRates()).ToList() ?? null;
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return null;
        }
    }

    public static List<ExchangeRates> SelectMiddayRates(List<ExchangeRates> bnb, DateTime date)
    {
        var id = 0;
        var prev = bnb[0];
        date = date.Date.AddHours(12);

        var bnbD = new List<ExchangeRates>();
        foreach (var line in bnb)
        {
            while (line.Date > date)
            {
                if (prev.Date.Day != date.Day)
                    prev.Date = date.Date;
                var item = prev.Clone();
                item.Id = ++id;
                item.Date = item.Date.Date; // очистить часы минуты
                bnbD.Add(item);
                date = date.AddDays(1);
            }

            prev = line;

        }

        return bnbD;
    }
}
