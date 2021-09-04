using AutoWrapper;
using Consumables;
using Contexts;
using Http;
using Interface.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using SartainStudios.Cryptography;
using SartainStudios.Token;
using Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

const string ApplicationVersion = "1.0.0";
string ApplicationName = builder.Configuration["ApplicationInformation:ApplicationName"];
int AuthenticationExpirationInMinutes = int.Parse(builder.Configuration["Authentication:ExpirationInMinutes"]);
string AuthenticationSecret = builder.Configuration["Authentication:Secret"];
string CorsPolicyName = "CorsOpenPolicy";

#region Filters
builder.Services.Configure<ApiBehaviorOptions>(options => { options.SuppressModelStateInvalidFilter = true; });
builder.Services.AddMvc(options => { options.Filters.Add(typeof(ValidateModelStateAttribute)); });

builder.Services.AddMvc(options => { options.Filters.Add(typeof(ArgumentExceptionHandlerAttribute)); });
#endregion

// Add cors
builder.Services.AddCors(o => o.AddPolicy(CorsPolicyName, builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }));

#region services
builder.Services.AddSingleton(typeof(IAutoWrapperHttp<>), typeof(AutoWrapperHttp<>));
builder.Services.AddSingleton<IHash, Hash>();

builder.Services.AddSingleton<IUserService, UserService>();

var logPath = builder.Configuration.GetSection("LogWriteLocation").Value;
builder.Services.AddSingleton<SartainStudios.Log.ILog>(new SartainStudios.Log.Log(logPath));

builder.Services.AddSingleton<IToken>(new JwtToken(AuthenticationSecret, AuthenticationExpirationInMinutes));
#endregion

#region consumables
builder.Services.AddSingleton<IUserConsumable, UserConsumable>();
#endregion

#region contexts
builder.Services.AddSingleton<IUserContext, UserContext>();
#endregion

builder.Services.AddControllers();

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc(ApplicationVersion,
                    new OpenApiInfo { Title = ApplicationName, Version = ApplicationVersion });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = "Jwt";
    options.DefaultChallengeScheme = "Jwt";
})
.AddJwtBearer(
    "Jwt",
    options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(AuthenticationSecret)),
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(AuthenticationExpirationInMinutes)
            };
        });

var app = builder.Build();

app.UseAuthentication();

app.UseCors(CorsPolicyName);

if (builder.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint($"/swagger/{ApplicationVersion}/swagger.json",
                       $"{ApplicationName} {ApplicationVersion}"));
}

app.UseHttpsRedirection();

app.UseApiResponseAndExceptionWrapper();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
