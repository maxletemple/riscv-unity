using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Emulator : MonoBehaviour
{
    public Console console;
    public Reg64 RAMSize = 1024 * 1024; // 1 MB
    public Reg64 RAMStartAddress = 0x1000; // Start address for RAM
    public Reg64 UARTStartAddress = 0x10000000; // Start address for UART
    public Reg64 UARTSize = 0x1000; // Size of UART memory region
    
    public int cyclesPerUpdate = 10000;
    private MemoryBus memoryBus;
    private Cpu cpu;
    private bool reportDone = false;
    void Start()
    {
        memoryBus = new MemoryBus();

        RAM ram = new RAM(RAMStartAddress, RAMSize);
        ram.LoadFromFile(Application.dataPath + "/main.bin");
        memoryBus.AddRegion(ram, RAMStartAddress, RAMStartAddress + RAMSize);

        ROM rom = new ROM(0, 1024);
        rom.LoadFromFile(Application.dataPath + "/boot.bin");
        memoryBus.AddRegion(rom, 0x0000, 0x0400);

        UART uart = new UART(console.WriteChar);
        memoryBus.AddRegion(uart, UARTStartAddress, UARTStartAddress + UARTSize);

        cpu = new Cpu(memoryBus);
    }

    void FixedUpdate()
    {
        var startTime = (double) Stopwatch.GetTimestamp() / Stopwatch.Frequency;
        for (int i = 0; i < cyclesPerUpdate; i++)
        {
            // Simulate a CPU cycle
            cpu.DoCycle();
        }
        var endTime = (double) Stopwatch.GetTimestamp() / Stopwatch.Frequency;

        var elapsedTime = endTime - startTime;
        var frequency = cyclesPerUpdate / elapsedTime;
        if ((int)Time.realtimeSinceStartup % 10 == 0)
        {
            if (!reportDone)
            {
                reportDone = true;
                Debug.Log($"CPU frequency: {(frequency/1_000_000).ToString("F2")} MHz");
            }
        }
        else
        {
            reportDone = false;
        }
        
    }
}
