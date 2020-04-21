namespace NMFConvertionBasedOnPython
{
    internal class PacketHeader
    {
        public sbyte PacketType { get; set; }
        public short PacketSubtype { get; set; }
        public sbyte StreamId { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public uint PacketSize { get; set; }
        public uint ParameterSize { get; set; }

    }
}
