using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace HealthChecks.SomeModelService.Health
{
    public class ApiHealthCheck : IHealthCheck
    {
        public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = new CancellationToken())
        {
            //we're looking to see if a file called "MyApiLicenseFile.txt exists in the
            //root directory of our API.  If not, then this is unhealthy.
            //If the file does exist, then all good and healthy.
            var basePath = AppDomain.CurrentDomain.BaseDirectory;
            var isHealthy = File.Exists($"{basePath}MyApiLicenseFile.txt");
            if(isHealthy)
            {
                return Task.FromResult(HealthCheckResult.Healthy("I am one healthy microservice API"));
            }

            return Task.FromResult(HealthCheckResult.Unhealthy("I am the sad, unhealthy microservice API.  Cannot find my license file 'MyApiLicenseFile.txt'"));
        }
    }
}