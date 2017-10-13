function s2ab(s) {
    if (typeof ArrayBuffer !== 'undefined') {
        var buf = new ArrayBuffer(s.length);
        var view = new Uint8Array(buf);
        for (var i = 0; i != s.length; ++i) view[i] = s.charCodeAt(i) & 0xFF;
        return buf;
    } else {
        var buf = new Array(s.length);
        for (var i = 0; i != s.length; ++i) buf[i] = s.charCodeAt(i) & 0xFF;
        return buf;
    }
}

function exportToExcel(tableId, fileName) {
    var reportTable = document.getElementById(tableId);

    var wb = XLSX.utils.table_to_book(reportTable, { sheet: "Data" });
    var wbout = XLSX.write(wb, { bookType: 'xlsx', bookSST: true, type: 'binary' });
    var fname = fileName + '.xlsx';
    try {
        saveAs(new Blob([s2ab(wbout)], { type: "application/octet-stream" }), fname);
    } catch (e) {
        if (typeof console != 'undefined') console.log(e, wbout);
    }

    return wbout;
}