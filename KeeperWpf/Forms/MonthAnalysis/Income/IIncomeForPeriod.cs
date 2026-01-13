namespace KeeperWpf;

public interface IIncomeForPeriod
{
    void Fill(ListOfLines list);
    decimal GetTotal();
}