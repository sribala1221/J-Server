using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Linq;
using System.Runtime.CompilerServices;


namespace ServerAPI.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void AddScopedImplementations(this IServiceCollection services)
        {
            // ReSharper disable once PossibleNullReferenceException
            foreach (Type type in Assembly.GetEntryAssembly().GetTypes()
                .Where(t => t.Namespace == "ServerAPI.Services")
                .Where(t => !t.GetTypeInfo().IsDefined(typeof(CompilerGeneratedAttribute), true))
                .Where(t => t.GetTypeInfo().IsClass))
            {
                services.AddScoped(type.GetTypeInfo().GetInterface("I" + type.Name), type);
            }
        }

    }
}
