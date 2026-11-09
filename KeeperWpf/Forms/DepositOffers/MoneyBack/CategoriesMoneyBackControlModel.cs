using System.Collections.Generic;
using Caliburn.Micro;

namespace KeeperWpf;

public class CategoriesMoneyBackControlModel : PropertyChangedBase
{
    public BindableCollection<MoneyBackCategoryModel> Categories { get; set; }
        = new BindableCollection<MoneyBackCategoryModel>();

    public void SwitchModel(List<MoneyBackCategoryModel>? categories)
    {
        Categories = new BindableCollection<MoneyBackCategoryModel>(
            categories ?? new List<MoneyBackCategoryModel>());
        NotifyOfPropertyChange(nameof(Categories));
    }
}
