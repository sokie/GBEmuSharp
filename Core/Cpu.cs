using GBEmuSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBEmuSharp.Core
{

    class Cpu
    {
        public Registers Registers  { get; set; }
        public Flags Flags { get; set; }
        public Memory Rom { get; set; }

        internal byte NextByte()
        {
            return Rom.Read(Registers.PC);
        }

        public void Step()
        { 
            byte opcode = NextByte();
            switch (opcode) {
                case 0x04: { Registers.B = IncrementRegister(Registers.B); break; }
                case 0x05: { Registers.B = DecrementRegister(Registers.B); break; }
                case 0x0C: { Registers.C = IncrementRegister(Registers.C); break; }
                case 0x0D: { Registers.C = DecrementRegister(Registers.C); break; }
                case 0x14: { Registers.D = IncrementRegister(Registers.D); break; }
                case 0x15: { Registers.D = DecrementRegister(Registers.D); break; }
                case 0x1C: { Registers.E = IncrementRegister(Registers.E); break; }
                case 0x1D: { Registers.E = DecrementRegister(Registers.E); break; }
                case 0x24: { Registers.H = IncrementRegister(Registers.H); break; }
                case 0x25: { Registers.H = DecrementRegister(Registers.H); break; }

            }
        }

        /*
         *  Flags affected:
            Z - Set if result is zero.
            N - Reset.
            H - Set if carry from bit 3.
            C - Not affected.         */
        //4 cycles
        internal byte IncrementRegister(byte register)
        {
            byte result = ++register;
            Flags.SetH((0x0F & result) < (0x0F & register));
            Flags.SetN(false);
            Flags.SetZ(register == 0);
            return result;
        }

        /*
         * 
         * Flags affected:
            Z - Set if reselt is zero.
            N - Set.
            H - Set if no borrow from bit 4.
            C - Not affected
          */
        internal byte DecrementRegister(byte register)
        {
            byte result = --register;
            Flags.SetH((register & 0x0F) == 0);
            Flags.SetN(true);
            Flags.SetZ(register == 0);
            return result;
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

        public void setHL(ushort hl)
        {
            H = (byte)(hl >> 8);
            L = (byte)(hl & 0xff);
        }
    }

    class Flags
    {

        /* Flags are set up as follows
         * 7 6 5 4 3 2 1 0
           Z N H C 0 0 0 0
        */

        internal bool IME { get; set; } //(interrupt master enable)

        private byte flags;

        public bool IsZ()
        {
            return GetBit(flags, 7);
        }

        public bool IsN()
        {
            return GetBit(flags, 6);
        }

        public bool IsH()
        {
            return GetBit(flags, 5);
        }

        public bool IsC()
        {
            return GetBit(flags, 4);
        }

        public void SetZ(bool z)
        {
            SetBit(7, z);
        }

        public void SetN(bool n)
        {
            SetBit(6, n);
        }

        public void SetH(bool h)
        {
            SetBit( 5, h);
        }

        public void SetC(bool c)
        {
            SetBit(4, c);
        }

        private static bool GetBit(int byteValue, int position)
        {
            return (byteValue & (1 << position)) != 0;
        }

        private void SetBit(int position, bool value) {
            if (value)
            {
                flags |= (byte)(1 << position);
            }
            else {
                flags &= (byte)(~(1 << position));
            }
        }

    }
}
