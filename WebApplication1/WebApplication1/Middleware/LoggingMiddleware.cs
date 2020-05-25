using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WebApplication1.Middleware
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            httpContext.Request.EnableBuffering();

            if (httpContext.Request != null)
            {
                string httpMethod = httpContext.Request.Method.ToString();
                string path = httpContext.Request.Path;
                string queryString = httpContext.Request?.QueryString.ToString();
                string bodyString = "";

                using (StreamReader reader
                 = new StreamReader(httpContext.Request.Body, Encoding.UTF8, true, 1024, true))
                {
                    bodyString = await reader.ReadToEndAsync();
                    httpContext.Request.Body.Position = 0;
                }

                using (StreamWriter file = new StreamWriter(@"..\requestsLog.txt", true))
                {
                    file.WriteLine("" +
                        "Method: " + httpMethod +
                        "; Path: " + path +
                        "; QueryString: " + queryString +
                        "; Body: " + bodyString + ";");
                }
            }

            await _next(httpContext);
        }
    }
}
