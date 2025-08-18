using Microsoft.Extensions.DependencyInjection;
using Models.ImporterInterfaces;

[assembly: CaptureConsole]

namespace DependencyInjection.Tests;

public class ImporterRegisterersTests
{
    [Fact]
    public void RegisterCreditCardImporters_ShouldRegister()
    {
        var services = new ServiceCollection();
        ImporterRegisterers.RegisterCreditCardImporters(services);
        var app = services.BuildServiceProvider();

        var importers = app.GetService<IEnumerable<ICreditCardImporter>>();

        Assert.NotNull(importers);
        Assert.True(importers.Any());
    }
}
