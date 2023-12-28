namespace BenchTests.Scheduler;

[TestClass]
public class Initialise
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
    public async Task Reset()
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
                
                stp

                jsr bitbench_scheduler:reset

                stp

                ; create procs
                Init.Setup();                
                ");

        snapshot.Snap();

        emulator.Emulate();

        snapshot.Compare()
            .CanChange(Registers.A)
            .Is(MemoryAreas.BankedRam, 0x01a300, 0x00) // looking at 'realtime' processes
            .Is(MemoryAreas.BankedRam, 0x01a303, 0x0f) // number of normal processes to check before looping
            .Is(MemoryAreas.BankedRam, 0x01a305, 0x0f) // number of realtime processes to check before looping
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();    
    }    

    [TestMethod]
    public async Task ResetGet()
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
                
                jsr bitbench_scheduler:reset

                clc

                stp

                jsr bitbench_scheduler:get_next_process

                stp

                ; create procs
                Init.Setup();                
                ");

        snapshot.Snap();

        emulator.Emulate();

        snapshot.Compare()
            .Is(Registers.A, 0x00)
            .CanChange(Registers.X)
            .Is(MemoryAreas.BankedRam, 0x01a300, 0x01) // looking at normal processes
            .Is(MemoryAreas.BankedRam, 0x01a302, 0x00) // normal process index
            .Is(MemoryAreas.BankedRam, 0x01a303, 0x00) // number of normal processes to check before looping
            .Is(MemoryAreas.BankedRam, 0x01a304, 0x00) // realtime index
            .Is(MemoryAreas.BankedRam, 0x01a305, 0x00) // realtime count
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();    
    }        
}