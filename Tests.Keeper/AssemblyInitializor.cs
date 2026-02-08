namespace Tests.Keeper;

[TestClass]
public sealed class AssemblyInitializor
{
    [AssemblyInitialize]
    public static async Task AssemblyInit(TestContext ctx)
        => await DbTestHelper.InitializeTemplateAsync();
}
