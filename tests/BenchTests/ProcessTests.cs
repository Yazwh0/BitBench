namespace BenchTests;

[TestClass]
public class ProcessTests
{
    [TestMethod]
    public async Task CreateProcess()
    {
        var (emulator, snapshot) = await X16TestHelper.EmulateTemplateChanges(@"
                import Globals=""..\..\..\..\..\src\bench\globals.bmasm"";
                import Scheduler=""..\..\..\..\..\src\bench\scheduler.bmasm"";
                import Init=""..\..\..\..\..\src\bench\initialisation.bmasm"";
                import Memory=""..\..\..\..\..\src\bench\memory.bmasm"";
                import Process=""..\..\..\..\..\src\bench\process.bmasm"";
                .machine CommanderX16R42

                .org $810

                cld
                sei

                ldx #$ff
                txs

                lda #1
                sta RAM_BANK
                sta IEN
                stz CTRL

                jsr bitbench_memory:initialise
                jsr bitbench_scheduler:initialise
                jsr bitbench_process:initialise

                lda #02 ; main bank is #2

                stp

                jsr bitbench_process:createprocess
                
                stp

                ; create procs
                Init.Setup();                
                ");

        snapshot.Snap();

        emulator.Emulate();

        snapshot.Compare()
            .Is(Registers.A, 0x01) // process ID
            .Is(Registers.X, 0x04) // stack block
            .Is(Registers.Y, 0x05) // data block
            .Is(CpuFlags.Carry, false) // no error
            // allocated blocks
            .Is(MemoryAreas.BankedRam, 0x01a004, 1) // block 4 allocated to process 1
            .Is(MemoryAreas.BankedRam, 0x01a005, 1) // block 5 allocated to process 1
            // process data
            .Is(MemoryAreas.BankedRam, 0x01a404, 2) // process primary bank 2
            .Is(MemoryAreas.BankedRam, 0x01a406, 4) // block 4 is stack block
            .Is(MemoryAreas.BankedRam, 0x01a407, 5) // block 5 is data block
            // temp variables
            .CanChange(MemoryAreas.BankedRam, 0x01a306, 0x01a307) // temp variables
            .CanChange(MemoryAreas.Ram, 0xd8, 0xdb) // ZP temp variables
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();
    }
}