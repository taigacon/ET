﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace BK
{
	public sealed class Session : Entity
	{
		private static int RpcId { get; set; }
		private AChannel channel;
		public int Error;

		private readonly Dictionary<int, Action<IResponse>> requestCallback = new Dictionary<int, Action<IResponse>>();
		private readonly List<byte[]> byteses = new List<byte[]>() { new byte[1], new byte[0], new byte[0]};

		public NetworkComponent Network { get; private set; }

		public void Awake(NetworkComponent net, AChannel aChannel)
		{
			this.Network = net;
			this.Error = 0;
			this.channel = aChannel;
			this.requestCallback.Clear();
			ulong id = this.InstanceId;
			channel.ErrorCallback += (c, e) =>
			{
				this.Error = e;
				this.Network.Remove(id); 
			};
			channel.ReadCallback += this.OnRead;
			
			this.channel.Start();
		}
		public override void Dispose()
		{
			if (this.IsDisposed)
			{
				return;
			}

			ulong id = this.InstanceId;

			base.Dispose();
			
			foreach (Action<IResponse> action in this.requestCallback.Values.ToArray())
			{
				action.Invoke(new ResponseMessage { Error = this.Error });
			}

			if (this.Error != 0)
			{
				Log.Error($"session dispose: {id} {this.Error}");
			}

			this.Error = 0;
			this.channel.Dispose();
			this.Network.Remove(id);
			this.requestCallback.Clear();
		}

		public IPEndPoint RemoteAddress
		{
			get
			{
				return this.channel.RemoteAddress;
			}
		}

		public ChannelType ChannelType
		{
			get
			{
				return this.channel.ChannelType;
			}
		}

		public void OnRead(Packet packet)
		{
			try
			{
				this.Run(packet);
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		private async void Run(Packet packet)
		{
			//交由Unity线程执行
			await Awaiters.NextFrame;

			byte flag = packet.Flag;
			ushort opcode = packet.Opcode;
			
#if !SERVER
			if (OpcodeHelper.IsClientHotfixMessage(opcode))
			{
				this.Network.MessageDispatcher.Dispatch(this, packet);
				return;
			}
#endif

			// flag第一位为1表示这是rpc返回消息,否则交由MessageDispatcher分发
			if ((flag & 0x01) == 0)
			{
				this.Network.MessageDispatcher.Dispatch(this, packet);
				return;
			}

			object message;
			try
			{
				OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
				Type responseType = opcodeTypeComponent.GetType(opcode);
				message = this.Network.MessagePacker.DeserializeFrom(responseType, packet.Stream);
				//Log.Debug($"recv: {JsonHelper.ToJson(message)}");
			}
			catch (Exception e)
			{
				// 出现任何消息解析异常都要断开Session，防止客户端伪造消息
				Log.Error($"opcode: {opcode} {this.Network.Count} {e} ");
				this.Error = ErrorCode.ERR_PacketParserError;
				this.Network.Remove(this.InstanceId);
				return;
			}
				
			IResponse response = message as IResponse;
			if (response == null)
			{
				throw new Exception($"flag is response, but message is not! {opcode}");
			}
			Action<IResponse> action;
			if (!this.requestCallback.TryGetValue(response.RpcId, out action))
			{
				return;
			}
			this.requestCallback.Remove(response.RpcId);

			action(response);
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

			cancellationToken.Register(() => this.requestCallback.Remove(rpcId));

			request.RpcId = rpcId;
			this.Send(0x00, request);
			return tcs.Task;
		}

		public void Send(IMessage message)
		{
			this.Send(0x00, message);
		}

		public void Reply(IResponse message)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}

			this.Send(0x01, message);
		}

		public void Send(byte flag, IMessage message)
		{
			OpcodeTypeComponent opcodeTypeComponent = this.Network.Entity.GetComponent<OpcodeTypeComponent>();
			ushort opcode = opcodeTypeComponent.GetOpcode(message.GetType());
			byte[] bytes = this.Network.MessagePacker.SerializeToByteArray(message);

			Send(flag, opcode, bytes);
		}

		public void Send(byte flag, ushort opcode, byte[] bytes)
		{
			if (this.IsDisposed)
			{
				throw new Exception("session已经被Dispose了");
			}
			this.byteses[0][0] = flag;
			this.byteses[1] = BitConverter.GetBytes(opcode);
			this.byteses[2] = bytes;

#if SERVER
			// 如果是allserver，内部消息不走网络，直接转给session,方便调试时看到整体堆栈
			if (this.Network.AppType == AppType.AllServer)
			{
				Session session = this.Network.Entity.GetComponent<NetInnerComponent>().Get(this.RemoteAddress);

				Packet packet = ((TChannel)this.channel).parser.packet;

				packet.Flag = flag;
				packet.Opcode = opcode;
				packet.Stream.Seek(0, SeekOrigin.Begin);
				packet.Stream.SetLength(bytes.Length);
				Array.Copy(bytes, 0, packet.Bytes, 0, bytes.Length);
				session.Run(packet);
				return;
			}
#endif

			channel.Send(this.byteses);
		}
	}
}