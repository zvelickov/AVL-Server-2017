namespace Taxi.Communication.Server.PhoneSwitch
{
    public enum MessageType
    {
        SYNCH_INCOMMING, ASYNCH_INCOMMING, OUTGOING
    }

    public enum Command
    {
        BINNARY_INTEGER_READ = 1,
        ASCII_INTEGER_READ = 2,
        GENERAL_READ = 3,
        INTEGER_WRITE = 4,
        CHAR_WRITE = 5,
        CHAR_READ = 6
    }

    public enum PhoneExchangeMessageType
    {
        NN = 1, // Not Valid message
        NA = 2, // Not answered call
        RC = 3, // Received call
        AN = 4, // Answered call
        CC = 5, // Closed
        CA = 6,  // Titka
        AC = 7,  // ON HOLD
        PA = 8  // Parking
    }

    public enum PhoneExchangeCommand
    {

    }

}