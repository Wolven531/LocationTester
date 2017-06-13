using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Locator : MonoBehaviour
{
    public GameObject Map;
    public Text Accuracy_Horiz;
    public Text Accuracy_Vert;
    public Text Altitude;
    public Text Frames;
    public Text LastUpdate;
    public Text Latitude;
    public Text Longitude;
    public Text Status;

    int waitCount = 0;

    void Start()
    {
        StartCoroutine(StartLoc());
    }

    void Update()
    {
        Frames.text = string.Format("Frames: {0}", Time.frameCount);

        UpdateLocDisp();
    }

    IEnumerator StartLoc()
    {
        // NOTE: wait for location service to be enabled
        while (!Input.location.isEnabledByUser)
        {
            Status.text = string.Format("{0} Waiting 1s for Location Services to be enabled...", waitCount);
            waitCount++;
            yield return new WaitForSeconds(1);
        }

        // NOTE: start service BEFORE querying location
        Input.location.Start(0.1f, 0.1f);

        // NOTE: wait until service initializes
        while (Input.location.status == LocationServiceStatus.Initializing)
        {
            Status.text = string.Format("{0} Waiting 1s for Location Services to initialize...", waitCount);
            waitCount++;
            yield return new WaitForSeconds(1);
        }

        UpdateLocDisp();
    }

    void UpdateLocDisp()
    {
        if (Input.location == null)
        {
            Status.text = "Input.location was null";
            return;
        }
        if (Input.location.status == LocationServiceStatus.Failed)
        {
            Status.text = "Input.location failed";
            return;
        }
        if (Input.location.status != LocationServiceStatus.Running)
        {
            Status.text = string.Format("Input.location.status was not running, status={0}", Input.location.status);
            return;
        }

        Status.text = string.Format(
            "Running... (x,y) = ({0}, {1})",
            MapUtils.LonToX(Input.location.lastData.longitude),
            MapUtils.LatToY(Input.location.lastData.latitude)
        );

        System.DateTime dt = Locator.UnixTimeStampToDateTime(Input.location.lastData.timestamp);
        int lblCols = 0;

        LastUpdate.text = string.Format("{0}: {1} {2}", "Last Update".PadLeft(lblCols), dt.ToShortDateString(), dt.ToLongTimeString());
        Altitude.text = string.Format("{0}: {1:N7}", "Altitude".PadLeft(lblCols), Input.location.lastData.altitude);
        Latitude.text = string.Format("{0}: {1:N7}", "Latitude".PadLeft(lblCols), Input.location.lastData.latitude);
        Longitude.text = string.Format("{0}: {1:N7}", "Longitude".PadLeft(lblCols), Input.location.lastData.longitude);
        Accuracy_Horiz.text = string.Format("{0}: {1}", "Horizontal Acc".PadLeft(lblCols), Input.location.lastData.horizontalAccuracy);
        Accuracy_Vert.text = string.Format("{0}: {1}", "Vertical Acc".PadLeft(lblCols), Input.location.lastData.verticalAccuracy);
    }

    /// <summary>
    /// This is borrowed from https://stackoverflow.com/questions/249760/how-to-convert-a-unix-timestamp-to-datetime-and-vice-versa
    /// </summary>
    /// <param name="unixTimeStamp" type="double"></param>
    /// <returns type="DateTime"></returns>
    public static System.DateTime UnixTimeStampToDateTime(double unixTimeStamp)
    {
        // NOTE: Unix timestamp is seconds past epoch
        System.DateTime dtDateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
        dtDateTime = dtDateTime.AddSeconds(unixTimeStamp).ToLocalTime();
        return dtDateTime;
    }

    void OnDestroy()
    {
        // NOTE: stop service
        Input.location.Stop();
    }
}

/// <summary>
/// Borrowed from https://gist.github.com/flarb/4980598
/// </summary>
public class MapUtils
{
    static float GOOGLEOFFSET = 268435456.0f;
    // NOTE: GOOGLEOFFSET / Mathf.PI
    static float GOOGLEOFFSET_RADIUS = 85445659.44705395f;
    static float MATHPI_180 = Mathf.PI / 180.0f;
    static private float preLonToX1 = GOOGLEOFFSET_RADIUS * (Mathf.PI / 180.0f);

    public static int LonToX(float lon)
    {
        return ((int) Mathf.Round(GOOGLEOFFSET + preLonToX1 * lon));
    }

    public static int LatToY(float lat)
    {
        return (int) Mathf.Round(GOOGLEOFFSET - GOOGLEOFFSET_RADIUS * Mathf.Log((1.0f + Mathf.Sin(lat * MATHPI_180)) / (1.0f - Mathf.Sin(lat * MATHPI_180))) / 2.0f);
    }

    public static float XToLon(float x)
    {
        return ((Mathf.Round(x) - GOOGLEOFFSET) / GOOGLEOFFSET_RADIUS) * 180.0f / Mathf.PI;
    }

    public static float YToLat(float y)
    {
        return (Mathf.PI / 2.0f - 2.0f * Mathf.Atan(Mathf.Exp((Mathf.Round(y) - GOOGLEOFFSET) / GOOGLEOFFSET_RADIUS))) * 180.0f / Mathf.PI;
    }

    public static float adjustLonByPixels(float lon, int delta, int zoom)
    {
        return XToLon(LonToX(lon) + (delta << (21 - zoom)));
    }

    public static float adjustLatByPixels(float lat, int delta, int zoom)
    {
        return YToLat(LatToY(lat) + (delta << (21 - zoom)));
    }
}