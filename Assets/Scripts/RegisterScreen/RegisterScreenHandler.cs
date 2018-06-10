using Sfs2X;
using Sfs2X.Core;
using Sfs2X.Entities.Data;
using Sfs2X.Requests;
using Sfs2X.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class RegisterScreenHandler : MonoBehaviour
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
    public InputField confirmInput;
    public InputField emailInput;
    public Text errorText;


    //----------------------------------------------------------
    // Private properties
    //----------------------------------------------------------

    private SmartFox sfs;
    private String CMD_SUBMIT = "$SignUp.Submit";
    private Guid Id;

    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (sfs != null)
        {
            sfs.ProcessEvents();
        }
    }

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


    }

    private void enableLoginUI(bool enable)
    {
        nameInput.enabled = enable;
        passInput.enabled = enable;
        confirmInput.enabled = enable;
        emailInput.enabled = enable;
    }

    private void reset()
    {
        // Remove SFS2X listeners
        // This should be called when switching scenes, so events from the server do not trigger code in this scene
        sfs.RemoveAllEventListeners();

        // Enable interface
        enableLoginUI(true);
    }

    public void RegisButtonClick()
    {
        enableLoginUI(false);

        // Set connection parameters
        Sfs2X.Util.ConfigData cfg = new ConfigData();
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
        sfs.AddEventListener(SFSEvent.EXTENSION_RESPONSE, OnExtensionResponse);

        // Connect to SFS2X
        sfs.Connect(cfg);
        
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

    private void OnLogin(BaseEvent evt)
    {
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

                // Resgister
                SendRegister();
                
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Exception handling login request: " + ex.Message + " " + ex.StackTrace);
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
                SceneManager.LoadScene(1);
            }
            else
                Debug.Log("SignUp Error:" + (String)evt.Params["errorMessage"]);
        }
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

            // Login to zone first with Guest Id
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

    private void SendRegister()
    {
        SFSObject sfso = new SFSObject();
        //Id = new Guid();
        //sfso.PutUtfString("Id", Id.ToString());
        sfso.PutUtfString("username", nameInput.text);
        sfso.PutUtfString("password", passInput.text);
        sfso.PutUtfString("email", emailInput.text);
        sfso.PutUtfString("country", "Viet Nam");
        sfso.PutInt("age", 27);

        sfs.Send(new ExtensionRequest(CMD_SUBMIT, sfso));
    }
}
