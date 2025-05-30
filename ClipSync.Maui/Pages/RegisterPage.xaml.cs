using ClipSync.Shared;

namespace ClipSync.Maui.Pages;

public partial class RegisterPage : ContentPage
{

    HttpClient globalhttpclient = new();

    public RegisterPage()
    {
        InitializeComponent();
    }

    // private void Button_GetEmailCode(object sender, EventArgs e)
    // {

    // }

    private void RadioButtonChanged(object sender, CheckedChangedEventArgs e)
    {
        EmailStackLayout.IsVisible = e.Value;
        EmailStackLayout.IsEnabled = e.Value;
    }

    private async Task Button_SendVerificationCode(object sender, EventArgs e)
    {
        // UserClass oneuser = new(){ Username = EntryUsername.Text, };
        // app.Map("/sendverificationemail/{emailaddress}"
        string emailaddress = EntryEmailAddress.Text.ToLower().Trim();
        string response = await globalhttpclient.GetStringAsync(MyShared.BackendUrl +
        $"/sendverificationemail/{emailaddress}");
        
    }

    private void Button_ConfirmVerificationCode(object sender, EventArgs e)
    {

    }
    

}