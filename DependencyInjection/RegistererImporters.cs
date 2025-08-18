using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Models.ImporterInterfaces;

namespace DependencyInjection;

public static class RegistererImporters
{
    public static IServiceCollection RegisterCreditCardImporters(this IServiceCollection services)
    {
        _ = Importers.Load.loaded; // for forcing the loading of the assembly
        Type ICreditCardImporterType = typeof(ICreditCardImporter);

        foreach (
            Type Importer in AppDomain
                .CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try
                    {
                        var t = a.GetTypes();
                        return t;
                    }
                    catch
                    {
                        return [];
                    }
                })
                .Where(t =>
                    ICreditCardImporterType.IsAssignableFrom(t)
                    && t is { IsAbstract: false, IsInterface: false }
                )
        )
        {
            var method = typeof(RegistererImporters).GetMethod(
                nameof(addSingeltonImporter),
                BindingFlags.Static | BindingFlags.NonPublic
            )!;
            method.MakeGenericMethod(Importer).Invoke(null, [services]);
        }

        return services;
    }

    private static void addSingeltonImporter<T>(IServiceCollection services)
        where T : class, ICreditCardImporter
    {
        services.AddSingleton<ICreditCardImporter, T>();
    }
}
