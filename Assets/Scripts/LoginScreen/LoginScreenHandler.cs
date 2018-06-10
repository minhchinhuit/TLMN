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
    private String userName;
    private String userPass;

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

            // Login
            sfs.Send(new Sfs2X.Requests.LoginRequest(nameInput.text, passInput.text, Zone));
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

                // Startup up UDP
                sfs.InitUDP(Host, UdpPort);

                // Load Waiting Room
                SceneManager.LoadScene(3);
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Exception handling login request: " + ex.Message + " " + ex.StackTrace);
        }
    }

    private void OnLoginError(BaseEvent evt)
    {
        // Disconnect
        sfs.Disconnect();

        // Remove SFS2X listeners and re-enable interface
        reset();

        // Show error message
        errorText.text = "Login failed: " + (string)evt.Params["errorMessage"];
              
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

        sfs.AddEventListener(SFSEvent.CONNECTION, OnConnection);
        sfs.AddEventListener(SFSEvent.CONNECTION_LOST, OnConnectionLost);
        sfs.AddEventListener(SFSEvent.LOGIN, OnLogin);
        sfs.AddEventListener(SFSEvent.LOGIN_ERROR, OnLoginError);
        sfs.AddEventListener(SFSEvent.UDP_INIT, OnUdpInit);

        // Connect to SFS2X
        sfs.Connect(cfg);
    }

    public void RegisterButtonClick()
    {

        SceneManager.LoadScene(2);
    }

    public void FBLoginClick()
    {
        var perms = new List<string>() { "public_profile", "email" };
        FB.LogInWithReadPermissions(perms, AuthCallback);
    }

    private void AuthCallback(ILoginResult result)
    {
        if (FB.IsLoggedIn)
        {
            // AccessToken class will have session details
            var aToken = Facebook.Unity.AccessToken.CurrentAccessToken;
            // Print current access token's User ID
            Debug.Log(aToken.UserId);
            // Print current access token's granted permissions
            foreach (string perm in aToken.Permissions)
            {
                Debug.Log(perm);
            }
        }
        else
        {
            Debug.Log("User cancelled login");
        }
    }
}

