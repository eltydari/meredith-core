﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WhyNotEarth.Meredith.App.Configuration;
using WhyNotEarth.Meredith.Data.Entity;
using WhyNotEarth.Meredith.Data.Entity.Models;
using WhyNotEarth.Meredith.Identity;

namespace WhyNotEarth.Meredith.App.Auth
{
    public static class AuthenticationExtensions
    {
        public static void AddCustomAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtOptions = configuration.GetSection("Jwt").Get<JwtOptions>();
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            services
                .AddIdentity<User, Role>()
                .AddUserManager<UserManager>()
                .AddRoleManager<RoleManager>()
                .AddEntityFrameworkStores<MeredithDbContext>()
                .AddDefaultTokenProviders();

            services
                .AddAuthentication(o =>
                {
                    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                    o.DefaultForbidScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddCookie()
                .AddJwtBearer(config =>
                {
                    config.RequireHttpsMetadata = false;
                    config.SaveToken = true;
                    config.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidIssuer = jwtOptions.Issuer,
                        ValidAudience = jwtOptions.Issuer,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.Key)),
                        ClockSkew = TimeSpan.Zero
                    };
                })
                .AddGoogle(options =>
                {
                    var config = configuration.GetSection("Authentication:Google");
                    options.ClientId = config["ClientId"];
                    options.ClientSecret = config["ClientSecret"];
                    options.Events.OnRemoteFailure = HandleOnRemoteFailure;

                    // Profile picture
                    options.Scope.Add("profile");
                    options.Events.OnCreatingTicket = context =>
                    {
                        var picture = context.User.GetProperty("picture").GetString();

                        context.Identity.AddClaim(new Claim("picture", picture));

                        return Task.CompletedTask;
                    };
                })
                .AddFacebook(options =>
                {
                    var config = configuration.GetSection("Authentication:Facebook");
                    options.ClientId = config["ClientId"];
                    options.ClientSecret = config["ClientSecret"];
                    options.Events.OnRemoteFailure = HandleOnRemoteFailure;

                    // Profile picture
                    options.Fields.Add("picture");
                    options.Events.OnCreatingTicket = context =>
                    {
                        //var profileImg = context.User["picture"]["data"].Value<string>("url");
                        var picture = context.User.GetProperty("picture").GetProperty("data").GetProperty("url").GetString();

                        context.Identity.AddClaim(new Claim("picture", picture));

                        return Task.CompletedTask;
                    };
                });

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "auth";
                options.Cookie.HttpOnly = false;
                options.Cookie.SameSite = SameSiteMode.None;
                options.LoginPath = null;
            });
        }

        public static IApplicationBuilder UseCustomAuthentication(this IApplicationBuilder app)
        {
            app.UseAuthentication().
                Use(async (context, next) =>
            {
                // If the default identity failed to authenticate (cookies)
                if (context.User.Identities.All(i => !i.IsAuthenticated))
                {
                    var principal = new ClaimsPrincipal();
                    var jwtAuth = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
                    if (jwtAuth?.Principal != null)
                    {
                        principal.AddIdentities(jwtAuth.Principal.Identities);
                        context.User = principal;
                    }
                }

                await next();
            });

            return app;
        }

        private static Task HandleOnRemoteFailure(RemoteFailureContext context)
        {
            context.Response.Redirect(context.Properties.RedirectUri);
            context.HandleResponse();

            return Task.FromResult(0);
        }
    }
}