using System;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.MemoryStorage;
using JobShedulingViaMq.ConfigModels;
using JobShedulingViaMq.Helpers.Abstraction;
using JobShedulingViaMq.Helpers.Concretion;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace JobShedulingViaMq
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<IMqServices, MqServices>();
            
            services.AddHangfire(config =>
            {
                config.SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                    .UseSimpleAssemblyNameTypeSerializer()
                    .UseDefaultTypeSerializer()
                    .UseMemoryStorage();
            });
            services.AddHangfireServer();

            services.AddOptions();
            services.Configure<JobOptions>(Configuration.GetSection(nameof(JobOptions)));
            services.Configure<RabbitMqOptions>(Configuration.GetSection(nameof(RabbitMqOptions)));
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IRecurringJobManager recurringJobManager,
            IOptions<JobOptions> jobOptions, IMqServices mqServices)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var jobs = jobOptions.Value;
            app.UseHangfireServer();
            app.UseHangfireDashboard("/dash");
            foreach (var job in jobs.Options)
            {
                recurringJobManager.AddOrUpdate(job.Name,
                    () => mqServices.PublishExecutingJob(job.Name),
                    job.CronExpression.Trim());
            }
        }
    }
}