using Microsoft.Extensions.DependencyInjection;
using Models.ExporterInterfaces;

namespace DependencyInjection;

public static class ExporterRegisterers
{
    public static IServiceCollection RegisterCreditCardExporters(this IServiceCollection services)
    {
        Exporters.Load.LoadAssembly();
        new InterfaceImplementationsRegisterer<ICreditCardTransactionExporter>().Register(services);
        return services;
    }
}
