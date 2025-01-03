using System.Text.Json;
using LeanCode.Components;
using LeanCode.CQRS.AspNetCore;
using LeanCode.CQRS.Validation.Fluent;
using LeanCode.TestBed.Api;
using Microsoft.OpenApi.Models;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddSerilog();
builder.Services.AddFluentValidation(TypesCatalog.Of<TestCommandCH>());
builder.Services.AddCQRS(TypesCatalog.Of<TestCommand>(), TypesCatalog.Of<TestCommandCH>());
builder.Services.AddCQRSApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

app.MapOpenApi();
app.MapRemoteCQRS(
    "/",
    c =>
    {
        c.Commands = p => p.Secure().Validate();
        c.Queries = p => p.Secure();
    }
);
app.UseReDoc(opt =>
{
    opt.SpecUrl("/openapi/v1.json");
    opt.RoutePrefix = "redoc";
});
app.UseSwaggerUI(opt =>
{
    opt.SwaggerEndpoint("/openapi/v1.json", "Test API");
    opt.RoutePrefix = "swagger";
});

app.Run();
