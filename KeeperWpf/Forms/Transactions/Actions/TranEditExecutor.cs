using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Caliburn.Micro;
using KeeperInfrastructure;
using MigraDoc.DocumentObjectModel.Tables;

namespace KeeperWpf;

[ExportViewModel(ViewModelLifetime.SingleInstance)]
public class TranEditExecutor(TranModel model, IWindowManager windowManager,
    KeeperDataModel dataModel, OneTranViewModel oneTranViewModel,
    TransactionsRepository transactionsRepository, FuellingsRepository fuellingsRepository,
    AskReceiptDeletionViewModel askReceiptDeletionViewModel)
{
    public async Task EditSelected()
    {
        var selectedTran = model.SelectedTranWrappedForDataGrid.Tran;
        var selectedTranId = selectedTran.Id;

        // форма редактирования не должна изменять их транзакцию,
        // поэтому внутри Clone(), Id и Receipt не копируются
        oneTranViewModel.Init(selectedTran, false);
        bool? result = await windowManager.ShowDialogAsync(oneTranViewModel);

        if (!result.HasValue || !result.Value) return;

        oneTranViewModel.TranInWork.CopyInto(selectedTran);
        selectedTran.Id = selectedTranId;

        await transactionsRepository.UpdateTransaction(selectedTran);
        dataModel.Transactions.Remove(selectedTran.Id);
        dataModel.Transactions.Add(selectedTran.Id, selectedTran);

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
            await AddReceiptTransactions(oneTranViewModel);
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
            await AddOneTran(oneTranViewModel.TranInWork.Clone());

        if (oneTranViewModel.IsOneMore)
            await AddAfterSelected();

        model.IsCollectionChanged = true;
    }

    private async Task AddOneTran(TransactionModel tran)
    {
        await ShiftTransactionsAfterInserted(tran, 1);

        var tr = await transactionsRepository.AddTransaction(tran, dataModel.AcMoDict);
        dataModel.Transactions.Add(tr.Id, tr);
        var tranWrappedForDatagrid = new TranWrappedForDataGrid(tran);
        model.Rows.Add(tranWrappedForDatagrid);
        model.SelectedTranWrappedForDataGrid = tranWrappedForDatagrid;
    }

    private async Task ShiftTransactionsAfterInserted(TransactionModel tran, int shift = 1)
    {
        var wrappedTransactionsAfterInserted =
            model.Rows.Where(t => t.Tran.Timestamp.Date == tran.Timestamp.Date && t.Tran.Timestamp >= tran.Timestamp).ToList();
        foreach (var wrapped in wrappedTransactionsAfterInserted)
        {
            wrapped.Tran.Timestamp = wrapped.Tran.Timestamp.AddMinutes(shift);
            dataModel.Transactions[wrapped.Tran.Id].Timestamp = wrapped.Tran.Timestamp;
            await transactionsRepository.UpdateTransaction(wrapped.Tran);
        }
    }

    private int GetNextReceiptId(DateTime date)
    {
        var sameDayTransactions = dataModel.Transactions.Values
            .Where(t => t.Timestamp.Date == date.Date).ToList();
        return sameDayTransactions.Any() ? sameDayTransactions.Max(r => r.Receipt) + 1 : 1;
    }

    private IEnumerable<TransactionModel> ToTransactionModelList(
        TransactionModel tranInWork, int receiptId, List<Tuple<decimal, AccountItemModel, string>> tuples)
    {
        var timestamp = tranInWork.Timestamp;
        foreach (var tuple in tuples)
        {
            var tran = tranInWork.Clone();
            tran.Timestamp = timestamp;
            tran.Receipt = receiptId;
            tran.Amount = tuple.Item1;
            tran.Category = tuple.Item2;
            tran.Comment = tuple.Item3;
            yield return tran;

            timestamp = timestamp.AddMinutes(1);
        }
    }

    private async Task AddReceiptTransactions(OneTranViewModel oneTranForm)
    {
        var oneTran = oneTranForm.TranInWork;
        var receiptId = GetNextReceiptId(oneTran.Timestamp);
        var list = ToTransactionModelList(oneTran, receiptId, oneTranForm.ReceiptList!).ToList();

        await ShiftTransactionsAfterInserted(list.First(), list.Count);
        var addedTransactions = await transactionsRepository.AddTransactions(list, dataModel.AcMoDict);
        addedTransactions.ForEach(t => dataModel.Transactions.Add(t.Id, t));
        addedTransactions.ForEach(t => model.Rows.Add(new TranWrappedForDataGrid(t)));
        model.SelectedTranWrappedForDataGrid = 
            model.Rows.First(w => w.Tran.Id == addedTransactions.Last().Id);
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
