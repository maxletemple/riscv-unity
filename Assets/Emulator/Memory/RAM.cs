using System;

public class RAM : AddressableMemory
{
    private const long MAX_RAM_SIZE = 0x10000000; // 256 MB

    public RAM(Reg64 startAddress, Reg64 size) : base(startAddress, size)
    {
        if (size > MAX_RAM_SIZE)
            throw new ArgumentException($"RAM size cannot exceed {MAX_RAM_SIZE} bytes.");
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