using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public byte Write(ushort address, byte value) => memory[address - offset] = value;

        public void Write(ushort address, byte[] value) => Array.Copy(value, 0, memory, address - offset, value.Length);
    }
}
