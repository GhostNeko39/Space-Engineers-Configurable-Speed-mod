﻿namespace midspace.Speed.ConfigurableScript.Messages
{
    using System;
    using System.Globalization;
    using System.Text;
    using midspace.Speed.ConfigurableScript;
    using ProtoBuf;
    using Sandbox.Definitions;
    using Sandbox.ModAPI;

    /// <summary>
    /// this is to do the actual work of setting new prices and stock levels.
    /// </summary>
    [ProtoContract]
    public class MessageConfig : MessageBase
    {
        #region properties

        /// <summary>
        /// The key config item to set.
        /// </summary>
        [ProtoMember(1)]
        public string ConfigName;

        /// <summary>
        /// The value to set the config item to.
        /// </summary>
        [ProtoMember(2)]
        public string Value;

        #endregion

        public static void SendMessage(string configName, string value)
        {
            ConnectionHelper.SendMessageToServer(new MessageConfig { ConfigName = configName.ToLower(), Value = value });
        }

        public override void ProcessClient()
        {
            // never processed on client
        }

        public override void ProcessServer()
        {
            var player = MyAPIGateway.Players.FindPlayerBySteamId(SenderSteamId);

            if (player == null)
                return;

            // Only Admin can change config.
            if (!player.IsAdmin())
            {
                ConfigurableSpeedComponentLogic.Instance.ServerLogger.WriteWarning("A Player without Admin \"{0}\" {1} attempted to access ConfigSpeed.", SenderDisplayName, SenderSteamId);
                return;
            }

            // These will match with names defined in the RegEx patterm <ConfigurableSpeedComponentLogic.ConfigSpeedPattern>
            switch (ConfigName)
            {
                #region reset all settings to stock.

                case "resetall":
                    {
                        ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.LargeShipMaxSpeed = ConfigurableSpeedComponentLogic.Instance.DefaultDefinitionValues.LargeShipMaxSpeed;
                        ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.SmallShipMaxSpeed = ConfigurableSpeedComponentLogic.Instance.DefaultDefinitionValues.SmallShipMaxSpeed;
                        ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.EnableThrustRatio = false;
                        ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.ThrustRatio = 1;
                        ConfigurableSpeedComponentLogic.Instance.IsModified = true;

                        var msg = new StringBuilder();
                        msg.AppendFormat("All settings have been reset to stock standard game settings.");
                        msg.AppendLine();
                        msg.AppendLine("Once you have finished your changes, you must save the game and then restart it immediately for it to take effect.");
                        msg.AppendLine();
                        msg.AppendLine("If you only save the game and do not restart, any player that connects will experience issues.");
                        MessageClientDialogMessage.SendMessage(SenderSteamId, "ConfigSpeed", " ", msg.ToString());
                    }
                    break;

                #endregion

                #region LargeShipMaxSpeed

                case "large":
                case "largeship":
                case "largeshipmaxspeed":
                case "largeshipspeed":
                    if (!string.IsNullOrEmpty(Value))
                    {
                        decimal decimalTest;
                        if (decimal.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimalTest))
                        {
                            if (decimalTest >= 1 && decimalTest <= 150000000m)
                            {
                                ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.LargeShipMaxSpeed = decimalTest;
                                ConfigurableSpeedComponentLogic.Instance.IsModified = true;

                                var msg = new StringBuilder();
                                msg.AppendFormat("LargeShipMaxSpeed updated to: {0:N0} m/s\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.LargeShipMaxSpeed);
                                msg.AppendFormat("SmallShipMaxSpeed is: {0:N0} m/s\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.SmallShipMaxSpeed);
                                msg.AppendLine();
                                msg.AppendLine();
                                msg.AppendLine("Once you have finished your changes, you must save the game and then restart it immediately for it to take effect.");
                                msg.AppendLine();
                                msg.AppendLine("If you only save the game and do not restart, any player that connects will experience issues.");
                                MessageClientDialogMessage.SendMessage(SenderSteamId, "ConfigSpeed", " ", msg.ToString());

                                // Default values:
                                //VRage.Game.MyObjectBuilder_EnvironmentDefinition.Defaults.LargeShipMaxSpeed
                                //VRage.Game.MyObjectBuilder_EnvironmentDefinition.Defaults.SmallShipMaxSpeed

                                return;
                            }
                        }
                    }

                    MessageClientTextMessage.SendMessage(SenderSteamId, "ConfigSpeed", "The new maximum ship speed limit can only be between {0:N0} and {1:N0}", 1, 150000000m);
                    break;

                #endregion

                #region SmallShipMaxSpeed

                case "small":
                case "smallship":
                case "smallshipmaxspeed":
                case "smallshipspeed":
                    if (!string.IsNullOrEmpty(Value))
                    {
                        decimal decimalTest;
                        if (decimal.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimalTest))
                        {
                            if (decimalTest >= 1 && decimalTest <= 150000000m)
                            {
                                ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.SmallShipMaxSpeed = decimalTest;
                                ConfigurableSpeedComponentLogic.Instance.IsModified = true;

                                var msg = new StringBuilder();
                                msg.AppendFormat("SmallShipMaxSpeed updated to: {0:N0} m/s\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.SmallShipMaxSpeed);
                                msg.AppendFormat("LargeShipMaxSpeed is: {0:N0} m/s\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.LargeShipMaxSpeed);
                                msg.AppendLine();
                                msg.AppendLine();
                                msg.AppendLine("Once you have finished your changes, you must save the game and then restart it immediately for it to take effect.");
                                msg.AppendLine();
                                msg.AppendLine("If you only save the game and do not restart, any player that connects will experience issues.");
                                MessageClientDialogMessage.SendMessage(SenderSteamId, "ConfigSpeed", " ", msg.ToString());
                                return;
                            }
                        }
                    }

                    MessageClientTextMessage.SendMessage(SenderSteamId, "ConfigSpeed", "The new maximum ship speed limit can only be between {0:N0} and {1:N0}", 1, 150000000m);
                    break;

                #endregion

                #region EnableThrustRatios

                case "enablethrustratio":
                case "lockthrustratio":
                    if (!string.IsNullOrEmpty(Value))
                    {
                        bool boolTest;
                        if (Value.TryWordParseBool(out boolTest))
                        {
                            SetEnableThrustRatio(boolTest);
                            return;
                        }
                    }

                    MessageClientTextMessage.SendMessage(SenderSteamId, "ConfigSpeed", "EnableThrustRatios can only be set True/On/Yes or False/Off/No.");
                    break;

                #endregion

                #region ThrustRatio

                case "thrustratio":
                    if (!string.IsNullOrEmpty(Value))
                    {
                        decimal decimalTest;
                        if (decimal.TryParse(Value, NumberStyles.Any, CultureInfo.InvariantCulture, out decimalTest))
                        {
                            if (decimalTest >= 0.1m && decimalTest <= 1000m)
                            {
                                ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.ThrustRatio = decimalTest;
                                ConfigurableSpeedComponentLogic.Instance.IsModified = true;

                                var msg = new StringBuilder();
                                msg.AppendFormat("ThrustRatio updated to: x{0:N3}\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.ThrustRatio);
                                //msg.AppendLine();
                                //msg.AppendLine();
                                //msg.AppendLine("Once you have finished your changes, you must save the game and then restart it immediately for it to take effect.");
                                //msg.AppendLine();
                                //msg.AppendLine("If you only save the game and do not restart, any player that connects will experience issues.");
                                MessageClientDialogMessage.SendMessage(SenderSteamId, "ConfigSpeed", " ", msg.ToString());
                                return;
                            }
                        }
                    }

                    MessageClientTextMessage.SendMessage(SenderSteamId, "ConfigSpeed", "The ThrustRatio can only be between {0:N0} and {1:N0}", 0.1m, 1000m);
                    break;

                #endregion

                #region default

                default:
                    {
                        var msg = new StringBuilder();
                        msg.AppendLine("Current settings are:");
                        msg.AppendFormat("  Maximum Large Ship speed: {0:N0} m/s\r\n", MyDefinitionManager.Static.EnvironmentDefinition.LargeShipMaxSpeed);
                        msg.AppendFormat("  Maximum Small Ship speed: {0:N0} m/s\r\n", MyDefinitionManager.Static.EnvironmentDefinition.SmallShipMaxSpeed);
                        msg.AppendFormat("  Enable Thrust Ratio: {0}\r\n", ConfigurableSpeedComponentLogic.Instance.OldEnvironmentComponent.EnableThrustRatio ? "Yes" : "No");
                        msg.AppendFormat("  Thrust Ratio: x{0:N3}\r\n", ConfigurableSpeedComponentLogic.Instance.OldEnvironmentComponent.ThrustRatio);
                        msg.AppendLine();

                        if (ConfigurableSpeedComponentLogic.Instance.IsModified)
                        {
                            msg.AppendLine("The new settings have been set to:");
                            msg.AppendFormat("  Maximum Large Ship speed: {0:N0} m/s\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.LargeShipMaxSpeed);
                            msg.AppendFormat("  Maximum Small Ship speed: {0:N0} m/s\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.SmallShipMaxSpeed);
                            msg.AppendFormat("  Enable Thrust Ratio: {0}\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.EnableThrustRatio ? "Yes" : "No");
                            msg.AppendFormat("  Thrust Ratio: x{0:N3}\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.ThrustRatio);
                            msg.AppendLine();
                            msg.AppendLine("You must save and restart/reload the game to apply these settings.");
                        }
                        else
                        {
                            msg.AppendFormat("No new settings have been made.\r\n");
                        }

                        msg.AppendLine();
                        msg.AppendLine();
                        msg.AppendFormat("Any new maximum ship speed limit can be set from {0:N0} to {1:N0} m/s.\r\n", 1, 150000000m);
                        msg.AppendLine();
                        ShowExamples(msg);

                        MessageClientDialogMessage.SendMessage(SenderSteamId, "ConfigSpeed", " ", msg.ToString());
                    }
                    break;

                #endregion
            }
        }

        private void SetEnableThrustRatio(bool setting)
        {
            ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.EnableThrustRatio = setting;
            ConfigurableSpeedComponentLogic.Instance.IsModified = true;

            var msg = new StringBuilder();
            msg.AppendFormat("EnableThrustRatio updated to: {0}\r\n", ConfigurableSpeedComponentLogic.Instance.EnvironmentComponent.EnableThrustRatio ? "Yes" : "No");
            msg.AppendLine();
            msg.AppendLine();
            msg.AppendLine("Once you have finished your changes, you must save the game and then restart it immediately for it to take effect.");
            msg.AppendLine();
            msg.AppendLine("If you only save the game and do not restart, any player that connects will experience issues.");
            MessageClientDialogMessage.SendMessage(SenderSteamId, "ConfigSpeed", " ", msg.ToString());
        }

        private void ShowExamples(StringBuilder msg)
        {
            msg.AppendLine("Examples:");
            msg.AppendLine("  /configspeed");
            msg.AppendLine("  /configspeed ResetAll");
            msg.AppendLine("  /configspeed LargeShipSpeed 850");
            msg.AppendLine("  /configspeed SmallShipSpeed 420");
            msg.AppendLine("  /configspeed EnableThrustRatio on");
            msg.AppendLine("  /configspeed ThrustRatio 10");

        }
    }
}
