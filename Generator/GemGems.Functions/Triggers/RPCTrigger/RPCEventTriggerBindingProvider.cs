using Microsoft.Azure.WebJobs.Host.Triggers;
using System.Reflection;
using System.Threading.Tasks;

namespace GemGems.Functions.Triggers.RPCTrigger
{
    public class RPCEventTriggerBindingProvider : ITriggerBindingProvider
    {
        public Task<ITriggerBinding> TryCreateAsync(TriggerBindingProviderContext context)
        {
            var result = Task.FromResult<ITriggerBinding>(default);
            var attribute = context.Parameter.GetCustomAttribute<RPCEventTriggerAttribute>(false);

            if (attribute != null)
            {
                result = Task.FromResult<ITriggerBinding>(new RPCEventTriggerBinding(attribute));
            }

            return result;
        }
    }
}
