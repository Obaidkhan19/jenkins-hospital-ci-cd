using HospitalApi;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSingleton<PatientService>();
var app = builder.Build();

// APP_ENV is set by the pipeline (development / production)
var env = Environment.GetEnvironmentVariable("APP_ENV") ?? "local";

app.MapGet("/", () => $"Hospital API is running. Environment: {env}");
app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
app.MapGet("/patients", (PatientService s) => s.GetAll());
app.MapGet("/patients/{id:int}", (int id, PatientService s) =>
    s.GetById(id) is { } p ? Results.Ok(p) : Results.NotFound());

app.Run();
