public interface IMemoryType
{
    void Write64(Reg64 address, Reg64 value);
    Reg64 Read64(Reg64 address);
    
    void Write32(Reg64 address, uint value);
    uint Read32(Reg64 address);
    
    void Write16(Reg64 address, ushort value);
    ushort Read16(Reg64 address);
    
    void Write8(Reg64 address, byte value);
    byte Read8(Reg64 address);
}