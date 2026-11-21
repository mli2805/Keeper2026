using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using Caliburn.Micro;

namespace KeeperWpf;

public class MyMessageBoxViewModel : Screen
{
    private readonly string _caption;
    public List<ListLine> Lines { get; set; }
    public Visibility OkVisibility { get; set; }
    public Visibility CancelVisibility { get; set; }


    public MyMessageBoxViewModel(MessageType messageType, string message)
    {
        Lines = new List<ListLine>()
        {
            new ListLine(){Line = message, FontWeight = FontWeights.Bold}
        };

        _caption = messageType.GetLocalizedString();
        OkVisibility = messageType.ShouldOkBeVisible();
        CancelVisibility = messageType.ShouldCancelBeVisible();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="messageType"></param>
    /// <param name="strs"></param>
    /// <param name="focusedString">Номер жирной строки, считая с 0. Если 777, то все жирные</param>
    public MyMessageBoxViewModel(MessageType messageType, List<string> strs, int focusedString = 0)
    {
        Lines = strs.Select(s => new ListLine() {Line = s}).ToList();
        
        if (focusedString >= 0 && focusedString < Lines.Count)
            Lines[focusedString].FontWeight = FontWeights.Bold;
        else if (focusedString == 777)
            Lines.ForEach(l => l.FontWeight = FontWeights.Bold);

        _caption = messageType.GetLocalizedString();
        OkVisibility = messageType.ShouldOkBeVisible();
        CancelVisibility = messageType.ShouldCancelBeVisible();
    }

    public MyMessageBoxViewModel(MessageType messageType, List<ListLine> lines)
    {
        Lines = lines;

        _caption = messageType.GetLocalizedString();
        OkVisibility = messageType.ShouldOkBeVisible();
        CancelVisibility = messageType.ShouldCancelBeVisible();
    }

    protected override void OnViewLoaded(object view)
    {
        DisplayName = _caption;
    }
    public async void OkButton()
    {
        await TryCloseAsync(true);
    }

    public async void CancelButton()
    {
        await TryCloseAsync(false);
    }

    /// <summary>Just for debug purposes </summary>
    [Localizable(false)]
    [ExcludeFromCodeCoverage]
    public override string ToString()
    {
        return "MyMessageBoxViewModel:" + Lines.FirstOrDefault();
    }
}
