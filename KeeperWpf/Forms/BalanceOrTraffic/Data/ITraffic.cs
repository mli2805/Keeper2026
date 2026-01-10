using System;
using System.Collections.Generic;

namespace KeeperWpf;

interface ITraffic
{
    void EvaluateAccount();
    IEnumerable<KeyValuePair<DateTime, ListLine>>ColoredReport(BalanceOrTraffic mode);
    string Total { get; }

}