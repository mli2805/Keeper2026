using System.Collections.Generic;
using Caliburn.Micro;

namespace KeeperWpf;

public class PartnersMoneyBackControlModel : PropertyChangedBase
{
    public BindableCollection<MoneyBackPartnerModel> Partners { get; set; }
        = new BindableCollection<MoneyBackPartnerModel>();

    public void SwitchModel(List<MoneyBackPartnerModel>? partners)
    {
        Partners = new BindableCollection<MoneyBackPartnerModel>(
            partners ?? new List<MoneyBackPartnerModel>());
        NotifyOfPropertyChange(nameof(Partners));
    }
}
