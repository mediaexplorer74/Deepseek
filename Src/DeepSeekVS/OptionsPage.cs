using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace DeepSeekVS
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    /// </summary>
    /// <remarks>
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane,
    /// usually implemented by the package implementer.
    /// <para>
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its
    /// implementation of the IVsUIElementPane interface.
    /// </para>
    /// </remarks>
    [Guid("346bf8be-ee4d-468d-a627-a4d1223ec71c")]
    [ComVisible(true)]
    public class OptionsPage : DialogPage
    {
        private string _apiKey = "";
        private string _model = "deepseek/deepseek-r1:free";

        [Category("DeepSeek")]
        [DisplayName("API Key")]
        [Description("API key for OpenRouter")]
        public string ApiKey
        {
            get => _apiKey;
            set => _apiKey = value;
        }

        [Category("DeepSeek")]
        [DisplayName("AI Model")]
        [Description("Model to use for chat")]
        public string Model
        {
            get => _model;
            set => _model = value;
        }
    }
}   

