using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Exp.Data;
using Microsoft.AspNetCore.Authentication.Cookies;
using Azure.Identity;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ExpContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ResponseContext")
));


builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    })
        .AddCookie(options =>
        {
            //options.LoginPath = "/account/signin";
            options.LoginPath = "/signin"; 
            options.LogoutPath = "/signout";
        })
        .AddGoogle(options =>
        {
            options.ClientId = "875386829601-cjsafmch1qtbc2joob6q991q71tlqr6d.apps.googleusercontent.com";
            options.ClientSecret = "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";
        })
        .AddLinkedIn(options =>
         {
             options.ClientId = "86eu8a752dqo6x";
             options.ClientSecret = "Wq0EcTUUS5BL9q9A";

             options.Scope.Add("r_liteprofile");
             options.Scope.Add("r_emailaddress");
             options.Scope.Add("w_member_social");
         });
builder.Services.AddControllersWithViews();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["sendchat_connectionstring:blob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["sendchat_connectionstring:queue"], preferMsi: true);
});

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

app.UseAuthentication(); 

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();