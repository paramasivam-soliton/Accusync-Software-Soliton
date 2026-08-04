// --------------------------------------------------------------------------------
// <copyright file="DataParserServiceCollectionExtensions.cs" company="Natus Sensory">
//     Copyright (c) 2026 Natus Sensory. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------

using AccuSync.Application.Abstractions.Parsing;
using AccuSync.Adapters.DataParser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AccuSync.Adapters.DataParser.DependencyInjection
{
    /// <summary>
    /// Wires up the file-format-exchange services (import parsing, QR generation).
    /// Neither implementation holds per-request state, so both are registered as singletons.
    /// </summary>
    public static class DataParserServiceCollectionExtensions
    {
        public static IServiceCollection AddDataParserServices(this IServiceCollection services)
        {
            services.AddSingleton<IImportService, ImportService>();
            services.AddSingleton<IQrCodeGenerator, QrCodeGenerator>();

            return services;
        }
    }
}
