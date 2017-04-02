/* JS for ObjectRequest/New */

new autoComplete({
    selector: '[data-auto-complete="true"]',
    minChars: 3,
    source: function (term, suggest) {
        term = term.toLowerCase();

        var xhr = new XMLHttpRequest();
        xhr.open('GET', '../../Api/WijDelen.ObjectSharing/Archetypes?input=' + term);
        xhr.setRequestHeader('Accept', 'application/json');
        xhr.onload = function () {
            if (xhr.status === 200) {
                console.log(xhr.responseText);
                suggest(JSON.parse(xhr.responseText));
            }
            else {
                console.log('Request failed.  Returned status of ' + xhr.status);
            }
        };
        xhr.send();
    }
});

var element = document.getElementById('ExtraInfo');
element.oninput = function() {
    var remainingCharacterCountElement = document.getElementById('remainingCharacterCount');
    var remainCharacterTextElement = document.getElementById('remainCharacterText');
    var progressBar = document.getElementById('objectrequest-progressbar-fill');
    var enoughCharactersTextElement = document.getElementById('enoughCharactersText');

    var remainingCharacters = 30 - element.value.length;

    remainCharacterTextElement.style.display = remainingCharacters > 0 ? 'inline' : 'none';
    remainingCharacterCountElement.innerText = remainingCharacters;
    enoughCharactersTextElement.style.display = remainingCharacters > 0 ? 'none' : 'inline';

    var percentage = Math.min(element.textLength * 100 / 30, 100);
    progressBar.style.width = percentage.toString() + '%';
};