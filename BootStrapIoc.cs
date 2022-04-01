using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
namespace TestUrl
{
    public class BootStrapIoc
    {
        private static IServiceCollection serviceCollection { get; } = new ServiceCollection();
        public static void Init()
        {
            IConfiguration configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", true, true).Build();
            serviceCollection.AddSingleton(configuration);
            serviceCollection.AddSingleton<IMethod, MyMethod>();
            serviceCollection.AddDbContext<MyDataContext>();
            serviceCollection.AddLogging(configure =>
            {
                configure.AddConfiguration(configuration.GetSection("Logging"));
                configure.AddConsole();
            });
        }
        public static T GetServices<T>()
        {
            return serviceCollection.BuildServiceProvider().GetService<T>();
        }
    }
}
