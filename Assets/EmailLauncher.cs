using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EmailLauncher : MonoBehaviour
{
    string receiverEmail = "guygir@gmail.com";
    string emailSubject = "התוצאות שלי במשחק הבחירות";
    string emailBody = "Email Body";
    public TMP_InputField myEmail;

    public void LaunchEmailClient()
    {
        receiverEmail = myEmail.text;
        emailBody = "ה" + FindObjectOfType<CardLoader>().GetCurrentAmountToPick() + " תכונות שבחרתי הן: ";
        List<string> chosen = FindObjectOfType<GridManager>().ListAllPicked();
        for (int i = 0; i < chosen.Count; i++)
        {
            emailBody += chosen[i]+", ";
        }
        emailBody = emailBody.Substring(0, emailBody.Length - 2);
        emailBody += ".";

        // Encode the subject and body to be included in the mailto URL
        string encodedSubject = System.Uri.EscapeDataString(emailSubject);
        string encodedBody = System.Uri.EscapeDataString(emailBody);

        // Form the mailto URL
        string mailtoURL = "mailto:" + receiverEmail + "?subject=" + encodedSubject + "&body=" + encodedBody;

        // Open the default email client with the prepared subject and body
        Application.OpenURL(mailtoURL);
    }
}
