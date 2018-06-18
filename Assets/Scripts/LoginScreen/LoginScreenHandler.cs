using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Util;
using System;
using Sfs2X.Requests;
using Facebook.Unity;
using Sfs2X.Entities.Data;

public class LoginScreenHandler : MonoBehaviour
{

    //----------------------------------------------------------
    // Editor public properties
    //----------------------------------------------------------

    [Tooltip("IP address or domain name of the SmartFoxServer 2X instance")]
    public string Host = "127.0.0.1";

    [Tooltip("TCP port listened by the SmartFoxServer 2X instance")]
    public int TcpPort = 9933;

    [Tooltip("UDP port listened by the SmartFoxServer 2X instance")]
    public int UdpPort = 9933;

    [Tooltip("Name of the SmartFoxServer 2X Zone to join")]
    public string Zone = "BasicExamples";

    //----------------------------------------------------------
    // UI elements
    //----------------------------------------------------------

    public InputField nameInput;
    public InputField passInput;
    public Button registerButton;
    public Button loginButton;
    public Button fbLoginButton;
    public Text errorText;
    // roi do, lay teaxt = cai nameinput.gettext

    //----------------------------------------------------------
    // Private properties
    //----------------------------------------------------------

    private SmartFox sfs;
    private String userNameFB;
    private String userEmailFB;
    private LOGIN task;
    private String CMD_SUBMIT = "$SignUp.Submit";

    enum LOGIN
    {
        CASUAL,
        FACEBOOK,
        DOREGISTERFBINFO,
        REGISTERED
    }

    public Dictionary<string, object> FBUserDetails { get; private set; }

    // Use this for initialization
    void Start()
    {

    }

    //----------------------------------------------------------
    // Unity calback methods
    //----------------------------------------------------------

    void Awake()
    {
        Application.runInBackground = true;

#if UNITY_WEBPLAYER
		if (!Security.PrefetchSocketPolicy(Host, TcpPort, 500)) {
			Debug.LogError("Security Exception. Policy file loading failed!");
		}
#endif

        // Enable interface
        enableLoginUI(true);

        // Init FB
        if (!FB.IsInitialized)
        {
            // Initialize the Facebook SDK
            FB.Init(InitCallback, OnHideUnity);
        }
        else
        {
            // Already initialized, signal an app activation App Event
            FB.ActivateApp();
        }
    }

    private void InitCallback()
    {
        if (FB.IsInitialized)
        {
            // Signal an app activation App Event
            FB.ActivateApp();
            // Continue with Facebook SDK
            // ...
        }
        else
        {
            Debug.Log("Failed to Initialize the Facebook SDK");
        }
    }

    private void OnHideUnity(bool isGameShown)
    {
        if (!isGameShown)
        {
            // Pause the game - we will need to hide
            Time.timeScale = 0;
        }
        else
        {
            // Resume the game - we're getting focus again
            Time.timeScale = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (sfs != null)
            sfs.ProcessEvents();
    }

    //----------------------------------------------------------
    // Private helper methods
    //----------------------------------------------------------

    private void enableLoginUI(bool enable)
    {
        nameInput.enabled = enable;
        passInput.enabled = enable;
        loginButton.enabled = enable;
        registerButton.enabled = enable;
    }

    private void reset()
    {
        // Remove SFS2X listeners
        // This should be called when switching scenes, so events from the server do not trigger code in this scene
        sfs.RemoveAllEventListeners();

        // Enable interface
        enableLoginUI(true);

    }

    //----------------------------------------------------------
    // SmartFoxServer event listeners
    //----------------------------------------------------------

    private void OnConnection(BaseEvent evt)
    {
        if ((bool)evt.Params["success"])
        {
            // Save reference to SmartFox instance; it will be used in the other scenes
            SmartFoxConnection.Connection = sfs;

            Debug.Log("SFS2X API version: " + sfs.Version);
            Debug.Log("Connection mode is: " + sfs.ConnectionMode);
            Debug.Log(task);
            // Normal Login
            if (task == LOGIN.CASUAL)
                sfs.Send(new Sfs2X.Requests.LoginRequest(nameInput.text, passInput.text, Zone));

            // Facebook Login at first time, we need to register first
            if (task == LOGIN.FACEBOOK || task == LOGIN.REGISTERED)
                sfs.Send(new LoginRequest(FBUserDetails["username"].ToString(), "Passc0de", Zone));

            // Register the facebook information to database
            if (task == LOGIN.DOREGISTERFBINFO)
                sfs.Send(new LoginRequest(""));

        }
        else
        {
            // Remove SFS2X listeners and re-enable interface
            reset();

            // Show error message
            errorText.text = "Connection failed; is the server running at all?";
        }
    }

    private void OnConnectionLost(BaseEvent evt)
    {
        // Remove SFS2X listeners and re-enable interface
        reset();

        string reason = (string)evt.Params["reason"];

        if (reason != ClientDisconnectionReason.MANUAL)
        {
            // Show error message
            errorText.text = "Connection was lost; reason is: " + reason;
        }
    }

    private void OnLogin(BaseEvent evt)
    {
        // Initialize UDP communication
        // Host and port have been configured in the ConfigData object passed to the SmartFox.Connect method
        //sfs.InitUDP();

        try
        {

            if (evt.Params.Contains("success") && !(bool)evt.Params["success"])
            {
                string loginErrorMessage = (string)evt.Params["errorMessage"];
                Debug.Log("Login error: " + loginErrorMessage);

            }
            else
            {
                Debug.Log("Logged in successfully");

                // Register fb infor
                if (task == LOGIN.DOREGISTERFBINFO)
                {
                    DoRegisterFBInfo();
                }

                // Startup up UDP
                //sfs.InitUDP(Host, UdpPort);

                if (task == LOGIN.CASUAL)
                {
                    GameBoard.UserName = nameInput.text;
                    // Load Waiting Room
                    SceneManager.LoadScene(3);
                }

                if (task == LOGIN.FACEBOOK)
                {
                    GameBoard.UserName = FBUserDetails["username"].ToString();
                    // Load Waiting Room
                    SceneManager.LoadScene(3);
                }

            }
        }
        catch (Exception ex)
        {
            Debug.Log("Exception handling login request: " + ex.Message + " " + ex.StackTrace);
        }
    }

    private void DoRegisterFBInfo()
    {
        SFSObject sfso = new SFSObject();
        sfso.PutUtfString("username", FBUserDetails["username"].ToString());
        sfso.PutUtfString("password", "Passc0de");
        sfso.PutUtfString("email", FBUserDetails["email"].ToString());
        sfso.PutUtfString("country", "Viet Nam");
        sfso.PutInt("age", 25);
        CMD_SUBMIT = "$SignUp.Submit";
        Debug.Log(CMD_SUBMIT);
        sfs.Send(new ExtensionRequest(CMD_SUBMIT, sfso));
    }

    private void OnLoginError(BaseEvent evt)
    {
        // Disconnect
        sfs.Disconnect();

        // Remove SFS2X listeners and re-enable interface
        reset();

        // Show error message
        errorText.text = "Login failed: " + (string)evt.Params["errorMessage"];
        if (task == LOGIN.FACEBOOK)
        {
            task = LOGIN.DOREGISTERFBINFO;
            Login();
        }

    }

    private void OnUdpInit(BaseEvent evt)
    {
        // Remove SFS2X listeners
        reset();

        if ((bool)evt.Params["success"])
        {
            // Set invert mouse Y option
            //OptionsManager.InvertMouseY = invertMouseToggle.isOn;

        }
        else
        {
            // Disconnect
            sfs.Disconnect();

            // Show error message
            errorText.text = "UDP initialization failed: " + (string)evt.Params["errorMessage"];
        }
    }

    public void LoginButtonClick()
    {
        task = LOGIN.CASUAL;
        Login();
    }

    private void SubscribeEvents()
    {
        sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
        sfs.AddEventListener(SFSEvent.UDP_INIT, OnUdpInit);
        sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);
    }

    void OnApplicationQuit()
    {
        Debug.Log("Application close in " + Time.time);
        SmartFoxConnection.Disconnect();
        Debug.Log("Application Quit");
    }

    private void Login()
    {
        enableLoginUI(false);

        // Set connection parameters
        ConfigData cfg = new ConfigData();
        cfg.Host = Host;
        cfg.Port = TcpPort;
        cfg.Zone = Zone;
        cfg.UdpHost = Host;
        cfg.UdpPort = UdpPort;

        // Initialize SFS2X client and add listeners
        sfs = new SmartFox();

        // Set ThreadSafeMode explicitly, or Windows Store builds will get a wrong default value (false)
        sfs.ThreadSafeMode = true;
        SubscribeEvents();

        // Connect to SFS2X
        sfs.Connect(cfg);
    }

    private void OnExtensionResponse(BaseEvent evt)
    {
        String cmd = (String)evt.Params["cmd"];
        ISFSObject sfso = (ISFSObject)evt.Params["params"];
        Debug.Log(sfso);
        Debug.Log(cmd);
        if (cmd == CMD_SUBMIT)
        {

            if (sfso.ContainsKey("success"))
            {
                Debug.Log("Success, thanks for registering");
                //SceneManager.LoadScene(3);
                //sfs.Send(new LogoutRequest());
                SmartFoxConnection.Disconnect();
                reset();

                // Login with registered account
                task = LOGIN.REGISTERED;
                Login();

            }
            else
                Debug.Log("SignUp Error:" + (String)evt.Params["errorMessage"]);
        }
    }

    public void RegisterButtonClick()
    {

        SceneManager.LoadScene(2);
    }

    public void FBLoginClick()
    {
        // Authentication with facebook account
        FBAuth();
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log("UserId: " + aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log("Permission: " + perm);
            }
            StartCoroutine(FetchFBProfile());
            //FetchFBProfile();
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }

    private void FBAuth()
    {
        if (FB.IsInitialized)
        {
            List<string> perms = new List<string>();
            perms.Add("public_profile");
            perms.Add("email");
            perms.Add("user_location");
            perms.Add("user_friends");
            perms.Add("user_gender");
            FB.LogInWithReadPermissions(perms, AuthCallback);
        }
    }

    private IEnumerator FetchFBProfile()
    {
        FB.API("/me?fields=name,email,gender,location", HttpMethod.GET, FetchProfileCallback, new Dictionary<string, string>() { });
        FB.API("/me/picture?redirect=false", HttpMethod.GET, ProfilePhotoCallback);
        yield return new WaitForSeconds(5);
        StartCoroutine(FBLogin());
        //yield return null;     
    }

    // Login with details got from fb
    private IEnumerator FBLogin()
    {
        task = LOGIN.FACEBOOK;
        Login();
        yield return null;
    }

    private void FetchProfileCallback(IGraphResult result)
    {

        Debug.Log(result.RawResult.ToString());

        FBUserDetails = (Dictionary<string, object>)result.ResultDictionary;
        //StartCoroutine(FetchFBProfilePicture());

        Debug.Log("Profile: name: " + FBUserDetails["name"]);
        Debug.Log("Profile: id: " + FBUserDetails["id"]);
        Debug.Log("Profile: email: " + FBUserDetails["email"]);

        FBUserDetails.Add("username", "fb" + FBUserDetails["id"]);

    }

    private void ProfilePhotoCallback(IGraphResult result)
    {
        if (String.IsNullOrEmpty(result.Error) && !result.Cancelled)
        { //if there isn't an error
            IDictionary data = result.ResultDictionary["data"] as IDictionary; //create a new data dictionary
            string photoURL = data["url"] as String; //add a URL field to the dictionary
            if (FBUserDetails != null)
                FBUserDetails.Add("avatar", photoURL);

            //StartCoroutine(fetchProfilePic(photoURL)); //Call the coroutine to download the photo
        }
    }

    private IEnumerator fetchProfilePic(string url)
    {
        WWW www = new WWW(url); //use the photo url to get the photo from the web
        yield return www; //wait until it has downloaded
        //this.profilePic = www.texture; //set your profilePic Image Component's sprite to the photo
    }
}

