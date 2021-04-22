using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using IntegrationTestExample.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace IntegrationTestExample
{
    // This is the Empty ASP.NET Core sample project with just a couple changes to demonstrate having an endpoint that
    // uses IHttpClientFactory to make http requests which we'll need to mock for integration tests.
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure a named http client for requests to the GitHub API. This is taken straight from the docs:
            // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests?view=aspnetcore-5.0#named-clients
            services.AddHttpClient("github", c =>
            {
                c.BaseAddress = new Uri("https://api.github.com/");
                c.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");
                c.DefaultRequestHeaders.Add("User-Agent", "HttpClientFactory-Sample");
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // A simple endpoint that lists my GitHub repos. In a real app, this might be a controller with a
                // GitHubService, for example. Note that if you were to run this server, it would make requests to the
                // *real* GitHub API. In an integration test, we would want to mock that...
                endpoints.MapGet("/", async context =>
                {
                    var factory = context.RequestServices.GetRequiredService<IHttpClientFactory>();
                    var client = factory.CreateClient("github");

                    var repos = await client.GetFromJsonAsync<IEnumerable<GitHubRepository>>("/users/maxkagamine/repos");

                    context.Response.ContentType = "text/plain; charset=UTF-8";

                    foreach (var repo in repos)
                    {
                        await context.Response.WriteAsync($"{repo.Name} ({repo.Language}, ★{repo.Stars})\n");
                    }
                });
            });
        }
    }
}
