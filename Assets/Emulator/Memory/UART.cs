
using System;
using System.Collections.Generic;
using UnityEngine;

public class UART : IMemoryType
{
    public const int RBR_ADDRESS = 0x00; // Receiver Buffer Register
    public const int THR_ADDRESS = 0x00; // Transmitter Holding Register
    public const int IER_ADDRESS = 0x01; // Interrupt Enable Register
    public const int FCR_ADDRESS = 0x02; // Interrupt Identification Register
    public const int ISR_ADDRESS = 0x02; // Interrupt Status Register
    public const int LCR_ADDRESS = 0x03; // Line Control Register
    public const int MCR_ADDRESS = 0x04; // Modem Control Register
    public const int LSR_ADDRESS = 0x05; // Line Status Register

    public const int FIFO_SIZE = 16; // Size of the FIFO buffer

    private Queue<byte> receiveBuffer = new Queue<byte>();
    private Queue<byte> transmitBuffer = new Queue<byte>();
    private Action<byte> onReceiveCallback;

    private byte IER = 0x00; // Interrupt Enable Register
    private byte FCR = 0x00; // Interrupt Identification Register
    private byte ISR = 0x01; // Interrupt Status Register
    private byte LCR = 0x00; // Line Control Register
    private byte MCR = 0x00; // Modem Control Register
    private byte LSR = 0x60; // Line Status Register (default: no errors, data ready)

    public UART(Action<byte> onReceiveCallback = null)
    {
        this.onReceiveCallback = onReceiveCallback;
    }

    public void Write8(Reg64 address, byte value)
    {
        switch ((int)address)
        {
            case THR_ADDRESS:
                if (transmitBuffer.Count < FIFO_SIZE)
                {
                    transmitBuffer.Enqueue(value);
                    // If a receive callback is set, call it with the transmitted value
                    onReceiveCallback(value);
                }
                break;
            case IER_ADDRESS:
                IER = value;
                break;
            case FCR_ADDRESS:
                FCR = value;
                // Clear the receive buffer if FIFO is enabled
                if ((FCR & 0x01) != 0)
                {
                    receiveBuffer.Clear();
                }
                break;
            case LCR_ADDRESS:
                LCR = value;
                break;
            case MCR_ADDRESS:
                MCR = value;
                break;
            default:
                throw new System.ArgumentOutOfRangeException("Invalid UART register address.");
        }
    }

    public byte Read8(Reg64 address)
    {
        switch ((int)address)
        {
            case RBR_ADDRESS:
                if (receiveBuffer.Count > 0)
                {
                    return receiveBuffer.Dequeue();
                }
                else
                {
                    return 0x00;
                }
            case IER_ADDRESS:
                return IER;
            case ISR_ADDRESS:
                return ISR;
            case LCR_ADDRESS:
                return LCR;
            case MCR_ADDRESS:
                return MCR;
            case LSR_ADDRESS:
                // Update LSR based on the state of the receive buffer
                if (receiveBuffer.Count > 0)
                {
                    LSR |= 0x01; // Data ready
                }
                else
                {
                    LSR &= unchecked((byte)~0x01); // No data ready
                }
                return LSR;
            default:
                throw new System.ArgumentOutOfRangeException("Invalid UART register address.");
        }
    }

    public void Write64(Reg64 address, Reg64 value)
    {
        for (int i = 0; i < 8; i++)
        {
            Write8(address + i, (byte)((value >> (i * 8)) & 0xFF));
        }
    }

    public Reg64 Read64(Reg64 address)
    {
        Reg64 value = 0;
        for (int i = 0; i < 8; i++)
        {
            value |= (Reg64)Read8(address + i) << (i * 8);
        }
        return value;
    }

    public void Write32(Reg64 address, uint value)
    {
        for (int i = 0; i < 4; i++)
        {
            Write8(address + i, (byte)((value >> (i * 8)) & 0xFF));
        }
    }

    public uint Read32(Reg64 address)
    {
        uint value = 0;
        for (int i = 0; i < 4; i++)
        {
            value |= (uint)Read8(address + i) << (i * 8);
        }
        return value;
    }

    public void Write16(Reg64 address, ushort value)
    {
        for (int i = 0; i < 2; i++)
        {
            Write8(address + i, (byte)((value >> (i * 8)) & 0xFF));
        }
    }

    public ushort Read16(Reg64 address)
    {
        ushort value = 0;
        for (int i = 0; i < 2; i++)
        {
            value |= (ushort)(Read8(address + i) << (i * 8));
        }
        return value;
    }

}