"use strict";

var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

//Disable send button until connection is established
document.getElementById("sendButton").disabled = true;

connection.on("ReceiveMessage", function (user, message) {
    var p = document.createElement("p");
    p.setAttribute("style", "margin:2px;");
    document.getElementById("messagesList").appendChild(p);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    p.textContent = `${user}: ${message}`;
    var msgDiv = document.getElementById("messagesList");
    msgDiv.scrollTop = msgDiv.scrollHeight;
});

connection.on("ShutConnection", function (user, message) {
    connection.stop();
    var p = document.createElement("p");
    p.setAttribute("style", "margin:2px;");
    document.getElementById("messagesList").appendChild(p);
    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    p.textContent = `Chat opened in another tab, disconnected.`;
    var msgDiv = document.getElementById("messagesList");
    msgDiv.scrollTop = msgDiv.scrollHeight;
});

connection.on("ActiveUserList", function (message) {

    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.
    var userArray = message.split("&");
    userArray.shift();
    userArray.pop();
    userArray.forEach((userNick) => {
        var li = document.createElement("li");
        li.setAttribute("style", "list-style-type: none;");
        document.getElementById("activeUsersDiv").appendChild(li);
        li.textContent = `${userNick}`;
    });
});

connection.on("NewUserConnected", function (message) {

    // We can assign user-supplied strings to an element's textContent because it
    // is not interpreted as markup. If you're assigning in any other way, you 
    // should be aware of possible script injection concerns.

    var li = document.createElement("li");
    li.setAttribute("style", "list-style-type: none;");
    document.getElementById("activeUsersDiv").appendChild(li);
    li.textContent = `${message}`;

});

connection.on("UserDisconnected", function (message) {

    var ancestor = document.getElementById('activeUsersDiv'),
        descendents = ancestor.getElementsByTagName('li');

    for (var i = 0; i < descendents.length; i++) {
        if (descendents[i].innerHTML == message) {
            descendents[i].remove();
            break;
        }
    }

});

connection.start().then(function () {
    document.getElementById("sendButton").disabled = false;
}).catch(function (err) {
    return console.error(err.toString());
});

document.getElementById("sendButton").addEventListener("click", function (event) {
    var message = document.getElementById("messageInput").value;
    if (message === '') { }
    else {
        document.getElementById("messageInput").value = '';
        connection.invoke("SendMessage", message).catch(function (err) {
            return console.error(err.toString());
        });
        event.preventDefault();
    }
});