namespace BenchTests;

[TestClass]
public class SchedulerTests_OneRealtime
{ 
    [TestMethod]
    public async Task Get()
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

                ldy #0 ; realtime
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
            .Is(Registers.A, 0x01) // Process 1 is next
            .Is(MemoryAreas.BankedRam, 0x01a300, 0x00) // looking at 'realtime' processes
            .Is(MemoryAreas.BankedRam, 0x01a301, 0x01) // current process
            .Is(MemoryAreas.BankedRam, 0x01a302, 0x00) // normal process index
            .Is(MemoryAreas.BankedRam, 0x01a303, 0x0f) // normal process count
            .Is(MemoryAreas.BankedRam, 0x01a304, 0x02) // realtime index to look from next time
            .Is(MemoryAreas.BankedRam, 0x01a305, 0x0d) // realtime process count
            .CanChange(Registers.Y)
            .CanChange(Registers.X)
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();    
    }
    
    [TestMethod]
    public async Task GetGet()
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

                ldy #0 ; realtime
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
            .Is(MemoryAreas.BankedRam, 0x01a300, 0x01) // looking at normal processes
            .Is(MemoryAreas.BankedRam, 0x01a302, 0x00) // index to look from next time, shouldn't be changed
            .Is(MemoryAreas.BankedRam, 0x01a303, 0x00) // number of processes to check before looping
            .Is(MemoryAreas.BankedRam, 0x01a304, 0x00) // index to look from next time, shouldn't be changed
            .Is(MemoryAreas.BankedRam, 0x01a305, 0x00) // number of processes to check before looping
            .CanChange(Registers.Y)
            .CanChange(Registers.X)
            .CanChange(MemoryAreas.BankedRam, 0x01a204) // scratch space
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();    
    }

    [TestMethod]
    public async Task GetGetReset()
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
                jsr bitbench_scheduler:get_next_process
                
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
            .Is(MemoryAreas.BankedRam, 0x01a302, 0x00) // index to look from next time
            .Is(MemoryAreas.BankedRam, 0x01a303, 0x0f) // number of processes to check before looping
            .Is(MemoryAreas.BankedRam, 0x01a304, 0x00) // realtime index to check next time
            .Is(MemoryAreas.BankedRam, 0x01a305, 0x0f) // number of realtime processes to check before looping
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();     
    }

    [TestMethod]
    public async Task GetGetResetGet()
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

                ldy #0 ; realtime
                jsr bitbench_scheduler:enable_process

                jsr bitbench_scheduler:reset
                jsr bitbench_scheduler:get_next_process
                jsr bitbench_scheduler:get_next_process

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
            .Is(Registers.A, 0x01) // Process 1 is next
            .Is(MemoryAreas.BankedRam, 0x01a300, 0x00) // looking at 'realtime' processes
            .Is(MemoryAreas.BankedRam, 0x01a301, 0x01) // current process
            .Is(MemoryAreas.BankedRam, 0x01a302, 0x00) // normal process index
            .Is(MemoryAreas.BankedRam, 0x01a303, 0x0f) // normal process count
            .Is(MemoryAreas.BankedRam, 0x01a304, 0x02) // realtime index to look from next time
            .Is(MemoryAreas.BankedRam, 0x01a305, 0x0d) // realtime process count
            .CanChange(Registers.Y)
            .CanChange(Registers.X)
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();
    }
    
    [TestMethod]
    public async Task GetGetResetGetGet()
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

                ldy #0 ; realtime
                jsr bitbench_scheduler:enable_process

                jsr bitbench_scheduler:reset
                jsr bitbench_scheduler:get_next_process
                jsr bitbench_scheduler:get_next_process

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
            .Is(MemoryAreas.BankedRam, 0x01a302, 0x00) // index to look from next time, shouldn't be changed
            .Is(MemoryAreas.BankedRam, 0x01a303, 0x00) // number of processes to check before looping
            .CanChange(Registers.Y)
            .CanChange(Registers.X)
            .IgnoreNumericCpuFlags()
            .IgnoreVia()
            .IgnoreVera()
            .IgnoreStackHistory()
            .AssertNoOtherChanges();    
    }
}