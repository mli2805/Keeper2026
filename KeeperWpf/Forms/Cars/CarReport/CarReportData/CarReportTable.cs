using System.Collections.Generic;

namespace KeeperWpf;

public class CarReportTable
{
    public string Russian;
    public string English;
    public List<CarReportTableRow> Table;

    public CarReportTable(string russian, string english, List<CarReportTableRow> table)
    {
        Russian = russian;
        English = english;
        Table = table;
    }
}