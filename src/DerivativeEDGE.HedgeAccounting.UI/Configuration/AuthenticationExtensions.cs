namespace DerivativeEDGE.HedgeAccounting.UI.Configuration;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });

        services.AddAuthorization(o => { o.AddPolicy("auth", p => { p.RequireAuthenticatedUser(); }); });

        if (configuration.GetValue("PROXY_MODE", "NONE").Equals("LocalDevelopment", StringComparison.OrdinalIgnoreCase))
        {
            services.AddAuthentication(o =>
            {
                o.DefaultAuthenticateScheme = "Test";
                o.DefaultChallengeScheme = "Test";
            })
                    .AddScheme<AuthenticationSchemeOptions, LocalAuthHandler>("Test", options => { });
        }
        else
        {
            services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = configuration["JWT_AUDIENCE"],
                    ValidIssuer = configuration["JWT_ISSUER"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JWT_ISSUERSIGNINGKEY"] ?? ""))
                };
            });
        }




        return services;
    }

    public static void UseUnauthorizedEndpoint(this WebApplication app)
    {
        app.MapGet("/unauthorized", () => "Unauthorized Request, Use Reverse Proxy");
    }

}
