using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Resources;
using System.Security.AccessControl;
using System.Text;
using ClientServerInterface;

namespace SP_Lab_6_client.Chat
{

    public enum Sound
    {
        ApplicationStart,
        FileReject,
        FileRequest,
        FileSent,
        MessageReceived,
        MessageSent,
        UserLeft
    }

    public class Res
    {
        public static ResourceManager Rm = new ResourceManager(typeof (LocalRes.SP_Lab_6));
    }

    public static class AliveInfo
    {
        public static List<UserInfo> Users { get; set; }

        public static CultureInfo CurrentCulture { get; set; }

        public static ChatClient Chat { get; set; }

        public static readonly Dictionary<Sound, SoundPlayer> Sounds;

        public static bool IsSoundsLoaded { get; private set; }

        public static string SoundLoadError { get; private set; }

        static AliveInfo()
        {
            CurrentCulture = new CultureInfo("ru");
            Sounds = new Dictionary<Sound, SoundPlayer>(6);
            try
            {
                InitSounds();
                IsSoundsLoaded = true;
            }
            catch (Exception ex)
            {
                IsSoundsLoaded = false;
                SoundLoadError = ex.Message;
            }
            
        }

        static void InitSounds()
        {
            var s = new FileStream("Sound\\ApplicationStart.wav", FileMode.Open, FileAccess.Read, FileShare.Read);
            var sp = new SoundPlayer(s);
            Sounds.Add(Sound.ApplicationStart, sp);

            s = new FileStream("Sound\\FileReject.wav", FileMode.Open, FileAccess.Read, FileShare.Read);
            sp = new SoundPlayer(s);
            Sounds.Add(Sound.FileReject, sp);

            s = new FileStream("Sound\\FileRequest.wav", FileMode.Open, FileAccess.Read, FileShare.Read);
            sp = new SoundPlayer(s);
            Sounds.Add(Sound.FileRequest, sp);

            s = new FileStream("Sound\\FileSent.wav", FileMode.Open, FileAccess.Read, FileShare.Read);
            sp = new SoundPlayer(s);
            Sounds.Add(Sound.FileSent, sp);

            s = new FileStream("Sound\\MessageReceive.wav", FileMode.Open, FileAccess.Read, FileShare.Read);
            sp = new SoundPlayer(s);
            Sounds.Add(Sound.MessageReceived, sp);

            s = new FileStream("Sound\\MessageSent.wav", FileMode.Open, FileAccess.Read, FileShare.Read);
            sp = new SoundPlayer(s);
            Sounds.Add(Sound.MessageSent, sp);

            s = new FileStream("Sound\\UserLeft.wav", FileMode.Open, FileAccess.Read, FileShare.Read);
            sp = new SoundPlayer(s);
            Sounds.Add(Sound.UserLeft, sp);

            //SoundPlayer simpleSound = new SoundPlayer(strAudioFilePath);
        }

    }

    // ReSharper disable InconsistentNaming
    public static class FunctionsParameters
    {
        public const string GENERAL_MESSAGE = "gnrl_msg";
    }
    // ReSharper restore InconsistentNaming

    public delegate void VoidDelegate();

}
