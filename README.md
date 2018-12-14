# chatServer
SchoolChatServerExcercise

This is a project that i did as an excercise.

It's a server for a chat application that uses the following protocol:

SERVER SIDE
Action->Response
Client connection -> CLIENTX (X is an integer starting from 0)
When receiving a name(Nname) the server checks if it already exists -> Y(Accepted)/N(Already Existing)
Recieving "L" -> Send U with a csv strign ("|") with the name of the connected users
Recieve a message request (Mreciever1,reciever2|message) -> OK(All the messages sent)/Rreciever1,reciever2(recievers did not recieve the message)
Send message to clients -> Msender|message
Client disconnects -> Update User List

CLIENT SIDE
Action->Message to send
Connection to server -> Nname(to seto your name)
Request online users list -> L
Send message -> Mreciever|message / Mreciever1,reciever2|message
Send message to all -> M*|message

ATTENTION!
All the users name are case unsensitive and can't contain the characters '|' and ','

Ho modificato la descrizione


