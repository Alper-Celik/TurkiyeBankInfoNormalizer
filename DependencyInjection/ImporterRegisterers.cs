// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

using Microsoft.Extensions.DependencyInjection;
using Models.ImporterInterfaces;

namespace DependencyInjection;

public static class ImporterRegisterers
{
    public static IServiceCollection RegisterCreditCardImporters(this IServiceCollection services)
    {
        Importers.Load.LoadAssembly(); // for forcing the loading of the assembly
        new InterfaceImplementationsRegisterer<ICreditCardImporter>().Register(services);
        return services;
    }
}
