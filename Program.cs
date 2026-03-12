using HeadHunterJobPopularTagsMonitor.Components;
using HeadHunterJobPopularTagsMonitor.Constants;
using HeadHunterJobPopularTagsMonitor.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services
    .AddHttpClient<HeadHunterHttpService>(httpClient =>
    {
        httpClient.BaseAddress = new Uri("https://api.hh.ru/");
        httpClient.DefaultRequestHeaders.Add("User-Agent", HttpClientConstants.UserAgent);
    })
    .SetHandlerLifetime(TimeSpan.FromMinutes(5));

builder.Services
    .AddSingleton<IHeadHunterHttpService, HeadHunterHttpService>()
    .AddSingleton<IJobKeySkillsRequestService, JobKeySkillsRequestService>();

builder.Services
    .AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
