namespace NMFConvertionBasedOnPython
{
    internal class PacketHeader
    {
        public int PacketType { get; set; }
        public short PacketSubtype { get; set; }
        public int StreamId { get; set; }
        public double StartTime { get; set; }
        public double EndTime { get; set; }
        public uint PacketSize { get; set; }
        public uint ParameterSize { get; set; }

    }
}