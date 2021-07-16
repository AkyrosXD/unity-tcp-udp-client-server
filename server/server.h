#ifndef SERVER_H
#include <pthread.h>
#include <sys/socket.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <fcntl.h>
#include <signal.h>
#include <unistd.h>
#include <string.h>
#include <stdlib.h>
#include <stdio.h>
#include "unity.h"

#define TCP_SERVER_PORT 5004
#define UDP_SERVER_PORT 5025

#define MAX_DATA_LENGTH 1024

typedef unsigned char byte_t;

typedef short packetlen_t;

typedef struct PacketBase
{
    // packet id
    byte_t id;
    
    // the whole length of the packet
    packetlen_t length;
} __attribute__((packed)) PacketBase;

typedef struct Packet
{
    // the base...
    PacketBase base;

    // all the data in the packet
    byte_t* data;
} Packet;

typedef struct ClientUser
{
    // tcp socket for sending and receiving data
    int tcp_socket;

    // udp client
    // this helpes us send data through udp to a specific connection
    struct sockaddr* udp_client;

    // id of the user/player
    long long player_id;

    // the last number of bytes that have been sent to the client
    unsigned long long last_bytes_sent;

    // if the user has joined the match
    int has_joined;

    // name of the player
    char player_name[16];

    // position of the player
    Vector3 position;

    // rotation of the player
    Quaternion rotation;
} ClientUser;

int start_server();
void add_user(ClientUser* user);
void remove_user(ClientUser* user);
int get_user_index(long long id);
void send_tcp_packet(ClientUser* user, byte_t packetId, void* data, size_t data_length);
void send_udp_packet(int udpSocket, const struct sockaddr* client, socklen_t clientSize, byte_t packetId, void* data, size_t data_length);
void handle_tcp_packet(ClientUser* user, Packet packet);
void handle_udp_packet(int udpSocket, const struct sockaddr* client, socklen_t clientSize, Packet packet);
void* udp_server_thread(void* ptrUdpSocket);
void* user_thread(void* ptrTcpSocket);
#define SERVER_H
#endif