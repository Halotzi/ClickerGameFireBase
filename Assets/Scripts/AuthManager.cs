using Firebase;
using Firebase.Auth;
using System.Collections;
using TMPro;
using UnityEngine;

public class AuthManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus _dependencyStatus;
    public FirebaseAuth _firebaseAuth;
    public FirebaseUser _firebaseUser;

    [Header("Login")]
    public TMP_InputField _emailLoginField;
    public TMP_InputField _passwordLoginField;
    public TMP_Text _warningLoginText;
    public TMP_Text _confirmLoginText;

    [Header("Register")]
    public TMP_InputField _usernameRegisterField;
    public TMP_InputField _emailRegisterField;
    public TMP_InputField _passwordRegisterField;
    public TMP_InputField _passwordRegisterVerifyField;
    public TMP_Text _warningRegisterText;

    private void Awake()
    {
        CheckDependency();
    }

    /// <summary>
    /// Check that all of the necessary dependencies for Firebase are present on the system
    /// </summary>
    private void CheckDependency()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            _dependencyStatus = task.Result;
            if (_dependencyStatus == DependencyStatus.Available)
                InitializeFirebase();

            else
                Debug.LogError("Could not resolve all Firebase dependencies");
        });
    }

    private void InitializeFirebase()
    {
        Debug.Log("Setting up firebase Auth");
        _firebaseAuth = FirebaseAuth.DefaultInstance;
    }



    #region Login
    public void LoginButton()
    {

    }
    private IEnumerator Login(string email, string password)
    {
        var LoginTask = _firebaseAuth.SignInWithEmailAndPasswordAsync(email, password);//Sending the email and passeord
        yield return new WaitUntil(predicate: () => LoginTask.IsCompleted); //Wait until the task complete

        if (LoginTask.Exception is not null)
        {
            Debug.LogWarning(message: $"Failed to register task with {LoginTask.Exception}");
            FirebaseException firebaseEx = LoginTask.Exception.GetBaseException() as FirebaseException;
            AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

            string message = "Login Failed";
            switch (errorCode)
            {
                case AuthError.MissingEmail:
                    message = "Missing Email";
                    break;
                case AuthError.MissingPassword:
                    message = "Missing Password";
                    break;
                case AuthError.WrongPassword:
                    message = "Wrong Password";
                    break;
                case AuthError.InvalidEmail:
                    message = "Invalid Email";
                    break;
                case AuthError.UserNotFound:
                    message = "User not found";
                    break;
            }
            _warningLoginText.text = message;
        }

        else
        {
            //User in logged in
            //Now get the result
            _firebaseUser = LoginTask.Result.User;
            Debug.LogFormat($"User sign in successfully: {_firebaseUser.DisplayName} , {_firebaseUser.Email}");
            _warningLoginText.text = string.Empty;
            _confirmLoginText.text = "Logged In";

        }
    }
    #endregion

    #region Register
    public void RegisterButton()
    {

    }

    private IEnumerator Register(string _email, string _password, string _username)
    {
        if (_username == "")
        {
            //If the username field is blank show a warning
            _warningRegisterText.text = "Missing Username";
        }
        else if (_passwordRegisterField.text != _passwordRegisterVerifyField.text)
        {
            //If the password does not match show a warning
            _warningRegisterText.text = "Password Does Not Match!";
        }
        else
        {
            //Call the Firebase auth signin function passing the email and password
            var RegisterTask = _firebaseAuth.CreateUserWithEmailAndPasswordAsync(_email, _password);
            //Wait until the task completes
            yield return new WaitUntil(predicate: () => RegisterTask.IsCompleted);

            if (RegisterTask.Exception != null)
            {
                //If there are errors handle them
                Debug.LogWarning(message: $"Failed to register task with {RegisterTask.Exception}");
                FirebaseException firebaseEx = RegisterTask.Exception.GetBaseException() as FirebaseException;
                AuthError errorCode = (AuthError)firebaseEx.ErrorCode;

                string message = "Register Failed!";
                switch (errorCode)
                {
                    case AuthError.MissingEmail:
                        message = "Missing Email";
                        break;
                    case AuthError.MissingPassword:
                        message = "Missing Password";
                        break;
                    case AuthError.WeakPassword:
                        message = "Weak Password";
                        break;
                    case AuthError.EmailAlreadyInUse:
                        message = "Email Already In Use";
                        break;
                }
                _warningRegisterText.text = message;
            }
            else
            {
                //User has now been created
                //Now get the result
                _firebaseUser = RegisterTask.Result.User;

                if (_firebaseUser != null)
                {
                    //Create a user profile and set the username
                    UserProfile profile = new UserProfile { DisplayName = _username };

                    //Call the Firebase auth update user profile function passing the profile with the username
                    var ProfileTask = _firebaseUser.UpdateUserProfileAsync(profile);
                    //Wait until the task completes
                    yield return new WaitUntil(predicate: () => ProfileTask.IsCompleted);

                    if (ProfileTask.Exception != null)
                    {
                        //If there are errors handle them
                        Debug.LogWarning(message: $"Failed to register task with {ProfileTask.Exception}");
                        FirebaseException firebaseEx = ProfileTask.Exception.GetBaseException() as FirebaseException;
                        AuthError errorCode = (AuthError)firebaseEx.ErrorCode;
                        _warningRegisterText.text = "Username Set Failed!";
                    }

                    else
                    {
                        //Username is now set
                        //Now return to login screen
                        UIManager.instance.LoginScreen();
                        _warningRegisterText.text = "";
                    }
                }
            }

            #endregion

        }
    }

}
