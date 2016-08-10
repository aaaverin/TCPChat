﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Security;

namespace Engine.Network
{
  public class Pool
  {
    private const int DefaultSize = 2048;
    
    public struct Guard : IDisposable
    {
      private readonly Pool owner;
      public MemoryStream Stream { get; private set; }

      [SecurityCritical]
      public Guard(Pool guardOwner, MemoryStream stream)
      {
        owner = guardOwner;
        Stream = stream;
      }

      [SecuritySafeCritical]
      public void Dispose()
      {
        owner.Put(Stream);
      }
    }

    private struct Container : IEquatable<Container>
    {
      public readonly MemoryStream Data;
      public readonly int Size;

      [SecurityCritical]
      public Container(MemoryStream data)
      {
        Data = data;
        Size = data.Capacity;
      }

      [SecurityCritical]
      public Container(int size)
      {
        Data = null;
        Size = size;
      }

      [SecuritySafeCritical]
      public override bool Equals(object obj)
      {
        if (ReferenceEquals(obj, null))
          return false;

        if (!(obj is Container))
          return false;

        return Equals((Container)obj);
      }

      [SecuritySafeCritical]
      public bool Equals(Container other)
      {
        return Size == other.Size;
      }

      [SecuritySafeCritical]
      public override int GetHashCode()
      {
        return Size;
      }
    }

    private class Comparer : IComparer<Container>
    {
      [SecuritySafeCritical]
      public int Compare(Container x, Container y)
      {
        return x.Size.CompareTo(y.Size);
      }
    }

    private readonly int maxSize;
    private readonly List<Container> storage;
    private readonly Comparer comaprer;
    
    [SecurityCritical]
    public Pool(int maxPoolSize)
    {
      maxSize = maxPoolSize;
      storage = new List<Container>(maxSize);
      comaprer = new Comparer();
    }
    
    [SecurityCritical]
    public Guard GetWithGuard(int? size = null)
    {
      return new Guard(this, Get(size));
    }
    
    [SecurityCritical]
    public MemoryStream Get(int? size = null)
    {
      lock (storage)
      {
        var streamSize = size ?? DefaultSize;
        var index = storage.BinarySearch(new Container(streamSize), comaprer);
        if (index < 0)
        {
          index = ~index;
          index++;

          if (index >= storage.Count)
          {
            if (size == null && storage.Count >= 1)
              index = 0;
            else
              return new MemoryStream(streamSize);
          }
        }

        var result = storage[index];
        storage.RemoveAt(index);
        return result.Data;
      }
    }
    
    [SecurityCritical]
    public void Put(MemoryStream data)
    {
      lock (storage)
      {
        if (storage.Count < maxSize)
        {
          data.Position = 0;
          data.SetLength(0);

          var container = new Container(data);
          var index = storage.BinarySearch(container, comaprer);
          if (index >= 0)
            storage.Insert(index, container);
          else
            storage.Insert(~index, container);
        }
      }
    }
  }
}
