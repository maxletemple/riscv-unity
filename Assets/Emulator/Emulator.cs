using System.Diagnostics;
using System.IO;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class Emulator : MonoBehaviour
{
    public Console console;
    public Reg64 RAMSize = 1024 * 1024; // 1 MB
    public Reg64 RAMStartAddress = 0x80000000; // Start address for RAM
    public Reg64 UARTStartAddress = 0x10000000; // Start address for UART
    public Reg64 UARTSize = 0x1000; // Size of UART memory region

    string bootFilePath = "boot.bin";
    string ramFilePath = "program.bin";

    public int cyclesPerUpdate = 10000;
    private MemoryBus memoryBus;
    private Cpu cpu;
    private bool reportDone = false;
    double lastTime = 0;
    void Start()
    {
        memoryBus = new MemoryBus();

        RAM ram = new RAM(RAMStartAddress, RAMSize);
        ram.LoadFromFile(Application.streamingAssetsPath + "/" + ramFilePath);
        memoryBus.AddRegion(ram, RAMStartAddress, RAMStartAddress + RAMSize);

        RAM bios = new RAM(0, 0x10000000);
        bios.LoadFromFile(Application.streamingAssetsPath + "/" + bootFilePath);
        memoryBus.AddRegion(bios, 0x0000, 0x10000000);

        UART uart = new UART(console.WriteChar);
        memoryBus.AddRegion(uart, UARTStartAddress, UARTStartAddress + UARTSize);

        cpu = new Cpu(memoryBus);
    }

    void FixedUpdate()
    {
        for (int i = 0; i < cyclesPerUpdate; i++)
        {
            // Simulate a CPU cycle
            cpu.DoCycle();
        }
        var endTime = (double) Stopwatch.GetTimestamp() / Stopwatch.Frequency;

        var elapsedTime = endTime - lastTime;
        var frequency = cyclesPerUpdate / elapsedTime;
        if ((int)Time.realtimeSinceStartup % 3 == 0)
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
        lastTime = (double) Stopwatch.GetTimestamp() / Stopwatch.Frequency;
        
    }
}
