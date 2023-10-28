using AuthenticationWithAzureAd;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;
using System.Security.Cryptography.Xml;

var builder = WebApplication.CreateBuilder(args);

//builder.Host() == Program serilog

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"))
        .EnableTokenAcquisitionToCallDownstreamApi()
            .AddMicrosoftGraph(builder.Configuration.GetSection("MicrosoftGraph"))
            .AddInMemoryTokenCaches();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AzureTeachPolicy", policy => policy.Requirements.Add(new GroupAuthorizationRequirement("9fa43fee-9aa6-43f6-974d-46149af7fc66")));
});

builder.Services.AddTransient<IAuthorizationHandler, GroupAuthorizationHandler>();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() { Title = "Login com o swagger and azure", Version = "v1" });
    c.AddSecurityDefinition("ouath2", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Oauth 2.0 uses AuthorizationCode flow",
        Name = "oauth2.0",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.OAuth2,
        Flows = new Microsoft.OpenApi.Models.OpenApiOAuthFlows()
        {// Todas infos registração cliente swagger
            AuthorizationCode = new Microsoft.OpenApi.Models.OpenApiOAuthFlow
            {
                AuthorizationUrl =  new Uri("https://login.microsoftonline.com/46bda5f6-9a46-4f87-808d-ff5c2eb927de/oauth2/v2.0/authorize"),
                TokenUrl = new Uri("https://login.microsoftonline.com/46bda5f6-9a46-4f87-808d-ff5c2eb927de/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    {"api://0c941fa0-64e4-4a30-8e10-07b00700bd99/acesso_usuario", "Acess Api as User" }
                }
            }
        }
    });

    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement()
    {
        { 
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{ Type =  ReferenceType.SecurityScheme, Id = "ouath2" }
            },
            new [] { "api://0c941fa0-64e4-4a30-8e10-07b00700bd99/acesso_usuario" }
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId("b6c05754-fb7e-4f50-a72b-e777709893c7");
        c.OAuthUsePkce();
        c.OAuthScopeSeparator(" ");
    });
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
