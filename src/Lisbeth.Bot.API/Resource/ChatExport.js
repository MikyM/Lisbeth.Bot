let container = document.getElementById("container");
container.classList.add("d-flex", "flex-column", "align-items-center");

let usersWrapper = document.getElementById("users-wrapper");
usersWrapper.classList.add("pb-2", "d-flex", "flex-column", "align-items-start");

let users = document.getElementsByClassName("user");
for (let i = 0; i < users.length; i++) {
    users[i].classList.add("p-1", "d-flex", "align-items-center");
}

let usersInfo = document.getElementsByClassName("user-info");
for (let i = 0; i < usersInfo.length; i++) {
    usersInfo[i].classList.add("m-1");
}

let messagesWrapper = document.getElementById("messages-wrapper");
messagesWrapper.classList.add("pb-2");

let messages = document.getElementsByClassName("message");
for (let i = 0; i < messages.length; i++) {
    messages[i].classList.add("d-flex", "align-items-start", "flex-column");
}

let messageReactions = document.getElementsByClassName("message-reactions");
for (let i = 0; i < messageReactions.length; i++) {
    messageReactions[i].classList.add("d-flex", "align-items-center");
}

let reactionWrappers = document.getElementsByClassName("reaction-wrapper");
for (let i = 0; i < reactionWrappers.length; i++) {
    reactionWrappers[i].classList.add("d-flex", "align-items-center", "justify-content-center");
}

$(".image").on("click",
    function() {
        window.open($(this).attr("src"), "_blank", "menubar=1,resizable=1");
    });