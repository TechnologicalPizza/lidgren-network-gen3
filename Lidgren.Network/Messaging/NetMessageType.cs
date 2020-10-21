﻿
namespace Lidgren.Network
{
    internal enum NetMessageType : byte
    {
        Unconnected = 0,

        UserUnreliable = 1,

        UserSequenced1 = 2,
        UserSequenced2 = 3,
        UserSequenced3 = 4,
        UserSequenced4 = 5,
        UserSequenced5 = 6,
        UserSequenced6 = 7,
        UserSequenced7 = 8,
        UserSequenced8 = 9,
        UserSequenced9 = 10,
        UserSequenced10 = 11,
        UserSequenced11 = 12,
        UserSequenced12 = 13,
        UserSequenced13 = 14,
        UserSequenced14 = 15,
        UserSequenced15 = 16,
        UserSequenced16 = 17,
        UserSequenced17 = 18,
        UserSequenced18 = 19,
        UserSequenced19 = 20,
        UserSequenced20 = 21,
        UserSequenced21 = 22,
        UserSequenced22 = 23,
        UserSequenced23 = 24,
        UserSequenced24 = 25,
        UserSequenced25 = 26,
        UserSequenced26 = 27,
        UserSequenced27 = 28,
        UserSequenced28 = 29,
        UserSequenced29 = 30,
        UserSequenced30 = 31,
        UserSequenced31 = 32,
        UserSequenced32 = 33,

        UserReliableUnordered = 34,

        UserReliableSequenced1 = 35,
        UserReliableSequenced2 = 36,
        UserReliableSequenced3 = 37,
        UserReliableSequenced4 = 38,
        UserReliableSequenced5 = 39,
        UserReliableSequenced6 = 40,
        UserReliableSequenced7 = 41,
        UserReliableSequenced8 = 42,
        UserReliableSequenced9 = 43,
        UserReliableSequenced10 = 44,
        UserReliableSequenced11 = 45,
        UserReliableSequenced12 = 46,
        UserReliableSequenced13 = 47,
        UserReliableSequenced14 = 48,
        UserReliableSequenced15 = 49,
        UserReliableSequenced16 = 50,
        UserReliableSequenced17 = 51,
        UserReliableSequenced18 = 52,
        UserReliableSequenced19 = 53,
        UserReliableSequenced20 = 54,
        UserReliableSequenced21 = 55,
        UserReliableSequenced22 = 56,
        UserReliableSequenced23 = 57,
        UserReliableSequenced24 = 58,
        UserReliableSequenced25 = 59,
        UserReliableSequenced26 = 60,
        UserReliableSequenced27 = 61,
        UserReliableSequenced28 = 62,
        UserReliableSequenced29 = 63,
        UserReliableSequenced30 = 64,
        UserReliableSequenced31 = 65,
        UserReliableSequenced32 = 66,

        UserReliableOrdered1 = 67,
        UserReliableOrdered2 = 68,
        UserReliableOrdered3 = 69,
        UserReliableOrdered4 = 70,
        UserReliableOrdered5 = 71,
        UserReliableOrdered6 = 72,
        UserReliableOrdered7 = 73,
        UserReliableOrdered8 = 74,
        UserReliableOrdered9 = 75,
        UserReliableOrdered10 = 76,
        UserReliableOrdered11 = 77,
        UserReliableOrdered12 = 78,
        UserReliableOrdered13 = 79,
        UserReliableOrdered14 = 80,
        UserReliableOrdered15 = 81,
        UserReliableOrdered16 = 82,
        UserReliableOrdered17 = 83,
        UserReliableOrdered18 = 84,
        UserReliableOrdered19 = 85,
        UserReliableOrdered20 = 86,
        UserReliableOrdered21 = 87,
        UserReliableOrdered22 = 88,
        UserReliableOrdered23 = 89,
        UserReliableOrdered24 = 90,
        UserReliableOrdered25 = 91,
        UserReliableOrdered26 = 92,
        UserReliableOrdered27 = 93,
        UserReliableOrdered28 = 94,
        UserReliableOrdered29 = 95,
        UserReliableOrdered30 = 96,
        UserReliableOrdered31 = 97,
        UserReliableOrdered32 = 98,

        UserNetStream1 = 99,
        UserNetStream2 = 100,
        UserNetStream3 = 101,
        UserNetStream4 = 102,
        UserNetStream5 = 103,
        UserNetStream6 = 104,
        UserNetStream7 = 105,
        UserNetStream8 = 106,
        UserNetStream9 = 107,
        UserNetStream10 = 108,
        UserNetStream11 = 109,
        UserNetStream12 = 110,
        UserNetStream13 = 111,
        UserNetStream14 = 112,
        UserNetStream15 = 113,
        UserNetStream16 = 114,
        UserNetStream17 = 115,
        UserNetStream18 = 116,
        UserNetStream19 = 117,
        UserNetStream20 = 118,
        UserNetStream21 = 119,
        UserNetStream22 = 120,
        UserNetStream23 = 121,
        UserNetStream24 = 122,
        UserNetStream25 = 123,
        UserNetStream26 = 124,
        UserNetStream27 = 125,
        UserNetStream28 = 126,
        UserNetStream29 = 127,
        UserNetStream30 = 128,
        UserNetStream31 = 129,
        UserNetStream32 = 130,

        LibraryError = 131,
        Ping = 132, // used for RTT calculation
        Pong = 133, // used for RTT calculation
        Connect = 134,
        ConnectResponse = 135,
        ConnectionEstablished = 136,
        Acknowledge = 137,
        Disconnect = 138,
        Discovery = 139,
        DiscoveryResponse = 140,
        NatPunchMessage = 141, // send between peers
        NatIntroduction = 142, // send to master server
        ExpandMTURequest = 143,
        ExpandMTUSuccess = 144

        // 111 left
    }
}