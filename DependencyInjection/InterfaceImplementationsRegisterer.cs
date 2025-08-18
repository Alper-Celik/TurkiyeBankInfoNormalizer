using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection;

public class InterfaceImplementationsRegisterer<TBaseType>
    where TBaseType : class
{
    public InterfaceImplementationsRegisterer()
    {
        _addSingletonImporterMethod =
            typeof(InterfaceImplementationsRegisterer<TBaseType>).GetMethod(
                nameof(AddSingletonImporter),
                BindingFlags.Static | BindingFlags.NonPublic
            )!;

        _implementationTypes = AppDomain
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
                typeof(TBaseType).IsAssignableFrom(t)
                && t is { IsAbstract: false, IsInterface: false }
            );
    }

    private readonly MethodInfo _addSingletonImporterMethod;
    private readonly IEnumerable<Type> _implementationTypes;

    public IServiceCollection Register(IServiceCollection services)
    {
        foreach (Type implementation in _implementationTypes)
        {
            _addSingletonImporterMethod.MakeGenericMethod(implementation).Invoke(null, [services]);
        }
        return services;
    }

    private static void AddSingletonImporter<TImplementationType>(IServiceCollection services)
        where TImplementationType : class, TBaseType
    {
        services.AddSingleton<TBaseType, TImplementationType>();
    }
}
