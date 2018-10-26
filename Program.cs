using GBEmuSharp.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GBEmuSharp
{
    class Program
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
        Memory rom = new Memory(0x8000, 0); //TODO: initialise to something
        Memory videoRam = new Memory(0x2000, 0x8000);
        Memory ram = new Memory(0x2000, 0xC000);
        Memory oamRam = new Memory(0x00a0, 0xfe00); // Sprite attribute table

        static void Main(string[] args)
        {
            
        }

        public byte ReadByte(ushort address)
        {
            if (address < 0x8000)
                return rom.Read(address);
            else if (address >= 0x8000 && address < 0xA000)
                return videoRam.Read(address);
            else if (address >= 0xC000 && address < 0xE000)
                return ram.Read(address);
            else if (address >= 0xFE00 && address < 0xFEA0)
                return oamRam.Read(address);
            else throw new Exception("Bad address received");
        }

        public void WriteByte(ushort address, byte data)
        {
            if (address >= 0xC000 && address < 0xE000)
                ram.Write(address, data);
            else if (address >= 0x8000 && address < 0xA000)
                videoRam.Write(address, data);
            else if (address >= 0xFE00 && address < 0xFEA0)
                oamRam.Write(address, data);
            else
                throw new Exception("Bad address received");
        }

    }
}
