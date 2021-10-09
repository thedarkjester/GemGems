using Microsoft.Azure.WebJobs.Host.Executors;
using Microsoft.Azure.WebJobs.Host.Listeners;
using Nethereum.ABI.FunctionEncoding;
using Nethereum.Contracts;
using Nethereum.Hex.HexTypes;
using Nethereum.RPC.Eth.DTOs;
using Nethereum.Web3;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GemGems.Functions.Triggers.RPCTrigger
{
    public class RPCEventListener : IListener
    {
        private const int EventPollingDelay = 1000;

        private readonly ITriggeredFunctionExecutor _executor;
        private readonly RPCEventTriggerAttribute _attribute;
        private CancellationTokenSource _cts = null;
        private Event _event = null;
        private HexBigInteger _filter = null;

        public RPCEventListener(ITriggeredFunctionExecutor executor, RPCEventTriggerAttribute attribute)
        {
            _executor = executor;
            _attribute = attribute;
        }

        public void Cancel()
        {
            StopAsync(CancellationToken.None).Wait();
        }

        //TODO
        public void Dispose() { }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            var web3 = new Web3(_attribute.Url);
            var contract = web3.Eth.GetContract(_attribute.Abi, _attribute.ContractAddress);

            _event = contract.GetEvent(_attribute.EventName);
            _filter = await _event.CreateFilterAsync();

            ListenAsync(_cts.Token);
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _cts.Cancel();

            return Task.CompletedTask;
        }

        private async void ListenAsync(CancellationToken cancellationToken)
        {
            await Task.Run(async () =>
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    var eventsData = await _event.GetFilterChangeDefaultAsync(_filter);

                    ProcessEvents(eventsData, cancellationToken);
                    await Task.Delay(EventPollingDelay);
                }
            });
        }

        private void ProcessEvents(List<EventLog<List<ParameterOutput>>> eventsData, CancellationToken cancellationToken)
        {
            eventsData
                .Select(eventData => ExtractEventData(eventData.Event, eventData.Log))
                .ToList()
                .ForEach(rpcEventData => _executor.TryExecuteAsync(new TriggeredFunctionData { TriggerValue = rpcEventData }, cancellationToken));
        }

        private RPCEventData ExtractEventData(List<ParameterOutput> eventParams, FilterLog log)
        {
            Dictionary<string, string> values = eventParams.ToDictionary(eventParam => eventParam.Parameter.Name, eventParam => eventParam.Result.ToString());

            return new RPCEventData
            {
                Values = values,
                BlockNumber = log.BlockNumber
            };
        }
    }
}
