using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMFConvertionBasedOnPython
{
    public class FileReader
    {
        private string _filePath = string.Empty;
        public FileReader(string filePath)
        {
            _filePath = filePath;
        }

        public void ConvertToWav()
        {
            int previous_stream_id = -1;
            var processDictionary = new Dictionary<int, Process>();
            string outputFile = string.Empty;
            try
            {
                foreach (var packet in ChunkGenerator())
                {
                    if (packet.StreamId != previous_stream_id && !processDictionary.ContainsKey(packet.StreamId))
                    {
                        var process = new Process();
                        processDictionary.Add(packet.StreamId, process);
                        outputFile = Path.Combine(Path.GetDirectoryName(_filePath), $"{Path.GetFileNameWithoutExtension(_filePath)}_stream{packet.StreamId.ToString()}.wav");
                        var format = Decoders.AudioFormat.Find(x => x.Key == 0).Value;
                        if (packet.Compression > 0)
                        {
                            format = Decoders.AudioFormat.Find(x => x.Key == packet.Compression).Value;
                        }
                        processDictionary[packet.StreamId].StartInfo.FileName = "C:\\utils\\ffmpeg.exe";
                        processDictionary[packet.StreamId].StartInfo.Arguments = GetArguments(format, packet.RawChunkData, outputFile);
                        previous_stream_id = packet.StreamId;
                    }
                }
                foreach (var key in processDictionary.Keys)
                {
                    processDictionary[key].Start();
                    processDictionary[key].WaitForExit();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private string GetArguments(string compressionFormat, string rawData, string outputFile)
        {
            return $" -hide_banner -y -f {compressionFormat} -i {rawData} {outputFile}";
        }

        private List<Packets> ChunkGenerator()
        {
            var packets = new List<Packets>();
            var packet_header_start = 0;
            try
            {
                FileStream fs = new FileStream(_filePath, FileMode.Open, FileAccess.Read);
                BinaryReader binaryReader = new BinaryReader(fs);
                var binaryData = File.ReadAllBytes(_filePath);
                while (true)
                {
                    var packet_header_end = packet_header_start + 28;
                    var currentBinaryData = GetCurrentBinaryData(binaryData, packet_header_start, packet_header_end);
                    var packetHeader = GetPacketHeader(currentBinaryData);
                    if ((packetHeader.PacketType == 4 && packetHeader.PacketSubtype == 0) ||
                        (packetHeader.PacketType == 4 && packetHeader.PacketSubtype == 3) ||
                        (packetHeader.PacketType == 5 && packetHeader.PacketSubtype == 300))
                    {
                        var chunkStart = packet_header_end + packetHeader.ParameterSize;
                        var chunkEnd = (chunkStart + packetHeader.PacketSize - packetHeader.ParameterSize);
                        var chunkLength = chunkEnd - chunkStart;
                        var chunkFormat = $"{chunkLength}s";
                        var chunkData = GetCurrentBinaryData(binaryData, chunkStart, chunkEnd);
                        var rawData = BitConverter.ToString(chunkData);
                        packets.Add(new Packets()
                        {
                            Compression = GetCompressionType(currentBinaryData, packet_header_start, packet_header_end, packetHeader.ParameterSize),
                            StreamId = packetHeader.StreamId,
                            RawChunkData = rawData
                        });
                    }
                    packet_header_start += (int)packetHeader.PacketSize + 28;
                    if (packetHeader.PacketType == 7)
                    {
                        break;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return packets;
        }

        private byte[] GetCurrentBinaryData(byte[] binaryData, long startIndex, long endIndex)
        {
            var currentBinaryData = new byte[endIndex - startIndex];
            int position = 0;
            for (long index = startIndex; index < endIndex; index++)
            {
                currentBinaryData[position] = binaryData[index];
                position++;
            }
            return currentBinaryData;
        }

        private int GetCompressionType(byte[] binaryReader, int packet_header_start, int packet_header_end, uint parameterSize)
        {
            int compressionType = -1;
            return compressionType;
        }

        private PacketHeader GetPacketHeader(byte[] binaryData)
        {
            var packetHeader = new PacketHeader();
            try
            {
                var chunkData = binaryData.Skip(0).Take(1).ToArray();
                var data = StructConverter.Unpack("b", chunkData);
                packetHeader.PacketType = Convert.ToInt32(data[0]);

                chunkData = binaryData.Skip(1).Take(2).ToArray();
                data = StructConverter.Unpack("h", chunkData);
                packetHeader.PacketSubtype = Convert.ToInt16(data[0]);

                chunkData = binaryData.Skip(3).Take(1).ToArray();
                data = StructConverter.Unpack("b", chunkData);
                packetHeader.StreamId = Convert.ToInt32(data[0]);

                chunkData = binaryData.Skip(4).Take(8).ToArray();
                data = StructConverter.Unpack("d", chunkData);
                packetHeader.StartTime = Convert.ToDouble(data[0]);

                chunkData = binaryData.Skip(12).Take(8).ToArray();
                data = StructConverter.Unpack("d", chunkData);
                packetHeader.EndTime = Convert.ToDouble(data[0]);

                chunkData = binaryData.Skip(20).Take(4).ToArray();
                data = StructConverter.Unpack("I", chunkData);
                packetHeader.PacketSize = Convert.ToUInt32(data[0]);

                chunkData = binaryData.Skip(24).Take(4).ToArray();
                data = StructConverter.Unpack("I", chunkData);
                packetHeader.ParameterSize = Convert.ToUInt32(data[0]);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return packetHeader;
        }
    }
}
