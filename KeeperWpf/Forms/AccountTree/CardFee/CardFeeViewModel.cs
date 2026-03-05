using Caliburn.Micro;
using KeeperDomain;
using System.Collections.Generic;
using System;
using System.Linq;
using KeeperModels;
using KeeperInfrastructure;

namespace KeeperWpf;

public class CardFeeViewModel(KeeperDataModel dataModel, ShellPartsBinder shellPartsBinder, 
    TransactionsRepository transactionsRepository) : Screen
{
    public string BankLine { get; set; } = null!;
    public string CardLine { get; set; } = null!;
    public string CardCurrency { get; set; } = null!;

    public decimal Amount { get; set; }
    public DatePickerWithTrianglesVm MyDatePickerVm { get; set; } = null!;
    public string Comment { get; set; } = "";

    private AccountItemModel _card = null!;
    private AccountItemModel _bank = null!;

    protected override void OnViewLoaded(object view)
    {
        DisplayName = "Списана комиссия";
    }

    public void Initialize(AccountItemModel card)
    {
        _card = card;
        _bank = dataModel.AcMoDict[card.BankAccount!.BankId];
        BankLine = $"Банк \"{_bank.Name}\" списал комиссию";
        CardLine = $"с карты \"{card.Name}\"";
        CardCurrency = _card.BankAccount!.MainCurrency.ToString().ToUpper();
        MyDatePickerVm = new DatePickerWithTrianglesVm() { SelectedDate = DateTime.Today };
    }

    public async void Save()
    {
        var id = dataModel.Transactions.Keys.Max() + 1;
        var thisDateTrans = dataModel.Transactions.Values
            .Where(t => t.Timestamp.Date == MyDatePickerVm.SelectedDate)
            .OrderBy(l => l.Timestamp)
            .LastOrDefault();
        var timestamp = thisDateTrans?.Timestamp ?? MyDatePickerVm.SelectedDate;
        var tranModel1 = new TransactionModel()
        {
            Id = id,
            Timestamp = timestamp.AddMinutes(1),
            Operation = OperationType.Расход,
            MyAccount = _card,
            Counterparty = _bank,
            Category = dataModel.CardFeeCategory(),
            Amount = Amount,
            Currency = _card.BankAccount!.MainCurrency,
            Tags = new List<AccountItemModel>(),
            Comment = Comment,
        };
        dataModel.Transactions.Add(tranModel1.Id, tranModel1);
        await transactionsRepository.AddTransactions(new List<TransactionModel>() { tranModel1 });

        shellPartsBinder.JustToForceBalanceRecalculation = DateTime.Now;

        await TryCloseAsync();
    }

    public async void Cancel()
    {
        await TryCloseAsync();
    }
}
