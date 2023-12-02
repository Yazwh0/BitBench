namespace BenchTests;

[TestClass]
public class UnitTest1
{
    [TestMethod]
    public async Task TestMethod1()
    {
        var emulator = new Emulator();

        await X16TestHelper.Emulate(@"
                .machine CommanderX16R42

                import Globals=""c:\Documents\Source\BitBench\src\bench\globals.bmasm"";
                import Init=""c:\Documents\Source\BitBench\src\bench\initialisation.bmasm"";
                import Memory=""c:\Documents\Source\BitBench\src\bench\memory.bmasm"";

                .machine CommanderX16R40
                .org $810
                stp
                lda #$ff

                stp",
                emulator);

        var snapshot = emulator.Snapshot();

        emulator.Emulate();

        snapshot.Compare().CanChange(Registers.A).Is(CpuFlags.Negative, true).AssertNoChanges();

    }
}