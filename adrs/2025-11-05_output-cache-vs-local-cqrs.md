# ADR: Microsoft Output Cache middleware in our local CQRS execution support

**Status:** accepted  
**Date:** 2025.11.05  
**Decision-makers:** Emil Dragańczuk  
**Consulted:** Jakub Fijałkowski

## We drop support for CQRS local execution when using the Output Caching middleware

### Context and Problem Statement

We aim to use ASP.NET Core's built in [OutputCaching middleware](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-9.0) in our CQRS pipeline, because it seems to handle caching gracefully, with somewhat deep integration with the HTTP context. However, in our local CQRS execution we have a quite simple implementation of the HttpContext ([Local CallContext](https://github.com/leancodepl/corelibrary/tree/v9.0-preview/src/CQRS/LeanCode.CQRS.AspNetCore/Local/Context)), missing many parts that OutputCaching middleware expects.

### Considered Options

* Enhance our implementation of our `LocalCallContext`
* Drop support for CQRS local execution when using the Output Caching middleware

### Decision Outcome

Chosen option: "Drop support for CQRS local execution when using the Output Caching middleware", because the OutputCaching middleware is too deeply integrated with the HTTP implementation of the `HttpContext` and mimicking it locally is too big of task, possibly with many caveats.

#### Consequences

* Good, because of the simplicity of introducing proper caching in our API and handling the main case - caching HTTP pipeline.
* Bad, because now there explicitly exists our first supported middleware that is not supported in our CQRS local execution pipeline.

## More Information

Snippet of verified conversation with Sonnet 4.5 about the deepness of dependency of the Output Caching middleware on the proper HTTP implementation of the `HttpContext`:

## Detailed Explanation with Code References

Based on the actual `OutputCacheMiddleware.cs` from the ASP.NET Core repository, here's exactly what the middleware requires and why `NullHttpResponse` fails:

### 1. **Stream Wrapping/Shimming** (Lines 464-472)

```csharp
internal void ShimResponseStream(OutputCacheContext context)
{
    // Shim response stream
    context.OriginalResponseStream = context.HttpContext.Response.Body;
    context.OutputCacheStream = new OutputCacheStream(
        context.OriginalResponseStream,
        _options.MaximumBodySize,
        StreamUtilities.BodySegmentSize,
        () => StartResponse(context));
    context.HttpContext.Response.Body = context.OutputCacheStream;
}
```

**What it does:** The middleware **replaces** `HttpResponse.Body` with a custom `OutputCacheStream` wrapper. This wrapper:

* Buffers all writes in memory (via `RecyclableSequenceBuilder`)
* Calls `_startResponseCallback()` on every write operation
* Disables buffering if the body exceeds `MaximumBodySize`

**Why** `NullHttpResponse` fails: Your `NullHttpResponse.Body` returns `Stream.Null`, and the setter is a no-op:

```csharp
public override Stream Body
{
    get => Stream.Null;
    set { }  // ❌ Assignment ignored!
}
```

When the middleware tries to replace the Body stream, nothing happens. Later, when your CQRS finalizer writes to `httpContext.Response.Body` (line 44 in `HttpContextExtensions.cs`), it's writing to `Stream.Null` instead of the wrapped `OutputCacheStream`, so **nothing gets buffered**.

---

### 2. **Response Started Callback** (Lines 152-164 in `InvokeAwaited`)

```csharp
async Task<OutputCacheEntry?> ExecuteResponseAsync()
{
    // Hook up to listen to the response stream
    ShimResponseStream(context);

    try
    {
        await _next(httpContext);  // ← Your CQRS middlewares run here

        // The next middleware might change the policy
        foreach (var policy in policies)
        {
            await policy.ServeResponseAsync(context, httpContext.RequestAborted);
        }

        // If there was no response body, check the response headers now...
        StartResponse(context);
```

The `StartResponse(context)` call (line 163) eventually invokes `OnStartResponse` (lines 441-451):

```csharp
private bool OnStartResponse(OutputCacheContext context)
{
    if (!context.ResponseStarted)
    {
        context.ResponseStarted = true;
        context.ResponseTime = _options.TimeProvider.GetUtcNow();
        return true;
    }
    return false;
}
```

**What it does:** This tracks when the response **starts** (first write or explicit call). It sets `context.ResponseTime` and marks `ResponseStarted = true`.

**Why it matters:** The middleware needs to know the exact moment headers/status are finalized. In your local context, `HttpResponse.HasStarted` is always `false` (because `NullHttpResponse` doesn't track state), so the middleware can't distinguish between "headers not sent" and "response completed."

---

### 3. **Status Code & Header Mutation** (Lines 451-460)

```csharp
internal void FinalizeCacheHeaders(OutputCacheContext context)
{
    if (context.AllowCacheStorage)
    {
        // Create the cache entry now
        var response = context.HttpContext.Response;
        var headers = response.Headers;

        context.CachedResponseValidFor = context.ResponseExpirationTimeSpan ?? _options.DefaultExpirationTimeSpan;

        // Setting the date on the raw response headers.
        headers.Date = HeaderUtilities.FormatDate(context.ResponseTime!.Value);
```

**What it does:** Modifies `Response.StatusCode` and `Response.Headers` (adding `Date`, `Age`, etc.) before writing the body.

**Why** `NullHttpResponse` fails: Your stub ignores all mutations:

```csharp
public override int StatusCode
{
    get => 0;
    set { }  // ❌ Silently discarded
}

public override IHeaderDictionary Headers => NullHeaderDictionary.Empty;  // ❌ Read-only stub
```

So even though the middleware sets headers like `Date` and `Age`, they disappear into the void.

---

### 4. **OnStarting/OnCompleted Callbacks** (Not explicitly shown but required by IHttpResponseFeature)

Although not directly visible in the middleware code, ASP.NET Core's response pipeline relies on `IHttpResponseFeature`:

```csharp
public interface IHttpResponseFeature
{
    int StatusCode { get; set; }
    string? ReasonPhrase { get; set; }
    IHeaderDictionary Headers { get; set; }
    Stream Body { get; set; }
    bool HasStarted { get; }

    void OnStarting(Func<object, Task> callback, object state);
    void OnCompleted(Func<object, Task> callback, object state);
}
```

The output cache middleware (and other middleware like compression) registers callbacks via `OnStarting` to finalize headers before the first write. Your `NullHttpResponse` doesn't expose these hooks (it's just a bare `HttpResponse` override with no feature backing).

---

### 5. **Body Buffering for Cache Storage** (Lines 420-436)

```csharp
internal async ValueTask FinalizeCacheBodyAsync(OutputCacheContext context)
{
    if (context.AllowCacheStorage && context.OutputCacheStream.BufferingEnabled
        && context.CachedResponse is not null)
    {
        // ...
        var cachedResponseBody = context.OutputCacheStream.GetCachedResponseBody();

        if (!contentLength.HasValue || contentLength == cachedResponseBody.Length
            || (cachedResponseBody.Length == 0 && HttpMethods.IsHead(context.HttpContext.Request.Method)))
        {
            // transfer lifetime from the buffer to the cached response
            context.CachedResponse.SetBody(cachedResponseBody, recycleBuffers: true);
```

**What it does:** After the pipeline completes, the middleware calls `OutputCacheStream.GetCachedResponseBody()` to retrieve the buffered response body as a `ReadOnlySequence<byte>`.

**Why it fails locally:** Since `Response.Body` was never replaced (setter is no-op), `context.OutputCacheStream` was never actually used. When `GetCachedResponseBody()` is called, it returns an empty buffer even though your CQRS finalizer wrote data to `Stream.Null`.

---

### 6. **Serving Cached Responses** (Lines 305-328)

```csharp
// Copy the cached response body
var body = context.CachedResponse.Body;

if (!body.IsEmpty)
{
    try
    {
        await context.CachedResponse.CopyToAsync(response.BodyWriter, context.HttpContext.RequestAborted);
    }
    catch (OperationCanceledException)
    {
        context.HttpContext.Abort();
    }
}
```

**What it does:** When serving from cache, the middleware writes directly to `response.BodyWriter` (a `PipeWriter` exposed by `IHttpResponseBodyFeature`).

**Why it matters:** If you later implement a real local response, you'll need to provide a working `PipeWriter` (typically backed by a `Pipe` or `MemoryStream`-backed stream).

---

## Summary Table

| **Requirement** | **What Middleware Needs** | **What** `NullHttpResponse` Provides | **Result** |
| --- | --- | --- | --- |
| **Body Stream**  | Writable `Stream` that can be replaced (setter works)  | `Stream.Null` (getter); setter is no-op  | ❌ Writes disappear; no buffering  |
| **StatusCode**  | Mutable `int` property  | Always returns `0`; setter is no-op  | ❌ Status changes ignored  |
| **Headers**  | Mutable `IHeaderDictionary`  | Returns empty read-only stub  | ❌ Header mutations ignored  |
| **HasStarted**  | Real tracking of first write/flush  | Always `false` (not implemented)  | ❌ Can't detect when response begins  |
| **OnStarting**  | Callback registration before first write  | Not implemented (base throws `NotSupportedException`)  | ❌ Can't finalize headers  |
| **OnCompleted**  | Callback after response finishes  | Not implemented  | ❌ Can't clean up resources  |
| **BodyWriter**  | `PipeWriter` for efficient writes  | Not exposed (requires `IHttpResponseBodyFeature`)  | ❌ Can't serve cached responses efficiently  |

---

## Conclusion

The `OutputCacheMiddleware` fundamentally assumes it's running in a real ASP.NET Core pipeline with:

1. A **replaceable response body stream** (for buffering)
2. **Mutable status/headers** (for adding cache directives)
3. **Lifecycle hooks** (`OnStarting`/`OnCompleted`)
4. **State tracking** (`HasStarted`, `ResponseTime`)

Your `NullHttpResponse` provides **none of these**, so the middleware silently fails: it thinks it's caching responses, but the buffer is never populated, and all writes vanish into `Stream.Null`.

