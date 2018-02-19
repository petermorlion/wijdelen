function exportToExcel(tableId, fileName) {
    var elt = document.getElementById(tableId);
    var wb = XLSX.utils.table_to_book(elt, { sheet: fileName });
    return XLSX.writeFile(wb, fileName + '.xlsx');
}