namespace BenchTests;

[TestClass]
public class SchedulerTests
{ 
    [TestMethod]
    public async Task EnableProcess()
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
                jsr bitbench_process:createprocess

                ldy #02 ; required frame count for this process
                
                stp

                jsr bitbench_scheduler:enable_process

                stp

                ; create procs
                Init.Setup();                
                ");

        snapshot.Snap();

        emulator.Emulate();

        snapshot.Compare()
            .Is(MemoryAreas.BankedRam, 0x01a204, 0x01) // 1 frame until wake
            .Is(MemoryAreas.BankedRam, 0x01a205, 0x02) // call every 2 frames
            .Is(MemoryAreas.BankedRam, 0x01a207, 0x00) // is active
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();    
    }

    [TestMethod]
    public async Task OneNormalProcess()
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
                jsr bitbench_process:createprocess

                ldy #01
                jsr bitbench_scheduler:enable_process

                jsr bitbench_scheduler:reset
                
                stp

                jsr bitbench_scheduler:get_next_process

                stp

                ; create procs
                Init.Setup();                
                ");

        snapshot.Snap();

        emulator.Emulate();

        snapshot.Compare()
            .Is(Registers.A, 0x01)      // Process 1 is next
            .Is(MemoryAreas.BankedRam, 0x01a300, 0x01) // looking at 'normal' processes
            .Is(MemoryAreas.BankedRam, 0x01a301, 0x01) // current process
            .Is(MemoryAreas.BankedRam, 0x01a302, 0x02) // index to look from next time
            .Is(MemoryAreas.BankedRam, 0x01a303, 0x0e) // number of processes to check before looping
            .CanChange(Registers.Y)
            .CanChange(Registers.X)
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();    
    }
    
    [TestMethod]
    public async Task OneNormalProcess_AllDone()
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
                jsr bitbench_process:createprocess

                ldy #01
                jsr bitbench_scheduler:enable_process

                jsr bitbench_scheduler:reset
                jsr bitbench_scheduler:get_next_process
                
                stp

                jsr bitbench_scheduler:get_next_process

                stp

                ; create procs
                Init.Setup();                
                ");

        snapshot.Snap();

        emulator.Emulate();

        snapshot.Compare()
            .Is(Registers.A, 0x00) // Nothing else to do
            .Is(MemoryAreas.BankedRam, 0x01a302, 0x00) // index to look from next time
            .CanChange(Registers.Y)
            .CanChange(Registers.X)
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();    
    }
}