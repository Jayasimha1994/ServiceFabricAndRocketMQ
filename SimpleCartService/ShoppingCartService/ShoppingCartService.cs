using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.ServiceFabric.Data.Collections;
using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using System.ServiceModel;
using Common;
using Microsoft.ServiceFabric.Services.Communication.Wcf.Runtime;
using Microsoft.ServiceFabric.Services.Communication.Wcf;

namespace ShoppingCartService
{
    internal sealed class ShoppingCartService : StatefulService, IShoppingCartService
    {
        public ShoppingCartService(StatefulServiceContext context)
            : base(context)
        { }
        public async Task AddItem(ShoppingCartItem item)
        {
            var cart = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCartItem>>("myCart");
            using (var tx = this.StateManager.CreateTransaction())
            {
                await cart.AddOrUpdateAsync(tx, item.ProductName, item, (k, v) => item);
                await tx.CommitAsync();
            }
        }
        public async Task DeleteItem(string productName)
        {
            var cart = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCartItem>>("myCart");
            using (var tx = this.StateManager.CreateTransaction())
            {
                var existing = await cart.TryGetValueAsync(tx, productName);
                if (existing.HasValue)
                    await cart.TryRemoveAsync(tx, productName);
                await tx.CommitAsync();
            }
        }
        public async Task<List<ShoppingCartItem>> GetItems()
        {
            var cart = await this.StateManager.GetOrAddAsync<IReliableDictionary<string, ShoppingCartItem>>("myCart");

            using (var tx = this.StateManager.CreateTransaction())
            {
                var ret = from t in (await cart.CreateEnumerableAsync(tx)).ToEnumerable() select t.Value;
                return ret.ToList();
            }
        }
        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                    new WcfCommunicationListener<IShoppingCartService>(
                        wcfServiceObject:this,
                        serviceContext:context,
                        endpointResourceName: "ServiceEndpoint",
                        listenerBinding: WcfUtility.CreateTcpListenerBinding()
                    )
            )};
        }
        private NetTcpBinding CreateListenBinding()
        {
            NetTcpBinding binding = new NetTcpBinding(SecurityMode.None)
            {
                SendTimeout = TimeSpan.MaxValue,
                ReceiveTimeout = TimeSpan.MaxValue,
                OpenTimeout = TimeSpan.FromSeconds(5),
                CloseTimeout = TimeSpan.FromSeconds(5),
                MaxConnections = int.MaxValue,
                MaxReceivedMessageSize = 1024 * 1024
            };
            binding.MaxBufferSize = (int)binding.MaxReceivedMessageSize;
            binding.MaxBufferPoolSize = Environment.ProcessorCount * binding.MaxReceivedMessageSize;

            return binding;
        }
    }
}
