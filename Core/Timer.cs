using System;

namespace GBEmuSharp.Core
{
    class Timer
    {
        public const double GBClock = 4194304;

        public Mmu Mmu { get; set; }
        public Cpu Cpu { get; set; }

        //This register is incremented 16384 times a second
        public byte Div
        {
            get { return Mmu.Read(0xFF04); }
            set { Mmu.Write(0xFF04, value); }
        }

        public byte Counter
        {
            get { return Mmu.Read(0xFF05); }
            set { Mmu.Write(0xFF05, value); }
        }

        public byte Mod
        {
            get { return Mmu.Read(0xFF06); }
            set { Mmu.Write(0xFF06, value); }
        }

        public byte Control
        {
            get { return Mmu.Read(0xFF07); }
            set { Mmu.Write(0xFF07, value); }
        }

        public int GetCounterFrequency()
        {
            switch (Control & 3)
            {
                case 0:
                    return 4096;
                case 1:
                    return 262144;
                case 2:
                    return 65536;
                case 3:
                    return 16384;
            }
            return 0;
        }
        //TIMA Timer Counter
        //TMA Timer Modulo
        //TAC Timer Control
        public void Step(int cycles) {

            int divCycles = (int)GBClock / 16384 * cycles;
            for (int i = 0; i < divCycles; i++) {
                Div++;
            }
            
            if ((Control & 4) == 1)
            {
                int counterCycles = (int)GBClock / GetCounterFrequency() * cycles;
                for (int i = 0; i < counterCycles; i++)
                {
                    Counter++;

                    if (Counter == 0) {
                        Counter = Mod;
                        Cpu.IF |= 4;
                    }
                }
            }
        }

        public byte ReadRegister(ushort address)
        {
            switch (address)
            {
                case 0xFF04:
                    return Div;
                case 0xFF05:
                    return Counter;
                case 0xFF06:
                    return Mod;
                case 0xFF07:
                    return Control;
            }
            throw new ArgumentOutOfRangeException(nameof(address));
        }

        public void WriteRegister(ushort address, byte value)
        {
            switch (address)
            {
                case 0xFF04:
                    Div = 0;
                    break;
                case 0xFF05:
                    Counter = value;
                    break;
                case 0xFF06:
                    Mod = value;
                    break;
                case 0xFF07:
                    Control = (byte)(value & 0b111);
                    break;
            }
        }
    }
}
