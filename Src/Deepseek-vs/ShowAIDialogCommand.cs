using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using System;
using System.ComponentModel.Design;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
using Task = System.Threading.Tasks.Task;
using Microsoft.VisualStudio.Extensibility;
//using Microsoft.VisualStudio.Extensibility.Commands;
using EnvDTE;

namespace OpenRouterAIExtension
{
    /// <summary>
    /// A sample command for showing a dialog.
    /// </summary>
    //[VisualStudioContribution]
    public class ShowAIDialogCommand : Command
    {
        /// <summary>
        /// Command ID.
        /// </summary>
        public const int CommandId = 0x0100;

        /// <summary>
        /// Command menu group (command set GUID).
        /// </summary>
        public static readonly Guid CommandSet = new Guid("9db101ff-9ba0-475f-91d7-43cb9b9f7ccb");

        /// <summary>
        /// VS Package that provides this command, not null.
        /// </summary>
        private readonly AsyncPackage package;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShowAIDialogCommand"/> class.
        /// Adds our command handlers for menu (commands must exist in the command table file)
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        /// <param name="commandService">Command service to add command to, not null.</param>
        private ShowAIDialogCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            this.package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(CommandSet, CommandId);
            var menuItem = new MenuCommand(this.Execute, menuCommandID);
            commandService.AddCommand(menuItem);
        }

      
        /// <summary>
        /// Gets the instance of the command.
        /// </summary>
        public static ShowAIDialogCommand Instance
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the service provider from the owner package.
        /// </summary>
        private Microsoft.VisualStudio.Shell.IAsyncServiceProvider ServiceProvider
        {
            get
            {
                return this.package;
            }
        }

        /// <summary>
        /// Initializes the singleton instance of the command.
        /// </summary>
        /// <param name="package">Owner package, not null.</param>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            // Switch to the main thread - the call to AddCommand in ShowAIDialogCommand's constructor requires
            // the UI thread.
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            OleMenuCommandService commandService = await package.GetServiceAsync(typeof(IMenuCommandService)) as OleMenuCommandService;
            Instance = new ShowAIDialogCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            AIDialog dialog = new AIDialog(); // Window-based dialog (UC) 
            dialog.ShowDialog();//.ShowModal();
        }

        public object AddControl(object Owner, int Position = 1)
        {
            throw new NotImplementedException();
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }

        public string Name => throw new NotImplementedException();

        public Commands Collection => throw new NotImplementedException();

        public DTE DTE => throw new NotImplementedException();

        public string Guid => throw new NotImplementedException();

        public int ID => throw new NotImplementedException();

        public bool IsAvailable => throw new NotImplementedException();

        public object Bindings { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public string LocalizedName => throw new NotImplementedException();
    }
}
