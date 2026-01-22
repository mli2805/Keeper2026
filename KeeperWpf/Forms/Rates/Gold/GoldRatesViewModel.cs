using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperDomain;
using KeeperInfrastructure;

namespace KeeperWpf;

public class GoldRatesViewModel : PropertyChangedBase
{
    private readonly KeeperDataModel _keeperDataModel;
    private readonly MetalRatesRepository _metalRatesRepository;

    private List<GoldCoinsModel> _rows = null!;
    public List<GoldCoinsModel> Rows
    {
        get => _rows;
        set
        {
            if (Equals(value, _rows)) return;
            _rows = value;
            NotifyOfPropertyChange();
        }
    }

    public GoldRatesViewModel(KeeperDataModel keeperDataModel, MetalRatesRepository metalRatesRepository)
    {
        _keeperDataModel = keeperDataModel;
        _metalRatesRepository = metalRatesRepository;
    }

    public void Initialize()
    {
        Rows = _keeperDataModel.MetalRates
            .Where(m => m.Metal == Metal.Gold && m.Proba == 900)
            .Select(m => new GoldCoinsModel()
            {
                Id = m.Id,
                Date = m.Date,
                MinfinGold900Rate = m.Price,
                BynUsd = _keeperDataModel.GetRate(m.Date, CurrencyCode.BYN).Value
            })
            .ToList();
    }


    public async Task Recount()
    {
        await Save(); // сохраняет ВСЕ строки таблицы на экране в модель данных
        Initialize(); // заново загружает в таблицу на экране из модели данных, 
    }

    private async Task Save()
    {
        _keeperDataModel.MetalRates = Rows.Select(l => new MetalRate()
        {
            Id = l.Id,
            Date = l.Date,
            Metal = Metal.Gold,
            Proba = 900,
            Price = l.MinfinGold900Rate
        }).ToList();

        await _metalRatesRepository.UpdateWholeList(_keeperDataModel.MetalRates);
    }
}
