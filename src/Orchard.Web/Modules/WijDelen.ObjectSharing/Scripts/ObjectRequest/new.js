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