/* JS for Chat/Index */

var newMessageTextArea = document.getElementById('NewMessage');

newMessageTextArea.onkeydown = function (event) {
    if (event.ctrlKey && event.keyCode === 13) {
        document.getElementById('newMessageForm').submit();
    }
};