﻿using HarmonyLib;
using RestoreMonarchy.MoreHomes.Services;
using Rocket.API;
using Rocket.API.Collections;
using Rocket.Core;
using Rocket.Core.Plugins;
using Rocket.Unturned.Chat;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using UnityEngine;
using Logger = Rocket.Core.Logging.Logger;

namespace RestoreMonarchy.MoreHomes
{
    public class MoreHomesPlugin : RocketPlugin<MoreHomesConfiguration>
    {        
        public static MoreHomesPlugin Instance { get; private set; }
        public IRocketPlugin TeleportationPlugin { get; private set; }

        public Dictionary<string, DateTime> PlayerCooldowns { get; set; }

        public DataService DataService { get; private set; }

        public Color MessageColor { get; set; }

        public const string HarmonyInstanceId = "com.restoremonarchy.morehomes";
        private Harmony HarmonyInstance;

        protected override void Load()
        {            
            Instance = this;
            MessageColor = UnturnedChat.GetColorFromName(Configuration.Instance.MessageColor, Color.green);

            PlayerCooldowns = new Dictionary<string, DateTime>();
            
            HarmonyInstance = new Harmony(HarmonyInstanceId);
            HarmonyInstance.PatchAll(Assembly);

            DataService = gameObject.AddComponent<DataService>();

            R.Plugins.OnPluginsLoaded += OnPluginsLoaded;

            Logger.Log($"{Name} {Assembly.GetName().Version} has been loaded!", ConsoleColor.Yellow);
            Logger.Log("Brought to you by RestoreMonarchy.com", ConsoleColor.Yellow);
        }

        private void OnPluginsLoaded()
        {
            IRocketPlugin plugin = R.Plugins.GetPlugin("Teleportation");
            if (plugin != null)
            {   
                TeleportationPlugin = plugin;
            }
        }

        public override TranslationList DefaultTranslations => new TranslationList(){
            { "HomeCooldownWarn", "You have to wait {0} to use this command again" },
            { "HomeDelayWarn", "You will be teleported to your bed in {0} seconds" },
            { "MaxHomesWarn", "You cannot have more beds" },
            { "NoBedsToTeleport", "You don't have any bed to teleport or name doesn't match any" },
            { "BedDestroyed", "Your bed got destroyed. Teleportation canceled" },
            { "WhileDriving", "You cannot teleport while driving" },
            { "HomeSuccess", "Successfully teleported You to your {0} bed!" },
            { "HomeList", "Your beds: " },
            { "NoHomes", "You don't have any bed claimed" },
            { "DestroyHomeFormat", "Format: /destroyhome <BedName>" },
            { "HomeNotFound", "No home match {0} name" },
            { "DestroyHomeSuccess", "Successfully destroyed and unclaimed your {0} home!" },
            { "RenameHomeFormat", "Format: /renamehome <HomeName> <NewName>" },
            { "HomeAlreadyExists", "You already have a home named {0}" },
            { "RenameHomeSuccess", "Successfully renamed home {0} to {1}!" },
            { "WhileRaid", "You can't teleport while in raiding" },
            { "WhileCombat", "You can't teleport while in combat" },
            { "RestoreHomesSuccess", "Successfully restored {0} homes!" }
        };

        protected override void Unload()
        {
            HarmonyInstance?.UnpatchAll(HarmonyInstanceId);
            HarmonyInstance = null;

            R.Plugins.OnPluginsLoaded -= OnPluginsLoaded;

            Logger.Log($"{Name} has been unloaded!", ConsoleColor.Yellow);
        }
    }
}
