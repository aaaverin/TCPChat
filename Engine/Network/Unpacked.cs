﻿using Engine.Network.Connections;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security;
using System.Security.Permissions;

namespace Engine.Network
{
  [Serializable]
  public struct Unpacked<T> : IPoolable, ISerializable
      where T : IPackage
  {
    private readonly Packer owner;
    private readonly MemoryStream stream;

    private readonly T package;
    private readonly byte[] rawData;

    public T Package
    {
      [SecuritySafeCritical]
      get { return package; }
    }

    public byte[] RawData
    {
      [SecuritySafeCritical]
      get
      {
        if (rawData != null)
          return rawData;
        if (stream != null)
          return stream.GetBuffer();
        return null;
      }
    }

    public int RawLength
    {
      [SecuritySafeCritical]
      get
      {
        if (rawData != null)
          return rawData.Length;
        if (stream != null)
          return (int)stream.Length;
        return 0;
      }
    }

    MemoryStream IPoolable.Stream
    {
      [SecuritySafeCritical]
      get { return stream; }
    }

    [SecurityCritical]
    public Unpacked(Packer owner, T package, MemoryStream stream)
    {
      this.owner = owner;
      this.stream = stream;
      this.rawData = null;
      this.package = package;
    }

    [SecurityCritical]
    public Unpacked(T package, byte[] rawData)
    {
      this.owner = null;
      this.stream = null;
      this.rawData = rawData;
      this.package = package;
    }

    [SecurityCritical]
    private Unpacked(SerializationInfo info, StreamingContext context)
    {
      owner = null;
      stream = null;
      rawData = (byte[]) info.GetValue("rawData", typeof(byte[]));
      package = (T) info.GetValue("Package", typeof(T));
    }

    [SecuritySafeCritical]
    public void Dispose()
    {
      if (owner != null)
        owner.Release(this);
    }

    [SecurityCritical]
    [SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
    public void GetObjectData(SerializationInfo info, StreamingContext context)
    {
      byte[] rawDataArray = null;
      if (rawData != null)
      {
        rawDataArray = rawData;
      }
      else if (stream != null)
      {
        rawDataArray = new byte[(int)stream.Length];
        Array.Copy(stream.GetBuffer(), rawDataArray, rawDataArray.Length);
      }

      info.AddValue("rawData", rawDataArray, typeof(byte[]));
      info.AddValue("Package", Package, typeof(T));
    }
  }
}
