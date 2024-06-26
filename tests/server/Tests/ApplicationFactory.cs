﻿using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Infrastructure;
using Tests.Infrastructure;

namespace Tests;

internal class ApplicationFactory : WebApplicationFactory<Program>, IHttpClienFactory
{
    public ApplicationFactory()
    {
    }

    //public ApiKeySettings ApiKeys { get; set; }

    public FixedClock Clock { get; set; } = null!;

    public Action<IServiceCollection>? ConfigureServices { get; set; }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(configBuilder =>
        {
            var additionalConfig = new Dictionary<string, string>() { };

            configBuilder.AddInMemoryCollection(additionalConfig!);
        });

        return base.CreateHost(builder);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        if (ConfigureServices != null)
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<IClock>();
                services.AddSingleton<IClock>(Clock);
                //services.RemoveAll<ApiKeySettings>();
                //services.AddSingleton(ApiKeys);
                ConfigureServices?.Invoke(services);
            });
    }
}