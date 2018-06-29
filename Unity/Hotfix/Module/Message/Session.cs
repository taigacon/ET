using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BK;

namespace BKHotfix
{
	/// <summary>
	/// 用来收发热更层的消息
	/// </summary>
	public class Session: Entity
	{
		public BK.Session session;

		private static int RpcId { get; set; }
		private readonly Dictionary<int, Action<IResponse>> requestCallback = new Dictionary<int, Action<IResponse>>();

		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}
			
			base.Dispose();

			foreach (Action<IResponse> action in this.requestCallback.Values.ToArray())
			{
				action.Invoke(new ResponseMessage { Error = this.session.Error });
			}

			this.requestCallback.Clear();

			this.session.Dispose();
		}

		public void Run(BK.Session s, Packet packet)
		{
			ushort opcode = packet.Opcode;
			byte flag = packet.Flag;

			OpcodeTypeComponent opcodeTypeComponent = Game.OpcodeTypeComponent;
			Type responseType = opcodeTypeComponent.GetType(opcode);
			object message = this.session.Network.MessagePacker.DeserializeFrom(responseType, packet.Stream);

			if ((flag & 0x01) > 0)
			{
				IResponse response = message as IResponse;
				if (response == null)
				{
					throw new Exception($"flag is response, but hotfix message is not! {opcode}");
				}
				
				Action<IResponse> action;
				if (!this.requestCallback.TryGetValue(response.RpcId, out action))
				{
					return;
				}
				this.requestCallback.Remove(response.RpcId);

				action(response);
				return;
			}

			Game.MessageDispatherComponent.Handle(session, new MessageInfo(opcode, message));
		}

		public void Send(IMessage message)
		{
			Send(0x00, message);
		}

		public void Send(byte flag, IMessage message)
		{
			ushort opcode = Game.OpcodeTypeComponent.GetOpcode(message.GetType());
			byte[] bytes = ProtobufHelper.ToBytes(message);
			session.Send(flag, opcode, bytes);
		}

		public void Send(byte flag, ushort opcode, byte[] bytes)
		{
			session.Send(flag, opcode, bytes);
		}

		public Task<IResponse> Call(IRequest request)
		{
			int rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				try
				{
					if (ErrorCode.IsRpcNeedThrowException(response.Error))
					{
						throw new RpcException(response.Error, response.Message);
					}

					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {request.GetType().FullName}", e));
				}
			};

			request.RpcId = rpcId;
			
			this.Send(0x00, request);
			return tcs.Task;
		}

		public Task<IResponse> Call(IRequest request, CancellationToken cancellationToken)
		{
			int rpcId = ++RpcId;
			var tcs = new TaskCompletionSource<IResponse>();

			this.requestCallback[rpcId] = (response) =>
			{
				try
				{
					if (ErrorCode.IsRpcNeedThrowException(response.Error))
					{
						throw new RpcException(response.Error, response.Message);
					}

					tcs.SetResult(response);
				}
				catch (Exception e)
				{
					tcs.SetException(new Exception($"Rpc Error: {request.GetType().FullName}", e));
				}
			};

			cancellationToken.Register(() => { this.requestCallback.Remove(rpcId); });

			request.RpcId = rpcId;

			this.Send(0x00, request);
			return tcs.Task;
		}
	}
}
