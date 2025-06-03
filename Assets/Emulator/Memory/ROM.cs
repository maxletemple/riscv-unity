using System;

public class ROM : AddressableMemory
{

    public ROM(Reg64 startAddress, Reg64 size) : base(startAddress, size){}

    public override void Write64(Reg64 address, Reg64 value)
    {
        throw new Exception("Cannot write to ROM memory.");
    }
    public override void Write32(Reg64 address, uint value)
    {
        throw new Exception("Cannot write to ROM memory.");
    }
    public override void Write16(Reg64 address, ushort value)
    {
        throw new Exception("Cannot write to ROM memory.");
    }
    public override void Write8(Reg64 address, byte value)
    {
        throw new Exception("Cannot write to ROM memory.");
    }

    public void LoadFromFile(string filePath)
    {
        if (!System.IO.File.Exists(filePath))
            throw new System.IO.FileNotFoundException("ROM file not found.", filePath);

        byte[] fileData = System.IO.File.ReadAllBytes(filePath);
        if (fileData.Length > memory.Length)
            throw new ArgumentException("ROM file is larger than allocated memory size.");

        Array.Copy(fileData, memory, fileData.Length);
    }
}