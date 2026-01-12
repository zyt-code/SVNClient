using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using Svns.Models;
using Svns.Services.Localization;

namespace Svns.ViewModels;

public partial class DeleteDialogViewModel : ViewModelBase
{
    public string Title { get; }
    public string ConfirmMessage { get; }
    public ObservableCollection<SvnStatus> FilesToDelete { get; }

    public DeleteDialogViewModel(IEnumerable<SvnStatus> files)
    {
        var fileList = files.ToList();
        FilesToDelete = new ObservableCollection<SvnStatus>(fileList);

        if (fileList.Count == 1)
        {
            Title = Localize.Get("Delete.TitleSingle");
            ConfirmMessage = string.Format(Localize.Get("Delete.ConfirmMessage"), fileList[0].FileName);
        }
        else
        {
            Title = string.Format(Localize.Get("Delete.TitleMultiple"), fileList.Count);
            ConfirmMessage = Localize.Get("Delete.Confirm");
        }
    }
}
