﻿//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由工具生成。
//     运行时版本:4.0.30319.42000
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

namespace Zongsoft.Tools.Deployer.Properties {
    using System;
    
    
    /// <summary>
    ///   一个强类型的资源类，用于查找本地化的字符串等。
    /// </summary>
    // 此类是由 StronglyTypedResourceBuilder
    // 类通过类似于 ResGen 或 Visual Studio 的工具自动生成的。
    // 若要添加或移除成员，请编辑 .ResX 文件，然后重新运行 ResGen
    // (以 /str 作为命令选项)，或重新生成 VS 项目。
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   返回此类使用的缓存的 ResourceManager 实例。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Zongsoft.Tools.Deployer.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   重写当前线程的 CurrentUICulture 属性，对
        ///   使用此强类型资源类的所有资源查找执行重写。
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   查找类似 ( 的本地化字符串。
        /// </summary>
        internal static string DeploymentComplete_CountBegin {
            get {
                return ResourceManager.GetString("DeploymentComplete.CountBegin", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 ) 的本地化字符串。
        /// </summary>
        internal static string DeploymentComplete_CountEnd {
            get {
                return ResourceManager.GetString("DeploymentComplete.CountEnd", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 ,  的本地化字符串。
        /// </summary>
        internal static string DeploymentComplete_CountSparator {
            get {
                return ResourceManager.GetString("DeploymentComplete.CountSparator", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Failure: {0} 的本地化字符串。
        /// </summary>
        internal static string DeploymentComplete_FailedCount {
            get {
                return ResourceManager.GetString("DeploymentComplete.FailedCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Deploy the {0} file is complete and a total of {1} file copy operations have been performed. 的本地化字符串。
        /// </summary>
        internal static string DeploymentComplete_Message {
            get {
                return ResourceManager.GetString("DeploymentComplete.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Success: {0} 的本地化字符串。
        /// </summary>
        internal static string DeploymentComplete_SucceedCount {
            get {
                return ResourceManager.GetString("DeploymentComplete.SucceedCount", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The specified {0} deployment file does not exist. 的本地化字符串。
        /// </summary>
        internal static string DeploymentFileNotExists_Message {
            get {
                return ResourceManager.GetString("DeploymentFileNotExists.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The specified {0} directory does not exist. 的本地化字符串。
        /// </summary>
        internal static string DirectoryNotExists_Message {
            get {
                return ResourceManager.GetString("DirectoryNotExists.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Error:  的本地化字符串。
        /// </summary>
        internal static string Error_Prompt {
            get {
                return ResourceManager.GetString("Error.Prompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The specified {0} file failed to be deleted. 的本地化字符串。
        /// </summary>
        internal static string FileDeleteFailed_Message {
            get {
                return ResourceManager.GetString("FileDeleteFailed.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The specified {0} file was deleted successfully. 的本地化字符串。
        /// </summary>
        internal static string FileDeleteSucceed_Message {
            get {
                return ResourceManager.GetString("FileDeleteSucceed.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The {0} File deployment to
        ///      {1} failed. 的本地化字符串。
        /// </summary>
        internal static string FileDeployFailed_Message {
            get {
                return ResourceManager.GetString("FileDeployFailed.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The {0} File deployment to
        ///      {1} failed(overwrite={2}). Note: Because the destination file already exists. 的本地化字符串。
        /// </summary>
        internal static string FileDeployFailed_Never_Message {
            get {
                return ResourceManager.GetString("FileDeployFailed.Never.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The {0} File deployment to
        ///      {1} failed(overwrite={2}). Note: Because the destination file is newer than the source file. 的本地化字符串。
        /// </summary>
        internal static string FileDeployFailed_Newer_Message {
            get {
                return ResourceManager.GetString("FileDeployFailed.Newer.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The {0} File deployment to
        ///      {1} was successful. 的本地化字符串。
        /// </summary>
        internal static string FileDeploySucceed_Message {
            get {
                return ResourceManager.GetString("FileDeploySucceed.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The specified {0} file does not exist. 的本地化字符串。
        /// </summary>
        internal static string FileNotExists_Message {
            get {
                return ResourceManager.GetString("FileNotExists.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The input &quot;{0}&quot; is an invalid parameter format. 的本地化字符串。
        /// </summary>
        internal static string InvalidArgumentFormat_Message {
            get {
                return ResourceManager.GetString("InvalidArgumentFormat.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Latest 的本地化字符串。
        /// </summary>
        internal static string Latest {
            get {
                return ResourceManager.GetString("Latest", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The deployment file parameter is not specified and there is no default deployment file(.deploy) in the current directory. 的本地化字符串。
        /// </summary>
        internal static string MissingArguments_Message {
            get {
                return ResourceManager.GetString("MissingArguments.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 [NuGet] The download of the specified {0}@{1} package failed, please try again later or change the download source. 的本地化字符串。
        /// </summary>
        internal static string NuGet_DownloadFailed_Message {
            get {
                return ResourceManager.GetString("NuGet:DownloadFailed.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 [NuGet] The specified {0} argument is invalid and is located in the {1} file. 的本地化字符串。
        /// </summary>
        internal static string NuGet_IllegalArgument_Message {
            get {
                return ResourceManager.GetString("NuGet:IllegalArgument.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 [NuGet] The {1} version of the {0} package was not found. 的本地化字符串。
        /// </summary>
        internal static string NuGet_NotFound_Message {
            get {
                return ResourceManager.GetString("NuGet:NotFound.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The {0} resolver in the {1} expression is undefined. 的本地化字符串。
        /// </summary>
        internal static string ResolverUndefined_Message {
            get {
                return ResourceManager.GetString("ResolverUndefined.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The {0} resolver in the {1} expression is undefined and located in the {2} file. 的本地化字符串。
        /// </summary>
        internal static string ResolverUndefinedInFile_Message {
            get {
                return ResourceManager.GetString("ResolverUndefinedInFile.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Tips:  的本地化字符串。
        /// </summary>
        internal static string Tips_Prompt {
            get {
                return ResourceManager.GetString("Tips.Prompt", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The {0} variable in the {1} expression is undefined. 的本地化字符串。
        /// </summary>
        internal static string VariableUndefined_Message {
            get {
                return ResourceManager.GetString("VariableUndefined.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The {0} variable in the {1} expression is undefined and located in the {2} file. 的本地化字符串。
        /// </summary>
        internal static string VariableUndefinedInFile_Message {
            get {
                return ResourceManager.GetString("VariableUndefinedInFile.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 The specified {1} command option value contains an undefined {0} variable. 的本地化字符串。
        /// </summary>
        internal static string VariableUndefinedInOption_Message {
            get {
                return ResourceManager.GetString("VariableUndefinedInOption.Message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   查找类似 Warn:  的本地化字符串。
        /// </summary>
        internal static string Warn_Prompt {
            get {
                return ResourceManager.GetString("Warn.Prompt", resourceCulture);
            }
        }
    }
}
