﻿using Rocket.API;
using Rocket.Unturned;
using Rocket.Unturned.Chat;
using Rocket.Unturned.Player;
using SDG.Unturned;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Timers;
using UnityEngine;
using MoreHomes.Models;

namespace MoreHomes.Commands
{
    public class CommandHome : IRocketCommand
    {

        public string Help
        {
            get { return "Teleports you to your bed."; }
        }

        public string Name
        {
            get { return "home"; }
        }

        public string Syntax
        {
            get { return "<bedName>"; }
        }

        public List<string> Aliases
        {
            get { return new List<string>(); }
        }

        public AllowedCaller AllowedCaller
        {
            get { return AllowedCaller.Both; }
        }

        public List<string> Permissions
        {
            get
            {
                return new List<string>() { "home" };
            }
        }

        public void Execute(IRocketPlayer caller, string[] command)
        {
            UnturnedPlayer unturnedPlayer = (UnturnedPlayer)caller;
            Vector3 position;

            if (unturnedPlayer.Stance == EPlayerStance.DRIVING || unturnedPlayer.Stance == EPlayerStance.SITTING)
            {
                UnturnedChat.Say(caller, U.Translate("command_generic_teleport_while_driving_error", new object[0]));
                throw new WrongUsageOfCommandException(caller, this);
            }

            if (command.Count() == 1)
            {
                PlayerBed bed = MoreHomes.Instance.Database.GetBedByName(unturnedPlayer.CSteamID, command[0]);
                if (bed != null)
                {
                    var region = BarricadeManager.regions[bed.X, bed.Y];
                    if (region != null)
                    {
                        InteractableBed interactableBed = null;
                        foreach (var drop in region.drops)
                        {
                            if (drop.interactable is InteractableBed && drop.interactable.transform.position.ToString() == bed.Position)
                            {
                                interactableBed = (InteractableBed)drop.interactable;
                                break;
                            }                            
                        }
                        
                        if (interactableBed != null && interactableBed.owner == unturnedPlayer.CSteamID)
                            position = interactableBed.transform.position;
                        else
                        {
                            
                            UnturnedChat.Say(caller, String.Format(MoreHomes.Instance.Translate("command_home_not_found"), command[0]), Color.red);
                            return;
                        }
                    }
                    else
                    {
                        MoreHomes.Instance.Database.RemoveBedByName(unturnedPlayer.CSteamID, command[0]);
                        UnturnedChat.Say(caller, MoreHomes.Instance.Translate("command_home_not_found", command[0]), Color.red);
                        return;
                    }
                } else
                {
                    UnturnedChat.Say(caller, MoreHomes.Instance.Translate("command_home_not_found", command[0]), Color.red);
                    return;
                }
                               
            } else
            {
                if (!BarricadeManager.tryGetBed(unturnedPlayer.CSteamID, out position, out byte rot))
                {
                    UnturnedChat.Say(caller, MoreHomes.Instance.Translate("no_home"));
                    return;
                }
            }

            if (MoreHomes.Instance.Configuration.Instance.TeleportationDelay != 0)
            {
                Timer timer = new Timer(MoreHomes.Instance.Configuration.Instance.TeleportationDelay * 1000);
                timer.AutoReset = false;
                timer.Elapsed += Timer_Elapsed;
                timer.Start();
                UnturnedChat.Say(caller, String.Format(MoreHomes.Instance.Translate("command_home_delay"), MoreHomes.Instance.Configuration.Instance.TeleportationDelay), Color.grey);
            }
            else
            {
                TeleportPlayer(false);
            }

            void Timer_Elapsed(object sender, ElapsedEventArgs e)
            {
                TeleportPlayer(true);
            }

            void TeleportPlayer(bool isDelayed)
            {
                if (isDelayed)
                {
                    if (!unturnedPlayer.Dead)
                    {
                        unturnedPlayer.Teleport(position, unturnedPlayer.Rotation);
                    }
                    else
                    {
                        UnturnedChat.Say(caller, MoreHomes.Instance.Translate("command_home_died"));
                    }
                }
                else
                {
                    unturnedPlayer.Teleport(position, unturnedPlayer.Rotation);
                }

            }

        }
    }
}
