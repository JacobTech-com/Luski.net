using Luski.net.Enums;
using Luski.net.Interfaces;
using Luski.net.JsonTypes;
using Luski.net.Sound;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static Luski.net.Exceptions;

namespace Luski.net.Sockets
{
    internal class SocketAudioClient : IAudioClient
    {
        internal SocketAudioClient(long Channel, Func<Exception, Task>? error)
        {
            this.Channel = Channel;
            errorin = error;
            Muted = false;
            PrototolClient.DataComplete += new Protocol.DelegateDataComplete(OnProtocolClient_DataComplete);
            DataRecived += SocketAudioClient_DataRecived;
        }

        public event Func<Task>? Connected;

        public bool Muted { get; private set; }

        public bool Deafened { get; private set; }

        public void ToggleMic()
        {
            if (Muted == true)
            {
                Muted = false;
            }
            else
            {
                Muted = true;
            }
        }

        public void ToggleAudio()
        {
            if (Deafened == true)
            {
                Deafened = false;
            }
            else
            {
                Deafened = true;
            }
        }

        public void RecordSoundFrom(RecordingDevice Device)
        {
            if (Connectedb)
            {
                StartRecordingFromSounddevice_Client(Device);
            }
            else
            {
                throw new NotConnectedException(this, "The call has not been connected yet!");
            }
        }

        public void PlaySoundTo(PlaybackDevice Device)
        {
            if (Connectedb)
            {
                StartPlayingToSounddevice_Client(Device);
            }
            else
            {
                throw new NotConnectedException(this, "The call has not been connected yet!");
            }
        }

        public void JoinCall()
        {
            if (Connected == null)
            {
                throw new MissingEventException("Connected");
            }
            else
            {
                //get info
                string data;
                while (true)
                {
                    if (Server.CanRequest)
                    {
                        using HttpClient web = new();
                        web.DefaultRequestHeaders.Add("token", Server.Token);
                        web.DefaultRequestHeaders.Add("id", Channel.ToString());
                        data = web.GetAsync($"https://{Server.Domain}/Luski/api/{Server.API_Ver}/GetCallInfo").Result.Content.ReadAsStringAsync().Result;
                        break;
                    }
                }
                IncomingHTTP json = JsonSerializer.Deserialize<IncomingHTTP>(data);
                call c = JsonSerializer.Deserialize<call>(json.data.ToString());
                Server.ServerOut.Send(JsonRequest.Send(DataType.Join_Call, JsonRequest.JoinCall(Channel)).ToString());
                Samples = c.samples;
            }
        }

        private class call
        {
            public int samples { get; set; } = default!;
            public string[] members { get; set; } = default!;
        }

        public void LeaveCall()
        {
            Server.ServerOut.Send(JsonRequest.Send(DataType.Leave_Call, JsonRequest.JoinCall(Channel)).ToString());
            StopRecordingFromSounddevice_Client();
        }

        private readonly Protocol PrototolClient = new(ProtocolTypes.LH, Encoding.Default);
        private JitterBuffer RecordingJitterBuffer = new(null, JitterBuffer, 20);
        private JitterBuffer PlayingJitterBuffer = new(null, JitterBuffer, 20);
        private readonly Func<Exception, Task>? errorin;
        private event Func<string, Task> DataRecived;
        private static readonly uint JitterBuffer = 5;
        private readonly int BitsPerSample = 16;
        private long SequenceNumber = 4596;
        private readonly int Channels = 1;
        private Recorder? RecorderClient;
        private bool Connectedb = false;
        private bool recording = false;
        private long m_TimeStamp = 0;
        private Player? PlayerClient;
        private readonly long Channel;

        private void StopPlayingToSounddevice_Client()
        {
            if (PlayerClient != null)
            {
                PlayerClient.Close();
                PlayerClient = null;
            }

            if (PlayingJitterBuffer != null)
            {
                PlayingJitterBuffer.Stop();
            }
        }

        private async Task SocketAudioClient_DataRecived(string arg)
        {
            cdata d = JsonSerializer.Deserialize<cdata>(arg);
            byte[] data = Convert.FromBase64String(d.data);
            PrototolClient.Receive_LH(this, data);
        }

        private class cdata
        {
            public string data { get; set; } = default!;
            public long from { get; set; } = default!;
        }

        private void SendData(byte[] data)
        {
            if (!Connectedb)
            {
                return;
            }

            Server.ServerOut?.Send(JsonRequest.Send(DataType.Call_Data, JsonRequest.SendCallData(PrototolClient.ToBytes(data), Channel)));
        }

        internal void Givedata(dynamic data)
        {
            DataRecived.Invoke(((object)data).ToString());
        }

        private int _samp;

        internal int Samples
        {
            get => _samp;
            set
            {
                _samp = value;
                Connectedb = true;
                if (Connected is not null) Connected.Invoke();
                PlaySoundTo(Devices.GetDefaltPlaybackDevice());
                RecordSoundFrom(Devices.GetDefaltRecordingDevice());
            }
        }

        private bool playing = false;

        private void StartPlayingToSounddevice_Client(PlaybackDevice device)
        {
            if (playing)
            {
                StopPlayingToSounddevice_Client();
            }
            playing = true;
            if (PlayingJitterBuffer != null)
            {
                PlayingJitterBuffer.DataAvailable -= new JitterBuffer.DelegateDataAvailable(OnJitterBufferClientDataAvailablePlaying);

                PlayingJitterBuffer = new JitterBuffer(null, JitterBuffer, 20);
                PlayingJitterBuffer.DataAvailable += new JitterBuffer.DelegateDataAvailable(OnJitterBufferClientDataAvailablePlaying);
                PlayingJitterBuffer.Start();
            }

            if (PlayerClient == null)
            {
                PlayerClient = new Player();
                PlayerClient.Open(device.Name, Samples, BitsPerSample, Channels, (int)JitterBuffer);
            }
        }

        private void OnJitterBufferClientDataAvailablePlaying(object sender, RTPPacket rtp)
        {
            try
            {
                if (PlayerClient != null)
                {
                    if (PlayerClient.Opened)
                    {
                        if (Deafened == false)
                        {
                            byte[] linearBytes = Utils.MuLawToLinear(rtp.Data, BitsPerSample, Channels);
                            PlayerClient.PlayData(linearBytes, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.StackFrame sf = new(true);
                errorin.Invoke(new Exception(string.Format("Exception: {0} StackTrace: {1}. FileName: {2} Method: {3} Line: {4}", ex.Message, ex.StackTrace, sf.GetFileName(), sf.GetMethod(), sf.GetFileLineNumber())));
            }
        }

        private void StartRecordingFromSounddevice_Client(RecordingDevice device)
        {
            try
            {
                if (recording)
                {
                    StopRecordingFromSounddevice_Client();
                }
                recording = true;
                InitJitterBufferClientRecording();
                int bufferSize = 0;
                bufferSize = Utils.GetBytesPerInterval((uint)Samples, BitsPerSample, Channels) * 4;

                if (bufferSize > 0)
                {
                    RecorderClient = new Recorder();
                    RecorderClient.DataRecorded += new Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard_Client);

                    if (RecorderClient.Start(device.Name, Samples, BitsPerSample, Channels, 8, bufferSize))
                    {

                        RecordingJitterBuffer.Start();
                    }
                }
            }
            catch (Exception ex)
            {
                errorin.Invoke(ex);
            }
        }

        private void StopRecordingFromSounddevice_Client()
        {
            RecorderClient.Stop();

            RecorderClient.DataRecorded -= new Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard_Client);
            RecorderClient = null;

            RecordingJitterBuffer.Stop();
        }

        private void InitJitterBufferClientRecording()
        {
            if (RecordingJitterBuffer != null)
            {
                RecordingJitterBuffer.DataAvailable -= new JitterBuffer.DelegateDataAvailable(OnJitterBufferClientDataAvailableRecording);
            }

            RecordingJitterBuffer = new JitterBuffer(null, 8, 20);
            RecordingJitterBuffer.DataAvailable += new JitterBuffer.DelegateDataAvailable(OnJitterBufferClientDataAvailableRecording);
        }

        private void OnJitterBufferClientDataAvailableRecording(object sender, RTPPacket rtp)
        {
            if (Muted == false && rtp != null && rtp.Data != null && rtp.Data.Length > 0)
            {
                byte[] rtpBytes = rtp.ToBytes();
                SendData(rtpBytes);
            }
        }

        private void OnDataReceivedFromSoundcard_Client(byte[] data)
        {
            try
            {
                lock (this)
                {
                    int bytesPerInterval = Utils.GetBytesPerInterval((uint)Samples, BitsPerSample, Channels);
                    int count = data.Length / bytesPerInterval;
                    int currentPos = 0;
                    for (int i = 0; i < count; i++)
                    {
                        byte[] partBytes = new byte[bytesPerInterval];
                        Array.Copy(data, currentPos, partBytes, 0, bytesPerInterval);
                        currentPos += bytesPerInterval;
                        RTPPacket rtp = ToRTPPacket(partBytes, BitsPerSample, Channels);

                        RecordingJitterBuffer.AddData(rtp);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }

        private RTPPacket ToRTPPacket(byte[] linearData, int bitsPerSample, int channels)
        {
            byte[] mulaws = Utils.LinearToMulaw(linearData, bitsPerSample, channels);

            RTPPacket rtp = new()
            {
                Data = mulaws,
                CSRCCount = 0,
                Extension = false,
                HeaderLength = RTPPacket.MinHeaderLength,
                Marker = false,
                Padding = false,
                PayloadType = 0,
                Version = 2,
                SourceId = 0
            };

            try
            {
                rtp.SequenceNumber = Convert.ToUInt16(SequenceNumber);
                SequenceNumber++;
            }
            catch (Exception)
            {
                SequenceNumber = 0;
            }
            try
            {
                rtp.Timestamp = Convert.ToUInt32(m_TimeStamp);
                m_TimeStamp += mulaws.Length;
            }
            catch (Exception)
            {
                m_TimeStamp = 0;
            }

            return rtp;
        }

        private void OnProtocolClient_DataComplete(object sender, byte[] data)
        {
            try
            {
                if (PlayerClient != null && PlayerClient.Opened)
                {
                    RTPPacket rtp = new(data);

                    if (rtp.Data != null)
                    {
                        if (PlayingJitterBuffer != null)
                        {
                            PlayingJitterBuffer.AddData(rtp);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}
