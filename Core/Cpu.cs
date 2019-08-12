using System;

namespace GBEmuSharp.Core
{

    class Cpu
    {
        public enum InterruptFlags : byte
        {
            None = 0,
            VBlank = (1 << 0),
            LcdStat = (1 << 1),
            Timer = (1 << 2),
            Serial = (1 << 3),
            Joypad = (1 << 4)
        }

        public Registers Registers { get; set; }
        public Flags Flags { get; set; }
        public int clockCycles { get; set; }
        public Mmu Mmu { get; set; }
        /*
          IME - Interrupt Master Enable Flag (Write Only)
          0 - Disable all Interrupts
          1 - Enable all Interrupts that are enabled in IE Register (FFFF)
        */
        private bool IME { get; set; } //(interrupt master enable)
        private bool Halted { get; set; } //(interrupt master enable)

        public Cpu()
        {
            Registers = new Registers();
            Flags = new Flags();
            Mmu = new Mmu();
        }

        public void Reset()
        {
            Registers.PC = 0x100;
            Registers.SP = 0xFFFE;
            Registers.SetAF(0x01B0);
            Registers.SetBC(0x0013);
            Registers.SetDE(0x00D8);
            Registers.SetHL(0x014D);
        }

        public byte IE
        {
            get { return Mmu.Read(0xFFFF); }
            set { Mmu.Write(0xFFFF, value); }
        }

        public byte IF
        {
            get { return Mmu.Read(0xFF0F); }
            set { Mmu.Write(0xFF0F, value); }
        }

        private byte ReadByte()
        {
            byte val = Mmu.Read(Registers.PC);
            Registers.PC++;
            return val;
        }

        private ushort NextWord()
        {
            //LSB first
            return (ushort)(Mmu.Read(Registers.PC) + (Mmu.Read(Registers.PC) << 8));
        }

        private short NextSWord()
        {
            return Mmu.Read(Registers.PC);
        }

        private sbyte NextUnsignedByte()
        {
            return Mmu.SRead(Registers.PC);
        }

        private void HandleInterrupts()
        {
            int bitShift = 0;
            byte testbit = 1;
            long interrupts = IE & IF;

            while (bitShift < 5)
            {
                if ((testbit & interrupts) == testbit)
                {
                    IME = false;

                    IF -= testbit;

                    Registers.SP -= 1;
                    Mmu.Write(Registers.SP, (byte)(Registers.PC >> 8));

                    Registers.SP -= 1;
                    Mmu.Write(Registers.SP, (byte)(Registers.PC & 0xFF));

                    Registers.PC = (ushort)(0x0040 + (bitShift * 0x08));

                    //CPUTicks += 5;
                    break;
                }
                testbit = (byte)(1 << ++bitShift);
            }
        }

        public void Step()
        {
            clockCycles = 0;
            byte opcode = ReadByte();
            switch (opcode)
            {
                case 0x00:
                    { Nop(); break; }
                case 0x10:
                    { Halt(); break; }
                case 0x76:
                    { Halt(); break; }
                case 0xF3:
                    {
                        IME = false;
                        clockCycles = 4;
                        break;
                    }
                case 0xFB:
                    {
                        IME = true;
                        clockCycles = 4;
                        break;
                    }
                case 0x04: { Registers.B = Inc(Registers.B); break; }
                case 0x05: { Registers.B = Dec(Registers.B); break; }
                case 0x0C: { Registers.C = Inc(Registers.C); break; }
                case 0x0D: { Registers.C = Dec(Registers.C); break; }
                case 0x14: { Registers.D = Inc(Registers.D); break; }
                case 0x15: { Registers.D = Dec(Registers.D); break; }
                case 0x1C: { Registers.E = Inc(Registers.E); break; }
                case 0x1D: { Registers.E = Dec(Registers.E); break; }
                case 0x24: { Registers.H = Inc(Registers.H); break; }
                case 0x25: { Registers.H = Dec(Registers.H); break; }

                case 0x27: { DaA(); break; }
                case 0x2C: { Registers.L = Inc(Registers.L); break; }
                case 0x2D: { Registers.L = Dec(Registers.L); break; }
                case 0x2F: { CPL(); break; }
                case 0x34: { IncHL(); break; }
                case 0x35: { DecHL(); break; }
                case 0x37: { SCF(); break; }
                case 0x3C: { Registers.A = Inc(Registers.A); break; }
                case 0x3D: { Registers.A = Dec(Registers.A); break; }
                case 0x3F: { CCF(); break; }
                case 0x80: { AddReg(Registers.B); break; }
                case 0x81: { AddReg(Registers.C); break; }
                case 0x82: { AddReg(Registers.D); break; }
                case 0x83: { AddReg(Registers.E); break; }
                case 0x84: { AddReg(Registers.H); break; }
                case 0x85: { AddReg(Registers.L); break; }
                case 0x86: { AddHL(); break; }
                case 0x87: { AddReg(Registers.A); break; }
                case 0x88: { AddReg_C(Registers.B); break; }
                case 0x89: { AddReg_C(Registers.C); break; }
                case 0x8A: { AddReg_C(Registers.D); break; }
                case 0x8B: { AddReg_C(Registers.E); break; }
                case 0x8C: { AddReg_C(Registers.H); break; }
                case 0x8D: { AddReg_C(Registers.L); break; }
                case 0x8E: { AddHL_C(); break; }
                case 0x8F: { AddReg_C(Registers.A); break; }
                case 0x90: { SubReg(Registers.B); break; }
                case 0x91: { SubReg(Registers.C); break; }
                case 0x92: { SubReg(Registers.D); break; }
                case 0x93: { SubReg(Registers.E); break; }
                case 0x94: { SubReg(Registers.H); break; }
                case 0x95: { SubReg(Registers.L); break; }
                case 0x96: { SubHL(); break; }
                case 0x97: { SubReg(Registers.A); break; }
                case 0x98: { SubReg_C(Registers.B); break; }
                case 0x99: { SubReg_C(Registers.C); break; }
                case 0x9A: { SubReg_C(Registers.D); break; }
                case 0x9B: { SubReg_C(Registers.E); break; }
                case 0x9C: { SubReg_C(Registers.H); break; }
                case 0x9D: { SubReg_C(Registers.L); break; }
                case 0x9E: { SubHL_C(); break; }
                case 0x9F: { SubReg_C(Registers.A); break; }
                case 0xA0: { ANDReg(Registers.B); break; }
                case 0xA1: { ANDReg(Registers.C); break; }
                case 0xA2: { ANDReg(Registers.D); break; }
                case 0xA3: { ANDReg(Registers.E); break; }
                case 0xA4: { ANDReg(Registers.H); break; }
                case 0xA5: { ANDReg(Registers.L); break; }
                case 0xA6: { ANDHL(); break; }
                case 0xA7: { ANDReg(Registers.A); break; }
               
                case 0xB0: { OR(Registers.B); break; }
                case 0xB1: { OR(Registers.C); break; }
                case 0xB2: { OR(Registers.D); break; }
                case 0xB3: { OR(Registers.E); break; }
                case 0xB4: { OR(Registers.H); break; }
                case 0xB5: { OR(Registers.L); break; }
                case 0xB6: { ORHL(); break; }
                case 0xB7: { OR(Registers.A); break; }
                case 0xB8: { Cmp(Registers.B); break; }
                case 0xB9: { Cmp(Registers.C); break; }
                case 0xBA: { Cmp(Registers.D); break; }
                case 0xBB: { Cmp(Registers.E); break; }
                case 0xBC: { Cmp(Registers.H); break; }
                case 0xBD: { Cmp(Registers.L); break; }
                case 0xBE: { CmpHL(); break; }
                case 0xBF: { Cmp(Registers.A); break; }

                case 0xC0: { RetNZ(); break; }
                case 0xC7: { Restart(opcode); break; }
                case 0xC8: { RetZ(); break; }
                case 0xC9: { Ret(); break; }
                case 0xCF: { Restart(opcode); break; }
                case 0xD0: { RetNC(); break; }
                case 0xD7: { Restart(opcode); break; }
                case 0xD8: { RetC(); break; }
                case 0xD9: { RetI(); break; }
                case 0xDF: { Restart(opcode); break; }
                case 0xE7: { Restart(opcode); break; }
                case 0xE9: { JmpReg(Registers.GetHL()); break; }
                case 0xEF: { Restart(opcode); break; }
                case 0xF7: { Restart(opcode); break; }
                case 0xFF: { Restart(opcode); break; }

                //16 bit calls/jumps
                case 0xC2: { JmpNZ(); break; }
                case 0xC3: { Jump(); break; }
                case 0xC4: { CallNZ(); break; }
                case 0xCA: { JmpZ(); break; }
                case 0xCC: { CallZ(); break; }
                case 0xCD: { Call(); break; }
                case 0xD2: { JmpNC(); break; }
                case 0xD4: { CallNC(); break; }
                case 0xDA: { JmpC(); break; }
                case 0xDC: { CallC(); break; }

                //Relative Jumps
                case 0x18: { JR(); break; }
                case 0x20: { JRNZ(); break; }
                case 0x28: { JRZ(); break; }
                case 0x30: { JRNC(); break; }
                case 0x38: { JRC(); break; }

                case 0xC1: { Registers.SetBC(PopStack()); break; }
                case 0xC5: { PushStack(Registers.GetBC()); break; }
                case 0xD1: { Registers.SetDE(PopStack()); break; }
                case 0xD5: { PushStack(Registers.GetDE()); break; }
                case 0xE1: { Registers.SetHL(PopStack()); break; }
                case 0xE5: { PushStack(Registers.GetHL()); break; }
                case 0xF1: { Registers.SetAF(PopStack()); break; }
                case 0xF5: { PushStack(Registers.GetAF()); break; }

                case 0xA8: { Registers.A = XorReg(Registers.B); break; }
                case 0xA9: { Registers.A = XorReg(Registers.C); break; }
                case 0xAA: { Registers.A = XorReg(Registers.D); break; }
                case 0xAB: { Registers.A = XorReg(Registers.E); break; }
                case 0xAC: { Registers.A = XorReg(Registers.H); break; }
                case 0xAD: { Registers.A = XorReg(Registers.L); break; }
                case 0xAE: { Registers.A = XorHL(); break; }
                case 0xAF: { Registers.A = XorReg(Registers.A); break; }

                case 0x01: {LoadBC(); break; }
               
                case 0x11: { LoadDE(); break; }
                case 0x21: { LoadHL(); break; }
                case 0x31: { LoadSP(); break; }

                //Prefix CB
                case 0xCB: //extended ops table
                    byte extendedOp = ReadByte();
                    switch (extendedOp)
                    {
                        case 0x00: { break; } //RLC B
                    }
                    break;
                default: { Console.WriteLine("Missing opcode " + opcode.ToString("X2"));  break; }
            }
        }

        private void LoadHL()
        {
            ushort nn = ReadUShortPC();
            Registers.SetHL(nn);

            String op = String.Format("LD HL, 0x{0:X}",
                                nn);
            Console.WriteLine(op);
            clockCycles = 12;
        }

        private void LoadDE()
        {
            ushort nn = ReadUShortPC();
            Registers.SetDE(nn);

            String op = String.Format("LD DE, 0x{0:X}",
                                nn);
            Console.WriteLine(op);
            clockCycles = 12;
        }

        private void LoadBC()
        {
            ushort nn = ReadUShortPC();
            Registers.SetBC(nn);

            String op = String.Format("LD BC, 0x{0:X}",
                                nn);
            Console.WriteLine(op);
            clockCycles = 12;
        }

        private void LoadSP() {
            ushort nn = ReadUShortPC();
            Registers.SP = nn;

            String op = String.Format("LD SP, 0x{0:X}",
                                nn);
            Console.WriteLine(op);
            clockCycles = 12;
        }

        private void CmpHL()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            byte result = (byte)(Registers.A - HL);

            Flags.C = (Registers.A - HL) < 0;
            Flags.Z = result == 0;
            Flags.N = true;

            Flags.H = (result & 0x0F) == 0x0F;

            clockCycles = 8;
        }

        private void Cmp(byte val)
        {
            byte result = (byte)(Registers.A - val);
            Flags.Z = result == 0;
            Flags.N = true;

            Flags.C = (Registers.A - val) < 0;
            Flags.H = (result & 0x0F) == 0x0F;

           clockCycles = 4;
        }

        private void ORHL()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            byte result = (byte)(Registers.A | HL);
            Registers.A = result;

            Flags.Z = result == 0;
            Flags.N = false;
            Flags.H = false;
            Flags.C = true;
            clockCycles = 8;
        }

        private void OR(byte val)
        {
            byte result = (byte)(Registers.A | val);
            Registers.A = result;

            Flags.Z = result == 0;
            Flags.N = false;
            Flags.H = false;
            Flags.C = true;
            clockCycles = 4;
        }

        private void ANDHL()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            byte result = (byte)(Registers.A & HL);
            Registers.A = result;

            Flags.Z = result == 0;
            Flags.N = false;
            Flags.H = true;
            Flags.C = true;
            clockCycles = 8;
        }

        private void ANDReg(byte val)
        {
            byte result = (byte)(Registers.A & val);
            Registers.A = result;

            Flags.Z = result == 0;
            Flags.N = false;
            Flags.H = true;
            Flags.C = true;
            clockCycles = 4;
        }

        private void SubHL_C()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            byte result;
            if (Flags.C)
            {
                Flags.C = ((Registers.A - HL - 1) < 0);
                result = (byte)(Registers.A - HL - 1);
            }
            else
            {
                Flags.C = ((Registers.A - HL) < 0);
                result = (byte)(Registers.A - HL);
            }
            Flags.Z = (result == 0);
            Flags.H = (result & 0x0F) == 0x0F;
            Flags.N = true;
            clockCycles = 8;
        }


        private void SubReg_C(byte val)
        {
            byte result;
            if (Flags.C)
            {
                Flags.C = ((Registers.A - val - 1) < 0);
                result = (byte)(Registers.A - val - 1);
            }
            else
            {
                Flags.C = ((Registers.A - val) < 0);
                result = (byte)(Registers.A - val);
            }
            Flags.Z = (result == 0);
            Flags.H = (result & 0x0F) == 0x0F;
            Flags.N = true;
            clockCycles = 8;
        }

        private void SubHL()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            byte result = (byte)(Registers.A - HL);
            Registers.A = result;

            Flags.Z = result == 0x00;

            Flags.N = true;
            Flags.H = (result & 0x0F) == 0x0F;
            Flags.C = ((Registers.A - HL - 1) < 0);

            clockCycles = 8;
        }

        private void AddHL_C()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            if (Flags.C)
            {
                Flags.C = ((Registers.A + HL + 1) > 255);
                Registers.A =(byte)(Registers.A + HL + 1);
            }
            else
            {
                Flags.C = ((Registers.A + HL) > 255);
                Registers.A += HL;
            }
                
            Flags.Z = (Registers.A == 0);
            Flags.H = (Registers.A & 0x0F) == 0x0F;
            Flags.N = false;

            clockCycles = 8;
        }

        private void AddHL()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            Flags.C = ((Registers.A + HL) > 255);
            Registers.A += HL;
            Flags.Z = (Registers.A == 0);
            Flags.H = (Registers.A & 0x0F) == 0x0F;
            Flags.N = false;

            clockCycles = 8;
        }

        private void AddReg_C(byte val)
        {
            byte A = Registers.A;
            byte result;
      
            if (Flags.C)
            {
                Flags.C = A + val + 1 > 255;

                result = (byte)(A + val + 1);
            }
            else
            {
                Flags.C = A + val > 255;
                result = (byte)(A + val );
            }
          
            Flags.N = false;

            Flags.H = (result & 0x0F) == 0x0F;

            Registers.A = result;

            if (result == 0x00)
            {
                Flags.Z = true;
            }
            else
            {
                Flags.Z = false;
            }

            clockCycles = 8;
        }


        private void AddReg(byte val)
        {
            if (Registers.A + val > 255)
            {
                Flags.C = true;
            }
            else {
                Flags.C = false;
            }
            Registers.A += val;

            Flags.Z = (Registers.A == 0);
            Flags.H = (Registers.A & 0x0F) == 0x0F;
            Flags.N = false;
            clockCycles = 8;
        }

        private void SubReg(byte val)
        {
            Registers.A -= val;

            Flags.Z = (Registers.A == 0);
            Flags.H = (Registers.A & 0x0F) == 0x0F;
            Flags.N = false;
            clockCycles = 4;
        }

        private void IncHL()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            HL++;

            Mmu.Write(Registers.GetHL(), HL);

            if (HL == 0x00)
            {
                Flags.Z = true;
            }
            else
            {
                Flags.Z = false;
            }

            Flags.N = false;
            Flags.H = (HL & 0x0F) == 0x0F; //TODO: double check?

            clockCycles = 12;
        }

        private void DecHL()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            HL--;

            Mmu.Write(Registers.GetHL(), HL);

            if (HL == 0x00)
            {
                Flags.Z = true;
            }
            else
            {
                Flags.Z = false;
            }

            Flags.N = true;
            Flags.H = (HL & 0x0F) == 0x0F;

            clockCycles = 12;
        }

        private void CPL()
        {
            Registers.A = (byte)~Registers.A;
            Flags.N = true;
            Flags.H = true;

            clockCycles = 4;
        }

        private void SCF()
        {
            Flags.N = false;
            Flags.H = false;
            Flags.C = true;

            clockCycles = 4;
        }

        private void CCF()
        {
            Flags.N = false;
            Flags.H = false;
            if (Flags.C)
            {
                Flags.C = false;
            }
            else
            {
                Flags.C = true;
            }
            
            clockCycles = 4;
        }

        private void DaA()
        {
            byte A = Registers.A;

            if (!Flags.N)
            {
                if (Flags.H || (A & 0xF) > 9)
                {
                    A += 0x06;
                }

                if (Flags.C || (A > 0x9F))
                {
                    A += 0x60;
                }
            }
            else
            {
                if (Flags.H)
                {
                    A = (byte)((A - 0x06) & 0xFF);
                }

                if (Flags.C)
                {
                    A -= 0x60;
                }
            }

            Flags.H = false;

            if ((A & 0x100) == 0x100)
            {
                Flags.C = true;
            }

            A &= 0xFF;

            if (A == 0x00)
            {
                Flags.Z = true;
            }
            else
            {
                Flags.Z = false;
            }

            Registers.A = A;
            clockCycles = 4;
        }

        private void Halt()
        {
            Halted = true;
 
            clockCycles = 0;
        }

        private byte XorReg(byte val) {
            
            byte result = (byte)(Registers.A ^ val);
            Flags.Z = result == 0;
            Flags.N = false;
            Flags.H = false;
            Flags.C = false;
            clockCycles = 4;
            return result;
        }

        private byte XorHL()
        {
            byte HL = Mmu.Read(Registers.GetHL());
            byte result = (byte)(Registers.A ^ HL);
            Flags.Z = result == 0;
            Flags.N = false;
            Flags.H = false;
            Flags.C = false;
            clockCycles = 8;
            return result;
        }

        private ushort PopStack() {
            ushort value = PopUShort();
            clockCycles = 12;
            return value;
        }

        private void PushStack(ushort data)
        {
            PushUShortToSP(data);
            clockCycles = 16;
        }

        private void Ret()
        {
            Registers.PC = PopUShort();

            clockCycles = 16;
        }

        private void Call()
        {
            ushort nn = ReadUShortPC();
            PushUShortToSP(Registers.PC);
            Registers.PC = nn;

            clockCycles = 24;
        }

        private void CallNZ()
        {
            if (!Flags.Z)
            {
                ushort nn = ReadUShortPC();
                PushUShortToSP(Registers.PC);
                Registers.PC = nn;

                clockCycles = 24;
            }
            else {
                clockCycles = 12;
                Registers.PC += 2;
            }
        }

        private void CallZ()
        {
            if (Flags.Z)
            {
                ushort nn = ReadUShortPC();
                PushUShortToSP(Registers.PC);
                Registers.PC = nn;

                clockCycles = 24;
            }
            else
            {
                clockCycles = 12;
                Registers.PC += 2;
            }
        }

        private void CallNC()
        {
            if (!Flags.C)
            {
                ushort nn = ReadUShortPC();
                PushUShortToSP(Registers.PC);
                Registers.PC = nn;

                clockCycles = 24;
            }
            else
            {
                clockCycles = 12;
                Registers.PC += 2;
            }
        }

        private void CallC()
        {
            if (Flags.C)
            {
                ushort nn = ReadUShortPC();
                PushUShortToSP(Registers.PC);
                Registers.PC = nn;

                clockCycles = 24;
            }
            else
            {
                clockCycles = 12;
                Registers.PC += 2;
            }
        }

        private void JmpZ()
        {
            if (Flags.Z)
            {
                ushort nn = ReadUShortPC();
                Registers.PC = nn;
                clockCycles = 16;
            }
            else
            {
                Registers.PC += 2;
                clockCycles = 12;
            }
        }

        private void JmpNZ()
        {
            if (!Flags.Z)
            {
                ushort nn = ReadUShortPC();
                Registers.PC = nn;
                clockCycles = 16;
            }
            else
            {
                Registers.PC += 2;
                clockCycles = 12;
            }
        }

        private void JmpC()
        {
            if (Flags.C)
            {
                ushort nn = ReadUShortPC();
                Registers.PC = nn;
                clockCycles = 16;
            }
            else
            {
                Registers.PC += 2;
                clockCycles = 12;
            }
        }

        private void JmpNC()
        {
            if (!Flags.C)
            {
                ushort nn = ReadUShortPC();
                Registers.PC = nn;
                clockCycles = 16;
            }
            else
            {
                Registers.PC += 2;
                clockCycles = 12;
            }
        }

        private void JmpReg(ushort address)
        {
            Registers.PC = Mmu.Read(address);
            clockCycles = 4;
        }

        private void Nop()
        {
            clockCycles = 4;
        }

        private void Restart(byte val)
        {
            PushUShortToSP(Registers.PC);
            Registers.PC = (ushort)(val - 0xC7);
            clockCycles = 32; //16?
        }

        private void JR()
        {
            sbyte e = (sbyte)ReadByte();
            Registers.PC += (ushort)e;
            clockCycles = 12;
        }

        private void JRNZ()
        {
            if (!Flags.Z)
            {
                JR();
            }
            else
            {
                Registers.PC++;
                clockCycles = 8;
            }
        }

        private void JRZ()
        {
            if (Flags.Z)
            {
                JR();
            }
            else
            {
                Registers.PC++;
                clockCycles = 8;
            }
        }

        private void JRNC()
        {
            if (!Flags.C)
            {
                JR();
            }
            else
            {
                Registers.PC++;
                clockCycles = 8;
            }
        }

        private void JRC()
        {
            if (Flags.C)
            {
                JR();
            }
            else
            {
                Registers.PC++;
                clockCycles = 8;
            }
        }

        private byte Inc(byte register)
        {
            byte result = ++register;
            Flags.H = ((0x0F & result) < (0x0F & register));
            Flags.N = (false);
            Flags.Z = (register == 0);

            clockCycles = 8;

            return result;
        }

        private byte Dec(byte register)
        {
            byte result = --register;
            Flags.H = ((register & 0x0F) == 0);
            Flags.N = (true);
            Flags.Z = (register == 0);

            clockCycles = 8;

            return result;
        }

        private void Jump()
        {
            Registers.PC = ReadUShortPC();
            clockCycles = 16;
        }

        private void RetNZ()
        {
            if (!Flags.Z)
            {
                Registers.PC = PopUShort();
                clockCycles = 20;
            }
            else
            {
                clockCycles = 8;
            }
        }

        private void RetZ()
        {
            if (Flags.Z)
            {
                Registers.PC = PopUShort();
                clockCycles = 20;
            }
            else
            {
                clockCycles = 8;
            }
        }

        private void RetC()
        {
            if (Flags.C)
            {
                Registers.PC = PopUShort();
                clockCycles = 20;
            }
            else
            {
                clockCycles = 8;
            }
        }

        private void RetNC()
        {
            if (!Flags.C)
            {
                Registers.PC = PopUShort();
                clockCycles = 20;
            }
            else
            {
                clockCycles = 8;
            }
        }

        private void RetI()
        {
            IME = true;
            Registers.PC = PopUShort();

            clockCycles = 16;
        }

        private ushort ReadUShortPC()
        {
            ushort val = Mmu.ReadUShort(Registers.PC);
            Registers.PC += 2;
            return val;
        }

        private ushort PopUShort()
        {
            ushort val = Mmu.ReadUShort(Registers.SP);
            Registers.SP += 2;
            return val;
        }

        private void PushUShortToSP(ushort val)
        {
            PushByteToSP(GetHighByte(val));
            PushByteToSP(GetLowByte(val));
        }


        private void PushByteToSP(byte val)
        {
            Registers.SP--;
            Mmu.Write(Registers.SP, val);
        }

        private byte GetHighByte(ushort dest)
        {
            return (byte)((dest >> 8) & 0xFF);
        }

        private byte GetLowByte(ushort dest)
        {
            return (byte)(dest & 0xFF);
        }
    }

    class Registers
    {
        internal byte A { get; set; }
        internal byte B { get; set; }
        internal byte C { get; set; }
        internal byte D { get; set; }
        internal byte E { get; set; }
        internal byte F { get; set; }
        internal byte H { get; set; }
        internal byte L { get; set; }

        // Initialised to 0x100 usually
        internal ushort SP { get; set; }

        // Initialised to FFFE usually
        internal ushort PC { get; set; }

        public ushort GetAF()
        {
            return (ushort)((A << 8) | F);
        }

        public ushort GetBC()
        {
            return (ushort)((B << 8) | C);
        }

        public ushort GetDE()
        {
            return (ushort)((D << 8) | E);
        }

        public ushort GetHL()
        { 
            return (ushort) ((H << 8) | L );
        }

        public void SetAF(ushort af)
        {
            A = (byte)(af >> 8);
            F = (byte)(af & 0xff);
        }

        public void SetBC(ushort bc)
        {
            B = (byte)(bc >> 8);
            C = (byte)(bc & 0xff);
        }

        public void SetDE(ushort de)
        {
            D = (byte)(de >> 8);
            E = (byte)(de & 0xff);
        }

        public void SetHL(ushort hl)
        {
            H = (byte)(hl >> 8);
            L = (byte)(hl & 0xff);
        }
    }

    class Flags
    {
  
        public bool Z { get; set; } //zero flag
        public bool N { get; set; } //subtract flag
        public bool H { get; set; } //half-carry flag
        public bool C { get; set; } //carry flag
       
    }
}
