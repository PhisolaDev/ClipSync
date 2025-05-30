using System.Text.Json;
using ClipSync.Shared;
using System.Net.Mail;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();


JsonElement secjson = JsonDocument.Parse(File.ReadAllText("mysec.json")).RootElement;
List<string> passphrasewords = JsonSerializer.Deserialize<List<string>>(secjson.GetProperty("phrasewords"));

HttpClient globalhttpclient = new();

Dictionary<string, UserClass> userdb = new();
Dictionary<string, ClipClass> clipdb = new();
Dictionary<string, string> emailverificationdic = new();


await programentry();

async Task programentry()
{
    // globalhttpclient.PostAsJsonAsync()
}


async Task<ClipClass> getclip(string username, string clippassphrase)
{
    ClipClass chosenclip = new();
    foreach (string clipid in userdb[username].ClipIDList)
    {
        if (clipdb[clipid].AuthToken == clippassphrase)
        { chosenclip = clipdb[clipid]; break; }
    }
    return chosenclip;
}



bool isemailfree(string emailaddress)
{
    bool response = true;
    foreach (UserClass oneuser in userdb.Values.ToList())
    {
        if (oneuser.Email.Length > 1)
        {
            if (oneuser.Email == emailaddress)
            { response = false; break; }
        }
    }
    return response;
}

bool isemailokay(string emailaddress)
{
    bool response = true;

    if (emailaddress.Contains("@") == false || emailaddress.Contains(".") == false ||
    emailaddress.Length < 5)
    { response = false; }

    //Just let the email fail
    // emailaddress = emailaddress.ToLower().Trim();
    // string alphabets = "abcdefghijklmnopqrstuvwxyz";
    // string specialchars = "";

    // foreach (Char onechar in emailaddress)
    // {
    //     if (alphabets.Contains(onechar) == true) { }
    //     else if ()
    // }

    return response;
}

bool isusernamefree(string username)
{
    bool response = true;
    foreach (UserClass oneuser in userdb.Values.ToList())
    {
        if (oneuser.Username.Length > 1)
        {
            if (oneuser.Username == username)
            { response = false; break; }
        }
    }
    return response;

}

bool isusernameokay(string username)
{
    bool response = true;
    string validcharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    foreach (char onechar in username)
    {
        if (validcharacters.Contains(onechar) == false)
        { response = false; break; }
    }
    return response;
}


string getapassphrase(int phraselength = 4)
{
    StringBuilder fullphrase = new();
    Random myrand = new();

    for(int counta = 0; counta < phraselength; counta++)
    { fullphrase.Append(passphrasewords[myrand.Next(passphrasewords.Count)] + " "); }

    return fullphrase.ToString().TrimEnd().Replace(" ", "-");
}

async Task sendemail(string recepientaddress, string subject, string message, string attachmentpath = "")
{
    string useremail = secjson.GetProperty("zemail").ToString();
    string password = secjson.GetProperty("zpass").ToString();
    string host = secjson.GetProperty("zhost").ToString();
    int port = secjson.GetProperty("zport").GetInt32();

    MailMessage mail = new MailMessage();
    SmtpClient myclient = new SmtpClient(host, port);

    mail.From = new MailAddress(useremail);
    mail.To.Add(recepientaddress);
    mail.Subject = subject;
    mail.Body = message;
    if (attachmentpath.Length > 0)
    {
        Attachment myattachment = new Attachment(attachmentpath);
        mail.Attachments.Add(myattachment);
    }

    myclient.Port = port;
    myclient.Credentials = new System.Net.NetworkCredential(useremail, password);
    myclient.EnableSsl = true;

    // myclient.Send(mail);
    await Task.Run(() => myclient.Send(mail));

}










app.MapGet("/", () => "Hello World!");


app.Map("/register", (UserClass oneuser) =>
{
    string response = "error";

    // if (oneuser.Email.Length > 1)
    // {
    //     if (isemailfree(oneuser.Email) == false)
    //     { response += "_email_in_use"; return response; }
    //     if (isemailokay(oneuser.Email) == false)
    //     { response += "_email_not_okay"; return response; }
    // }
    if (isusernamefree(oneuser.Username) == false)
    { response += "_username_in_use"; return response; }
    if (isusernameokay(oneuser.Username) == false)
    { response += "_username_not_okay"; return response; }

    userdb.Add(oneuser.Username, oneuser);
    response = "user_registered_successfully";

    return response;
});

app.Map("/sendverificationemail/{emailaddress}", async (string emailaddress) =>
{
    string response = "error";
    
    if (emailaddress.Length > 1)
    {
        if (isemailfree(emailaddress) == false)
        { response += "_email_in_use"; return response; }
        if (isemailokay(emailaddress) == false)
        { response += "_email_not_okay"; return response; }
    }

    string passphrase = "";
    if (emailverificationdic.Keys.ToList().Contains(emailaddress))
    { passphrase = emailverificationdic[emailaddress]; }
    else { passphrase = getapassphrase(); emailverificationdic.Add(emailaddress, passphrase); }

    await sendemail(emailaddress, "ClipSync Email Verification Code", passphrase);
    response = "email_sent_successfully";

    return response;
});

app.Map("/login", (UserClass oneuser) =>
{
    string response = "error";

    if (userdb.Keys.ToList().Contains(oneuser.Username))
    {
        if (userdb[oneuser.Username].HashedPassword == oneuser.HashedPassword)
        { response = "success"; }
    }

    return response;
});

app.Map("/addclip", (JsonElement combinedjson) =>
{
    UserClass clipuser = JsonSerializer.Deserialize<UserClass>(combinedjson.GetProperty("userclass"));
    ClipClass clip = JsonSerializer.Deserialize<ClipClass>(combinedjson.GetProperty("clipclass"));

    clipdb.Add(clip.ID, clip);
});

app.Map("/getclip/{username}/{clipauthtoken}", async (string username, string clipauthtoken) =>
{
    return await getclip(username, clipauthtoken);
});

app.Map("/editclip", (JsonElement combinedjson) =>
{
    UserClass clipuser = JsonSerializer.Deserialize<UserClass>(combinedjson.GetProperty("userclass"));
    ClipClass clip = JsonSerializer.Deserialize<ClipClass>(combinedjson.GetProperty("clipclass"));

    if (clipuser.HashedPassword == userdb[clip.ClipCreatorUsername].HashedPassword)
    { clipdb[clip.ID] = clip; }

});

app.Run();
