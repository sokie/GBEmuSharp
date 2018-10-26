using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBEmuSharp.Core
{

    /*
     * Memory map
 Interrupt Enable Register
 --------------------------- FFFF
 Internal RAM
 --------------------------- FF80
 Empty but unusable for I/O
 --------------------------- FF4C
 I/O ports
 --------------------------- FF00
 Empty but unusable for I/O
 --------------------------- FEA0
 Sprite Attrib Memory (OAM)
 --------------------------- FE00
 Echo of 8kB Internal RAM
 --------------------------- E000
 8kB Internal RAM
 --------------------------- C000
 8kB switchable RAM bank
 --------------------------- A000
 8kB Video RAM
 --------------------------- 8000 --
 16kB switchable ROM bank |
 --------------------------- 4000 |= 32kB Cartrigbe
 16kB ROM bank #0 |
 --------------------------- 0000 --
 */
    class Cpu
    {
       
       
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
