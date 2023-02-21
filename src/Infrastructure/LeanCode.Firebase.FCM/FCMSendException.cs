using System;

namespace LeanCode.Firebase.FCM;

public class FCMSendException : Exception
{
    public FCMSendException()
        : base("There was an error with one or more push notifications.")
    { }
}
