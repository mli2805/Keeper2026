using System.Collections.Generic;

namespace KeeperWpf;

public class CarReportTable
{
    public string Russian;
    public List<CarReportTableRow> Table;

    public CarReportTable(string russian, List<CarReportTableRow> table)
    {
        Russian = russian;
        Table = table;
    }
}