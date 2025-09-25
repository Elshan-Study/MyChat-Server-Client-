namespace ChatClient.ViewModels;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class BaseNotify : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}