namespace KeeperInfrastructure;

public static class AccountMapper
{
    public static AccountEf ToEf(this KeeperDomain.Account account)
    {
        return new AccountEf
        {
            
        };
    }

    public static KeeperDomain.Account FromEf(this AccountEf accountEf)
    {
        return new KeeperDomain.Account
        {
            
        };
    }
}
