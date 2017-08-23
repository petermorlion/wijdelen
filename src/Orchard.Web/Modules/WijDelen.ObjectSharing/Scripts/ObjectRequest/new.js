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
    var characterCount = element.value.length;
    var noCharactersText = document.getElementById('noCharactersText');
    noCharactersText.style.display = 'none';

    var oneCharacterText = document.getElementById('oneCharacterText');
    oneCharacterText.style.display = 'none';

    var tenCharactersText = document.getElementById('tenCharactersText');
    tenCharactersText.style.display = 'none';

    var twentyCharactersText = document.getElementById('twentyCharactersText');
    twentyCharactersText.style.display = 'none';

    var enoughCharactersTextElement = document.getElementById('enoughCharactersText');
    enoughCharactersTextElement.style.display = 'none';

    if (characterCount === 0) {
        noCharactersText.style.display = 'inline';
    } else if (characterCount >= 1 && characterCount < 10) {
        oneCharacterText.style.display = 'inline';
    } else if (characterCount >= 10 && characterCount < 20) {
        tenCharactersText.style.display = 'inline';
    } else if (characterCount >= 20 && characterCount < 30) {
        twentyCharactersText.style.display = 'inline';
    } else if (characterCount >= 30) {
        enoughCharactersTextElement.style.display = 'inline';
    }

    var percentage = Math.min(element.textLength * 100 / 30, 100);
    var progressBar = document.getElementById('objectrequest-progressbar-fill');
    progressBar.style.width = percentage.toString() + '%';
};