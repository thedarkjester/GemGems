using Microsoft.Azure.WebJobs.Description;
using Microsoft.Azure.WebJobs.Host.Config;

namespace GemGems.Functions.Triggers.RPCTrigger
{
    [Extension("RPCConfigProvider")]
    public class RPCEventConfigProvider : IExtensionConfigProvider
    {
        public void Initialize(ExtensionConfigContext context)
        {
            context
                .AddBindingRule<RPCEventTriggerAttribute>()
                .BindToTrigger<RPCEventData>(new RPCEventTriggerBindingProvider());
        }
    }
}
