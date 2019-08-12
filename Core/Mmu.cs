using System;

namespace GBEmuSharp.Core
{
    class Mmu
    {
        public Memory Rom { get; set; }
        public Memory VRam { get; set; }
        public Memory Ram { get; set; }
        public Memory OamRam { get; set; }

        public byte Read(ushort address)
        {
            if (address < 0x8000)
                return Rom.Read(address);
            else if (address >= 0x8000 && address < 0xA000)
                return VRam.Read(address);
            else if (address >= 0xC000 && address < 0xE000)
                return Ram.Read(address);
            else if (address >= 0xFE00 && address < 0xFEA0)
                return OamRam.Read(address);
            else throw new Exception("Bad address received " + address);
        }

        public ushort ReadUShort(ushort address)
        {
            ushort val = Read((ushort)(address + 1));
            val = (ushort)(val << 8);
            val |= Read(address);
            return val;
        }

        public sbyte SRead(ushort address)
        {
            if (address < 0x8000)
                return Rom.SRead(address);
            else if (address >= 0x8000 && address < 0xA000)
                return VRam.SRead(address);
            else if (address >= 0xC000 && address < 0xE000)
                return Ram.SRead(address);
            else if (address >= 0xFE00 && address < 0xFEA0)
                return OamRam.SRead(address);
            else throw new Exception("Bad address received " + address);
        }

        public void Write(ushort address, byte data)
        {
            if (address >= 0xC000 && address < 0xE000)
                Ram.Write(address, data);
            else if (address >= 0x8000 && address < 0xA000)
                VRam.Write(address, data);
            else if (address >= 0xFE00 && address < 0xFEA0)
                OamRam.Write(address, data);
            else
                throw new Exception("Bad address received " + address);
        }
    }
}
