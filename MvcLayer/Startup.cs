using BusinessLayer.Helpers;
using BusinessLayer.IoC;
using DatabaseLayer.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using MvcLayer.Mapper;
using Quartz;
using System.Security.Claims;

namespace MvcLayer
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public void ConfigureServices(IServiceCollection services)
        {
            string connectionData = Configuration.GetConnectionString("Data");
            //string connectionIdentity = Configuration.GetConnectionString("Identity");
            Container.RegisterContainer(services, connectionData);
            ////
            services.AddRazorPages();
            services.AddWindowsService();
            ///
            System.Net.ServicePointManager.SecurityProtocol = System.Net.SecurityProtocolType.Tls12;
            IdentityModelEventSource.ShowPII = true;
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor |
                    ForwardedHeaders.XForwardedProto |
                    ForwardedHeaders.XForwardedHost;
            });
            services.Configure<FormOptions>(options =>
            {
                // Set the limit to 512 MB
                options.ValueLengthLimit = int.MaxValue;
                options.MultipartBodyLengthLimit = int.MaxValue;
            });
            services.AddAutoMapper(typeof(MapperViewModel));
            services.AddDbContext<ContractsContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Data"));
            });
            
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(
                 options =>
                 {
                     options.LoginPath = "/Account/Login/";
                     options.LogoutPath = "/Account/Logout";
                     //options.Events.OnSigningOut = async e =>
                     //{
                     //    await e.HttpContext.RevokeRefreshTokenAsync();
                     //};
                 }
            )
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, o =>
            {
                o.ClientId = "ContractApplicationMVC";
                o.ClientSecret = "771994A5-E7FE-52CB-B11D-61EF6A8F8984";
                o.Authority = "https://aauth.rupbes.by/";
                o.ResponseType = OpenIdConnectResponseType.Code; // OpenIdConnectResponseType.CodeIdTokenToken; // "code id_token token";
                o.ResponseMode = OpenIdConnectResponseMode.Query;
                o.UsePkce = true; // o.UsePkce = false; 
                o.SaveTokens = true;
                o.GetClaimsFromUserInfoEndpoint = true;
                o.SignedOutCallbackPath = "/signout-callback-oidc"; // "/signout-callback-oidc" is a default endpoint. Do not use "signout-oidc"
                o.SignedOutRedirectUri = "/Account/PostLogout";
                o.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;

                o.Scope.Clear();
                o.Scope.Add("openid");
                o.Scope.Add("profile");
                o.Scope.Add("subject");
                o.Scope.Add("email");
                o.Scope.Add("offline_access");

                o.Scope.Add(ConstantsApp.ROLE_READ);
                o.Scope.Add(ConstantsApp.ROLE_EDIT);
                o.Scope.Add(ConstantsApp.ROLE_ADMIN);
                o.Scope.Add(ConstantsApp.ROLE_DELETE);
                o.Scope.Add(ConstantsApp.ROLE_CREATE);

                o.Scope.Add(ConstantsApp.ORG_BES);
                o.Scope.Add(ConstantsApp.ORG_TEC_2);
                o.Scope.Add(ConstantsApp.ORG_TEC_5);
                o.Scope.Add(ConstantsApp.ORG_BESM);
                o.Scope.Add(ConstantsApp.ORG_BETSS);
                o.Scope.Add(ConstantsApp.ORG_GES);
                o.Scope.Add(ConstantsApp.ORG_MAJOR);

                o.Scope.Add(ConstantsApp.GRP_CONTRACT);
                o.Scope.Add(ConstantsApp.GRP_ESTIMATE);
                o.Scope.Add(ConstantsApp.GRP_FINANCE);

                // requests a refresh token                          
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
                o.Events.OnTokenResponseReceived = cntxt =>
                {
                    return Task.CompletedTask;
                };
                o.Events.OnTokenValidated = cntxt =>
                {
                    if (cntxt.TokenEndpointResponse != null)
                    {
                        var scopes = cntxt.TokenEndpointResponse.Scope.Split(' ');
                        if (scopes != null && (cntxt.Principal != null) && cntxt.Principal.Identity is ClaimsIdentity identity)
                        {
                            foreach (var scope in scopes)
                            {
                                if (scope.Contains("Org"))
                                {
                                    identity.AddClaim(new Claim("org", scope));
                                }
                                if (scope.Contains("GRP_"))
                                {
                                    identity.AddClaim(new Claim("grp", scope));
                                }

                                identity.AddClaim(new Claim("scope", scope));
                            }
                        }
                    }
                    return Task.CompletedTask;
                };
                o.Events.OnRemoteFailure = cntxt =>
                {
                    cntxt.Response.Redirect("/");
                    cntxt.HandleResponse();
                    return Task.CompletedTask;
                };
            });

            services.AddAuthorization(options => {
               
                options.AddPolicy("ViewPolicy", policy =>
                   policy.RequireAssertion(context =>
                   {
                       bool r = context.User.HasClaim(c => (c.Type == "scope" || c.Value == "ContrView"));
                       return r;
                   }
                ));
                options.AddPolicy("CreatePolicy", policy =>
                  policy.RequireAssertion(context =>
                  {
                      bool r = context.User.HasClaim(c => (c.Type == "scope" || c.Value == "ContrCreate"));
                      return r;
                  }
               ));
                options.AddPolicy("EditPolicy", policy =>
                  policy.RequireAssertion(context =>
                  {
                      bool r = context.User.HasClaim(c => (c.Type == "scope" || c.Value == "ContrEdit"));
                      return r;
                  }
               ));
                options.AddPolicy("DeletePolicy", policy =>
                  policy.RequireAssertion(context =>
                  {
                      bool r = context.User.HasClaim(c => (c.Type == "scope" || c.Value == "ContrDelete"));
                      return r;
                  }
               ));
                options.AddPolicy("AdminPolicy", policy =>
                   policy.RequireAssertion(context =>
                   {
                       bool r = context.User.HasClaim(c => (c.Type == "scope"&& c.Value == "ContrAdmin"));
                       return r;
                   }
                ));
            });

            services.AddMvc();            
            services.AddHttpClient();
            services.AddControllersWithViews();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            app.Use((context, next) =>
            {
                // Scheme must be resetted
                context.Request.Scheme = "https"; // set the schema if it must be
                return next(context);
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseDeveloperExceptionPage();
                //app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePagesWithReExecute("/error", "?code={0}");
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }
    }
}
