Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(ComponentLibraryConstants.SyncfusionKey);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor()
    .AddCircuitOptions(options =>
    {
        options.DetailedErrors = true;
    });
builder.Services.AddSingleton<ITradeClient, TradeClient>();
builder.Services.AddComponentLibraryServices(builder.Configuration);
builder.Services.AddOAuth2(builder.Configuration);

builder.Services.AddSession(options => { options.IdleTimeout = TimeSpan.FromMinutes(15); });

builder.Services.AddHttpClient<IIdentityClient, IdentityClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration[ConfigurationKeys.IdentityApiBase]!);
    client.Timeout = new TimeSpan(0, 1, 30);
}).AddPolicyHandler(getRetryPolicy());

builder.Services.AddScoped<ApiTokenProvider>();
builder.Services.AddScoped<ApiTokenManager>();
builder.Services.AddTransient<AuthorizationHeaderHandler>();

// Added for JwtTokenForwardHandler usage
builder.Services.AddHttpContextAccessor();
builder.Services.AddTransient<JwtTokenForwardHandler>();

builder.Services.AddHttpClient<IHedgeAccountingApiService, HedgeAccountingApiService>()
    .ConfigureHttpClient(httpClient => httpClient.BaseAddress =
        new Uri(builder.Configuration[ConfigurationKeys.HedgeAccountingServiceUrl]))
    .AddHttpMessageHandler<AuthorizationHeaderHandler>();

builder.Services.AddFeatureManagement();
builder.Services
    .AddAutoMapper(typeof(Program), typeof(GetTrades))
    .AddSyncfusionBlazor()
    .AddBlazoredLocalStorage()
    .AddCustomAuthentication(builder.Configuration)
    .AddAuthorization(o => { o.AddPolicy("auth", p => { p.RequireAuthenticatedUser(); }); })
    .AddScoped<IAlertService, AlertService>()
    .AddClientContext<Program>().AddSplitIOFeatureManager(builder.Configuration)
    .AddScoped<IUserMetaDataService, UserMetaDataService>()
    .AddScoped<IUserAuthData>(s =>
    {
        var authStateProvider = s.GetRequiredService<AuthenticationStateProvider>();
        var authState = authStateProvider.GetAuthenticationStateAsync().Result; // Blocking call
        return new UserAuthData(authState.User.Claims);
    })
    .AddScoped<SpinnerService>()
    .AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly, typeof(HandlersClassAnchor).Assembly);
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LogExceptionBehavior<,>));
    })
    .AddLazyCache();

builder.Services.AddHealthChecks()
    .AddCheck("Main", () => HealthCheckResult.Healthy("This service is working."));

builder.Services.AddScoped<TokenProvider>();
builder.Services.AddHedgeAccountingHttpClients(builder.Configuration);
builder.AddOtel();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
    _ = app.RunTailwind("tailwind", "./");
}

app.UseForwardedHeaders();
app.Use((context, next) =>
{
    if (context.Request.Headers.TryGetValue("X-Forwarded-Prefix", out var pathBase))
    {
        context.Request.PathBase = pathBase.ToString();
    }

    return next();
});

app.UseRequestLocalization();
app.UsePathBase("/hedgeaccountingapp"); // Removed for reverse proxy Option A
app.UseStaticFiles();
app.UseRouting();
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.UseHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

app.UseHealthChecks("/health-services", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.UseUnauthorizedEndpoint();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.UseSession();

app.Run();

static IAsyncPolicy<HttpResponseMessage> getRetryPolicy() => HttpPolicyExtensions
    .HandleTransientHttpError()
    .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.NotFound)
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromMilliseconds(500 * retryAttempt)); // 0.5, 1, 1.5 seconds

/*
 * We need to expose the program class to WebApplicationFactory so that it is a shared context for the xUnit tests.
 * https://github.com/dotnet/aspnetcore/issues/38474
 */
public partial class Program
{
    protected Program()
    {
    }
}