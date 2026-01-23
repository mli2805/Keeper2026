using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperInfrastructure;
using KeeperModels;

namespace KeeperWpf;

public class TranEditExecutor(TranModel model, IWindowManager windowManager,
    KeeperDataModel dataModel, OneTranViewModel oneTranViewModel, 
    TransactionsRepository transactionsRepository, FuellingsRepository fuellingsRepository,
    AskReceiptDeletionViewModel askReceiptDeletionViewModel)
{
    public async Task EditSelected()
    {
        var selectedTran = model.SelectedTranWrappedForDataGrid.Tran;

        oneTranViewModel.Init(selectedTran, false);
        bool? result = await windowManager.ShowDialogAsync(oneTranViewModel);

        if (!result.HasValue || !result.Value) return;

        oneTranViewModel.GetTran().CopyInto(selectedTran);

        dataModel.Transactions.Remove(selectedTran.Id);
        dataModel.Transactions.Add(selectedTran.Id, selectedTran);
        await transactionsRepository.UpdateTransaction(selectedTran);

        model.SortedRows.Refresh();
        model.IsCollectionChanged = true;
    }

    public async Task AddAfterSelected()
    {
        var tranForAdding = PrepareTranForAdding();
        oneTranViewModel.Init(tranForAdding, true);
        bool? result = await windowManager.ShowDialogAsync(oneTranViewModel);

        if (!result.HasValue || !result.Value) return;

        if (oneTranViewModel.ReceiptList != null)
            await AddOneTranAndReceipt(oneTranViewModel);
        else if (oneTranViewModel.FuellingTran != null)
        {
            var transactionModel = oneTranViewModel.FuellingTran.Clone();
            transactionModel.MyAccount = oneTranViewModel.TranInWork.MyAccount;
            await AddOneTran(transactionModel);

            if (oneTranViewModel.FuellingModel != null)
            {
                var fm = oneTranViewModel.FuellingModel.Clone();
                fm.Transaction = transactionModel;
                fm.Comment = transactionModel.Comment;
                dataModel.FuellingVms.Add(fm);
                await fuellingsRepository.AddFuelling(fm);
            }
        }
        else
            await AddOneTran(oneTranViewModel.GetTran().Clone());

        if (oneTranViewModel.IsOneMore)
            await AddAfterSelected();

        model.IsCollectionChanged = true;
    }

    private async Task AddOneTran(TransactionModel tran)
    {
        tran.Id = dataModel.Transactions.Keys.Max() + 1;

        var wrappedTransactionsAfterInserted =
            model.Rows.Where(t => t.Tran.Timestamp.Date == tran.Timestamp.Date && t.Tran.Timestamp >= tran.Timestamp).ToList();
        foreach (var wrapped in wrappedTransactionsAfterInserted)
        {
            wrapped.Tran.Timestamp = wrapped.Tran.Timestamp.AddMinutes(1);
            dataModel.Transactions[wrapped.Tran.Id].Timestamp = wrapped.Tran.Timestamp;
            await transactionsRepository.UpdateTransaction(wrapped.Tran);
        }

        var tranWrappedForDatagrid = new TranWrappedForDataGrid(tran);
        model.Rows.Add(tranWrappedForDatagrid);
        model.SelectedTranWrappedForDataGrid = tranWrappedForDatagrid;

        dataModel.Transactions.Add(tran.Id, tran);
        await transactionsRepository.AddTransactions(new List<TransactionModel> { tran });
    }

    private async Task AddOneTranAndReceipt(OneTranViewModel oneTranForm)
    {
        var oneTran = oneTranForm.GetTran();
        var sameDayTransactions = dataModel.Transactions.Values.Where(t => t.Timestamp.Date == oneTran.Timestamp.Date).ToList();
        var receiptId = sameDayTransactions.Any() ? sameDayTransactions.Max(r => r.Receipt) + 1 : 1;
        foreach (var tuple in oneTranForm.ReceiptList!)
        {
            var tran = oneTran.Clone();
            tran.Receipt = receiptId;
            tran.Amount = tuple.Item1;
            tran.Category = tuple.Item2;
            tran.Comment = tuple.Item3;
            await AddOneTran(tran);
        }
    }

    private TransactionModel PrepareTranForAdding()
    {
        var tranForAdding = model.SelectedTranWrappedForDataGrid.Tran.Clone();
        tranForAdding.Timestamp = tranForAdding.Timestamp.AddMinutes(1);
        tranForAdding.Amount = 0;
        tranForAdding.AmountInReturn = 0;
        tranForAdding.Comment = "";
        return tranForAdding;
    }

    public async Task DeleteSelected()
    {
        if (model.SelectedTranWrappedForDataGrid.Tran.Receipt != 0)
        {
            await windowManager.ShowDialogAsync(askReceiptDeletionViewModel);
            if (askReceiptDeletionViewModel.Result == 0)
                return;
            if (askReceiptDeletionViewModel.Result == 1)
                await DeleteOneTransaction();
            else
                await DeleteWholeReceipt();
        }
        else
            await DeleteOneTransaction();
        model.IsCollectionChanged = true;
    }

    private async Task DeleteOneTransaction()
    {
        var trId = model.SelectedTranWrappedForDataGrid.Tran.Id;
        var wrappedTrans = new List<TranWrappedForDataGrid>() { model.SelectedTranWrappedForDataGrid };
        await Delete(wrappedTrans);
        var fuellingModel = dataModel.FuellingVms.FirstOrDefault(f => f.Transaction.Id == trId);
        if (fuellingModel != null)
        {
            dataModel.FuellingVms.Remove(fuellingModel);
            await fuellingsRepository.DeleteFuelling(fuellingModel.Id);
        }
    }

    private async Task DeleteWholeReceipt()
    {
        var wrappedTrans = model.Rows.Where(t =>
            t.Tran.Timestamp.Date == model.SelectedTranWrappedForDataGrid.Tran.Timestamp.Date
            && t.Tran.Receipt == model.SelectedTranWrappedForDataGrid.Tran.Receipt).ToList();

        await Delete(wrappedTrans);
    }

    private async Task Delete(List<TranWrappedForDataGrid> wrappedTrans)
    {
        int n = model.Rows.IndexOf(wrappedTrans.First());
        foreach (var wrappedTran in wrappedTrans)
        {
            dataModel.Transactions.Remove(wrappedTran.Tran.Id);
            model.Rows.Remove(wrappedTran);
        }
        await transactionsRepository.DeleteTransactions(wrappedTrans.Select(t => t.Tran.Id).ToList());

        if (n >= model.Rows.Count)
            n = model.Rows.Count - 1;
        model.SelectedTranWrappedForDataGrid = model.Rows.ElementAt(n);
    }
}
