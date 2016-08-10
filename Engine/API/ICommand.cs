﻿using Engine.Network;
using Engine.Network.Connections;
using System;

namespace Engine.API
{
  public interface ICommand
  {
    long Id { get; }
  }

  public interface ICommand<in TArgs> : ICommand
    where TArgs : CommandArgs
  {
    void Run(TArgs args);
  }

  [Serializable]
  public class CommandArgs
  {
    public Unpacked<IPackage> Unpacked { get; private set; }

    public CommandArgs(Unpacked<IPackage> unpacked)
    {
      Unpacked = unpacked;
    }
  }

  [Serializable]
  public class ServerCommandArgs 
    : CommandArgs
  {
    public string ConnectionId { get; private set; }
    
    public ServerCommandArgs(string connectionId, Unpacked<IPackage> unpacked)
      : base(unpacked)
    {
      ConnectionId = connectionId;
    }
  }

  [Serializable]
  public class ClientCommandArgs 
    : CommandArgs
  {
    public string PeerConnectionId { get; private set; }

    public ClientCommandArgs(string peerConnectionId, Unpacked<IPackage> unpacked)
      : base(unpacked)
    {
      PeerConnectionId = peerConnectionId;
    }
  }
}