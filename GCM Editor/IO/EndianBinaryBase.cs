using System;
using System.Collections.Generic;
using System.IO;

namespace Editor.IO
{
    public abstract unsafe class EndianBinaryBase
    {
        protected Stream Stream;
        private Stack<long> PositionStack;

        private Endianness Endian;
        private static readonly bool SystemLittleEndian = BitConverter.IsLittleEndian;
        protected bool EndianReverse;

        protected EndianBinaryBase(Stream Stream, Endianness Endian)
        {
            this.Stream = Stream;
            Endianness = Endian;

            PositionStack = new Stack<long>();
        }

        /// <summary>
        /// Gets or sets the endianness.
        /// </summary>
        public Endianness Endianness
        {
            get => Endian;
            set
            {
                Endian = value;
                EndianReverse = SystemLittleEndian != (Endian == Endianness.LittleEndian);
            }
        }

        /// <summary>
        /// Gets the underlying data stream.
        /// </summary>
        public Stream DataStream { get => Stream; }

        /// <summary>
        /// Gets or sets the position where the stream is operated.
        /// </summary>
        public long Position
        {
            get => Stream.Position;
            set => Stream.Position = value;
        }

        /// <summary>
        /// Gets the length of the stream in bytes.
        /// </summary>
        public long StreamLength { get => Stream.Length; }

        protected static void ReverseBytes(byte* Ptr, int Size, int Stride)
        {
            // If stride is odd, the middle byte would be put in the same place, so this works
            int HalfStride = Stride / 2;

            for (int i = 0; i < Size; i += Stride)
            {
                byte* Start = Ptr;
                byte* End = Ptr + Stride - 1;

                for (int j = 0; j < HalfStride; j++)
                {
                    byte A = *Start;
                    byte B = *End;

                    *End-- = A;
                    *Start++ = B;
                }

                Ptr += Stride;
            }
        }

        /// <summary>
        /// Sets the position of the <see cref="Stream"/>.
        /// </summary>
        /// <param name="Position">The new position.</param>
        /// <param name="Origin">Specifies what the position is relative to.</param>
        public void Seek(long Position, SeekOrigin Origin) => Stream.Seek(Position, Origin);

        /// <summary>
        /// Switches the endianness of the <see cref="EndianBinaryReaderOLD"/>.
        /// </summary>
        public void SwitchEndianness()
        {
            // Don't use field "Endian"
            if (Endianness == Endianness.BigEndian)
            {
                Endianness = Endianness.LittleEndian;
            }
            else if (Endianness == Endianness.LittleEndian)
            {
                Endianness = Endianness.BigEndian;
            }
        }

        /// <summary>
        /// Pushes the current <see cref="Stream.Position"/> into the position stack.
        /// </summary>
        public void PushPosition()
        {
            PositionStack.Push(Position);
        }

        /// <summary>
        /// Returns the <see cref="Stream.Position"/> at the top of the position stack without removing it.
        /// </summary>
        /// <returns>The stream position.</returns>
        public long PeekPosition()
        {
            return PositionStack.Peek();
        }

        /// <summary>
        /// Sets the <see cref="Position"/> to positionon top of the position stack, removes and returns it.
        /// </summary>
        /// <returns>The stream position.</returns>
        public long PopPosition()
        {
            long Popped = PositionStack.Pop();
            Position = Popped;

            return Popped;
        }
    }
}
