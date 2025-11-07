//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

using System;
using System.ServiceModel;
using System.Threading.Tasks;
using CoreWCF.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CoreWCF;
using CoreWCF.Configuration;
using CoreWCF.Description;

namespace Microsoft.Samples.BasicHttpService
{
    class Program
    {
        static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Configure Kestrel to listen on port 8000
            builder.WebHost.ConfigureKestrel(options =>
            {
                options.ListenLocalhost(8000);
            });

            // Add CoreWCF services
            builder.Services.AddServiceModelServices();
            builder.Services.AddServiceModelWebServices();
            builder.Services.AddServiceModelMetadata();
            builder.Services.AddSingleton<IServiceBehavior, UseRequestHeadersForMetadataAddressBehavior>();

            var app = builder.Build();

            // Configure CoreWCF endpoints
            ((IApplicationBuilder)app).UseServiceModel(serviceBuilder =>
            {
                serviceBuilder.AddService<Service>();
                serviceBuilder.AddServiceWebEndpoint<Service, IService>(new WebHttpBinding(), "/");
            });

            Uri baseAddress = new Uri("http://localhost:8000");
            Console.WriteLine("Service is hosted at: " + baseAddress.AbsoluteUri);
            Console.WriteLine("Service help page is at: " + baseAddress.AbsoluteUri + "help");

            // Start the application in a background task
            var runTask = Task.Run(() => app.RunAsync());

            // Wait for the service to be ready
            Task.Delay(1000).Wait();

            // Client code to test the service
            var binding = new System.ServiceModel.BasicHttpBinding();
            var endpoint = new System.ServiceModel.EndpointAddress(baseAddress);
using (var cf = new System.ServiceModel.ChannelFactory<IService>(binding, endpoint))
            {
                IService channel = cf.CreateChannel();

                string s;

                Console.WriteLine("Calling EchoWithGet via HTTP GET: ");
                s = channel.EchoWithGet("Hello, world");
                Console.WriteLine("   Output: {0}", s);

                Console.WriteLine("");
                Console.WriteLine("This can also be accomplished by navigating to");
                Console.WriteLine("http://localhost:8000/EchoWithGet?s=Hello, world!");
                Console.WriteLine("in a Web browser while this sample is running.");

                Console.WriteLine("");

                Console.WriteLine("Calling EchoWithPost via HTTP POST: ");
                s = channel.EchoWithPost("Hello, world");
                Console.WriteLine("   Output: {0}", s);

                Console.WriteLine("");
            }

            Console.WriteLine("Press any key to terminate");
            Console.ReadLine();

            app.Lifetime.StopApplication();
            runTask.Wait();
        }
    }
}