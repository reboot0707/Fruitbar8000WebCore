using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.OpenApi;
using prjFruitbar8000WebCore.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddDbContext<FruitBarDbContext>();
builder.Services.AddOpenApi("v2", options =>
{
    options.AddOperationTransformer((operation, context, cancellationToken) =>
    {
        if (operation.Parameters is null)
        {
            return Task.CompletedTask;
        }

        foreach (var parameter in operation.Parameters.OfType<OpenApiParameter>())
        {
            var description = context.Description.ParameterDescriptions.FirstOrDefault(x =>
                x.Name == parameter.Name && x.Source == BindingSource.Path);
            if (parameter.In != ParameterLocation.Path || description is null ||
                (Nullable.GetUnderlyingType(description.Type) ?? description.Type) != typeof(int) ||
                parameter.Schema is not OpenApiSchema schema)
            {
                continue;
            }

            // Swagger UI 會將 integer/string 聯合型別的路徑參數誤判為未填。
            // 複製 schema 後僅修正路徑整數型別，保留 JSON 本文接受數字字串的相容性。
            var pathSchema = (OpenApiSchema)schema.CreateShallowCopy();
            pathSchema.Type = JsonSchemaType.Integer;
            pathSchema.Format = "int32";
            pathSchema.Pattern = null;
            parameter.Schema = pathSchema;
            parameter.Required = true;
        }

        return Task.CompletedTask;
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
    {
        options.RoutePrefix = "openapi/docs";
        options.SwaggerEndpoint("/openapi/v2.json", "v2");
    });

}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();
