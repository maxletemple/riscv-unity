using System;

public struct Reg64
{
    private readonly ulong value;
    public Reg64(ulong value) => this.value = value;
    public static implicit operator Reg64(ulong value) => new Reg64(value);
    public static implicit operator ulong(Reg64 reg) => reg.value;
    public static implicit operator Reg64(long value) => new Reg64((ulong)value);
    public static implicit operator long(Reg64 reg) => (long)reg.value;
    public static implicit operator Reg64(int value) => new Reg64((ulong)value);
    public static implicit operator int(Reg64 reg) => (int)reg.value;
    public static implicit operator Reg64(uint value) => new Reg64(value);
    public static implicit operator uint(Reg64 reg) => (uint)reg.value;
    public static implicit operator Reg64(ushort value) => new Reg64((ulong)value);
    public static implicit operator ushort(Reg64 reg) => (ushort)reg.value;
    public static implicit operator Reg64(byte value) => new Reg64((ulong)value);
    public static implicit operator byte(Reg64 reg) => (byte)reg.value;

    public long getSigned(int bits)
    {
        if ((value & (ulong)(1 << (bits - 1))) != 0)
        {
            return (long)(value | ~((1UL << bits) - 1));
        }
        else
        {
            return (long)value;
        }
    }

    public ulong getUnsigned(int bits)
    {
        return value & ~(1UL << (bits - 1));
    }

    public override string ToString()
    {
        return $"0x{value:X16} ({(long)value})";
    }
}