﻿namespace Lidgren.Network
{
    public enum NetLogCode
    {
        Unknown = 0,
        PacketCallbackException,
        InvalidPacketSize,
        ConnectionReset,
        SendFailure,
        ReceiveFailure,
        FullSendFailure,
        MessageSizeExceeded,
        FragmentGroupTimeout,
        UPnPInvalidResponse,
        UPnPPortForwardFailed,
        UPnPPortDeleteFailed,
        UPnPExternalIPFailed,
        SuppressedUnreliableAck,
        SocketWouldBlock,
        DuplicateConnection,
        MissingHandshake,
        InvalidHandshake,
        MessageTypeDisabled,
        UnhandledLibraryMessage,
        UnconnectedLibraryMessage,
        HeartbeatException,
        DisconnectedHandshake,
        MissingIPv6ForDualStack,
        InvalidFragmentIndex,
        InvalidFragmentHeader,
        UnexpectedHandshakeStatus,
        NotConnected,
        AlreadyConnected,
        IgnoringMultipleConnects,
        UnexpectedApprove,
        UnexpectedLibraryError,
        SocketShutdownException,
        SocketCloseException,
        SocketRebindDelayed,
        PingPongMismatch,

        EarlyMessage,
        TooEarlyMessage,
        DuplicateMessage,
        DuplicateOrLateMessage,
        WithheldMessage,
        HostPortChanged,
        SocketBound,
        ConnectionTimedOut,
        DeadlineTimeoutInitialized,
        UnexpectedConnect,
        UnexpectedMTUExpandRequest,
        DuplicateFragment,
        ReceivedFragment,
        FragmentGroupFinished,
        ResendingConnect,
        ResendingRespondedConnect,
        UnhandledConnect,
        UnhandledHandshakeMessage,
        InitiatedAverageRoundtripTime,
        UpdatedAverageRoundtripTime,
        SimulatedLoss,
        SimulatedDelay,
        ExpandedMTU,
        AbortingConnectionAttempt,
        NATIntroductionReceived,
        NATPunchSent,
        HostNATPunchSuccess,
        ClientNATPunchSuccess,
    }
}