using CapheVanPhong.Application;
using CapheVanPhong.Infrastructure;
using CapheVanPhong.Infrastructure.Seeding;
using CapheVanPhong.Web.Components;
using MailKit.Security;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.Email;
using System.Net;

// Bootstrap logger — captures startup errors before full Serilog is configured
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Shared Windows Hosting does not support __ in environment variable names,
    // so we use a custom variable name and inject it into configuration manually.
    var smtpPasswordFromEnv = Environment.GetEnvironmentVariable("Cpvp_Email_SmtpPassword");
    if (!string.IsNullOrWhiteSpace(smtpPasswordFromEnv))
        builder.Configuration["Email:SmtpPassword"] = smtpPasswordFromEnv;

    // Configure Serilog from appsettings.json, then add the email sink for errors
    builder.Host.UseSerilog((context, loggerConfig) =>
    {
        loggerConfig.ReadFrom.Configuration(context.Configuration);

        var emailSection = context.Configuration.GetSection("Email");
        var smtpPassword = emailSection["SmtpPassword"];

        // Only add the email sink when credentials are present (skipped in development)
        if (!string.IsNullOrWhiteSpace(smtpPassword))
        {
            loggerConfig.WriteTo.Email(
                new EmailSinkOptions
                {
                    From = emailSection["ApplicationEmail"]!,
                    To = [emailSection["AdminEmail"]!],
                    Host = emailSection["SmtpHost"]!,
                    Port = int.Parse(emailSection["SmtpPort"] ?? "587"),
                    ConnectionSecurity = SecureSocketOptions.StartTls,
                    Credentials = new NetworkCredential(
                        emailSection["SmtpUsername"],
                        smtpPassword
                    )
                },
                restrictedToMinimumLevel: LogEventLevel.Error
            );
        }
    });

    // Add services to the container.
    builder.Services.AddRazorComponents()
        .AddInteractiveServerComponents();

    // Add Clean Architecture layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Add authentication and authorization
    builder.Services.AddCascadingAuthenticationState();

    var app = builder.Build();

    // Seed database with default roles and admin account
    using (var scope = app.Services.CreateScope())
    {
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }

    // Configure the HTTP request pipeline.
    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Error", createScopeForErrors: true);
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseAntiforgery();

    app.MapStaticAssets();
    app.MapRazorComponents<App>()
        .AddInteractiveServerRenderMode();

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    await Log.CloseAndFlushAsync();
}
