using System;
using System.Text;

namespace Editor.IO
{
    [Serializable]
    public class WrongMagicException : ArgumentException
    {
        public WrongMagicException(string Correct, string Wrong, long Position)
            : base($"An invalid magic was found at 0x{Position:X8}. The file is invalid or corrupt. \"{Correct}\" was expected but the following string was found:\n0x{BitConverter.ToString(Encoding.Default.GetBytes(Wrong)).Replace("-", "")} => {Wrong}")
        {

        }

        public WrongMagicException(string Correct, string Wrong, EndianBinaryReader Reader)
            : this(Correct, Wrong, Reader.Position - Wrong.Length)
        {

        }
    }
}
