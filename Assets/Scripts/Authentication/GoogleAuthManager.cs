using System;
using System.Collections;
using System.Net;
using System.Text;
using UnityEngine;
using CrudCustodian.Core;

namespace CrudCustodian.Authentication
{
    /// <summary>
    /// Low-level Google Sign-In implementation that adapts to the current platform:
    ///
    ///   • Android / iOS  — Uses the Google Sign-In Unity Plugin (com.google.signin).
    ///                      Drop the plugin .unitypackage into the project and the
    ///                      #if UNITY_ANDROID / UNITY_IOS blocks below will compile.
    ///
    ///   • Windows / macOS desktop — Uses a loopback OAuth 2.0 browser flow:
    ///                      1. Opens the system browser to Google's auth page.
    ///                      2. Spins up a local HTTP listener on localhost:PORT.
    ///                      3. Parses the authorization code from the redirect.
    ///                      4. Exchanges the code for an ID token via Google's
    ///                         token endpoint (done server-side in production;
    ///                         here we use a stub for offline / prototype use).
    ///
    ///   • Unity Editor    — Provides a simulated sign-in so you can test auth
    ///                       flows without leaving Play Mode.
    ///
    /// Events:
    ///   OnGoogleSignInSucceeded(userId, displayName)
    ///   OnGoogleSignInFailed(errorMessage)
    /// </summary>
    public class GoogleAuthManager : MonoBehaviour
    {
        // ── Events ─────────────────────────────────────────────────────────
        public event Action<string, string> OnGoogleSignInSucceeded;
        public event Action<string>         OnGoogleSignInFailed;

        // ── Inspector fields ───────────────────────────────────────────────
        [Header("Editor / Prototype Simulation")]
        [Tooltip("Simulated Google user ID returned in the Unity Editor. " +
                 "Has no effect in production builds.")]
        [SerializeField]
        private string editorSimulatedGoogleUserId = "editor_test_user_001";

        [Tooltip("Simulated Google display name returned in the Unity Editor.")]
        [SerializeField]
        private string editorSimulatedGoogleDisplayName = "Test Player";

        // ── Private state ──────────────────────────────────────────────────
        private bool isSignInCurrentlyInProgress;

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Begins the Google Sign-In flow appropriate for the current platform.
        /// Results are delivered via OnGoogleSignInSucceeded or OnGoogleSignInFailed.
        /// </summary>
        public void StartGoogleSignIn()
        {
            if (isSignInCurrentlyInProgress)
            {
                Debug.LogWarning("[GoogleAuthManager] Sign-in already in progress. Ignoring duplicate request.");
                return;
            }

            isSignInCurrentlyInProgress = true;

#if UNITY_EDITOR
            StartCoroutine(SimulateSignInInEditorCoroutine());
#elif UNITY_ANDROID || UNITY_IOS
            StartNativeMobileGoogleSignIn();
#elif UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
            StartCoroutine(DesktopBrowserOAuthFlowCoroutine());
#else
            HandleSignInFailure("Google Sign-In is not supported on this platform.");
#endif
        }

        // ── Editor simulation ──────────────────────────────────────────────

#if UNITY_EDITOR
        /// <summary>
        /// In the Unity Editor, waits one frame then fires a fake success so
        /// auth-dependent UI can be tested without a real Google account.
        /// </summary>
        private IEnumerator SimulateSignInInEditorCoroutine()
        {
            yield return null; // Let the calling frame finish.

            Debug.Log("[GoogleAuthManager] Editor simulation: sign-in succeeded.");
            HandleSignInSuccess(editorSimulatedGoogleUserId, editorSimulatedGoogleDisplayName);
        }
#endif

        // ── Mobile native sign-in ──────────────────────────────────────────

#if UNITY_ANDROID || UNITY_IOS
        /// <summary>
        /// Calls into the Google Sign-In Unity Plugin.
        ///
        /// SETUP REQUIRED:
        ///   1. Import the Google Sign-In Unity Plugin package.
        ///   2. On Android: place google-services.json in Assets/.
        ///   3. On iOS: place GoogleService-Info.plist in Assets/.
        ///   4. Replace the stub code below with actual plugin API calls.
        ///
        /// The real plugin calls look like:
        ///   GoogleSignIn.Configuration = new GoogleSignInConfiguration { WebClientId = ... };
        ///   GoogleSignIn.DefaultInstance.SignIn().ContinueWith(OnMobileSignInCallback);
        /// </summary>
        private void StartNativeMobileGoogleSignIn()
        {
            // ── Stub: replace with real Google Sign-In plugin calls ────────
            Debug.Log("[GoogleAuthManager] Mobile native sign-in would start here. " +
                      "Import the Google Sign-In Unity Plugin to enable real sign-in.");

            // For prototyping, fire the failure event so callers know to fall back to guest.
            HandleSignInFailure("Google Sign-In plugin not yet integrated. " +
                                "Import com.google.signin to enable.");
        }
#endif

        // ── Desktop loopback OAuth flow ────────────────────────────────────

#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_OSX
        /// <summary>
        /// Opens the system browser to Google's OAuth consent page, then
        /// listens on a local port for the redirect that carries the auth code.
        ///
        /// SETUP REQUIRED:
        ///   1. In Google Cloud Console, add "http://localhost:PORT/" as an
        ///      authorized redirect URI for your OAuth 2.0 Desktop client.
        ///   2. Replace YOUR_GOOGLE_WEB_CLIENT_ID in GameConstants.
        ///   3. In production, the code→token exchange should happen on your
        ///      backend server, not in the client, to protect client secrets.
        /// </summary>
        private IEnumerator DesktopBrowserOAuthFlowCoroutine()
        {
            string authorizationUrl = BuildGoogleAuthorizationUrl();

            // Open the system's default browser to the consent page.
            Application.OpenURL(authorizationUrl);
            Debug.Log($"[GoogleAuthManager] Desktop OAuth: opened browser to: {authorizationUrl}");

            // Listen for the redirect on localhost.
            yield return StartCoroutine(ListenForDesktopOAuthCallbackCoroutine());
        }

        /// <summary>
        /// Spins up a local HTTP listener and waits for Google to redirect
        /// back with the authorization code query parameter.
        /// Times out after 120 seconds.
        /// </summary>
        private IEnumerator ListenForDesktopOAuthCallbackCoroutine()
        {
            const float OAUTH_CALLBACK_TIMEOUT_SECONDS = 120f;
            float       secondsWaitedForCallback        = 0f;
            bool        callbackReceived                 = false;
            string      receivedAuthorizationCode        = string.Empty;
            string      listenerError                    = string.Empty;

            HttpListener localHttpListener = new HttpListener();
            localHttpListener.Prefixes.Add(GameConstants.GOOGLE_OAUTH_DESKTOP_REDIRECT_URI);

            try
            {
                localHttpListener.Start();
                Debug.Log($"[GoogleAuthManager] Desktop OAuth: listening on " +
                          $"{GameConstants.GOOGLE_OAUTH_DESKTOP_REDIRECT_URI}");
            }
            catch (Exception listenerStartException)
            {
                listenerError = $"Could not start local HTTP listener: {listenerStartException.Message}";
            }

            if (!string.IsNullOrEmpty(listenerError))
            {
                HandleSignInFailure(listenerError);
                yield break;
            }

            // Poll asynchronously so we don't block Unity's main thread.
            IAsyncResult pendingListenerResult = localHttpListener.BeginGetContext(null, null);

            while (!callbackReceived && secondsWaitedForCallback < OAUTH_CALLBACK_TIMEOUT_SECONDS)
            {
                if (pendingListenerResult.IsCompleted)
                {
                    HttpListenerContext callbackHttpContext = localHttpListener.EndGetContext(pendingListenerResult);
                    string callbackQueryString = callbackHttpContext.Request.Url.Query;

                    // Send a friendly HTML response so the browser doesn't show a blank page.
                    SendBrowserSuccessPage(callbackHttpContext.Response);

                    receivedAuthorizationCode = ExtractAuthCodeFromQueryString(callbackQueryString);
                    callbackReceived = true;
                }

                secondsWaitedForCallback += Time.unscaledDeltaTime;
                yield return null;
            }

            localHttpListener.Stop();

            if (!callbackReceived)
            {
                HandleSignInFailure("Google Sign-In timed out waiting for the browser callback.");
                yield break;
            }

            if (string.IsNullOrEmpty(receivedAuthorizationCode))
            {
                HandleSignInFailure("Google returned a callback but no authorization code was found.");
                yield break;
            }

            // In a production app, send receivedAuthorizationCode to your backend
            // server here, which exchanges it for tokens and returns the user profile.
            // For prototyping, we simulate success with a placeholder user ID.
            Debug.Log("[GoogleAuthManager] Desktop OAuth: authorization code received. " +
                      "Exchange for tokens on your backend server before shipping.");

            HandleSignInSuccess(
                googleUserId:      "desktop_oauth_user_placeholder",
                googleDisplayName: "Desktop Player");
        }

        /// <summary>Constructs the Google OAuth 2.0 authorization URL.</summary>
        private string BuildGoogleAuthorizationUrl()
        {
            string encodedRedirectUri = Uri.EscapeDataString(GameConstants.GOOGLE_OAUTH_DESKTOP_REDIRECT_URI);

            return "https://accounts.google.com/o/oauth2/v2/auth" +
                   $"?client_id={GameConstants.GOOGLE_OAUTH_WEB_CLIENT_ID}" +
                   $"&redirect_uri={encodedRedirectUri}" +
                   "&response_type=code" +
                   "&scope=openid%20email%20profile" +
                   "&access_type=offline";
        }

        /// <summary>Extracts the "code" parameter from a query string.</summary>
        private string ExtractAuthCodeFromQueryString(string queryString)
        {
            // queryString looks like "?code=4/xxx&scope=..."
            string codePrefix = "code=";
            int    codeStart  = queryString.IndexOf(codePrefix, StringComparison.Ordinal);
            if (codeStart < 0) return string.Empty;

            codeStart += codePrefix.Length;
            int codeEnd = queryString.IndexOf('&', codeStart);
            return codeEnd < 0
                ? queryString.Substring(codeStart)
                : queryString.Substring(codeStart, codeEnd - codeStart);
        }

        /// <summary>Writes a simple success HTML page to the browser tab.</summary>
        private void SendBrowserSuccessPage(HttpListenerResponse browserResponse)
        {
            const string successHtml =
                "<!DOCTYPE html><html><body style='font-family:sans-serif;text-align:center;margin-top:80px;'>" +
                "<h2>✅ Signed in successfully!</h2>" +
                "<p>You can now close this tab and return to <strong>Crud Custodian</strong>.</p>" +
                "</body></html>";

            byte[] successHtmlBytes = Encoding.UTF8.GetBytes(successHtml);
            browserResponse.ContentType     = "text/html; charset=UTF-8";
            browserResponse.ContentLength64 = successHtmlBytes.Length;
            browserResponse.OutputStream.Write(successHtmlBytes, 0, successHtmlBytes.Length);
            browserResponse.OutputStream.Close();
        }
#endif

        // ── Shared result handlers ─────────────────────────────────────────

        private void HandleSignInSuccess(string googleUserId, string googleDisplayName)
        {
            isSignInCurrentlyInProgress = false;
            OnGoogleSignInSucceeded?.Invoke(googleUserId, googleDisplayName);
        }

        private void HandleSignInFailure(string errorMessage)
        {
            isSignInCurrentlyInProgress = false;
            Debug.LogWarning($"[GoogleAuthManager] Sign-in failed: {errorMessage}");
            OnGoogleSignInFailed?.Invoke(errorMessage);
        }
    }
}
