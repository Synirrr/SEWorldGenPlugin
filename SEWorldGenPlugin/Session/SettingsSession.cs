﻿using SEWorldGenPlugin.ObjectBuilders;
using SEWorldGenPlugin.Utilities.SEWorldGenPlugin.Utilities;
using VRage.Game;
using VRage.Game.Components;
using VRage.Utils;

namespace SEWorldGenPlugin.Session
{
    [MySessionComponentDescriptor(MyUpdateOrder.NoUpdate, 1000)]
    public class SettingsSession : MySessionComponentBase
    {
        private const string FILE_NAME = "worldSettings.xml";

        public static SettingsSession Static;

        public MyObjectBuilder_PluginSettings Settings
        {
            get;
            set;
        }

        public override void Init(MyObjectBuilder_SessionComponent sessionComponent)
        {
            
        }

        public override void LoadData()
        {
            Static = this;
            MyLog.Default.WriteLine("Loading SettingsSession");
            if (FileUtils.FileExistsInWorldStorage(FILE_NAME, typeof(SettingsSession)))
            {
                MyLog.Default.WriteLine("Loading Settings file");
                Settings = FileUtils.ReadXmlFileFromWorld<MyObjectBuilder_PluginSettings>(FILE_NAME, typeof(SettingsSession));
            }
            else
            {
                MyLog.Default.WriteLine("Loading SettingsSession from manager");
                if (MySettings.Static == null)
                {
                    var s = new MySettings();
                    s.LoadSettings();
                }
                Settings = MySettings.Static.SessionSettings;
                MySettings.Static.SessionSettings = null;
            }
        }

        public override void SaveData()
        {
            FileUtils.WriteXmlFileToWorld(Settings, FILE_NAME, typeof(SettingsSession));
        }

        protected override void UnloadData()
        {
            FileUtils.WriteXmlFileToWorld(Settings, FILE_NAME, typeof(SettingsSession));
            Settings = null;
        }
    }
}
