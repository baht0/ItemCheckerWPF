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
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "16.10.0.0")]
    internal sealed partial class StartProperties : global::System.Configuration.ApplicationSettingsBase {
        
        private static StartProperties defaultInstance = ((StartProperties)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new StartProperties())));
        
        public static StartProperties Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool completionUpdate {
            get {
                return ((bool)(this["completionUpdate"]));
            }
            set {
                this["completionUpdate"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool whatIsNew {
            get {
                return ((bool)(this["whatIsNew"]));
            }
            set {
                this["whatIsNew"] = value;
            }
        }
    }
}
