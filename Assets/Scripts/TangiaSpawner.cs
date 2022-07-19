using System.Collections;
using UnityEngine;
using Tangia;

public class TangiaSpawner : MonoBehaviour
{
    // Get this ID from the Tangia game developer dashboard
    // The version of your game is used to ensure we only send interaction events this version understands
    // this is "Martins Test game"
    TangiaAPI api = new TangiaAPI("game_aaniPhdMIRffFYKPBNtMtA", "1.0.0", "https://tangia.staging.ultimatearcade.io");

    private string sessionKey
    {
        get { return PlayerPrefs.GetString("tangia-session-key"); }
        set { PlayerPrefs.SetString("tangia-session-key", value); }
    }

    public LoginResult LoginResult { get; set; }
    public bool IsLoggedIn { get { return !string.IsNullOrEmpty(sessionKey); } }

    private bool isPlaying = false;

    // The code that a streamer enters into your game is very short lived.
    // This call verifies it and exchanges it for a long lived session key
    public IEnumerator Login(string code)
    {
        yield return api.Login(code, lr => LoginResult = lr);
        if (LoginResult.Success)
        {
            sessionKey = LoginResult.SessionKey;
            api.SessionKey = sessionKey;
            if (isPlaying)
            {
                // we don't want code that waits for login to wait forever
                StartCoroutine(nameof(PollEvents));
            }
        }
        else
        {
            Debug.Log("login error: " + LoginResult.ErrorMessage);
        }
    }

    // Call this when the player starts playing and you'd be ready to accept events
    public void OnStartPlaying()
    {
        if (isPlaying)
            return;
        isPlaying = true;
        if (IsLoggedIn)
        {
            Debug.Log("start playing");
            api.SessionKey = sessionKey;
            StartCoroutine(nameof(PollEvents));
        }
    }

    // Call this when the player is not actively playing, e.g. when they're in a menu or
    // simply are closing the game
    public void OnStopPlaying()
    {
        if (!isPlaying)
            return;
        isPlaying = false;
        Debug.Log("stop playing");
        StopCoroutine(nameof(PollEvents));
        StartCoroutine(api.StopPlaying());
    }

    // This constantly checks for new events and notifies our backend of success or failure.
    // You don't need to worry about performance of this loop as the API is "long-polling".
    // This means, if there is no event ready yet it will wait for up to a minute before returning,
    // So this loop actually doesn't run very often
    private IEnumerator PollEvents()
    {
        Debug.Log("start PollEvents");
        while (true)
        {
            GameEventsResp resp = null;
            yield return api.PollEvents(e => resp = e);
            if (resp == null || resp.Events == null || resp.Events.Length == 0)
            {
                Debug.Log("got no events. Err: "+resp?.Error);
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            foreach (var evt in resp.Events)
            {
                Debug.Log("we got an event: " + evt.ToString());
                // TODO: check if the game can handle this interaction event right now and process it
                //   e.g. by spawning a new item depending on `evt.InteractionID`
                var couldHandleEvent = true;
                if (couldHandleEvent)
                    yield return api.AckEvent(evt.EventID);
                else
                    yield return api.RejectEvent(evt.EventID, "the reason why we rejected");
            }
        }
    }
}