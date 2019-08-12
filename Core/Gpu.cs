namespace GBEmuSharp.Core
{
    unsafe class Gpu
    {
        public enum GpuMode
        {
            Hblank = 0,
            VBlank = 1,
            ScanLineOAM = 2,
            ScanLineVRAM = 3
        }

        public Mmu Mmu { get; set; }
        public Cpu Cpu { get; set; }

        private GpuMode gpuMode;

        public void Step(int cycles) {
            //NOP for now
        }
    }
}
