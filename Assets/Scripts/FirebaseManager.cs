using Firebase;
using Firebase.Auth;
using Firebase.Database;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    [Header("Firebase")]
    public DependencyStatus _dependencyStatus;
    public FirebaseAuth _firebaseAuth;
    public FirebaseUser _firebaseUser;
    public DatabaseReference _databaseReference;

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

    [Header("UserData")]
    public TMP_Text _usernameName;
    public TMP_Text _currentScore;
    public Transform _scoreboardContent;
    public GameObject _scoreElementPrefab;

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
        _databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
    }

    #region Login

    public void LoginButton()
    {
        StartCoroutine(Login(_emailLoginField.text,_passwordLoginField.text));
    }

    private void ClearLogInFields()
    {
        _emailLoginField.text = string.Empty;
        _passwordLoginField.text = string.Empty;
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
            StartCoroutine(LoadUserData());
            yield return new WaitForSeconds(2f);
            _usernameName.text = _firebaseUser.DisplayName;
            UIManager.instance.DataScreen();
            _confirmLoginText.text = string.Empty;
            ClearLogInFields();
            ClearRegisterInFields();

        }
    }
    #endregion

    #region Register
    public void RegisterButton()
    {
        StartCoroutine(Register(_emailRegisterField.text, _passwordRegisterField.text, _usernameRegisterField.text));
    }

    private void ClearRegisterInFields()
    {
        _usernameRegisterField.text = string.Empty;
        _emailRegisterField.text = string.Empty;
        _passwordRegisterField.text = string.Empty;
        _passwordRegisterVerifyField.text = string.Empty;
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

            if (RegisterTask.Exception is not null)
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
                        ClearLogInFields();
                        ClearRegisterInFields();
                    }
                }
            }
        }
    }

    #endregion

    #region Sign Out

    public void SignOutButton()
    {
        _firebaseAuth.SignOut();
        UIManager.instance.LoginScreen();
        ClearLogInFields();
        ClearRegisterInFields();
    }

    #endregion

    #region Data

    public void SaveDataButton()
    {
        int newScore = int.Parse(_currentScore.text);
        newScore++;
        _currentScore.text = newScore.ToString();
       StartCoroutine(UpdateClicks(newScore));
    }


    private IEnumerator LoadUserData()
    {
        //Get the currently logged in user data
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception is not null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
        else if (DBTask.Result.Value == null)
        {
            //No data exists yet
            _currentScore.text = "0";
            _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("username").SetValueAsync(_firebaseUser.DisplayName);
        }
        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            _currentScore.text = snapshot.Child("clicks").Value.ToString();
        }
    }

    private IEnumerator UpdateClicks(int _clicks)
    {
        //Set the currently logged in user deaths
        var DBTask = _databaseReference.Child("users").Child(_firebaseUser.UserId).Child("clicks").SetValueAsync(_clicks);

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception != null)
        {
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");
        }
    }

    #endregion

    #region Scoreboard

    public void ScoreboardButton()
    {
        StartCoroutine(LoadScoreboardData());   
    }

    private IEnumerator LoadScoreboardData()
    {
        //Get all the users data ordered by kills amount
        var DBTask = _databaseReference.Child("users").OrderByChild("clicks").GetValueAsync();

        yield return new WaitUntil(predicate: () => DBTask.IsCompleted);

        if (DBTask.Exception is not null)
            Debug.LogWarning(message: $"Failed to register task with {DBTask.Exception}");

        else
        {
            //Data has been retrieved
            DataSnapshot snapshot = DBTask.Result;

            //Destroy any existing scoreboard elements
            foreach (Transform child in _scoreboardContent.transform)
            {
                Destroy(child.gameObject);
            }

            //Loop through every users UID
            foreach (DataSnapshot childSnapshot in snapshot.Children.Reverse<DataSnapshot>())
            {
                int clicks = int.Parse(childSnapshot.Child("clicks").Value.ToString());
                string username = childSnapshot.Child("username").Value.ToString();

                //Instantiate new scoreboard elements
                GameObject scoreboardElement = Instantiate(_scoreElementPrefab, _scoreboardContent);
                scoreboardElement.GetComponent<ScoreElement>().NewScoreElement(username, clicks);
            }

            //Go to scoareboard screen
            UIManager.instance.ScoreboardScreen();
        }
    }

    #endregion

}
