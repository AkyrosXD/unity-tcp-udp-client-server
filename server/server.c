#include "server.h"
#include "packets.h"

static ClientUser** users;
static size_t user_count = 0;
static pthread_mutex_t users_mutex = PTHREAD_MUTEX_INITIALIZER;

int start_server()
{
    int mainSocket = socket(AF_INET, SOCK_STREAM, IPPROTO_TCP);
    if (mainSocket == -1)
    {
        printf("Server error (-1)\n");
        return -1;
    }
    struct sockaddr_in addr;
    addr.sin_family = AF_INET;
    addr.sin_port = htons(TCP_SERVER_PORT);
    inet_pton(AF_INET, "0.0.0.0", &addr.sin_addr);
    int bindResult = bind(mainSocket, (struct sockaddr*)&addr, sizeof(addr));
    if (bindResult == -1)
    {
        printf("Server error (-3)\n");
        return -3;
    }
    int listenResult = listen(mainSocket, SOMAXCONN);
    if (listenResult == -1)
    {
        printf("Server error (-4)\n");
        return -4;
    }
    int udp_socket = socket(AF_INET, SOCK_DGRAM, 0);
    if (udp_socket == -1)
    {
        printf("Server error (-5)\n");
        return -5;
    }
    addr.sin_port = htons(UDP_SERVER_PORT);
    bindResult = bind(udp_socket, (struct sockaddr*)&addr, sizeof(addr));
    if (bindResult == -1)
    {
        printf("Server error (-6)\n");
        return -6;
    }
    signal(SIGPIPE, SIG_IGN);
    pthread_t threadid;
    pthread_create(&threadid, 0, udp_server_thread, (void*)&udp_socket);
    while (1)
    {
        int tcpSocket = accept(mainSocket, 0, 0);
        pthread_create(&threadid, 0, user_thread, (void*)&tcpSocket);
    }
    return 0;
}

void* udp_server_thread(void* ptrUdpSocket)
{
    int udpSocket = *(int*)ptrUdpSocket;
    socklen_t szClient = sizeof(struct sockaddr);
    const int UDP_MAX_DATA = 512;
    byte_t buffer[UDP_MAX_DATA];
    while (1)
    {
        struct sockaddr_in client;
        struct sockaddr* pClient = (struct sockaddr*)&client;
        long long bytesReceived = recvfrom(udpSocket, buffer, UDP_MAX_DATA, MSG_WAITALL, pClient, &szClient);
        if (bytesReceived > 0)
        {
            Packet packet;
            packet.base = *(PacketBase*)buffer;
            packet.data = buffer + sizeof(packet.base);
            handle_udp_packet(udpSocket, pClient, szClient, packet);
        }
        usleep(50);
    }
    return 0;
}

void* user_thread(void* ptrTcpSocket)
{
    ClientUser user;
    user.tcp_socket = *(int*)ptrTcpSocket;
    user.player_id = (long long)&user;
    printf("user connected: %lld\n", user.player_id);
    add_user(&user);
    byte_t* buffer = 0;
    int bBase = 0;
    int offset = 0;
    Packet tcpPacket;
    while (1)
    {
        // last_bytes_sent will equal to -1 if the last `send` function failed
        if (user.last_bytes_sent == -1)
        {
            break;
        }
        if (!bBase)
        {
            long long tcpBasebytesReceived = recv(user.tcp_socket, &tcpPacket.base + offset, sizeof(tcpPacket.base) - offset, 0);
            if (tcpBasebytesReceived > 0)
            {
                offset += tcpBasebytesReceived;
                bBase = (offset == sizeof(tcpPacket.base));
            }
            else
            {
                break;
            }
        }
        else
        {
            if (buffer == 0)
            {
                buffer = (byte_t*)malloc(tcpPacket.base.length);
                memcpy(buffer, &tcpPacket.base, sizeof(tcpPacket.base));
            }
            long long tcpBytesReceived = recv(user.tcp_socket, buffer + offset, tcpPacket.base.length - offset, 0);
            if (tcpBytesReceived > 0)
            {
                offset += tcpBytesReceived;
                if (offset < tcpPacket.base.length)
                {
                    continue;
                }
                tcpPacket.data = buffer + sizeof(tcpPacket.base);
                handle_tcp_packet(&user, tcpPacket);
                free(buffer);
                buffer = 0;
                offset = 0;
                bBase = 0;
            }
            else
            {
                break;
            }
        }
        usleep(50);
    }
    remove_user(&user);
    P_PlayerLeft playerLeft;
    playerLeft.id = user.player_id;
    strcpy(playerLeft.name, user.player_name);
    for (size_t i = 0; i < user_count; i++)
    {
        if (users[i]->has_joined)
        {
            send_tcp_packet(users[i], P_PLAYER_LEFT, &playerLeft, sizeof(P_PlayerLeft));
        }
    }
    pthread_exit(0);
    return 0;
}

void add_user(ClientUser* user)
{
    pthread_mutex_lock(&users_mutex);
    const size_t SZ_USER = sizeof(ClientUser*);
    size_t newSize = SZ_USER * (user_count + 1);
    users = (ClientUser**)realloc(users, newSize);
    users[user_count] = user;
    user_count++;
    pthread_mutex_unlock(&users_mutex);
}

void remove_user(ClientUser* user)
{
    pthread_mutex_lock(&users_mutex);
    if (users != 0 && user_count > 0)
    {
        for (size_t i = 0; i < user_count; i++)
        {
            if (users[i] == user)
            {
                int tcpSocket = users[i]->tcp_socket;
                close(tcpSocket);
                shutdown(tcpSocket, SHUT_WR);
                if (users[i]->udp_client != 0)
                {
                    free(users[i]->udp_client);
                }
                // swap the target user with the last user and then reallocate -1 the previous size
                // the last block gets removed, so the target user gets removed
                ClientUser* target = users[i];
                ClientUser* last = users[user_count - 1];
                users[user_count - 1] = target;
                users[i] = last;
                users = (ClientUser**)realloc(users, user_count - 1);
                user_count--;
                printf("user disconnected: %lld\n", user->player_id);
                break;
            }
        }
    }
    pthread_mutex_unlock(&users_mutex);
}

int get_user_index(long long id)
{
    if (users != 0 && user_count > 0)
    {
        for (size_t i = 0; i < user_count; i++)
        {
            if (users[i]->player_id == id)
                return (int)i;
        }
    }
    return -1;
}

void send_tcp_packet(ClientUser* user, byte_t packetId, void* data, size_t data_length)
{
    PacketBase pb;
    packetlen_t fullPacketSize = (packetlen_t)(sizeof(pb) + data_length);
    byte_t buffer[fullPacketSize];
    buffer[0] = packetId;
    memcpy(buffer + sizeof(pb.id), &fullPacketSize, sizeof(pb.length));
    memcpy(buffer + sizeof(pb), data, data_length);
    user->last_bytes_sent = send(user->tcp_socket, buffer, fullPacketSize, 0);
}

void send_udp_packet(int udpSocket, const struct sockaddr* client, socklen_t clientSize, byte_t packetId, void* data, size_t data_length)
{
    PacketBase pb;
    packetlen_t fullPacketSize = (packetlen_t)(sizeof(pb) + data_length);
    byte_t buffer[fullPacketSize];
    buffer[0] = packetId;
    memcpy(buffer + sizeof(pb.id), &fullPacketSize, sizeof(pb.length));
    memcpy(buffer + sizeof(pb), data, data_length);
    sendto(udpSocket, buffer, fullPacketSize, 0, client, clientSize);
}

void handle_tcp_packet(ClientUser* user, Packet packet)
{
    byte_t packetId = packet.base.id;
    byte_t* data = packet.data;
    switch (packetId)
    {
        case P_PLAYER_NAME:
        {
            P_PlayerName* pname = (P_PlayerName*)data;
            if (pname->name[0] == 0)
            {
                printf("Empty name -> do nothing\n");
            }
            else if (user->player_name[0] == 0)
            {
                P_PlayerNameSuccess pns;
                pns.assigned_id = user->player_id;
                strcpy(pns.name, pname->name);
                strcpy(user->player_name, pname->name);
                send_tcp_packet(user, P_PLAYER_NAME_SUCCESS, &pns, sizeof(P_PlayerNameSuccess));
            }
        }
        break;

        case P_PLAYER_JOINED:
        {
            if (!user->has_joined)
            {
                P_PlayerJoined* playerJoined = (P_PlayerJoined*)data;
                printf("Player joined: %s\n", playerJoined->name);
                for (size_t i = 0; i < user_count; i++)
                {
                    send_tcp_packet(users[i], P_PLAYER_JOINED, playerJoined, sizeof(P_PlayerJoined));
                    P_CreateMatchPlayer cmp;
                    if (users[i] != user && users[i]->has_joined)
                    {
                        cmp.id = users[i]->player_id;
                        strcpy(cmp.name, users[i]->player_name);
                        cmp.position = users[i]->position;
                        cmp.rotation = users[i]->rotation;
                        send_tcp_packet(user, P_CREATE_MATCH_PLAYER, &cmp, sizeof(P_CreateMatchPlayer));
                    }
                }
                user->has_joined = 1;
            }
        }
        break;

        case P_SEND_CHAT_MESSAGE:
        {
            P_SendChatMessage* incoming = (P_SendChatMessage*)data;
            if (incoming->message[0] != 0)
            {
                P_ReceiveChatMessage outgoing;
                strcpy(outgoing.sender, user->player_name);
                strcpy(outgoing.message, incoming->message);
                for (size_t i = 0; i < user_count; i++)
                {
                    send_tcp_packet(users[i], P_RECEIVE_CHAT_MESSAGE, &outgoing, sizeof(P_ReceiveChatMessage));            
                }
            }
        }
        break;
    
    default:
        break;
    }
}

void handle_udp_packet(int udpSocket, const struct sockaddr* client, socklen_t clientSize, Packet packet)
{
    byte_t packetId = packet.base.id;
    byte_t* data = packet.data;
    switch (packetId)
    {
        case P_PLAYER_MOVEMENT:
        {
            const float SPEED = 20.0f;
            P_PlayerMovement* playerMovement = (P_PlayerMovement*)data;
            P_UpdatePlayerMovement updateMovement;
            updateMovement.player_id = playerMovement->player_id;
            updateMovement.rotation = playerMovement->rotation;
            int userIndex = get_user_index(playerMovement->player_id);
            if (userIndex != -1)
            {
                // create the udp client for the user
                ClientUser* user = users[userIndex];
                if (user->udp_client == 0)
                {
                    user->udp_client = (struct sockaddr*)malloc(clientSize);
                    memcpy(user->udp_client, client, clientSize);
                }

                // this prevents "speed hacking" if the cheater modifies the axis in the packet
                playerMovement->dx *= (playerMovement->dx <= 1.0f);
                playerMovement->dy *= (playerMovement->dy <= 1.0f);

                // same as the client-sided calculation
                Vector3 right = Quaternion_Multiply(playerMovement->rotation, Vector3_right());
                Vector3 forward = Quaternion_Multiply(playerMovement->rotation, Vector3_forward());
                Vector3 mx = Vector3_Multiply(right, playerMovement->dx);
                Vector3 my = Vector3_Multiply(forward, playerMovement->dy);
                Vector3 motion = Vector3_Addition(mx, my);
                motion = Vector3_Multiply(motion, FIXED_DELTA_TIME * SPEED);

                updateMovement.motion = motion;
                user->position = Vector3_Addition(user->position, motion);
                user->rotation = playerMovement->rotation;

                // send the movement packet to all the players...
                for (size_t i = 0; i < user_count; i++)
                {
                    send_udp_packet(udpSocket, users[i]->udp_client, clientSize, P_UPDATE_PLAYER_MOVEMENT, &updateMovement, sizeof(P_UpdatePlayerMovement));
                }
            }
        }
        break;


    default:
        break;    
    }
}