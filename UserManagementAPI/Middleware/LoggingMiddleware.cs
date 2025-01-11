public class LoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger _logger;

    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Log the incoming request
        _logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");

        // Log the request headers
        foreach (var header in context.Request.Headers)
        {
            _logger.LogInformation($"Request header: {header.Key} = {header.Value}");
        }

        // Log the request body without consuming it
        // NOTE: Copilot helped me debug an error where the body was being consumed by the logger
        // before the User could be parsed for the actual api call
        context.Request.EnableBuffering();
        var requestBodyStream = new MemoryStream();
        await context.Request.Body.CopyToAsync(requestBodyStream);
        requestBodyStream.Seek(0, SeekOrigin.Begin);
        var requestBodyText = await new StreamReader(requestBodyStream).ReadToEndAsync();
        _logger.LogInformation($"Request body: {requestBodyText}");
        requestBodyStream.Seek(0, SeekOrigin.Begin);
        context.Request.Body = requestBodyStream;

        // Call the next middleware in the pipeline
        await _next(context);

        // Log the outgoing response
        _logger.LogInformation($"Outgoing response: {context.Response.StatusCode}");

        // Log the response headers
        foreach (var header in context.Response.Headers)
        {
            _logger.LogInformation($"Response header: {header.Key} = {header.Value}");
        }

        // Log the response body
        if (context.Response.ContentLength > 0)
        {
            var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
            _logger.LogInformation($"Response body: {responseBody}");
        }
    }
}