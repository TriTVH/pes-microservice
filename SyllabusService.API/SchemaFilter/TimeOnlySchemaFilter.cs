using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace SyllabusService.API.SchemaFilter
{
    public class TimeOnlySchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type == typeof(TimeOnly))
            {
                schema.Type = "string";
                schema.Format = "HH:mm";
                schema.Example = new OpenApiString("07:00");
            }
        }
    }

}
