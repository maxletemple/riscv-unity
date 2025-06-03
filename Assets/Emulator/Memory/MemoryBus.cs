using System;
using System.Collections.Generic;
using UnityEngine;


public class MemoryBus
{
    private List<(Reg64 startAdress, Reg64 endAddress, IMemoryType region)> memoryRegions;

    public MemoryBus()
    {
        memoryRegions = new List<(Reg64, Reg64, IMemoryType)>();
    }

    public void AddRegion(IMemoryType region, Reg64 startAddress, Reg64 endAddress)
    {
        memoryRegions.Add((startAddress, endAddress, region));
    }

    public void Write64(Reg64 address, Reg64 value)
    {
        foreach (var region in memoryRegions)
        {
            if (address >= region.startAdress && address < region.endAddress)
            {
                region.region.Write64(address - region.startAdress, value);
                return;
            }
        }
        throw new ArgumentOutOfRangeException("Address is out of bounds for any memory region.");
    }

    public Reg64 Read64(Reg64 address)
    {
        foreach (var region in memoryRegions)
        {
            if (address >= region.startAdress && address < region.endAddress)
            {
                return region.region.Read64(address - region.startAdress);
            }
        }
        throw new ArgumentOutOfRangeException("Address is out of bounds for any memory region.");
    }

    public void Write32(Reg64 address, uint value)
    {
        foreach (var region in memoryRegions)
        {
            if (address >= region.startAdress && address < region.endAddress)
            {
                region.region.Write32(address - region.startAdress, value);
                return;
            }
        }
        throw new ArgumentOutOfRangeException("Address is out of bounds for any memory region.");
    }

    public uint Read32(Reg64 address)
    {
        foreach (var region in memoryRegions)
        {
            if (address >= region.startAdress && address < region.endAddress)
            {
                return region.region.Read32(address - region.startAdress);
            }
        }
        throw new ArgumentOutOfRangeException("Address is out of bounds for any memory region.");
    }

    public void Write16(Reg64 address, ushort value)
    {
        foreach (var region in memoryRegions)
        {
            if (address >= region.startAdress && address < region.endAddress)
            {
                region.region.Write16(address - region.startAdress, value);
                return;
            }
        }
        throw new ArgumentOutOfRangeException("Address is out of bounds for any memory region.");
    }

    public ushort Read16(Reg64 address)
    {
        foreach (var region in memoryRegions)
        {
            if (address >= region.startAdress && address < region.endAddress)
            {
                return region.region.Read16(address - region.startAdress);
            }
        }
        throw new ArgumentOutOfRangeException("Address is out of bounds for any memory region.");
    }

    public void Write8(Reg64 address, byte value)
    {
        foreach (var region in memoryRegions)
        {
            if (address >= region.startAdress && address < region.endAddress)
            {
                region.region.Write8(address - region.startAdress, value);
                return;
            }
        }
        throw new ArgumentOutOfRangeException("Address is out of bounds for any memory region.");
    }

    public byte Read8(Reg64 address)
    {
        foreach (var region in memoryRegions)
        {
            if (address >= region.startAdress && address < region.endAddress)
            {
                return region.region.Read8(address - region.startAdress);
            }
        }
        throw new ArgumentOutOfRangeException("Address is out of bounds for any memory region.");
    }

    // public void LoadFromFile(string filePath, int startAddress)
    // {
    //     if (System.IO.File.Exists(filePath))
    //     {
    //         byte[] fileData = System.IO.File.ReadAllBytes(filePath);
    //         if (startAddress < 0 || startAddress + fileData.Length > memory.Length)
    //         {
    //             throw new ArgumentOutOfRangeException("Start address is out of bounds for memory.");
    //         }
    //         Array.Copy(fileData, 0, memory, startAddress, fileData.Length);
    //     }
    // }

    // public void DumpRegion(UInt64 startAddress, UInt64 endAddress)
    // {
    //     if (startAddress >= endAddress || startAddress < 0 || endAddress > (UInt64)memory.Length)
    //     {
    //         throw new ArgumentOutOfRangeException("Invalid address range for memory dump.");
    //     } else
    //     {
    //         for (UInt64 address = startAddress; address < endAddress; address++)
    //         {
    //             Debug.Log($"Address: {address:X16}, Value: {Read8(address):X2}");
    //         }
    //     }

    // }
}