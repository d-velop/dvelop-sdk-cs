# Logging in OTEL format

## Usage
You'll need to configure the Dotnet logging with his OTEL-Package

```
var hostBuilder = Host.CreateDefaultBuilder().ConfigureWebHostDefaults(webBuilder =>
{
    webBuilder
    .ConfigureLogging(loggingBuilder =>
    {
        loggingBuilder.ClearProviders();
        loggingBuilder.AddOtelJsonConsole();
    })
    .UseStartup<Startup>()
    .UseUrls(appUri.AbsoluteUri);
});
```
The logging itself can be configured within the appsettings.json
```
{
    "Logging": {
        "IncludeScopes": false,
        "LogLevel": {
            "Default": "Information",
            "Microsoft": "Error",
            "System": "Warning"
        }
    }
}
```
## Logging additional data for every request

To enable logging of additional data, you may want to write an Middleware.

Example
```
public class RequestLoggingMiddleware
{
    private readonly ILogger<RequestLoggingMiddleware> _logger;
 
    private readonly RequestDelegate _next;
 
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _logger = logger;
        _next = next;
    }
 
    public async Task InvokeAsync(HttpContext context)
    {
        using (_logger.BeginScope(new TracingLogScope(Activity.Current?.TraceId.ToString(), Activity.Current?.SpanId.ToString())))
        using (_logger.BeginScope(new TenantLogScope(RequestContext.Current.TenantId)))
        {
            await _next(context);
        }
    }
}
```

## Custom logging attributes

You may want to add additional data, like database access information.

In this case, you'll need to create an class derived from ``CustomLogAttributeState``.

```
ublic class DatabaseLogAttributeState : CustomLogAttributeState
{
    public string TimeUsed { get; set; }
    public string Name { get; set; }
    public string Statement { get; set; }
    public string Operation { get; set; }
 
    public override IEnumerable<CustomLogAttribute> Attributes
    {
        get
        {
            var attributes = new List<CustomLogAttribute>();
 
            if (!string.IsNullOrWhiteSpace(TimeUsed))
            {
                attributes.Add(new CustomLogAttributeProperty("timeUsed", TimeUsed));
            }
 
            var databaseAttributes = new List<CustomLogAttribute>();
 
            if (!string.IsNullOrWhiteSpace(Name))
            {
                databaseAttributes.Add(new CustomLogAttributeProperty("name", Name));
            }
            if (!string.IsNullOrWhiteSpace(Statement))
            {
                databaseAttributes.Add(new CustomLogAttributeProperty("statement", Statement));
            }
            if (!string.IsNullOrWhiteSpace(Operation))
            {
                databaseAttributes.Add(new CustomLogAttributeProperty("operation", Operation));
            }
 
            attributes.Add(new CustomLogAttributeObject("db", databaseAttributes));
 
            return attributes;
        }
    }
}
```

Now you can log a statement with additional information:
```

logger.Log(LogLevel.Debug, "Fetching batches from database", new DatabaseLogAttributeState
{
    Name = "MY_PROCESS",
    Operation = "SELECT",
    Statement = "SELECT * FROM MY_TABLE",
    TimeUsed = "252000000"
})
```

This will create a json log statement like the following:
```
{
    "time": "2021-06-09T11:33:17.9464445Z",
    "sev": 5,
    "body": "Fetching batches from database",
    "res": {
        "svc": {
            "name": "myapptoyourappconnector",
            "ver": "1.3.0.0",
            "inst": "62a6e5d08a6ce29a6a935fe11dd33199"
        }
    },
    "attr": {
        "timeUsed": "252000000",
        "db": {
            "name": "MY_PROCESS",
            "statement": "SELECT * FROM MY_TABLE",
            "operation": "SELECT"
        }
    },
    "vis": 1
}
```
