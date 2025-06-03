public abstract class AddressableMemory : IMemoryType
{
    protected byte[] memory;
    protected Reg64 startAddress;

    protected AddressableMemory(Reg64 startAddress, Reg64 size)
    {
        memory = new byte[(long)size];
        this.startAddress = startAddress;
    }

    public virtual void Write64(Reg64 address, Reg64 value)
    {
        for (int i = 0; i < 8; i++)
        {
            memory[(long)address + i] = (byte)((value >> (i * 8)) & 0xFF);
        }
    }

    public virtual Reg64 Read64(Reg64 address)
    {
        Reg64 value = 0;
        for (int i = 0; i < 8; i++)
        {
            value |= (Reg64)memory[(long)address + i] << (i * 8);
        }
        return value;
    }

    public virtual void Write32(Reg64 address, uint value)
    {
        for (int i = 0; i < 4; i++)
        {
            memory[(long)address + i] = (byte)((value >> (i * 8)) & 0xFF);
        }
    }
    public virtual uint Read32(Reg64 address)
    {
        uint value = 0;
        for (int i = 0; i < 4; i++)
        {
            value |= (uint)memory[(long)address + i] << (i * 8);
        }
        return value;
    }
    public virtual void Write16(Reg64 address, ushort value)
    {
        for (int i = 0; i < 2; i++)
        {
            memory[(long)address + i] = (byte)((value >> (i * 8)) & 0xFF);
        }
    }
    public virtual ushort Read16(Reg64 address)
    {
        ushort value = 0;
        for (int i = 0; i < 2; i++)
        {
            value |= (Reg64)(memory[(long)address + i] << (i * 8));
        }
        return value;
    }
    public virtual void Write8(Reg64 address, byte value)
    {
        memory[(long)address] = value;
    }
    public virtual byte Read8(Reg64 address)
    {
        return memory[(long)address];
    }
}