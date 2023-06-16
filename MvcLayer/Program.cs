using BusinessLayer.IoC;
//using DatabaseLayer.Data;
using Microsoft.EntityFrameworkCore;
using MvcLayer.Mapper;

var builder = WebApplication.CreateBuilder(args);
string connectionData = builder.Configuration.GetConnectionString("Data");
string connectionIdentity = builder.Configuration.GetConnectionString("Identity");
//Registeration OiC container from business layer

Container.RegisterContainer(builder.Services, connectionData);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAutoMapper(typeof(MapperViewModel));



var app = builder.Build();



// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();


//extra methods