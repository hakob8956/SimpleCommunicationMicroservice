using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace SimpleService
{
    public class CommonMethodsContainer
    {
        public static Dictionary<string, Tuple<Type, Type>> GetHandlerMethods()
        {
            return GetHandlerMethods(typeof(CommonMethodsContainer).Assembly);
        }
        private static Dictionary<string, Tuple<Type, Type>> GetHandlerMethods(Assembly assembly)
        {
            var handlers = assembly.GetTypes()
                 .Where(t => t.GetInterfaces().Any(x => x.IsGenericType
                && x.Name == "IRequestHandler`2"
                 )
                 && t.Name.EndsWith("Handler")).Select(t => new
                 {
                     Key = t.Name.Substring(0, t.Name.IndexOf("Handler")),
                     RequestType = t.GetInterfaces()
                 .Where(x => x.IsGenericType && x.Name == "IRequestHandler`2")
                 .First()
                 .GetGenericArguments()
                 .First(),

                     ResponseType = t.GetInterfaces()
                 .Where(x => x.IsGenericType && x.Name == "IRequestHandler`2")
                 .First()
                 .GetGenericArguments()
                 .Skip(1)
                 .First()
                 })
                 .ToList();

            return handlers
             .ToDictionary(kvp => kvp.Key, kvp => Tuple.Create(kvp.RequestType, kvp.ResponseType));
        }
    }
}
