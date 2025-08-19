// SPDX-FileCopyrightText: 2025 Alper Çelik <alper@alper-celik.dev>
//
// SPDX-License-Identifier: Apache-2.0

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
