using System.Collections.Generic;
using Caliburn.Micro;

namespace KeeperWpf;

public class CategoriesMoneyBackControlModel : PropertyChangedBase
{
    public BindableCollection<MoneyBackSpecificCategoryModel> Categories { get; set; }
        = new BindableCollection<MoneyBackSpecificCategoryModel>();

    public void SwitchModel(List<MoneyBackSpecificCategoryModel>? categories)
    {
        Categories = new BindableCollection<MoneyBackSpecificCategoryModel>(
            categories ?? new List<MoneyBackSpecificCategoryModel>());
        NotifyOfPropertyChange(nameof(Categories));
    }
}
