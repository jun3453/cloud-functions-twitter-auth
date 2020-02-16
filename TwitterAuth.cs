using Firebase.DynamicLinks;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class TwitterAuth : MonoBehaviour {

	// replace your cloud functions url
	string requestTokenUrl = "https://hoge.cloudfunctions.net/request-token";
	string accessTokenUrl = "https://hoge.cloudfunctions.net/access-token";

	string twitterAuthUrl = "https://api.twitter.com/oauth/authenticate?oauth_token=";

	void Start()
	{
		DynamicLinks.DynamicLinkReceived += OnDynamicLink;
		StartCoroutine(twitterAuthentication());
	}

	IEnumerator twitterAuthentication()
	{
		UnityWebRequest webRequest = UnityWebRequest.Post(requestTokenUrl, "");

		using (webRequest) {
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError) {
				// TODO Error handling
				Debug.Log(webRequest.error);
				Debug.Log(webRequest.responseCode);
			} else {
				Debug.Log(webRequest.downloadHandler.text);
				var result = JsonUtility.FromJson<RequestTokenResult>(webRequest.downloadHandler.text);
				string token = result.oauth_token;
				Application.OpenURL(twitterAuthUrl + token);
			}
		}
		yield return null;
	}

	// Display the dynamic link received by the application.
	void OnDynamicLink(object sender, EventArgs args)
	{
		// TODO Error handling

		var dynamicLinkEventArgs = args as ReceivedDynamicLinkEventArgs;

		string queryStr = dynamicLinkEventArgs.ReceivedDynamicLink.Url.Query.ToString();

		string target = "?verifier=";
		string tokens = queryStr.Substring(target.Length);
		string[] arr = tokens.Split(',');

		string oauthToken = arr[0];
		string oauthVerifier = arr[1];

		StartCoroutine(getAccessToken(oauthToken, oauthVerifier));
	}

	IEnumerator getAccessToken(string oauthToken, string oauthVerifier)
	{
		string url = accessTokenUrl + "?oauth_token=" + oauthToken + "&oauth_verifier=" + oauthVerifier;

		UnityWebRequest webRequest = UnityWebRequest.Get(url);

		using (webRequest) {
			yield return webRequest.SendWebRequest();

			if (webRequest.isNetworkError) {
				// TODO Error handling
				Debug.Log(webRequest.error);
				Debug.Log(webRequest.responseCode);
			} else {
				var result = JsonUtility.FromJson<AccessTokenResult>(webRequest.downloadHandler.text);
				string accessToken = result.oauth_token;
				string accessTokenSecret = result.oauth_token_secret;

				registTwitterAuth(accessToken, accessTokenSecret);
			}
		}
		yield return null;
	}

	private void registTwitterAuth(string accessToken, string secret)
	{
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;

		Firebase.Auth.Credential credential = Firebase.Auth.TwitterAuthProvider.GetCredential(accessToken, secret);
		auth.SignInWithCredentialAsync(credential).ContinueWith(task => {
			if (task.IsCanceled) {
				// TODO Error handling
				Debug.LogError("SignInWithCredentialAsync was canceled.");
				return;
			}
			if (task.IsFaulted) {
				// TODO Error handling
				Debug.LogError("SignInWithCredentialAsync encountered an error: " + task.Exception);
				return;
			}

			Firebase.Auth.FirebaseUser newUser = task.Result;
			Debug.LogFormat("User signed in successfully: {0} ({1})",newUser.DisplayName, newUser.UserId);
		});

	}
}

[Serializable]
public class RequestTokenResult {
	public string oauth_token;
	public string oauth_token_secret;
}


[Serializable]
public class AccessTokenResult {
	public string oauth_token;
	public string oauth_token_secret;
	public string user_id;
	public string screen_name;
}