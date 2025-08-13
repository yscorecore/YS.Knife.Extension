using System.Reflection;

namespace YS.Knife.Operations
{
    public static class MethodInfoExtensions
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, Operation> operationCaches = new System.Collections.Concurrent.ConcurrentDictionary<MethodInfo, Operation>();
        public static Operation GetOperation(this MethodInfo methodInfo)
        {
            return operationCaches.GetOrAdd(methodInfo, p =>
            {
                var attr = methodInfo.GetCustomAttribute<OperationAttribute>();
                return attr == null ? new Operation { Id = methodInfo.Name } : new Operation { Id = attr.Id, Description = attr.Description };
            });
        }
    }
}
