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
            var process = new Process();
            string outputFile = string.Empty;
            try
            {
                foreach (var packet in ChunkGenerator())
                {
                    if (packet.StreamId != previous_stream_id && !processDictionary.TryGetValue(packet.StreamId, out process))
                    {
                        processDictionary.Add(packet.StreamId, process);
                        outputFile = Path.Combine(Path.GetDirectoryName(_filePath), Path.GetFileNameWithoutExtension(_filePath), packet.StreamId.ToString(), ".wav");
                        var format = Decoders.AudioFormat.Find(x => x.Key == 0).Value;
                        if (packet.Compression > 0)
                        {
                            format = Decoders.AudioFormat.Find(x => x.Key == packet.Compression).Value;
                        }
                        processDictionary[packet.StreamId].StartInfo.FileName = "C:\\utils\\ffmpeg.exe";
                        processDictionary[packet.StreamId].StartInfo.Arguments = GetArguments(format);
                        previous_stream_id = packet.StreamId;
                    }
                    //processes[stream_id].stdin.write(raw_audio_chunk)
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

        private string GetArguments(string compressionFormat)
        {
            return $"C:\\utils\\ffmpeg.exe -hide_banner -y -f {compressionFormat} -i C:\\AudioFile\\audio_file1.nmf";
        }

        private List<Packets> ChunkGenerator()
        {
            var packets = new List<Packets>();
            var packet_header_start = 0;
            try
            {
                FileStream fs = new FileStream(_filePath, FileMode.Open);
                BinaryReader binaryReader = new BinaryReader(fs);
                while (true)
                {
                    var packet_header_end = packet_header_start + 28;
                    var packetHeader = GetPacketHeader(binaryReader, packet_header_start, packet_header_end);
                    if ((packetHeader.PacketType == 4 && packetHeader.PacketSubtype == 0) ||
                        (packetHeader.PacketType == 4 && packetHeader.PacketSubtype == 3) ||
                        (packetHeader.PacketType == 5 && packetHeader.PacketSubtype == 300))
                    {
                        var chunkStart = packet_header_end + packetHeader.ParameterSize;
                        var chunkEnd = (chunkStart + packetHeader.PacketSize - packetHeader.ParameterSize);
                        var chunkLength = chunkEnd - chunkStart;
                        var chunkFormat = $"{chunkLength}s";
                        var rawData = "";
                        packets.Add(new Packets()
                        {
                            Compression = GetCompressionType(binaryReader, packet_header_start, packet_header_end, packetHeader.ParameterSize),
                            StreamId = packetHeader.StreamId,
                            RawChunkData = rawData
                        });
                    }
                    packet_header_start += (int)packetHeader.PacketSize + 28;
                    if(packetHeader.PacketType == 7)
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

        private int GetCompressionType(BinaryReader binaryReader, int packet_header_start, int packet_header_end, uint parameterSize)
        {
            int compressionType = -1;
            foreach(var index in Enumerable.Range(0, (int)binaryReader.BaseStream.Length))
            {
                
            }
            return compressionType;
        }

        private PacketHeader GetPacketHeader(BinaryReader binaryReader, int startIndex, int endIndex)
        {
            throw new NotImplementedException();
        }
    }
}
