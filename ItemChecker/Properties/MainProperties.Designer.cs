﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace ItemChecker.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.3.0.0")]
    internal sealed partial class MainProperties : global::System.Configuration.ApplicationSettingsBase {
        
        private static MainProperties defaultInstance = ((MainProperties)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new MainProperties())));
        
        public static MainProperties Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool CompletionUpdate {
            get {
                return ((bool)(this["CompletionUpdate"]));
            }
            set {
                this["CompletionUpdate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SteamLoginSecure {
            get {
                return ((string)(this["SteamLoginSecure"]));
            }
            set {
                this["SteamLoginSecure"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string SessionBuff {
            get {
                return ((string)(this["SessionBuff"]));
            }
            set {
                this["SessionBuff"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int SteamCurrencyId {
            get {
                return ((int)(this["SteamCurrencyId"]));
            }
            set {
                this["SteamCurrencyId"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public decimal AnalysisStartBalance {
            get {
                return ((decimal)(this["AnalysisStartBalance"]));
            }
            set {
                this["AnalysisStartBalance"] = value;
            }
        }
    }
}
