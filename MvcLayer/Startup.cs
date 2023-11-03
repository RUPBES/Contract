using BusinessLayer.IoC;
using DatabaseLayer.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MvcLayer.Data;
using MvcLayer.Mapper;
using OpenIddict.Client;
using Quartz;
using static OpenIddict.Abstractions.OpenIddictConstants;

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
            string connectionIdentity = Configuration.GetConnectionString("Identity");
            Container.RegisterContainer(services, connectionData);

            services.AddAutoMapper(typeof(MapperViewModel));
            services.AddDbContext<ContractsContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Data"));               
            });
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(Configuration.GetConnectionString("Identity"));
                options.UseOpenIddict();
            });
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            })
                .AddCookie(options =>
                {
                    options.LoginPath = "/login";
                    options.LogoutPath = "/logout";
                    options.ExpireTimeSpan = TimeSpan.FromMinutes(50);
                    options.SlidingExpiration = false;
                });
            // OpenIddict offers native integration with Quartz.NET to perform scheduled tasks
            // (like pruning orphaned authorizations from the database) at regular intervals.
            services.AddQuartz(options =>
            {
                options.UseMicrosoftDependencyInjectionJobFactory();
                options.UseSimpleTypeLoader();
                options.UseInMemoryStore();
            });
            services.AddMvc();
            // Register the Quartz.NET service and configure it to block shutdown until jobs are complete.
            services.AddQuartzHostedService(options => options.WaitForJobsToComplete = true);
            services.AddOpenIddict()
                .AddCore(options =>
                {
                    options.UseEntityFrameworkCore()
                           .UseDbContext<ApplicationDbContext>();

                    // Developers who prefer using MongoDB can remove the previous lines
                    // and configure OpenIddict to use the specified MongoDB database:
                    // options.UseMongoDb()
                    //        .UseDatabase(new MongoClient().GetDatabase("openiddict"));

                    options.UseQuartz();
                })
                .AddClient(options =>
                {
                    options.AllowAuthorizationCodeFlow();

                    // Register the signing and encryption credentials used to protect
                    // sensitive data like the state tokens produced by OpenIddict.
                    options.AddDevelopmentEncryptionCertificate()
                           .AddDevelopmentSigningCertificate();

                    // Register the ASP.NET Core host and configure the ASP.NET Core-specific options.
                    options.UseAspNetCore()
                           .EnableStatusCodePagesIntegration()
                           .EnableRedirectionEndpointPassthrough()
                           .EnablePostLogoutRedirectionEndpointPassthrough();

                    // Register the System.Net.Http integration and use the identity of the current
                    // assembly as a more specific user agent, which can be useful when dealing with
                    // providers that use the user agent as a way to throttle requests (e.g Reddit).
                    options.UseSystemNetHttp()
                           .SetProductInformation(typeof(Program).Assembly);

                    // Add a client registration matching the client application definition in the server project.
                    options.AddRegistration(new OpenIddictClientRegistration
                    {
                        Issuer = new Uri("https://authsrv.rupbes.by:8011/", UriKind.Absolute),
                        ClientId = "contract-mvc",
                        ClientSecret = "771994A5-E7FE-52CB-B11D-61EF6A8F8984",
                        //ClientId = "ogm-mvc-2",
                        //ClientSecret = "568764A5-E7FE-42DC-A10D-77FD6A8F3622",
                        Scopes = { Scopes.Email, Scopes.Profile },

                        // Note: to mitigate mix-up attacks, it's recommended to use a unique redirection endpoint
                        // URI per provider, unless all the registered providers support returning a special "iss"
                        // parameter containing their URL as part of authorization responses. 
                        RedirectUri = new Uri("callback/login/local", UriKind.Relative),
                        PostLogoutRedirectUri = new Uri("callback/logout/local", UriKind.Relative)
                    });
                });
            services.AddHttpClient();
            services.AddControllersWithViews();
            // Register the worker responsible for creating the database used to store tokens.
            // Note: in a real world application, this step should be part of a setup script.
            services.AddHostedService<Worker>();
            /////////////////////////////////////////
            /////////////////////////////////////////
            /////////////////////////////////////////

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

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
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
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
            });
        }
    }
}
