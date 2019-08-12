using System;

namespace GBEmuSharp.Core
{
    class Memory
    {
        byte[] memory;
        int offset;

        public Memory(int size, int offset)
        {
            this.memory = new byte[size];
            this.offset = offset;
        }

        public byte Read(ushort address)
        {
            int index = address - offset;
            if (index < 0 || index >= memory.Length)
            {
                throw new IndexOutOfRangeException("Address: " + address);
            }
            return memory[index];
        }

        public ushort ReadWord(ushort address)
        {
            int index = address - offset;
            if (index < 0 || index >= memory.Length)
            {
                throw new IndexOutOfRangeException("Address: " + address);
            }
            return (ushort)(memory[index] + (memory[index+1] << 8));
        }

        public short ReadSWord(ushort address)
        {
            int index = address - offset;
            if (index < 0 || index >= memory.Length)
            {
                throw new IndexOutOfRangeException("Address: " + address);
            }
            return (short)(memory[index] + (memory[index + 1] << 8));
        }

        public sbyte SRead(ushort address)
        {
            int index = address - offset;
            if (index < 0 || index >= memory.Length)
            {
                throw new IndexOutOfRangeException("Address: " + address);
            }
            return (sbyte)memory[index];
        }

        public byte Write(ushort address, byte value) => memory[address - offset] = value;

        public void Write(ushort address, byte[] value) => Array.Copy(value, 0, memory, address - offset, value.Length);
    }
}
