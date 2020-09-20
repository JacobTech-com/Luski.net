using Luski.net.Interfaces;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using Luski.net.Sound;
using System.Threading.Tasks;
using static Luski.net.Exceptions;
using System.Text;

namespace Luski.net.Sockets
{
    internal class SocketAudioClient : IAudioClient
    {
        internal SocketAudioClient(ulong Channel, Func<Exception, Task> error)
        {
            DM = Channel;
            DataRecived += SocketAudioClient_DataRecived;
        }

        private Task SocketAudioClient_DataRecived(JObject arg)
        {
            dynamic d = arg.ToString();
            byte[] data = Encoding.UTF8.GetBytes((string)d.Data);
            PrototolClient.Receive_LH(this, data);
            throw new NotImplementedException();
        }

        internal ulong DM;
        private Func<Exception, Task> errorin;
        internal bool Connectedb = false;
        public event Func<Task> Connected;
        public event Func<JObject, Task> DataRecived;
        private JitterBuffer PlayingJitterBuffer = new JitterBuffer(null, 5, 20);
        private Recorder RecorderClient;
        private uint JitterBuffer = 5;
        private uint RecorderFactor = 4;
        private Player PlayerClient;
        private int BitsPerSample = 16;
        private int Channels = 1;
        private bool recording = false;
        private int m_SoundBufferCount = 8;
        private JitterBuffer RecordingJitterBuffer = new JitterBuffer(null, 5, 20);
        private long m_SequenceNumber = 4596;
        private long m_TimeStamp = 0;
        private int m_Version = 2;
        private bool m_Padding = false;
        private bool m_Extension = false;
        private int m_CSRCCount = 0;
        private bool m_Marker = false;
        private int m_PayloadType = 0;
        private uint m_SourceId = 0;
        private Protocol PrototolClient = new Protocol(ProtocolTypes.LH, Encoding.Default);

        public void JoinCall()
        {
            if (Connected == null) throw new MissingEventException("Connected");
            else
            {
                Server.ServerOut.Send(JsonRequest.Send("Join Call", JsonRequest.JoinCall(DM)).ToString());
            }
        }

        public void LeaveCall()
        {
            throw new NotImplementedException();
        }

        public void SendData(string data)
        {
            if (Connectedb) return;
            Server.ServerOut.Send(JsonRequest.Send("Call Data", JsonRequest.SendData(data)).ToString());
        }

        internal void Givedata(dynamic data)
        {
            JObject @out = new JObject();
            @out.Add("From", (ulong)data.Data.From);
            @out.Add("Data", (string)data.Data.Data);
            DataRecived.Invoke(@out);
        }

        private bool Deafend = false;

        internal int _samp;

        internal int Samples
        {
            get
            {
                return _samp;
            }
            set
            {
                _samp = value;
                Connectedb = true;
                Connected.Invoke();
                StartPlayingToSounddevice_Client();
                StartRecordingFromSounddevice_Client();
            }
        }

        private void StartPlayingToSounddevice_Client()
        {
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
                List<string> playbackNames = WinSound.GetPlaybackNames();
                PlayerClient.Open(playbackNames[0], Samples, BitsPerSample, Channels, (int)JitterBuffer);
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
                        if (Deafend == false)
                        {
                            byte[] linearBytes = Utils.MuLawToLinear(rtp.Data, BitsPerSample, Channels);
                            PlayerClient.PlayData(linearBytes, false);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.StackFrame sf = new System.Diagnostics.StackFrame(true);
                errorin.Invoke(new Exception(string.Format("Exception: {0} StackTrace: {1}. FileName: {2} Method: {3} Line: {4}", ex.Message, ex.StackTrace, sf.GetFileName(), sf.GetMethod(), sf.GetFileLineNumber())));
            }
        }

        private void StartRecordingFromSounddevice_Client()
        {
            try
            {
                if (recording == false)
                {
                    InitJitterBufferClientRecording();
                    int bufferSize = 0;
                    bufferSize = Utils.GetBytesPerInterval((uint)Samples, BitsPerSample, Channels) * (int)RecorderFactor;

                    if (bufferSize > 0)
                    {
                        RecorderClient = new Recorder();
                        List<string> recordingNames = WinSound.GetRecordingNames();
                        RecorderClient.DataRecorded += new Recorder.DelegateDataRecorded(OnDataReceivedFromSoundcard_Client);

                        if (RecorderClient.Start(recordingNames[0], Samples, BitsPerSample, Channels, m_SoundBufferCount, bufferSize))
                        {

                            RecordingJitterBuffer.Start();
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                errorin.Invoke(ex);
            }
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
            byte[] rtpBytes = rtp.ToBytes();
            SendData(Encoding.UTF8.GetString(rtpBytes));
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

            RTPPacket rtp = new RTPPacket();

            rtp.Data = mulaws;
            rtp.CSRCCount = m_CSRCCount;
            rtp.Extension = m_Extension;
            rtp.HeaderLength = RTPPacket.MinHeaderLength;
            rtp.Marker = m_Marker;
            rtp.Padding = m_Padding;
            rtp.PayloadType = m_PayloadType;
            rtp.Version = m_Version;
            rtp.SourceId = m_SourceId;

            try
            {
                rtp.SequenceNumber = Convert.ToUInt16(m_SequenceNumber);
                m_SequenceNumber++;
            }
            catch (Exception)
            {
                m_SequenceNumber = 0;
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
    }
}
