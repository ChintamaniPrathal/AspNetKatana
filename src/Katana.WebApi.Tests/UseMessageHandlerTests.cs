﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Gate.Builder;
using Owin;
using Shouldly;
using Xunit;

namespace Katana.WebApi.Tests
{
    public class UseMessageHandlerTests
    {
        [Fact]
        public void MessageHandlerWillBeCreatedByAppBuilder()
        {
            var builder = new AppBuilder();

            builder.UseMessageHandler<HelloWorldHandler>();

            var app = builder.Materialize<HttpMessageHandler>();


            app.ShouldBeTypeOf<HelloWorldHandler>();
            var handler = (HelloWorldHandler)app;
            handler.CtorTwoCalled.ShouldBe(true);
        }

        [Fact]
        public Task CallingAppDelegateShouldInvokeMessageHandler()
        {
            var builder = new AppBuilder();

            builder.UseMessageHandler<HelloWorldHandler>();

            var app = builder.Materialize<AppDelegate>();

            CallParameters call = new CallParameters();
            call.Environment = new Dictionary<string, object>
                          {
                              {"owin.Version", "1.0"},
                              {"owin.RequestMethod", "GET"},
                              {"owin.RequestScheme", "http"},
                              {"owin.RequestPathBase", ""},
                              {"owin.RequestPath", "/"},
                              {"owin.RequestQueryString", ""},
                          };
            call.Headers = new Dictionary<string, string[]>();

            return app.Invoke(call).Then(
                result =>
                {
                    result.Status.ShouldBe(200);
                    result.Headers["Content-Type"].ShouldBe(new[] { "text/plain; charset=utf-8" });
                });
        }

        [Fact]
        public Task CallingMessageHandlerShouldInvokeAppDelegate()
        {
            var builder = new AppBuilder();

            builder.UseMessageHandler<PassThroughHandler>();
            builder.Use<AppDelegate>(
                appDelegate =>
                {
                    return appCall =>
                    {
                        appCall.Headers.ShouldNotBe(null);
                        appCall.Environment.ShouldNotBe(null);

                        ResultParameters result = new ResultParameters();
                        result.Status = 200;
                        result.Headers = new Dictionary<string, string[]>() 
                            { {"Content-Type", new string[] { "text/plain; charset=utf-8"} } } ;
                        result.Properties = new Dictionary<string, object>();
                        result.Body = null;
                        return TaskHelpers.FromResult(result);
                    };
                });

            var app = builder.Materialize<AppDelegate>();

            CallParameters call = new CallParameters();
            call.Environment = new Dictionary<string, object>
                          {
                              {"owin.Version", "1.0"},
                              {"owin.RequestMethod", "GET"},
                              {"owin.RequestScheme", "http"},
                              {"owin.RequestPathBase", ""},
                              {"owin.RequestPath", "/"},
                              {"owin.RequestQueryString", ""},
                          };
            call.Headers = new Dictionary<string, string[]>();

            return app.Invoke(call).Then(
                result =>
                {
                    result.Status.ShouldBe(200);
                    result.Headers["Content-Type"].ShouldBe(new[] { "text/plain; charset=utf-8" });
                });
        }
    }
}
