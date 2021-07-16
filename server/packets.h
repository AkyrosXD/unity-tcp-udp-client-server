#ifndef PACKETS_H
enum E_PACKET
{
    P_PLAYER_NAME,
    P_PLAYER_NAME_SUCCESS,
    P_PLAYER_JOINED,
    P_CREATE_MATCH_PLAYER,
    P_PLAYER_MOVEMENT,
    P_UPDATE_PLAYER_MOVEMENT,
    P_SEND_CHAT_MESSAGE,
    P_RECEIVE_CHAT_MESSAGE,
    P_PLAYER_LEFT
};

#include "unity.h"

typedef struct P_PlayerName
{
    char name[16];
} P_PlayerName;

typedef struct P_PlayerNameSuccess
{
    long long assigned_id;
    char name[16];
} P_PlayerNameSuccess;

typedef struct P_PlayerJoined
{
    long long id;
    char name[16];
} P_PlayerJoined;

typedef struct P_CreateMatchPlayer
{
    long long id;
    char name[16];
    Vector3 position;
    Quaternion rotation;
} P_CreateMatchPlayer;

typedef struct P_PlayerMovement
{
    long long player_id;
    float dx;
    float dy;
    Quaternion rotation;
} P_PlayerMovement;

typedef struct P_UpdatePlayerMovement
{
    long long player_id;
    Quaternion rotation;
    Vector3 motion;

} P_UpdatePlayerMovement;

typedef struct P_SendChatMessage
{
    char message[64];
} P_SendChatMessage;

typedef struct P_ReceiveChatMessage
{
    char sender[16];
    char message[64];
} P_ReceiveChatMessage;

typedef struct P_PlayerLeft
{
    long long id;
    char name[16];
} P_PlayerLeft;

#define PACKETS_H
#endif
