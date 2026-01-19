using KeeperDomain;
using KeeperModels;
using OxyPlot;
using OxyPlot.Axes;
using OxyPlot.Series;
using System;
using System.Collections.Generic;
using System.Linq;

namespace KeeperWpf;

public class BalanceAndSaldoPoints
{
    public List<DataPoint> DailyBalancesPoints = new List<DataPoint>();
    public List<BarItem> MonthlySaldoItems = new List<BarItem>();
    public List<BarItem> AnnualSaldoItems = new List<BarItem>();
}

public class BalanceAndSaldoPointsCollector
{
    private Balance _balance = new Balance();
    private int _monthIndex;
    private int _yearIndex;
    private double _previousMonth;
    private double _previousYear;
    private double _balanceInUsd;
    private DateTime _currentDate = new DateTime(2001, 12, 31);
    private DateTime _previousDate;

    private BalanceAndSaldoPoints _points = new BalanceAndSaldoPoints();

    public BalanceAndSaldoPoints CollectPointsForThreeSeries(KeeperDataModel dataModel)
    {
        foreach (var tran in dataModel.Transactions.Values.OrderBy(t => t.Timestamp))
        {
            if (!tran.Timestamp.Date.Equals(_currentDate))
            {
                RegisterDay(tran, dataModel);
            }
            RegisterTran(tran);
        }
        RegisterDay(null, dataModel);

        return _points;
    }

    private void RegisterDay(TransactionModel? tran, KeeperDataModel dataModel)
    {
        _balanceInUsd = (double)dataModel.BalanceInUsd(_currentDate, _balance);
        _points.DailyBalancesPoints.Add(new DataPoint(DateTimeAxis.ToDouble(_currentDate), _balanceInUsd));

        _previousDate = _currentDate;
        _currentDate = tran?.Timestamp.Date ?? _currentDate.AddMonths(13);

        if (_currentDate.Month != _previousDate.Month)
        {
            if (_previousDate.Year != 2001)
            {
                //MonthLabels.Add(_previousDate.ToString("MM/yyyy"));
                _points.MonthlySaldoItems.Add(new BarItem(_balanceInUsd - _previousMonth, _monthIndex));
                _monthIndex++;
            }

            _previousMonth = _balanceInUsd;
        }

        if (_currentDate.Year != _previousDate.Year)
        {
            if (_previousDate.Year != 2001)
            {
                //YearLabels.Add(_previousDate.ToString("yyyy"));
                _points.AnnualSaldoItems.Add(new BarItem(_balanceInUsd - _previousYear, _yearIndex));
                _yearIndex++;
            }

            _previousYear = _balanceInUsd;
        }
    }

    private void RegisterTran(TransactionModel tran)
    {
        switch (tran.Operation)
        {
            case OperationType.Доход:
                _balance.Add(tran.Currency, tran.Amount);
                break;
            case OperationType.Расход:
                _balance.Sub(tran.Currency, tran.Amount);
                break;
            case OperationType.Перенос:
                if (tran.Timestamp.Date.Year == 2001)
                    _balance.Add(tran.Currency, tran.Amount);
                break;
            case OperationType.Обмен:
                _balance.Sub(tran.Currency, tran.Amount);
                _balance.Add(tran.CurrencyInReturn!.Value, tran.AmountInReturn);
                break;
        }
    }
}