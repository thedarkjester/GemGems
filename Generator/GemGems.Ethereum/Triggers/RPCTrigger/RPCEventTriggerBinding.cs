using Microsoft.Azure.WebJobs.Host.Bindings;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Microsoft.Azure.WebJobs.Host.Protocols;
using Microsoft.Azure.WebJobs.Host.Triggers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GemGems.Functions.Triggers.RPCTrigger
{
    internal class RPCEventTriggerBinding : ITriggerBinding
    {
        private readonly RPCEventTriggerAttribute _attribute;

        public RPCEventTriggerBinding(RPCEventTriggerAttribute attribute)
        {
            _attribute = attribute;
        }

        public Type TriggerValueType => typeof(RPCEventData);

        public IReadOnlyDictionary<string, Type> BindingDataContract => new Dictionary<string, Type>();

        public Task<ITriggerData> BindAsync(object value, ValueBindingContext context)
        {
            return Task.FromResult<ITriggerData>(new TriggerData(null, new Dictionary<string, object>()));
        }

        public Task<IListener> CreateListenerAsync(ListenerFactoryContext context)
        {
            return Task.FromResult<IListener>(new RPCEventListener(context.Executor, _attribute));
        }

        public ParameterDescriptor ToParameterDescriptor()
        {
            return default;
        }
    }
}
